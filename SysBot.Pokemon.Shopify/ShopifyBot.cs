using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Fleck;
using NLog;
using System.Net.Sockets;
using System.Linq;
using ShopifySharp;
namespace SysBot.Pokemon.Shopify;

public class ShopifyBot<T> where T : PKM, new()
{
    private static PokeTradeHub<T> Hub = default!;
    internal static TradeQueueInfo<T> Info => Hub.Queues.Info;

    internal static readonly List<ShopifyQueue<T>> QueuePool = new List<ShopifyQueue<T>>();

    private readonly ShopifySettings Settings;

    public ShopifyBot(ShopifySettings settings, PokeTradeHub<T> hub)
    {
        Hub = hub;
        Settings = settings;

        StartServer();

        LogUtil.LogInfo("Bot started.", nameof(ShopifyBot<T>));
    }

    private static ulong GetOrderIDFromWS(IWebSocketConnection socket)
    {
        string path = socket.ConnectionInfo.Path;
        var match = Regex.Match(path, @"[?&]id=(\d+)");
        if (match.Success && ulong.TryParse(match.Groups[1].Value, out ulong id))
        {
            return id;
        }
        return 0;
    }

    private void StartServer()
    {
        var server = new WebSocketServer("ws://0.0.0.0");

        server.Start(socket =>
        {
            socket.OnOpen = () => HandleSocketOpen(socket);
            socket.OnClose = () => HandleSocketClose(socket);
        });

        LogUtil.LogInfo("WebSocket server started at ws://0.0.0.0.", nameof(ShopifyBot<T>));
    }

    private async void HandleSocketOpen(IWebSocketConnection socket)
    {
        // Fetching orderID from the websocket.
        ulong orderID = GetOrderIDFromWS(socket);
        if (orderID == 0)
        {
            LogUtil.LogInfo($"Connection from {socket.ConnectionInfo.ClientIpAddress} did not include an orderID.", nameof(ShopifyBot<T>));
            await socket.Send(Settings.FR_InvalidOrderID);
            socket.Close();
            return;
        }

        // Logging and sending message to client that websocket opened.
        LogUtil.LogInfo($"New websocket from orderID {orderID}.", nameof(ShopifyBot<T>));
        await socket.Send(Settings.FR_ConnectionSuccess);

        if (!await CheckShopifyOrder(orderID))
        {
            LogUtil.LogInfo($"Failed to check orderID {orderID}.", nameof(ShopifyBot<T>));
            await socket.Send(Settings.FR_InvalidOrderID);
            socket.Close();
            return;
        }

        // Shopify logic.
        string showdownset = "Charizard @ Choice Specs\r\nAbility: Solar Power\r\nTera Type: Fire\r\nEVs: 252 SpA / 4 SpD / 252 Spe\r\nTimid Nature\r\nIVs: 0 Atk\r\nWeather Ball\r\nFocus Blast\r\nSolar Beam";
        //

        // Perform trade.
        var res = await PerformTrade(orderID, socket, showdownset);
        if (!res)
        {
            LogUtil.LogInfo($"Failed to initialize trade for orderID {orderID}.", nameof(ShopifyBot<T>));
            await socket.Send(Settings.FR_TradeInitFailed);
            socket.Close();
            return;
        }

        // When trade "TOTALLY" finished we can mark it as fulfilled.
        // For now its in the callback of the trade.

        LogUtil.LogInfo($"Trade finished orderID: {orderID}.", nameof(ShopifyBot<T>));
        socket.Close();
    }

    private void HandleSocketClose(IWebSocketConnection socket)
    {
        var orderID = GetOrderIDFromWS(socket);

        if (orderID == 0)
            return;

        Info.ClearTrade(orderID);
        LogUtil.LogInfo($"Websocket closed for orderID: {orderID}.", nameof(ShopifyBot<T>));
    }

    async private Task<bool> CheckShopifyOrder(ulong orderID)
    {
        try
        {
            var service = new OrderService(Settings.ShopUrl, Settings.AccessToken);
            var order = await service.GetAsync((long)orderID);

            if (order == null)
            {
                LogUtil.LogInfo($"Order ID {orderID} does not exist.", nameof(ShopifyBot<T>));
                return false;
            }
            return true;

        }
        catch (ShopifyException ex)
        {
            LogUtil.LogError($"An error occurred on orderID {orderID} : {ex.Message}", nameof(ShopifyBot<T>));
            return false;
        }
    }

    async public Task<bool> MarkedAsFulfilled(ulong orderID)
    {
        try
        {
            var fulfillmentService = new FulfillmentService(Settings.ShopUrl, Settings.AccessToken);
            var fulfillmentOrderService = new FulfillmentOrderService(Settings.ShopUrl, Settings.AccessToken);

            var openFulfillmentOrders = await fulfillmentOrderService.ListAsync((long)orderID);
            openFulfillmentOrders = openFulfillmentOrders.Where(f => f.Status == "open").ToList();

            if (openFulfillmentOrders.Count() == 0)
            {
                throw new Exception($"No open fulfillment orders found for orderID: {orderID}");
            }

            // Fulfill the line items
            var lineItems = openFulfillmentOrders.Select(o => new LineItemsByFulfillmentOrder
            {
                FulfillmentOrderId = o.Id.Value
            });

            var fulfillment = await fulfillmentService.CreateAsync(new FulfillmentShipping
            {
                Message = "Successfully delivered",
                FulfillmentRequestOrderLineItems = lineItems,
                NotifyCustomer = true,
            });

            return true;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"{ex.Message}", nameof(ShopifyBot<T>));
            return false;
        }
        
    }

    async private Task<bool> PerformTrade(ulong orderID, IWebSocketConnection socket, string showdownset)
    {
        bool IsUserInQueue(ulong userId)
        {
            return Info.GetIsUserQueued(entry => entry.UserID == userId).Count > 0;
        }

        if (IsUserInQueue(orderID))
        {
            await socket.Send(Settings.FR_AlreadyInQueue);
            LogUtil.LogInfo($"User is already in queue orderID: {orderID}", nameof(ShopifyBot<T>));
            return false;
        }

        var code = Info.GetRandomTradeCode();
        var requestRequestSignificance = RequestSignificance.None;
        var type = PokeRoutineType.LinkTrade;

        // a Showdownset is a format for a pokemon. Describing its attributes.
        var showdownSet = new ShowdownSet(showdownset);
        var template = AutoLegalityWrapper.GetTemplate(showdownSet);
        if (showdownSet.InvalidLines.Count != 0)
        {
            var msg = string.Format(Settings.FR_ShowdownParseError, orderID, string.Join("\n", showdownSet.InvalidLines));
            LogUtil.LogError(msg, nameof(ShopifyBot<T>));
            return false;
        }

        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pkm = sav.GetLegal(template, out var result);
        var la = new LegalityAnalysis(pkm);
        var spec = GameInfo.Strings.Species[template.Species];
        pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
        if (pkm is not T pk || !la.Valid)
        {
            var reason = result switch
            {
                "Timeout" => string.Format(Settings.FR_TimeoutError, spec),
                "VersionMismatch" => Settings.FR_VersionMismatchError,
                _ => string.Format(Settings.FR_GeneralError, spec),
            };
            var imsg = $"{orderID}: Oops! {reason}";
            if (result == "Failed")
                imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";
            LogUtil.LogError(imsg, nameof(ShopifyBot<T>));
            return false;
        }
        pk.ResetPartyStats();

        // Here we assume trainer name is the orderID. as well as the trainerID.
        var trainer = new PokeTradeTrainerInfo(orderID.ToString(), orderID);
        var notifier = new ShopifyTradeNotifier<T>(pk, code, orderID, socket, Hub.Config.Shopify, this);
        var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed : PokeTradeType.Specific;
        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, requestRequestSignificance == RequestSignificance.Favored);
        var trade = new TradeEntry<T>(detail, orderID, type, orderID.ToString());
        var added = Info.AddToTradeQueue(trade, orderID, requestRequestSignificance == RequestSignificance.Owner);

        var position = Info.CheckPosition(orderID);
        while (position.Position > 1)
        {
            await Task.Delay(5000);
            position = Info.CheckPosition(orderID);
            var botct = Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                await socket.Send(string.Format(Settings.FR_QueuePosition, position.Position));
                await socket.Send(string.Format(Settings.FR_QueueEstimatedTime, eta));
            }
        }

        return true;
    }
}

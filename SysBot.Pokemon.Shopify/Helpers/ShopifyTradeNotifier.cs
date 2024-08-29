using Fleck;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;

namespace SysBot.Pokemon.Shopify;

public class ShopifyTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
{
    private T Data { get; }
    private int Code { get; }
    private string OrderID { get; }
    private IWebSocketConnection Client { get; }
    private ShopifySettings Settings { get; }

    public ShopifyTradeNotifier(T data, int code, string orderID, IWebSocketConnection client, ShopifySettings settings)
    {
        Data = data;
        Code = code;
        OrderID = orderID;
        Client = client;
        Settings = settings;

        LogUtil.LogText($"Created trade details for orderID: {OrderID} - {Code}");
    }

    public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
    {
        LogUtil.LogText(message);
        SendMessage($"{message}");
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        OnFinish?.Invoke(routine);
        var line = string.Format(Settings.FR_TradeCanceled, msg);
        LogUtil.LogText(line);
        SendMessage(line);
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        OnFinish?.Invoke(routine);
        var tradedToUser = Data.Species;
        var message = tradedToUser != 0 ?
            string.Format(Settings.FR_TradeFinished, (Species)tradedToUser) :
            Settings.FR_TradeFinishedGeneric;
        LogUtil.LogText(message);
        SendMessage(message);
    }

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
        var msg = string.Format(Settings.FR_TradeInitialize, info.Code);
        LogUtil.LogText(msg);
        SendMessage(msg);
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var message = string.Format(Settings.FR_TradeSearching, routine.InGameName);
        LogUtil.LogText(message);
        SendMessage($"{message}");
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
    {
        var msg = message.Summary;
        if (message.Details.Count > 0)
            msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
        LogUtil.LogText(msg);
        SendMessage(msg);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        var msg = $"Details for {result.FileName}: " + message;
        LogUtil.LogText(msg);
        SendMessage(msg);
    }

    private void SendMessage(string message)
    {
        Client.Send(message);
    }
}

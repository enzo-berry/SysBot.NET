using PKHeX.Core;

namespace SysBot.Pokemon.Shopify;

public class ShopifyQueue<T>(T Entity, PokeTradeTrainerInfo Trainer, string Username)
    where T : PKM, new()
{
    public T Entity { get; } = Entity;
    public PokeTradeTrainerInfo Trainer { get; } = Trainer;
    public string UserName { get; } = Username;
    public string DisplayName => Trainer.TrainerName;
}

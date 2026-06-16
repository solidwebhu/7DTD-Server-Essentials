using OperenciaManager.Commands;
using OperenciaManager.Core;

public class UseTokenCommand : IChatCommand
{
    public string Name => "/usetoken";

    public void Execute(ClientInfo ci, string[] args)
    {
        if (!GameManager.Instance.World.Players.dict
            .TryGetValue(ci.entityId, out EntityPlayer player))
            return;

        // flageljük a folyamatot
        player.SetCVar("usetoken_drop", 1);

        // 🎯 EZ DOBJA EL VALÓBAN
        GameEventManager.Current.HandleAction(
            "action_dropdukecasino",
            player,
            player,
            false
        );

        ChatHook.SendPrivate(ci, "[AAAAAA]Tokenek eldobása...");
    }
}

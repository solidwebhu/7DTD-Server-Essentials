using OperenciaManager.Commands;
using OperenciaManager.Core;
using UnityEngine;

public class DropCasinoCommand : IChatCommand
{
    public string Name => "/dropcasino";
    private const string EventName = "action_drop_casino";

    public void Execute(ClientInfo ci, string[] args)
    {
        if (!GameManager.Instance.World.Players.dict
            .TryGetValue(ci.entityId, out EntityPlayer player))
        {
            ChatHook.SendPrivate(ci, "[FF0000]Player entity nem található[-]");
            return;
        }

        int total = CountAllDukeTokens(player);

        if (total <= 0)
        {
            ChatHook.SendPrivate(ci, "[FFFF00]Nincs nálad Duke Casino Token[-]");
            return;
        }

        // 🔥 ENGINE DROP
        GameEventManager.Current.HandleAction(
            EventName,
            player,
            player,
            false
        );

        ChatHook.SendPrivate(
            ci,
            $"[00FF00]Eldobva: {total} Duke Casino Token[-]"
        );
    }

    private static readonly FastTags<TagGroup.Global> DukeTags =
        FastTags<TagGroup.Global>.Parse("dukes");

    private static int CountAllDukeTokens(EntityPlayer player)
    {
        int total = 0;

        total += player.inventory.GetItemCount(DukeTags, -1, -1, true);

        if (player.bag != null)
        {
            total += player.bag.GetItemCount(DukeTags, -1, -1, true);
        }

        return total;
    }
}

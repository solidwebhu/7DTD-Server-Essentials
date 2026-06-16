using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class HomerunCommand : IChatCommand
    {
        public string Name => "/homerun";

        private const string EventName = "action_homerun";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            string userId = clientInfo.CrossplatformId.CombinedString;
            EntityPlayer player = GameManager.Instance.World.GetEntity(clientInfo.entityId) as EntityPlayer;

            if (player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            // Cooldown (120 mp)
            if (CommandCooldownManager.IsOnCooldown("/homerun", userId, 120, out int remaining))
            {
                ChatHook.SendPrivate(clientInfo,
                    $"[FF0000]A Homerun Derby még nem használható újra. Várj {remaining} másodpercet!");
                return;
            }

            // Event futtatása
            bool success = GameEventManager.Current.HandleAction(EventName, player, player, false);

            if (!success)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]A Homerun Derby esemény nem indult el. Lehet, hogy nincs engedélyezve a Twitch események között.");
                Log.Warning($"[Homerun] Sikertelen event: {EventName}");
                return;
            }

            ChatHook.SendPrivate(clientInfo, "[00FF00]Homerun Derby aktiválva! Üsd neki a zombikat a lufiknak, hogy pontot szerezz!");
            Log.Out($"[Homerun] {player.EntityName} aktiválta a homerun eseményt.");
        }
    }
}

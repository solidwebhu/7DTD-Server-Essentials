using OperenciaManager.Core;
using System.Linq;
using UnityEngine;

namespace OperenciaManager.Commands
{
    public class EjectCommand : IChatCommand
    {
        public string Name => "/eject";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Játékos entitás
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player) || player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található a játékos entitás.");
                return;
            }

            // Járműben ül?
            if (!(player.AttachedToEntity is EntityVehicle vehicle))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem ülsz járműben.");
                return;
            }

            // Ő a vezető?
            int slot = vehicle.FindAttachSlot(player);
            if (slot != 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Csak a jármű vezetője dobhat ki másokat.");
                return;
            }

            // Paraméter ellenőrzés
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Használat: /eject <név töredék>");
                return;
            }

            string query = args[0].Trim().ToLower();

            // Céljátékos keresése név töredék alapján
            EntityPlayer targetPlayer = GameManager.Instance.World.Players.list
                .FirstOrDefault(p =>
                    p != null &&
                    p.entityId != clientInfo.entityId &&
                    p.EntityName != null &&
                    p.EntityName.ToLower().Contains(query));

            if (targetPlayer == null)
            {
                ChatHook.SendPrivate(clientInfo, $"[FF0000]Nem található játékos a járműben ilyen névvel: {query}");
                return;
            }

            // Céljátékos valóban a járműben ül?
            if (targetPlayer.AttachedToEntity == null || targetPlayer.AttachedToEntity.entityId != vehicle.entityId)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Ez a játékos nincs a járművedben.");
                return;
            }

            // ClientInfo lekérése
            ClientInfo targetClient = ConnectionManager.Instance.Clients.ForEntityId(targetPlayer.entityId);
            if (targetClient == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található a játékos hálózati kapcsolata.");
                return;
            }

            // Kidobás
            targetPlayer.SendDetach();

            ChatHook.SendPrivate(clientInfo, $"[00FF00]{targetPlayer.EntityName} kidobva a járműből![-]");
            ChatHook.SendPrivate(targetClient, "[FF0000]A jármű vezetője kidobott téged![-]");
        }
    }
}

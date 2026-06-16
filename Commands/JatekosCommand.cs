using OperenciaManager.Core;
using System.Linq;
using UnityEngine;

namespace OperenciaManager.Commands
{
    public class JatekosCommand : IChatCommand
    {
        public string Name => "/jatekos";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Jogosultság
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.");
                return;
            }

            // Paraméter ellenőrzés
            if (args == null || args.Length < 2)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FFFF00]Használat: /jatekos <művelet> <játékos> [érték]");
                return;
            }

            string action = args[0].ToLower();
            string playerQuery = args[1].ToLower();

            // Játékos keresése
            EntityPlayer player = FindPlayer(playerQuery);
            if (player == null)
            {
                ChatHook.SendPrivate(clientInfo, $"[FF0000]Nem található játékos: {playerQuery}");
                return;
            }

            // RESET
            if (action == "reset")
            {
                player.KilledZombies = 0;
                player.KilledPlayers = 0;
                player.Died = 0;
                player.Progression.Level = 1;
                player.Progression.ExpToNextLevel = 0;

                ChatHook.SendPrivate(clientInfo,
                    $"[00FF00]{player.EntityName} statjai visszaállítva.");
                return;
            }

            // Műveletek, amikhez érték kell
            if (args.Length < 3)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Hiányzik az érték paraméter.");
                return;
            }

            if (!int.TryParse(args[2], out int value))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Értéknek számot adj meg.");
                return;
            }

            switch (action)
            {
                case "szint":
                    player.Progression.Level = value;
                    ChatHook.SendPrivate(clientInfo,
                        $"[00FF00]{player.EntityName} szintje beállítva: {value}");
                    break;

                case "xp":
                    player.Progression.ExpToNextLevel = value;
                    ChatHook.SendPrivate(clientInfo,
                        $"[00FF00]{player.EntityName} XP-je beállítva: {value}");
                    break;

                case "halalok":
                    {
                        // Runtime stat módosítás
                        player.Died = value;
                        player.bPlayerStatsChanged = true;

                        // Kliens felé azonnali frissítés
                        clientInfo.SendPackage(
                            NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player)
                        );

                        ChatHook.SendPrivate(clientInfo,
                            $"[00FF00]{player.EntityName} halálok száma beállítva: {value}");

                        break;
                    }



                case "killek":
                    player.KilledZombies = value;
                    ChatHook.SendPrivate(clientInfo,
                        $"[00FF00]{player.EntityName} zombi kill száma: {value}");
                    break;

                case "playerkillek":
                    player.KilledPlayers = value;
                    ChatHook.SendPrivate(clientInfo,
                        $"[00FF00]{player.EntityName} játékos kill száma: {value}");
                    break;

                case "health":
                    player.Stats.Health.Value = value;
                    ChatHook.SendPrivate(clientInfo,
                        $"[00FF00]{player.EntityName} HP beállítva: {value}");
                    break;

                case "stamina":
                    player.Stats.Stamina.Value = value;
                    ChatHook.SendPrivate(clientInfo,
                        $"[00FF00]{player.EntityName} stamina beállítva: {value}");
                    break;

                default:
                    ChatHook.SendPrivate(clientInfo,
                        "[FFFF00]Ismeretlen művelet. Elérhető: szint, xp, halalok, killek, playerkillek, health, stamina, reset");
                    break;
            }
        }

        private EntityPlayer FindPlayer(string query)
        {
            // ID alapján
            if (int.TryParse(query, out int id))
            {
                GameManager.Instance.World.Players.dict.TryGetValue(id, out EntityPlayer p);
                if (p != null) return p;
            }

            // Név alapján
            return GameManager.Instance.World.Players.list
                .FirstOrDefault(p =>
                    p != null &&
                    p.EntityName != null &&
                    p.EntityName.ToLower().Contains(query));
        }
    }
}

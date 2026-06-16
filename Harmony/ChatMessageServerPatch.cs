using HarmonyLib;
using OperenciaManager.Core;
using System;
using System.Reflection;

[HarmonyPatch(typeof(GameManager))]
[HarmonyPatch("ChatMessageServer")]
public class ChatPatch
{
    static bool Prefix(
        ClientInfo _cInfo,
        EChatType _chatType,
        int _senderEntityId,
        string _msg,
        System.Collections.Generic.List<int> _recipientEntityIds,
        EMessageSender _msgSender)
    {
        try
        {
            // ha nincs kliens vagy üzenet → engedjük tovább
            if (_cInfo == null || string.IsNullOrEmpty(_msg))
                return true;

            // parancsok menjenek tovább
            if (_msg.StartsWith("/"))
                return true;

            // csak a globális chatet interceptáljuk
            if (_chatType != EChatType.Global)
                return true;

            // entity lekérése
            EntityPlayer player = GameManager.Instance.World
                ?.GetEntity(_senderEntityId) as EntityPlayer;

            if (player == null)
                return true;

            // progression frissítés (LVL kijelzés miatt)
            ForceProgressionRefresh(player);

            int level = player.Progression?.GetLevel() ?? 0;

            // adminszint lekérése
            int adminLevel = GameManager.Instance.adminTools.Users
                .GetUserPermissionLevel(_cInfo.CrossplatformId);

            string name = _cInfo.playerName;
            string prefix;

            // prefix logika
            if (adminLevel == 0)
            {
                prefix = $"[FF0000]{name} (Tulajdonos):";
            }
            else if (adminLevel <= 2)
            {
                prefix = $"[00FFF0]{name} (Admin):";
            }
            else
            {
                prefix = $"[FFFFFF]{name} [FFF41F][LVL {level}][FFFFFF]:";
            }

            // végső formázott üzenet
            string formatted = $"{prefix} [FFFFFF]{_msg}";

            // saját chat kiküldése
            ChatHook.SendGlobalRaw(formatted);

            // saját logolás a serverlogba
            Log.Out($"INF OperenciaManager Chat: {name}: {_msg}");

            // blokkoljuk a vanilla chatet
            return false;
        }
        catch (Exception e)
        {
            Log.Error($"[ChatPatch] {e}");
            return true;
        }
    }

    // progression frissítés LVL kijelzéshez
    private static void ForceProgressionRefresh(EntityPlayer player)
    {
        try
        {
            var prog = player.Progression;
            if (prog == null)
                return;

            Type type = prog.GetType();
            string[] methods =
            {
                "CheckForLevelUp",
                "UpdateLevel",
                "UpdateForExpChange",
                "RecalculateLevel",
                "Update"
            };

            foreach (string m in methods)
            {
                MethodInfo mi = type.GetMethod(
                    m,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

                if (mi != null)
                {
                    mi.Invoke(prog, null);
                    break;
                }
            }
        }
        catch { }
    }
}

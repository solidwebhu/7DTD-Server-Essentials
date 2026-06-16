using HarmonyLib;
using UnityEngine;
using OperenciaManager.Systems;
using OperenciaManager.Comms;

namespace OperenciaManager.Patches
{
    [HarmonyPatch(typeof(NetPackageTELock))]
    [HarmonyPatch("ProcessPackage")]
    public class TELockPatch
    {
        static bool Prefix(NetPackageTELock __instance, World _world, GameManager _callbacks)
        {
            var pos = new Vector3i(__instance.posX, __instance.posY, __instance.posZ);
            var te = _world.GetTileEntity(__instance.clrIdx, pos);

            _world.Players.dict.TryGetValue(__instance.entityIdThatOpenedIt, out var player);

            // 🔍 Részletes debug
           /* {
                string teType = te?.GetType().Name ?? "null";
                string teClass = "N/A";

                if (te is TileEntity tileEntity)
                {
                    try { teClass = tileEntity.GetTileEntityType().ToString(); }
                    catch { }
                }

                Log.Out(
                    $"[OperenciaManager][TELock DEBUG] " +
                    $"player={player?.EntityName ?? "null"} " +
                    $"entityId={__instance.entityIdThatOpenedIt} " +
                    $"lockType={__instance.type} " +
                    $"pos={pos} " +
                    $"clrIdx={__instance.clrIdx} " +
                    $"lootEntityId={__instance.lootEntityId} " +
                    $"tileEntityType={teType} " +
                    $"tileEntityClass={teClass}"
                );
            }*/

            // 🔒 Védeni kívánt tile entity típusok
            bool isProtectedTE =
                te is TileEntityWorkstation ||
                te is TileEntityCollector; // <-- DEW COLLECTOR HOZZÁADVA

            if (!isProtectedTE)
                return true;

            bool inForeignClaim = ClaimAccessRegistry.IsInForeignClaim(__instance.entityIdThatOpenedIt);

            if (inForeignClaim && __instance.type == NetPackageTELock.TELockType.LockServer)
            {
                _world.GetGameManager().TEDeniedAccessClient(
                    __instance.clrIdx,
                    pos,
                    __instance.lootEntityId,
                    __instance.entityIdThatOpenedIt
                );

                Log.Out($"[OperenciaManager] Tiltva claim miatt: {player?.EntityName}");

                if (player != null)
                {
                    string intruderName = player.EntityName;
                    var ownerData = ClaimZoneResolver.GetClaimOwner(pos);
                    string ownerName = ownerData?.PlayerName?.SafeDisplayName ?? "ismeretlen játékos";

                    string niceName = te is TileEntityCollector ? "dew collector" : "workstation";

                    GameManager.Instance.ChatMessageServer(
                        null,
                        EChatType.Global,
                        -1,
                        $"[FF0000]Figyelem:[FFFFFF] {intruderName} megpróbált kinyitni egy {niceName}-t {ownerName} területén!",
                        null,
                        EMessageSender.None
                    );

                    DiscordNotifier.Send(
                        DiscordHooks.Guard,
                        "🛡️ Operencia Guard",
                        $"**{intruderName}** megpróbált kinyitni egy **{niceName}**-t {ownerName} területén.\n📍 Pozíció: `{pos}`",
                        "https://7daystodie.wiki.gg/images/LandClaimBlock.png"
                    );
                }

                return false;
            }

            return true;
        }
    }
}

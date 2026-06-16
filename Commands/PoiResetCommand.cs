using System.Collections;
using UnityEngine;
using OperenciaManager.Core;

namespace OperenciaManager.Commands
{
    public class PoiResetCommand : IChatCommand
    {
        public string Name => "/poireset";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Admin jogosultság ellenőrzése
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            // Játékos entitás lekérése
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            // POI meghatározása a játékos pozíciója alapján
            PrefabInstance poi = GameManager.Instance.World.GetPOIAtPosition(player.position);
            if (poi == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Nem vagy POI területen. Lépj be egy POI-ba a resethez.[-]");
                return;
            }

            GameManager.Instance.StartCoroutine(ResetPOI(poi, clientInfo));
        }

        private static IEnumerator ResetPOI(PrefabInstance poi, ClientInfo clientInfo)
        {
            World world = GameManager.Instance.World;
            string poiName = poi.prefab.PrefabName;

            // 1) Terrain reset a POI területén (víz, talaj, stb.)
            yield return poi.ResetTerrain(world);

            // 2) Blockok + tile entity-k + quest/sleeper/loot reset, chunk resend
            // FastTags.none → nincs speciális quest tag
            poi.ResetBlocksAndRebuild(world, FastTags<TagGroup.Global>.none);

            ChatHook.SendPrivate(clientInfo, $"[00FF00]A(z) \"{poiName}\" POI teljesen resetelve![-]");
            Log.Out($"[POIReset] POI resetelve: {poiName}");
        }
    }
}

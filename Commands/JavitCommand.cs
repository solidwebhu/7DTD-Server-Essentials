using OperenciaManager.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Commands
{
    public class JavitCommand : IChatCommand
    {
        public string Name => "/javit";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            int radius = 3;
            if (args.Length == 1 && int.TryParse(args[0], out int parsed))
            {
                if (parsed < 3 || parsed > 15)
                {
                    ChatHook.SendPrivate(clientInfo, "[FFFF00]A megadható érték 3 és 15 között lehet.[-]");
                    return;
                }
                radius = parsed;
            }

            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            Vector3 pos = player.position;

            int chunkX = World.toChunkXZ((int)pos.x);
            int chunkZ = World.toChunkXZ((int)pos.z);

            int minX = chunkX - radius;
            int maxX = chunkX + radius;
            int minZ = chunkZ - radius;
            int maxZ = chunkZ + radius;

            HashSetLong chunks = new HashSetLong();
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    chunks.Add(WorldChunkCache.MakeChunkKey(x, z));
                }
            }

            GameManager.Instance.StartCoroutine(DoFullChunkReset(chunks));

            ChatHook.SendPrivate(clientInfo, $"[00FF00]{radius} chunk sugarú körzet teljes újragenerálása elindítva.[-]");
        }

        private static IEnumerator DoFullChunkReset(HashSetLong chunks)
        {
            World world = GameManager.Instance.World;
            ChunkCluster cc = world.ChunkCache;
            ChunkProviderGenerateWorld provider = cc.ChunkProvider as ChunkProviderGenerateWorld;

            if (provider == null)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("ChunkProviderGenerateWorld nem elérhető.");
                yield break;
            }

           

            // 2) Chunkok újragenerálása
            foreach (long key in chunks)
            {
                if (!provider.GenerateSingleChunk(cc, key, true))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(
                        $"Nem sikerült újragenerálni a chunkot: {WorldChunkCache.extractX(key) << 4}, {WorldChunkCache.extractZ(key) << 4}");
                }
            }

            // 3) Chunkok elküldése a klienseknek
            world.m_ChunkManager.ResendChunksToClients(chunks);

            // 4) Mesh újraépítése
            if (DynamicMeshManager.Instance != null)
            {
                foreach (long key in chunks)
                {
                    DynamicMeshManager.Instance.AddChunk(key, true, true, null);
                }
            }

            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk reset kész.");
        }
    }
}

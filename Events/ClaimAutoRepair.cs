using UnityEngine;
using System.Collections.Generic;
using OperenciaManager.Core;

namespace OperenciaManager.Systems
{
    public class ClaimAutoRepair : MonoBehaviour
    {
        private static float nextRepairTime = 0f;
        private const int blocksPerClaim = 20; // több blokk javítása claimenként

        public static float GetNextRepairTime() => nextRepairTime;

        void Update()
        {
            if (Time.time < nextRepairTime) return;
            nextRepairTime = Time.time + 10f;

            if (GameManager.Instance?.World == null || GameManager.Instance.persistentPlayers == null)
                return;

            var chunkCluster = GameManager.Instance.World.ChunkCache;
            if (chunkCluster == null)
                return;

            var chunkKeys = chunkCluster.GetChunkKeysCopySync();
            if (chunkKeys == null || chunkKeys.Count == 0)
                return;

            foreach (long key in chunkKeys)
            {
                Chunk chunk = chunkCluster.GetChunkSync(key);
                if (chunk == null)
                    continue;

                List<Vector3i> claimBlocks;
                if (chunk.IndexedBlocks == null ||
                    !chunk.IndexedBlocks.TryGetValue("lpblock", out claimBlocks))
                    continue;

                if (claimBlocks == null || claimBlocks.Count == 0)
                    continue;

                Vector3i worldPos = chunk.GetWorldPos();

                foreach (Vector3i localPos in claimBlocks)
                {
                    Vector3i claimBlockPos = localPos + worldPos;
                    BlockValue bv = chunk.GetBlock(localPos);
                    if (!BlockLandClaim.IsPrimary(bv))
                        continue;

                    var persistentPlayers = GameManager.Instance.persistentPlayers;
                    PersistentPlayerData owner = persistentPlayers.GetLandProtectionBlockOwner(claimBlockPos);
                    if (owner == null || !IsLandProtectionValid(owner))
                        continue;

                    // Claim javítás engedélyezve van-e a tulajdonosnál?
                    string ownerId = owner.PlayerData.PrimaryId.CombinedString;
                    if (!ClaimRepairState.IsEnabled(ownerId))
                        continue;


                    int claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
                    int halfSize = claimSize / 2;

                    Vector3i min = new Vector3i(
                        claimBlockPos.x - halfSize,
                        0,
                        claimBlockPos.z - halfSize
                    );

                    Vector3i max = new Vector3i(
                        claimBlockPos.x + halfSize,
                        GameManager.Instance.World.GetHeight(claimBlockPos.x, claimBlockPos.z),
                        claimBlockPos.z + halfSize
                    );

                    int repaired = 0;

                    for (int x = min.x; x <= max.x && repaired < blocksPerClaim; x++)
                        for (int y = min.y; y <= max.y && repaired < blocksPerClaim; y++)
                            for (int z = min.z; z <= max.z && repaired < blocksPerClaim; z++)
                            {
                                Vector3i pos = new Vector3i(x, y, z);
                                BlockValue block = GameManager.Instance.World.GetBlock(pos);

                                if (block.damage > 0)
                                {
                                    block.damage = (byte)Mathf.Max(0, block.damage - 50); //részleges javítás
                                    GameManager.Instance.World.SetBlockRPC(0, pos, block);
                                    repaired++;
                                }
                            }

                    if (repaired > 0)
                    {
                        string ownerName = owner.PlayerName?.SafeDisplayName ?? "ismeretlen";
                        Log.Out($"[ClaimAutoRepair] {repaired} blokk javítva a claim zónában: {claimBlockPos} → {ownerName}");
                    }

                    // NINCS return → megy tovább a következő claimre
                }
            }
        }

        private bool IsLandProtectionValid(PersistentPlayerData ppData)
        {
            double expiryHours = GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
            return ppData != null && ppData.OfflineHours <= expiryHours;
        }
    }
}

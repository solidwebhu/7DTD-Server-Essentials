using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public static class ClaimZoneResolver
    {
        public static PersistentPlayerData GetClaimOwner(Vector3i position)
        {
            var persistentPlayers = GameManager.Instance.persistentPlayers;
            int claimSize = (GameStats.GetInt(EnumGameStats.LandClaimSize) - 1) / 2;

            foreach (var playerData in persistentPlayers.Players.Values)
            {
                var blocks = playerData.GetLandProtectionBlocks();
                if (blocks == null) continue;

                foreach (var blockPos in blocks)
                {
                    BlockValue block = GameManager.Instance.World.GetBlock(blockPos);
                    if (!BlockLandClaim.IsPrimary(block)) continue;

                    int dx = Mathf.Abs(blockPos.x - position.x);
                    int dz = Mathf.Abs(blockPos.z - position.z);
                    if (dx <= claimSize && dz <= claimSize)
                    {
                        return playerData;
                    }
                }
            }

            return null;
        }
    }
}

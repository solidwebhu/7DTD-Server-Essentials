using HarmonyLib;
using OperenciaManager.Core;
using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ChangeBlocks))]
    public class BlockPlacementPatch
    {
        static void Postfix(
            GameManager __instance,
            PlatformUserIdentifierAbs persistentPlayerId,
            List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                if (_blocksToChange == null || _blocksToChange.Count == 0)
                    return;

                World world = GameManager.Instance.World;
                if (world == null)
                    return;

                foreach (var change in _blocksToChange)
                {
                    // nem player source
                    if (change.changedByEntityId < 0)
                        continue;

                    int entityId = change.changedByEntityId;

                    ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(entityId);
                    if (ci == null)
                        continue;

                    int level = GameManager.Instance.adminTools.Users
                        .GetUserPermissionLevel(ci.CrossplatformId);

                    if (level <= 2)
                        continue;

                    Vector3i pos = change.pos;

                    BlockValue placed = world.GetBlock(pos);
                    if (placed.type == 0)
                        continue;

                    PrefabInstance poi = world.GetPOIAtPosition(pos);
                    if (poi?.prefab?.PrefabName == null)
                        continue;

                    if (!poi.prefab.PrefabName.Equals(
                            "operencia_center",
                            System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    string blockName = Block.list[placed.type].GetBlockName();

                    if (blockName.Equals(
                            "cntVendingMachine",
                            System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    // törlés
                    world.SetBlockRPC(0, pos, BlockValue.Air);

                    ChatHook.SendPrivate(ci,
                        "[FF0000]Ezen a területen csak [FFFFFF]Vending Machine helyezhető le!");
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[BlockPlacementPatch] " + e);
            }
        }
    }
}

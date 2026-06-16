using OperenciaManager.Core;
using System.Collections.Generic;
using UnityEngine;

public class TraderDoorWatcher : MonoBehaviour
{
    private Dictionary<int, bool> playerInsidePOI = new Dictionary<int, bool>();
    private float checkInterval = 2f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        if (GameManager.Instance?.World == null) return;

        World world = GameManager.Instance.World;
        var players = world.Players?.dict;
        if (players == null || players.Count == 0) return;

        foreach (var kvp in players)
        {
            EntityPlayer player = kvp.Value;
            if (player == null || player.IsDead()) continue;

            Vector3i pos = new Vector3i(player.position);
            PrefabInstance poi = world.GetPOIAtPosition(pos);
            bool isTraderPOI = poi?.prefab?.PrefabName?.ToLower().Contains("trader") == true;

            if (!playerInsidePOI.ContainsKey(player.entityId))
            {
                playerInsidePOI[player.entityId] = isTraderPOI;
                continue;
            }

            bool wasInside = playerInsidePOI[player.entityId];

            if (wasInside && !isTraderPOI)
            {
                CloseNearbyDoors(world, player);
                Log.Out($"[TraderDoorWatcher] {player.EntityName} elhagyta a trader POI-t. Ajtók bezárva.");
            }

            playerInsidePOI[player.entityId] = isTraderPOI;
        }
    }

    private void CloseNearbyDoors(World world, EntityPlayer player)
    {
        if (world == null || player == null) return;

        Vector3i center = new Vector3i(player.position);
        int radius = 10;

        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            for (int y = center.y - 2; y <= center.y + 4; y++)
            {
                for (int z = center.z - radius; z <= center.z + radius; z++)
                {
                    Vector3i blockPos = new Vector3i(x, y, z);
                    BlockValue block = world.GetBlock(blockPos);
                    Block blockType = block.Block;

                    if (blockType == null) continue;

                    string name = blockType.GetBlockName()?.ToLower();
                    if (string.IsNullOrEmpty(name)) continue;

                    bool isDoorLike = name.Contains("door") || name.Contains("gate") || name.Contains("hatch") || name.Contains("drawbridge");
                    if (!isDoorLike) continue;

                    bool isOpen = blockType is BlockDoor
                        ? BlockDoor.IsDoorOpen(block.meta)
                        : (block.meta & 1) == 1;

                    if (!isOpen) continue;

                    block.meta = (byte)((0) | ((int)block.meta & -2));
                    world.SetBlockRPC(blockPos, block);
                    Log.Out($"[TraderDoorWatcher] Ajtó vagy kapu bezárva: {name} @ {blockPos}");
                }
            }
        }
    }
}

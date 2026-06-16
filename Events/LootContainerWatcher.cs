using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public class LootContainerWatcher : MonoBehaviour
    {
        private static readonly HashSet<int> handled = new HashSet<int>();
        private float nextScanTime = 0f;

        void Update()
        {
            if (!GameManager.IsDedicatedServer)
                return;

            // flood védelem: 5x/sec
            if (Time.time < nextScanTime)
                return;
            nextScanTime = Time.time + 0.2f;

            World world = GameManager.Instance.World;
            if (world == null)
                return;

            foreach (var entity in world.Entities.list)
            {
                if (!(entity is EntityLootContainer loot))
                    continue;

                // csak drop bag
                if (loot.GetLootList() != "cntDropBag")
                    continue;

                // már feldolgozva
                if (handled.Contains(loot.entityId))
                    continue;

                // loot még nem készült el
                if (loot.isInventory == null && loot.isBag == null)
                    continue;

                // 🔍 PLAYER KERESÉS (distance + cvar)
                EntityPlayer owner = null;

                foreach (var p in world.Players.dict.Values)
                {
                    if (p == null)
                        continue;

                    if (p.GetCVar("usetoken_drop") != 1f)
                        continue;

                    float dist = Vector3.Distance(p.position, loot.position);
                    if (dist > 4f)
                        continue;

                    owner = p;
                    break;
                }

                if (owner == null)
                    continue;

                handled.Add(loot.entityId);

                int tokenCount = 0;

                CountStacks(loot.isInventory, ref tokenCount);
                CountStacks(loot.isBag, ref tokenCount);

                // 🧹 DROP BAG TÖRLÉSE (SZERVER OLDALON)
                world.RemoveEntity(
                    loot.entityId,
                    EnumRemoveEntityReason.Despawned
                );

                // flag reset
                owner.SetCVar("usetoken_drop", 0f);

                // 💬 CHAT
                GameManager.Instance.ChatMessageServer(
                    null,
                    EChatType.Whisper,
                    owner.entityId,
                    $"[00FF00]Felhasználtál {tokenCount} Duke Casino Tokent.[-]",
                    null,
                    EMessageSender.None
                );

                Log.Out($"[OperenciaManager] usetoken OK → {owner.EntityName}, {tokenCount} token");

                break; // egy drop = egy feldolgozás
            }
        }

        private void CountStacks(ItemStack[] stacks, ref int total)
        {
            if (stacks == null)
                return;

            for (int i = 0; i < stacks.Length; i++)
            {
                var stack = stacks[i];
                if (stack.IsEmpty())
                    continue;

                var ic = stack.itemValue.ItemClass;
                if (ic != null && ic.Name == "casinoCoin")
                {
                    total += stack.count;
                }
            }
        }
    }
}

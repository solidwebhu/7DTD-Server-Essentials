using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public class TokenHandler : MonoBehaviour
    {
        private class Pending
        {
            public int cost;
            public System.Action onSuccess;
            public System.Action onFail;
            public int waitTicks;
        }

        private static readonly Dictionary<int, Pending> pending =
            new Dictionary<int, Pending>();

        private static readonly HashSet<int> handledLoot =
            new HashSet<int>();

        // =========================
        // 🎯 PUBLIC ENTRY POINT
        // =========================
        public static bool TryConsume(
            EntityPlayer player,
            int cost,
            System.Action onSuccess,
            System.Action onFail)
        {
            if (pending.ContainsKey(player.entityId))
                return false;

            pending[player.entityId] = new Pending
            {
                cost = cost,
                onSuccess = onSuccess,
                onFail = onFail,
                waitTicks = 0
            };

            // flag a watchernek
            player.SetCVar("usetoken_drop", 1f);

            // 🎯 GAMEEVENT – ledob mindent
            GameEventManager.Current.HandleAction(
                "action_usetoken",
                player,
                player,
                false
            );

            return true;
        }

        // =========================
        // 🔁 UPDATE LOOP
        // =========================
        void Update()
        {
            if (!GameManager.IsDedicatedServer)
                return;

            var world = GameManager.Instance.World;
            if (world == null)
                return;

            foreach (var entity in world.Entities.list)
            {
                if (!(entity is EntityLootContainer loot))
                    continue;

                if (loot.GetLootList() != "cntDropBag")
                    continue;

                if (handledLoot.Contains(loot.entityId))
                    continue;

                // még nincs inventory feltöltve
                if (loot.isInventory == null && loot.isBag == null)
                    continue;

                // aktív tranzakció keresése
                foreach (var kvp in pending)
                {
                    int playerId = kvp.Key;
                    var tx = kvp.Value;

                    if (!world.Players.dict.TryGetValue(playerId, out var player))
                        continue;

                    // csak ha a player indította
                    if (player.GetCVar("usetoken_drop") < 1f)
                        continue;

                    handledLoot.Add(loot.entityId);

                    int tokenCount = 0;
                    CountStacks(loot.isInventory, ref tokenCount);
                    CountStacks(loot.isBag, ref tokenCount);

                    // 🧹 HÁTIZSÁK TÖRLÉS
                    world.RemoveEntity(
                        loot.entityId,
                        EnumRemoveEntityReason.Despawned
                    );

                    player.SetCVar("usetoken_drop", 0f);

                    // ❌ NINCS ELÉG
                    if (tokenCount < tx.cost)
                    {
                        tx.onFail?.Invoke();
                    }
                    else
                    {
                        int refund = tokenCount - tx.cost;

                        if (refund > 0)
                            GiveBack(player, refund);

                        tx.onSuccess?.Invoke();
                    }

                    pending.Remove(playerId);
                    return; // egy tranzakció / tick
                }
            }
        }

        // =========================
        // 🔢 TOKEN SZÁMOLÁS
        // =========================
        private static void CountStacks(ItemStack[] stacks, ref int total)
        {
            if (stacks == null)
                return;

            for (int i = 0; i < stacks.Length; i++)
            {
                var s = stacks[i];
                if (s.IsEmpty())
                    continue;

                var ic = s.itemValue?.ItemClass;
                if (ic != null && ic.Name == "casinoCoin")
                    total += s.count;
            }
        }

        // =========================
        // 🔄 REFUND
        // =========================
        private static void GiveBack(EntityPlayer player, int amount)
        {
            var ic = ItemClass.GetItemClass("casinoCoin", false);
            if (ic == null)
                return;

            var iv = new ItemValue(ic.Id, false);
            var stack = new ItemStack(iv, amount);

            var entity = (EntityItem)EntityFactory.CreateEntity(
                new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = stack,
                    pos = player.position,
                    lifetime = 1f,
                    belongsPlayerId = player.entityId
                });

            var world = GameManager.Instance.World;
            world.SpawnEntityInWorld(entity);

            var ci = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
            ci?.SendPackage(
                NetPackageManager.GetPackage<NetPackageEntityCollect>()
                    .Setup(entity.entityId, player.entityId)
            );

            world.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Despawned);
        }
    }
}

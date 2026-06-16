using OperenciaManager.Core;
using OperenciaManager.Comms;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public class RewardProfile
    {
        public string Name { get; set; }
        public int Tokens { get; set; }
        public int Xp { get; set; }
        public string DiscordTitle { get; set; }
        public string DiscordDescription { get; set; }
    }

    public static class RewardManager
    {
        private const string DiscordWebhookUrl = DiscordHooks.Events;

        public static void GiveReward(ClientInfo ci, EntityPlayer player, RewardProfile profile)
        {
            if (ci == null || player == null || profile == null)
            {
                Log.Warning("[RewardManager] Hiányzó paraméter.");
                return;
            }

            // 🎁 Token drop → helyette: token a zsebbe kerül
            if (profile.Tokens > 0)
            {
                var itemClass = ItemClass.GetItemClass("casinoCoin", false);
                if (itemClass != null)
                {
                    var itemValue = new ItemValue(itemClass.Id, false);
                    var itemStack = new ItemStack(itemValue, profile.Tokens);

                    // EntityItem létrehozása a játékos pozícióján
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = itemStack,
                        pos = player.position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = ci.entityId
                    });

                    // Spawn → Collect → Remove
                    GameManager.Instance.World.SpawnEntityInWorld(entityItem);
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, ci.entityId));
                    GameManager.Instance.World.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);

                    ChatHook.SendPrivate(ci, $"[00FF00]Jutalom: {profile.Tokens} Duke Casino Token hozzáadva az inventoryhoz.");
                }
            }


            // ✨ XP
            if (profile.Xp > 0)
            {
                if (player.isEntityRemote)
                {
                    var pkg = NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(player.entityId, profile.Xp, Progression.XPTypes.Other);
                    ConnectionManager.Instance.SendPackage(pkg, false, player.entityId, -1, -1, null, 192, false);
                }
                else
                {
                    player.Progression.AddLevelExp(profile.Xp, "_xpOther", Progression.XPTypes.Other, true, true);
                }
                ChatHook.SendPrivate(ci, $"[00FF00]Jutalom: {profile.Xp} XP hozzáadva.");
            }

            // 📣 Discord webhook
            string description = profile.DiscordDescription ??
                $"**{player.EntityName}** jutalmat kapott: {profile.Name}\n🎁 {profile.Tokens} token\n✨ {profile.Xp} XP";

            DiscordNotifier.Send(
                DiscordWebhookUrl,
                profile.DiscordTitle ?? "🎉 Jutalom kiosztva",
                description
            );
        }
    }
}

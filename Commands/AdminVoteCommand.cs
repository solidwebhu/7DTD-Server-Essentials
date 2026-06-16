using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class AdminVoteCommand : IChatCommand
    {
        public string Name => "/adminvote";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            var testReward = new RewardProfile
            {
                Name = "Szavazás teszt",
                Tokens = 1000,
                Xp = 1000,
                DiscordTitle = "🛠 AdminVote teszt",
                DiscordDescription = $"**{player.EntityName}** admin teszt jutalmat kapott.\n🎁 1000 token\n✨ 1000 XP"
            };

            RewardManager.GiveReward(clientInfo, player, testReward);
        }
    }
}

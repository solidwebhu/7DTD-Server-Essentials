using System.Net;
using System.IO;
using Newtonsoft.Json;
using OperenciaManager.Core;
using OperenciaManager.Commands;
using OperenciaManager.Comms;
using OperenciaManager.Systems;

public class VoteCommand : IChatCommand
{
    public string Name => "/vote";
    private const string ApiKey = "aPKdWpezcDbUsc46pbRSjvxuIoCF6IPeid7";
    private const string ApiBase = "https://7daystodie-servers.com/api/";

    public void Execute(ClientInfo clientInfo, string[] args)
    {
        string crossId = clientInfo.CrossplatformId?.CombinedString ?? "unknown";
        string steamId = GetSteamIdFromProfile(crossId);
        string playerName = clientInfo.playerName;

        if (steamId == null)
        {
            ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található regisztrált SteamID. Kérlek csatlakozz Steamről, vagy várj a profil mentésére.");
            Log.Out($"[VoteCommand] Nincs SteamID a profilban: {crossId}");
            return;
        }

        int voteStatus = CheckVoteStatus(steamId);
        switch (voteStatus)
        {
            case 0:
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található szavazat. Szavazz itt: https://7daystodie-servers.com/server/101255/");
                return;
            case 2:
                ChatHook.SendPrivate(clientInfo, "[FF0000]Már beváltottad a szavazatod.");
                return;
            case 1:
                if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
                {
                    ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.");
                    return;
                }

                var voteReward = new RewardProfile
                {
                    Name = "Szavazás",
                    Tokens = 2000,
                    Xp = 5000,
                    DiscordTitle = "🗳 Szavazás jutalom",
                    DiscordDescription = $"**{player.EntityName}** beváltotta a szavazás jutalmát!"
                };

                RewardManager.GiveReward(clientInfo, player, voteReward);
                MarkVoteClaimed(steamId);
                return;
        }
    }

    private string GetSteamIdFromProfile(string crossId)
    {
        string filePath = Path.Combine(GameIO.GetSaveGameDir(), "OperenciaManager", "PlayerProfiles", $"{crossId}.json");
        if (!File.Exists(filePath)) return null;

        try
        {
            var profile = JsonConvert.DeserializeObject<PlayerProfileStorage.PlayerProfile>(File.ReadAllText(filePath));
            return profile?.SteamId;
        }
        catch
        {
            return null;
        }
    }

    private int CheckVoteStatus(string steamId)
    {
        string url = $"{ApiBase}?object=votes&element=claim&key={ApiKey}&steamid={steamId}";
        try
        {
            using (var client = new WebClient())
            {
                string response = client.DownloadString(url);
                return int.TryParse(response.Trim(), out int result) ? result : 0;
            }
        }
        catch (System.Exception ex)
        {
            Log.Error("[VoteCommand] Hiba a szavazat ellenőrzésekor: " + ex.Message);
            return 0;
        }
    }

    private void MarkVoteClaimed(string steamId)
    {
        string url = $"{ApiBase}?action=post&object=votes&element=claim&key={ApiKey}&steamid={steamId}";
        try
        {
            using (var client = new WebClient())
            {
                client.UploadString(url, "");
            }
        }
        catch (System.Exception ex)
        {
            Log.Error("[VoteCommand] Nem sikerült beállítani a claim státuszt: " + ex.Message);
        }
    }
}

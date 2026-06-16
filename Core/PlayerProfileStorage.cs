using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OperenciaManager.Core;

namespace OperenciaManager.Systems
{
    public static class PlayerProfileStorage
    {
        private static readonly string ProfileFolder =
            Path.Combine(GameIO.GetSaveGameDir(), "OperenciaManager", "PlayerProfiles");

        public static void SaveProfile(ClientInfo ci)
        {
            if (ci == null)
            {
                Log.Warning("[PlayerProfileStorage] ClientInfo null, mentés megszakítva.");
                return;
            }

            Log.Out($"[PlayerProfileStorage] Mentés indul: playerName={ci.playerName}, entityId={ci.entityId}");

            string crossId = ci.CrossplatformId?.CombinedString ?? "<null>";
            string platformIdRaw = ci.PlatformId?.CombinedString ?? "<null>";
            string ip = ci.ip ?? "<null>";

            Log.Out($"[PlayerProfileStorage] CrossplatformId: {crossId}");
            Log.Out($"[PlayerProfileStorage] PlatformId: {platformIdRaw}");
            Log.Out($"[PlayerProfileStorage] PlatformId típus: {ci.PlatformId?.GetType().FullName ?? "<null>"}");
            Log.Out($"[PlayerProfileStorage] IP: {ip}");

            string steamId = null;

            if (!string.IsNullOrEmpty(platformIdRaw))
            {
                if (platformIdRaw.StartsWith("Steam_"))
                {
                    steamId = platformIdRaw.Substring("Steam_".Length);
                    Log.Out($"[PlayerProfileStorage] SteamID kinyerve prefix vágással: {steamId}");
                }
                else if (Regex.IsMatch(platformIdRaw, @"^\d{17}$"))
                {
                    steamId = platformIdRaw;
                    Log.Out($"[PlayerProfileStorage] SteamID kinyerve natív formában: {steamId}");
                }
                else
                {
                    Log.Warning($"[PlayerProfileStorage] PlatformId nem Steam formátumú: {platformIdRaw}");
                }
            }
            else
            {
                Log.Warning("[PlayerProfileStorage] PlatformId.CombinedString üres vagy null.");
            }

            try
            {
                Directory.CreateDirectory(ProfileFolder);
            }
            catch (Exception ex)
            {
                Log.Error($"[PlayerProfileStorage] Mappa létrehozási hiba: {ex.Message}");
                return;
            }

            string filePath = Path.Combine(ProfileFolder, $"{crossId}.json");

            PlayerProfile profile = File.Exists(filePath)
                ? JsonConvert.DeserializeObject<PlayerProfile>(File.ReadAllText(filePath))
                : new PlayerProfile
                {
                    CrossplatformId = crossId,
                    SteamId = steamId,
                    Name = ci.playerName,
                    Ip = ip,
                    FirstSeen = DateTime.UtcNow,
                };

            // Frissítjük az aktuális adatokat
            profile.Name = ci.playerName;
            profile.Ip = ip;
            if (!string.IsNullOrEmpty(steamId)) profile.SteamId = steamId;
            profile.LastSeen = DateTime.UtcNow;

            try
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(profile, Formatting.Indented));
                Log.Out($"[PlayerProfileStorage] Profil frissítve: {profile.Name} ({crossId})");
            }
            catch (Exception ex)
            {
                Log.Error($"[PlayerProfileStorage] Írási hiba: {ex.Message}");
            }
        }

        public static PlayerProfile LoadProfile(string crossId)
        {
            if (string.IsNullOrEmpty(crossId))
                return null;

            try
            {
                Directory.CreateDirectory(ProfileFolder);
            }
            catch (Exception ex)
            {
                Log.Error($"[PlayerProfileStorage] Mappa létrehozási hiba (Load): {ex.Message}");
                return null;
            }

            string filePath = Path.Combine(ProfileFolder, $"{crossId}.json");
            if (!File.Exists(filePath))
                return null;

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<PlayerProfile>(json);
            }
            catch (Exception ex)
            {
                Log.Error($"[PlayerProfileStorage] Beolvasási hiba: {ex.Message}");
                return null;
            }
        }

        public class PlayerProfile
        {
            public string CrossplatformId;
            public string SteamId;
            public string Name;
            public string Ip;
            public DateTime FirstSeen;
            public DateTime LastSeen;
        }
    }
}

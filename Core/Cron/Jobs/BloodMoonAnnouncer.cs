using OperenciaManager.Systems;
using OperenciaManager.Systems.Cron;
using OperenciaManager.Comms;
using UnityEngine;
using OperenciaManager.Core;

namespace OperenciaManager.Modules
{
    public static class BloodMoonAnnouncer
    {
        private const string DiscordWebhookUrl = "https://discord.com/api/webhooks/1441445953465876590/spvTg9hKjN2E-z09idkVfTSUdmS3nY3xptQx81xHFfsyK_SsatTzfmGK0qw9RSsoxiWY";

        public static void Init()
        {
            InternalCronService.Register(CronType.Hourly, Announce);
            Log.Out("[BloodMoonAnnouncer] Regisztrálva Hourly cronra.");
        }

        private static void Announce()
        {
            if (!GameManager.Instance?.GameHasStarted ?? true)
                return;

            ulong worldTime = GameManager.Instance.World.GetWorldTime();

            // ✅ Éjféli offset: így a napváltás éjfélkor történik, nem reggel 8-kor
            int currentDay = (int)((worldTime + 12000) / 24000);

            int bloodMoonFreq = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);

            // ✅ Stabil modulus alapú számítás
            int daysSinceLastBloodMoon = currentDay % bloodMoonFreq;
            int daysLeft = bloodMoonFreq - daysSinceLastBloodMoon;

            string chatMessage = null;
            string discordTitle = "🌕 Vérhold előrejelzés";
            string discordDescription = null;

            if (daysLeft > 1)
            {
                chatMessage = $"[ff0000]{daysLeft}[FFFFFF] nap múlva vérhold éjjele közeleg...";
                discordDescription = $"**{daysLeft} nap múlva vérhold lesz.**\nKészülj fel a sötétségre.";
            }
            else if (daysLeft == 1)
            {
                chatMessage = $"Holnap éjjel [ff0000]vérhold[FFFFFF] lesz. Készülj fel.";
                discordDescription = "**Holnap vérhold lesz.**\n🧟‍♂️ Éjszaka veszélyes lesz.";
            }
            else if (daysLeft == 0)
            {
                float timeOfDay = worldTime % 24000;
                if (timeOfDay < 22000)
                {
                    chatMessage = $"Ma éjjel [ff0000]vérhold[FFFFFF] lesz. Ne maradj kint sötétedés után.";
                    discordDescription = "**Ma éjjel vérhold lesz.**\n🌒 Maradj bent, ha élni akarsz.";
                }
                else
                {
                    chatMessage = $"[ff0000]A vérhold elkezdődött![FFFFFF] Maradj életben.";
                    discordTitle = "🔴 Vérhold elkezdődött!";
                    discordDescription = "**A vérhold elkezdődött!**\n🩸 Túlélés a cél. Ne hagyd, hogy elkapjanak.";
                }
            }

            if (chatMessage != null)
            {
                ChatHook.SendGlobal(chatMessage);
                Log.Out("[BloodMoonAnnouncer] Chat üzenet elküldve: " + chatMessage);

                try
                {
                    DiscordNotifier.Send(
                        DiscordWebhookUrl,
                        discordTitle,
                        discordDescription
                    );
                    Log.Out("[BloodMoonAnnouncer] Discord webhook elküldve: " + discordTitle);
                }
                catch (System.Exception ex)
                {
                    Log.Error("[BloodMoonAnnouncer] Discord webhook hiba: " + ex.Message);
                }
            }
        }
    }
}

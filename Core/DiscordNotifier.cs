using DiscordWebhook;
using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Comms
{
    public static class DiscordNotifier
    {
        public static void Send(
            string webhookUrl,
            string title,
            string description,
            string imageUrl = null
        )
        {
            if (string.IsNullOrEmpty(webhookUrl) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                Log.Warning("DiscordNotifier: Missing required fields.");
                return;
            }

            var webhook = new DiscordWebhook.DiscordWebhook(webhookUrl);

            var embed = new DiscordEmbed
            {
                Title = title,
                Description = description
            };

            if (!string.IsNullOrEmpty(imageUrl))
            {
                embed.Image = new EmbedMedia { Url = imageUrl };
            }

            var message = new DiscordMessage
            {
                Embeds = new List<DiscordEmbed> { embed }
            };

            webhook.Send(message);
        }
    }
}

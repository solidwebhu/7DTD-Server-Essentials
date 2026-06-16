using System;
using System.Collections.Generic;
using OperenciaManager.Core;
using OperenciaManager.Comms;
using OperenciaManager.Systems.Events.Challenges;
using UnityEngine;

namespace OperenciaManager.Systems.Events.Core
{
    public class EventManager
    {
        private static EventManager instance;

        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new EventManager();

                return instance;
            }
        }

        private IEventChallenge current;
        private EventContext context;

        private readonly HashSet<int> participants = new HashSet<int>();

        private DateTime endTime;
        private DateTime lastProgressNotify;

        private bool active;

        private const string DiscordWebhook = "https://discord.com/api/webhooks/1441445953465876590/spvTg9hKjN2E-z09idkVfTSUdmS3nY3xptQx81xHFfsyK_SsatTzfmGK0qw9RSsoxiWY";

        private EventManager()
        {
            lastProgressNotify = DateTime.MinValue;
            active = false;
        }

        // ======================
        // Init
        // ======================

        public void Init()
        {
            ModEvents.GameUpdate.RegisterHandler(OnGameUpdate);
            ModEvents.EntityKilled.RegisterHandler(OnEntityKilled);

            Log.Out("[EventManager] Initialized.");
        }

        public bool IsActive
        {
            get { return active; }
        }

        // ======================
        // Random start
        // ======================

        public void StartRandomEvent()
        {
            IEventChallenge challenge = ChallengeRegistry.GetRandom();
            if (challenge != null)
                StartEvent(challenge);
        }

        // ======================
        // Start / End
        // ======================

        public bool StartEvent(IEventChallenge challenge)
        {
            if (active)
                return false;

            current = challenge;
            context = new EventContext();
            context.StartTime = DateTime.UtcNow;

            participants.Clear();
            lastProgressNotify = DateTime.MinValue;

            endTime = DateTime.UtcNow.Add(challenge.Duration);
            active = true;

            current.OnStart(context);

            ChatHook.SendGlobal("[FFAA00]" + challenge.Name + " EVENT elindult!");
            ChatHook.SendGlobal("[AAAAAA]" + challenge.IntroText + " — /event join");

            return true;
        }

        public void EndEvent()
        {
            if (!active)
                return;

            List<string> winners = new List<string>();

            foreach (int id in participants)
            {
                if (current.IsCompleted(id))
                {
                    RewardPlayer(id);
                    winners.Add(GetPlayerName(id));
                }
            }

            current.OnEnd(context);

            if (winners.Count > 0)
                ChatHook.SendGlobal("[00FF00]Nyertesek: " + string.Join(", ", winners));
            else
                ChatHook.SendGlobal("[FF0000]Senki sem teljesítette az eventet.");

            SendDiscordSummary(winners);

            ChatHook.SendGlobal("[FF0000]" + current.Name + " EVENT véget ért.");

            current = null;
            context = null;
            participants.Clear();
            active = false;
        }

        // ======================
        // Join / Leave
        // ======================

        public void Join(int entityId)
        {
            if (!active)
                return;

            if (participants.Add(entityId))
            {
                context.GetPlayer(entityId);
                current.EventPlayerJoined(entityId);
            }
        }

        public void Leave(int entityId)
        {
            if (!participants.Remove(entityId))
                return;

            if (current != null)
                current.EventPlayerLeft(entityId);
        }

        // ======================
        // Hooks
        // ======================

        private void OnGameUpdate(ref ModEvents.SGameUpdateData data)
        {
            if (!active)
                return;

            if (current != null)
                current.OnGameUpdate();

            CheckMilestones();

            if ((DateTime.UtcNow - lastProgressNotify).TotalSeconds >= 30)
            {
                SendProgressUpdates();
                lastProgressNotify = DateTime.UtcNow;
            }

            if (DateTime.UtcNow >= endTime)
                EndEvent();
        }
        public void NotifyDeadeye(int entityId)
        {
            if (!participants.Contains(entityId))
                return;

            DeadeyeChallenge deadeye = current as DeadeyeChallenge;
            if (deadeye != null)
                deadeye.Register(entityId);
        }
        public void RegisterDeadeye(int entityId)
        {
            if (!active)
                return;

            if (!(current is DeadeyeChallenge deadeye))
                return;

            if (!participants.Contains(entityId))
                return;

            deadeye.RegisterHeadshot(entityId);
        }

        private void OnEntityKilled(ref ModEvents.SEntityKilledData data)
        {
            if (!active)
                return;

            EntityZombie zombie = data.KilledEntitiy as EntityZombie;
            EntityPlayer player = data.KillingEntity as EntityPlayer;

            if (zombie == null || player == null)
                return;

            if (!participants.Contains(player.entityId))
                return;

            DeadeyeChallenge deadeye = current as DeadeyeChallenge;
            if (deadeye == null)
                return;

            DamageResponse dmg = zombie.lastDamageResponse;

            bool headLike =
                dmg.HitBodyPart == EnumBodyPartHit.None ||
                (dmg.HitBodyPart & EnumBodyPartHit.Head) != EnumBodyPartHit.None;

            if (!headLike)
                return;

            if (!dmg.Critical)
                return;

            deadeye.OnEntityKilled(player, zombie);
        }


        // ======================
        // Progress + Milestone
        // ======================

        private void SendProgressUpdates()
        {
            foreach (int id in participants)
            {
                ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(id);
                if (ci == null)
                    continue;

                ChatHook.SendPrivate(ci,
                    "[9999FF]Event állapot: " + current.GetProgressText(id));
            }
        }

        private void CheckMilestones()
        {
            foreach (int id in participants)
            {
                PlayerEventState state = context.GetPlayer(id);

                int pct = ExtractPercent(current.GetProgressText(id));

                if (pct >= 25 && state.LastMilestone < 25) NotifyMilestone(id, 25);
                else if (pct >= 50 && state.LastMilestone < 50) NotifyMilestone(id, 50);
                else if (pct >= 75 && state.LastMilestone < 75) NotifyMilestone(id, 75);
                else if (pct >= 100 && state.LastMilestone < 100) NotifyMilestone(id, 100);
            }
        }

        private void NotifyMilestone(int id, int value)
        {
            PlayerEventState state = context.GetPlayer(id);
            state.LastMilestone = value;

            ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(id);
            if (ci != null)
                ChatHook.SendPrivate(ci, "[00FF00]Mérföldkő elérve: " + value + "%");
        }

        // ======================
        // Reward + Discord
        // ======================

        private void RewardPlayer(int entityId)
        {
            Log.Out("[EventManager] Reward: " + entityId);
        }

        private void SendDiscordSummary(List<string> winners)
        {
            try
            {
                string text = winners.Count > 0
                    ? "Nyertesek:\n" + string.Join("\n", winners)
                    : "Senki sem teljesítette.";

                DiscordNotifier.Send(DiscordWebhook, current.Name, text);
            }
            catch { }
        }

        // ======================
        // Helpers
        // ======================

        private string GetPlayerName(int id)
        {
            EntityPlayer p;
            if (GameManager.Instance.World.Players.dict.TryGetValue(id, out p))
                return p.EntityName;

            return id.ToString();
        }

        private int ExtractPercent(string progress)
        {
            try
            {
                string[] parts = progress.Split('/');
                float a = float.Parse(parts[0]);
                float b = float.Parse(parts[1].Split(' ')[0]);
                return Mathf.RoundToInt((a / b) * 100f);
            }
            catch
            {
                return 0;
            }
        }
    }
}

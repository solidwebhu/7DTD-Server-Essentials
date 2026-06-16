using OperenciaManager.Core;
using UnityEngine;
using System.Collections.Generic;

namespace OperenciaManager.Systems
{
    public class PoiEntryWatcher : MonoBehaviour
    {
        private readonly Dictionary<int, Vector3> lastPoi = new Dictionary<int, Vector3>();
        private float nextCheck;
        private const float interval = 1.5f;

        void Update()
        {
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + interval;

            if (GameManager.Instance?.World == null) return;

            var players = GameManager.Instance.World.GetPlayers();
            if (players == null) return;

            foreach (var player in players)
            {
                if (player?.QuestJournal == null) continue;

                var poi = GameManager.Instance.World.GetPOIAtPosition(player.position);
                if (poi?.prefab == null) continue;

                Vector3 poiCenter = poi.boundingBoxPosition;

                Vector3 last;
                if (lastPoi.TryGetValue(player.entityId, out last) &&
                    Vector3.Distance(last, poiCenter) < 1f)
                    continue;

                lastPoi[player.entityId] = poiCenter;

                foreach (var other in players)
                {
                    if (other == null || other == player) continue;
                    if (other.QuestJournal == null) continue;

                    if (player.IsFriendsWith(other)) continue;
                    if (player.Party != null && player.Party.ContainsMember(other)) continue;

                    foreach (var quest in other.QuestJournal.quests)
                    {
                        if (!quest.Active) continue;
                        if (!quest.RallyMarkerActivated) continue;

                        Vector3 questPoi;
                        if (!quest.GetPositionData(out questPoi, Quest.PositionDataTypes.POIPosition))
                            continue;

                        if (Vector3.Distance(questPoi, poiCenter) > 2f)
                            continue;

                        ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
                        if (ci == null) continue;

                        ChatHook.SendPrivate(
                            ci,
                            $"[FFAA00]Figyelem: **{other.EntityName}** aktív questet futtat ezen a POI-n."
                        );
                        goto NEXT_PLAYER;
                    }
                }

            NEXT_PLAYER:
                continue;
            }
        }
    }
}

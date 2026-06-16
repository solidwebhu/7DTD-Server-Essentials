using HarmonyLib;
using UnityEngine;

namespace OperenciaManager.Systems.Events.Core
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.AwardKill))]
    public class DeadeyeAwardKillPatch
    {
        static void Prefix(EntityAlive killer, EntityAlive killedEntity)
        {
            try
            {
                if (killer == null || killedEntity == null)
                    return;

                EntityPlayer player = killer as EntityPlayer;
                EntityZombie zombie = killedEntity as EntityZombie;

                if (player == null || zombie == null)
                    return;

                // EZ A KULCS
                DamageResponse dmg = zombie.RecordedDamage;

                if ((dmg.HitBodyPart & EnumBodyPartHit.Head) == EnumBodyPartHit.None)
                    return;

                // Headshot confirmed
                EventManager.Instance?.RegisterDeadeye(player.entityId);
            }
            catch (System.Exception e)
            {
                Log.Out("[Deadeye] Exception: " + e);
            }
        }
    }
}

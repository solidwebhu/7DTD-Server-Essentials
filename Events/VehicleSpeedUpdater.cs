using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public class VehicleSpeedUpdater : MonoBehaviour
    {
        private readonly Dictionary<int, float> smoothSpeed = new Dictionary<int, float>();
        private float timer;
        private const float UPDATE_INTERVAL = 0.05f;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer < UPDATE_INTERVAL)
                return;

            timer = 0f;

            foreach (var ci in ConnectionManager.Instance.Clients.List)
            {
                if (ci == null)
                    continue;

                EntityPlayer player;
                if (!GameManager.Instance.World.Players.dict.TryGetValue(ci.entityId, out player))
                    continue;

                EntityVehicle vehicle = player.AttachedToEntity as EntityVehicle;

                // === NINCS JÁRMŰBEN ===
                if (vehicle == null)
                {
                    // CVAR hard reset
                    player.SetCVar("vehicle_speed_int", 0);
                    player.SetCVar("vehicle_speed_smooth", 0);
                    player.SetCVar(".vehicleSpeedDisplay", 0);

                    // buff lifecycle cleanup
                    player.Buffs.RemoveBuff("buffVehicleSpeedDispatcher");
                    player.Buffs.RemoveBuff("buffVehicleSpeedActive");

                    smoothSpeed[ci.entityId] = 0f;
                    continue;
                }

                // === JÁRMŰBEN VAN ===
                if (!player.Buffs.HasBuff("buffVehicleSpeedDispatcher"))
                {
                    player.Buffs.AddBuff("buffVehicleSpeedDispatcher", -1, true, false, -1f);
                }

                // === SEBESSÉG MÉRÉS ===
                float rawSpeed = vehicle.GetVelocityPerSecond().magnitude * 3.6f;
                int speedInt = Mathf.Clamp(Mathf.RoundToInt(rawSpeed), 0, 200);
                player.SetCVar("vehicle_speed_int", speedInt);

                // === SIMÍTÁS ===
                float currentSmooth;
                if (!smoothSpeed.TryGetValue(ci.entityId, out currentSmooth))
                    currentSmooth = speedInt;

                currentSmooth = Mathf.Lerp(currentSmooth, speedInt, 0.2f);
                smoothSpeed[ci.entityId] = currentSmooth;

                player.SetCVar("vehicle_speed_smooth", Mathf.RoundToInt(currentSmooth));
            }
        }
    }
}

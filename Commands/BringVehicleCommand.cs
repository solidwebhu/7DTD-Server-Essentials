using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class BringVehicleCommand : IChatCommand
    {
        public string Name => "/hozd";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (args.Length == 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Használat: /hozd <járműnév pl: gyro, minibike, motor, bicikli, truck>");
                return;
            }

            string requested = args[0].ToLower();
            string targetClass = null;

            if (requested == "gyro") targetClass = "vehicleGyrocopter";
            else if (requested == "minibike") targetClass = "vehicleMinibike";
            else if (requested == "motor") targetClass = "vehicleMotorcycle";
            else if (requested == "bicikli") targetClass = "vehicleBicycle";
            else if (requested == "truck") targetClass = "vehicleTruck4x4";

            if (targetClass == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Ismeretlen járműnév.");
                return;
            }

            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player) || player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.");
                return;
            }

            Vector3 playerPos = player.GetPosition();
            string userId = clientInfo.CrossplatformId.CombinedString;
            TeleportMemory.SavePosition(userId, playerPos);

            EntityVehicle vehicle = FindVehicle(targetClass, clientInfo.CrossplatformId);
            if (vehicle != null)
            {
                vehicle.SetPosition(playerPos);
                VehicleMemoryStorage.SaveLastPosition(userId, targetClass, new Vector3i((int)playerPos.x, (int)playerPos.y, (int)playerPos.z));
                ChatHook.SendPrivate(clientInfo, "[00FF00]" + requested + " járműved teleportálva hozzád.");
                return;
            }

            if (!VehicleMemoryStorage.TryGetLastPosition(userId, targetClass, out Vector3i lastPos))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található jármű és nincs eltárolt pozíció sem.");
                return;
            }

            Vector3 chunkCenter = new Vector3(lastPos.x, lastPos.y, lastPos.z);
            NetPackageTeleportPlayer tp = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(chunkCenter, null, false);
            clientInfo.SendPackage(tp);

            ChatHook.SendPrivate(clientInfo, "[FFFF00]Jármű keresése folyamatban...");

            GameManager.Instance.StartCoroutine(new DelayedVehicleSearch(clientInfo, targetClass, playerPos, requested));
        }

        private EntityVehicle FindVehicle(string targetClass, PlatformUserIdentifierAbs ownerId)
        {
            foreach (Entity e in GameManager.Instance.World.Entities.list)
            {
                if (e is EntityVehicle v && v.vehicle != null && v.vehicle.OwnerId != null)
                {
                    string className = EntityClass.GetEntityClassName(v.entityClass);
                    if (className == targetClass && v.vehicle.OwnerId.Equals(ownerId))
                        return v;
                }
            }
            return null;
        }

        private class DelayedVehicleSearch : IEnumerator
        {
            private readonly float delay = 5.0f;
            private readonly float startTime;
            private readonly ClientInfo clientInfo;
            private readonly string targetClass;
            private readonly Vector3 originalPos;
            private readonly string requested;
            private bool done;

            public DelayedVehicleSearch(ClientInfo ci, string tc, Vector3 op, string rq)
            {
                clientInfo = ci;
                targetClass = tc;
                originalPos = op;
                requested = rq;
                startTime = Time.time;
                done = false;
            }

            public bool MoveNext()
            {
                if (done) return false;
                if (Time.time - startTime < delay) return true;

                EntityVehicle vehicle = FindVehicle(targetClass, clientInfo.CrossplatformId);
                if (vehicle != null)
                {
                    vehicle.SetPosition(originalPos);
                    VehicleMemoryStorage.SaveLastPosition(clientInfo.CrossplatformId.CombinedString, targetClass, new Vector3i((int)originalPos.x, (int)originalPos.y, (int)originalPos.z));
                    ChatHook.SendPrivate(clientInfo, "[00FF00]" + requested + " jármű megtalálva. Visszatérés...");
                }
                else
                {
                    ChatHook.SendPrivate(clientInfo, "[FF0000]A jármű chunkja nem aktiválható. Próbáld meg újra, vagy menj közelebb.");
                }

                NetPackageTeleportPlayer tpBack = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(originalPos, null, false);
                clientInfo.SendPackage(tpBack);

                TeleportMemory.ClearPosition(clientInfo.CrossplatformId.CombinedString);
                done = true;
                return false;
            }

            public void Reset() { }
            public object Current => null;

            private EntityVehicle FindVehicle(string targetClass, PlatformUserIdentifierAbs ownerId)
            {
                foreach (Entity e in GameManager.Instance.World.Entities.list)
                {
                    if (e is EntityVehicle v && v.vehicle != null && v.vehicle.OwnerId != null)
                    {
                        string className = EntityClass.GetEntityClassName(v.entityClass);
                        if (className == targetClass && v.vehicle.OwnerId.Equals(ownerId))
                            return v;
                    }
                }
                return null;
            }
        }
    }
}

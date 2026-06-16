using System;
using System.Collections.Generic;
using UnityEngine;
using OperenciaManager.Core;

namespace OperenciaManager.Commands
{
    public class VehicleInfoCommand : IChatCommand
    {
        public string Name => "/jarmuinfo";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.");
                return;
            }

            Vector3 playerPos = player.GetPosition();
            int found = 0;

            foreach (Entity e in GameManager.Instance.World.Entities.list)
            {
                if (!(e is EntityVehicle)) continue;

                EntityVehicle v = (EntityVehicle)e;
                if (v.vehicle == null || v.vehicle.OwnerId == null) continue;
                if (!v.vehicle.OwnerId.Equals(clientInfo.CrossplatformId)) continue;

                string className = EntityClass.list[v.entityClass].entityClassName;
                Vector3 pos = v.GetPosition();
                float distance = Vector3.Distance(playerPos, pos);
                int health = v.Health;

                string msg = $"[00FF00]{className} | Pozíció: {pos} | Élet: {health} | Távolság: {distance:F1}m";
                ChatHook.SendPrivate(clientInfo, msg);
                found++;
            }

            if (found == 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található jármű a tulajdonodban.");
            }
        }
    }
}

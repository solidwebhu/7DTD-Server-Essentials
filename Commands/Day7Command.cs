using OperenciaManager.Commands;
using System;
using System.Collections.Generic;


namespace OperenciaManager.Commands
{
    public class Day7Command : IChatCommand
    {
        public string Name => "/day7";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            try
            {
                int zombies = 0, animals = 0, bicycles = 0, miniBikes = 0, motorcycles = 0, trucks = 0, gyros = 0, crates = 0;

                int currentDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
                int bloodMoonInterval = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
                int nextBloodMoon = ((currentDay / bloodMoonInterval) + 1) * bloodMoonInterval;
                int daysRemaining = nextBloodMoon - currentDay;

                List<Entity> entities = GameManager.Instance.World.Entities.list;
                foreach (Entity e in entities)
                {
                    if (e.IsClientControlled()) continue;

                    string tags = e.EntityClass.Tags.ToString();
                    if (tags.Contains("zombie") && e.IsAlive()) zombies++;
                    else if (tags.Contains("animal") && e.IsAlive()) animals++;
                    else
                    {
                        string name = EntityClass.list[e.entityClass].entityClassName;
                        if (name == "vehicleBicycle") bicycles++;
                        else if (name == "vehicleMinibike") miniBikes++;
                        else if (name == "vehicleMotorcycle") motorcycles++;
                        else if (name == "vehicle4x4Truck") trucks++;
                        else if (name == "vehicleGyrocopter") gyros++;
                        else if (name == "sc_General") crates++;
                    }
                }

                string status;
                var bloodMoon = GameManager.Instance.World.aiDirector?.BloodMoonComponent;
                if (bloodMoon != null && bloodMoon.BloodMoonActive)
                    status = "A horda itt van!";
                else if (daysRemaining == 0)
                    status = "A következő horda ma éjjel érkezik!";
                else
                    status = $"A következő horda {daysRemaining} nap múlva érkezik.";

                string message =
                    $"Statisztika\n" +
                    $"{status}\n" +
                    $"Játékosok: {ConnectionManager.Instance.ClientCount()} | Zombik: {zombies} | Állatok: {animals}\n" +
                    $"Járművek: Biciklik {bicycles}, Minibikes {miniBikes}, Motorcycles {motorcycles}, 4x4 {trucks}, Gyros {gyros}\n" +
                    $"Supply crates: {crates}";

                Core.ChatHook.SendPrivate(clientInfo, message);
            }
            catch (Exception e)
            {
                Log.Out($"[OperenciaManager] Error in Day7Command.Execute: {e.Message}");
                Core.ChatHook.SendPrivate(clientInfo, "Error while retrieving Day7 status.");
            }
        }
    }
}

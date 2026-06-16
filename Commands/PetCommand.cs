using OperenciaManager.Core;
using OperenciaManager.Systems;
using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Commands
{
    public class PetCommand : IChatCommand
    {
        public string Name { get { return "/pet"; } }
        public bool IsAdmin { get { return false; } }

        // Engedélyezett petek (user input → entityClassName)
        private static readonly Dictionary<string, string> AllowedPets =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "wolf", "animalWolf" },
                { "bear", "animalBear" },
                { "lion", "animalMountainLion" }
            };

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (args == null || args.Length < 1)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Használat: /pet [lerak|gyere|marad|tamad|torol]");
                return;
            }

            string sub = args[0].ToLower();

            switch (sub)
            {
                case "lerak":
                    HandlePetSpawn(clientInfo, args);
                    break;

                case "gyere":
                    PetRegistry.SetState(clientInfo.entityId, PetState.Passive);
                    PetFollowerSystem.TeleportToOwner(clientInfo.entityId);
                    ChatHook.SendPrivate(clientInfo, "[00FF00]Már jövök gazdám!");
                    break;

                case "marad":
                    PetRegistry.SetState(clientInfo.entityId, PetState.Sit);
                    ChatHook.SendPrivate(clientInfo, "[00FF00]Igenis gazdám! A helyemen maradok!");
                    break;

                case "tamad":
                    PetRegistry.SetState(clientInfo.entityId, PetState.Guard);
                    ChatHook.SendPrivate(clientInfo, "[00FF00]Elkapom őket!");
                    break;

                case "torol":
                    HandlePetDelete(clientInfo);
                    break;

                default:
                    ChatHook.SendPrivate(clientInfo, "[FF0000]Ismeretlen alparancs.");
                    break;
            }
        }

        // ---------------------------------------------------------
        // PET LERAKÁSA /pet lerak <Wolf/Bear/Lion>
        // ---------------------------------------------------------
        private void HandlePetSpawn(ClientInfo clientInfo, string[] args)
        {
            EntityPlayer player;
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out player) || player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található a játékos entitás.[-]");
                return;
            }

            if (PetRegistry.HasPet(player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Már van egy aktív háziállatod!");
                return;
            }

            // Ha nincs megadva név → rövid használati üzenet
            if (args.Length < 2)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Használat: /pet lerak Wolf|Bear|Lion (kisbetűvel is írható!)");
                return;
            }

            string userInput = args[1];

            // Csak a 3 engedélyezett pet
            if (!AllowedPets.TryGetValue(userInput, out string entityClassName))
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Ismeretlen vagy tiltott pet név! Engedélyezett: Wolf, Bear, Lion");
                return;
            }

            int classId = EntityClass.FromString(entityClassName);
            if (classId == -1)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Hiba: az entitás nem található az EntityClass XML-ben![-]");
                return;
            }

            // Spawn pozíció
            Vector3 spawnPos = player.GetPosition() + player.transform.right * 2f;

            EntityAlive pet = EntityFactory.CreateEntity(classId, spawnPos) as EntityAlive;
            if (pet == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem sikerült létrehozni az entitást.[-]");
                return;
            }

            GameManager.Instance.World.SpawnEntityInWorld(pet);


            // REGISZTRÁCIÓ + KÖVETÉS
            PetRegistry.Register(pet, player);
            PetRegistry.SetState(pet, PetState.Follow);
            PetFollowerSystem.StartFollowing(pet, player);

            ChatHook.SendPrivate(clientInfo, $"[00FF00]{entityClassName} sikeresen lerakva![-]");
        }

        // ---------------------------------------------------------
        // PET TÖRLÉSE
        // ---------------------------------------------------------
        private void HandlePetDelete(ClientInfo clientInfo)
        {
            EntityPlayer player;
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található a játékos entitás.[-]");
                return;
            }

            EntityAlive pet = PetRegistry.GetPet(player);

            if (pet != null)
            {
                GameManager.Instance.World.RemoveEntity(pet.entityId, EnumRemoveEntityReason.Despawned);
            }

            PetRegistry.Remove(pet);

            ChatHook.SendPrivate(clientInfo, "[00FF00]A háziállatod törölve lett! Most már újra lerakhatsz egyet.");
        }
    }
}

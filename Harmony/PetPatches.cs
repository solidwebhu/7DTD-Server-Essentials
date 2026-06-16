using HarmonyLib;

namespace OperenciaManager.Systems
{
    [HarmonyPatch]
    public static class PetPatches
    {
        // ---------------------------------------------------------
        // 1) SERVER-SIDE ATTACK TARGET FILTER
        // ---------------------------------------------------------
        [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.SetAttackTarget))]
        [HarmonyPrefix]
        public static void Prefix_SetAttackTarget(EntityAlive __instance, ref EntityAlive _attackTarget, ref int _attackTargetTime)
        {
            // Nem pet → nem érdekel
            if (!PetRegistry.IsPet(__instance)) return;

            EntityPlayer owner = PetRegistry.GetOwner(__instance);
            PetState state = PetRegistry.GetState(__instance);

            // 1) JÁTÉKOSOK TILTÁSA (gazda + más játékosok)
            if (_attackTarget is EntityPlayer)
            {
                _attackTarget = null;
                _attackTargetTime = 0;
                __instance.ClearInvestigatePosition();
                return;
            }

            // 2) HA NEM GUARD MÓD → NE TÁMADJON SEMMIT
            if (state != PetState.Guard)
            {
                _attackTarget = null;
                _attackTargetTime = 0;
                __instance.ClearInvestigatePosition();
                return;
            }

            // 3) GUARD módban zombikat / hostile NPC-ket támadhat → engedjük
        }

        // ---------------------------------------------------------
        // 2) CLIENT-SIDE ATTACK TARGET FILTER
        // ---------------------------------------------------------
        [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.SetAttackTargetClient))]
        [HarmonyPrefix]
        public static void Prefix_SetAttackTargetClient(EntityAlive __instance, ref EntityAlive _attackTarget)
        {
            if (!PetRegistry.IsPet(__instance)) return;

            PetState state = PetRegistry.GetState(__instance);

            // 1) JÁTÉKOSOK TILTÁSA
            if (_attackTarget is EntityPlayer)
            {
                _attackTarget = null;
                return;
            }

            // 2) HA NEM GUARD MÓD → NE TÁMADJON SEMMIT
            if (state != PetState.Guard)
            {
                _attackTarget = null;
            }
        }

        // ---------------------------------------------------------
        // 3) SIT MÓDBAN NE MOZOGJON
        // ---------------------------------------------------------
        [HarmonyPatch(typeof(EntityMoveHelper), nameof(EntityMoveHelper.UpdateMoveHelper))]
        [HarmonyPrefix]
        public static bool Prefix_UpdateMoveHelper(EntityMoveHelper __instance)
        {
            EntityAlive pet = __instance.entity as EntityAlive;
            if (pet == null) return true;
            if (!PetRegistry.IsPet(pet)) return true;

            PetState state = PetRegistry.GetState(pet);

            // Sit módban teljesen tiltjuk a mozgást
            if (state == PetState.Sit)
            {
                return false;
            }

            return true;
        }
    }

    // ---------------------------------------------------------
    // 4) HARMONY BOOTSTRAP
    // ---------------------------------------------------------
    public static class PetHarmonyBootstrap
    {
        public static void Init()
        {
            var harmony = new Harmony("OperenciaManager.Pets");
            harmony.PatchAll(typeof(PetPatches));
            Log.Out("[PetHarmonyBootstrap] Harmony patchek aktiválva.");
        }
    }
}

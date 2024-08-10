using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using WellRoundedBalance.Gamemodes.Eclipse;
using SYS = System.Reflection.Emit;

namespace WellRoundedBalance.Misc
{
    public static class HarmonyHooks
    {
        public static Harmony harm;
        public static void Init()
        {
            harm = new Harmony(Main.PluginGUID);
            if (Main.LeagueOfLiteralGaysLoadeded)
            {
                PatchLoLEnemies();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void PatchLoLEnemies()
        {
            harm.CreateClassProcessor(typeof(RiftPatches)).Patch();
            harm.CreateClassProcessor(typeof(RiftFlinch)).Patch();

            var reksaiCard = RiftTitansMod.RiftTitansPlugin.ReksaiCard.Card;
            reksaiCard.spawnCard.directorCreditCost = 600;
            var master = reksaiCard.spawnCard.prefab.GetComponent<CharacterMaster>();
            var aiList = master.GetComponents<AISkillDriver>();
            foreach (var ai in aiList)
            {
                switch (ai.customName)
                {
                    case "Special":
                        break;
                    case "Seeker":
                        break;
                    case "ChaseHard":
                        ai.shouldSprint = false;
                        break;
                    case "Attack":
                        ai.driverUpdateTimerOverride = 0.5f;
                        ai.nextHighPriorityOverride = aiList.Last();
                        break;
                    case "Chase":
                        break;
                }
            }
            master.bodyPrefab.GetComponent<CharacterBody>().baseMoveSpeed = 10;
        }
    }

    [HarmonyPatch(typeof(RiftTitansMod.SkillStates.Chicken.Shoot), nameof(RiftTitansMod.SkillStates.Chicken.Shoot.Fire))]
    public class RiftPatches
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                yield return code;

                if (code.Calls(AccessTools.Method(typeof(BaseState), nameof(BaseState.GetAimRay))))
                {
                    yield return new CodeInstruction(SYS.OpCodes.Ldarg_0);
                    yield return new CodeInstruction(SYS.OpCodes.Call, AccessTools.PropertyGetter(typeof(EntityState), nameof(EntityState.characterBody)));
                    yield return new CodeInstruction(SYS.OpCodes.Ldsfld, AccessTools.Field(typeof(RiftTitansMod.Modules.Projectiles), nameof(RiftTitansMod.Modules.Projectiles.chickenProjectilePrefab)));
                    yield return new CodeInstruction(SYS.OpCodes.Call, AccessTools.Method(typeof(PredictionUtils), nameof(PredictionUtils.PredictAimrayNew)));
                }
            }
        }
    }

    [HarmonyPatch(typeof(RiftTitansMod.SkillStates.Blue.Slam), nameof(RiftTitansMod.SkillStates.Blue.Slam.GetMinimumInterruptPriority))]
    public class RiftFlinch
    {
        [HarmonyPostfix]
        private static void Postfix(ref InterruptPriority __result)
        {
            __result = InterruptPriority.Pain;
        }
    }
}

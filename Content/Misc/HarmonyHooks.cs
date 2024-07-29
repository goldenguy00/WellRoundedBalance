using System;
using HarmonyLib;
using RiftTitansMod.SkillStates.Blue;
using RiftTitansMod.SkillStates.Chicken;
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
                harm.CreateClassProcessor(typeof(RiftPatches)).Patch();
                harm.CreateClassProcessor(typeof(RiftFlinch)).Patch();
            }

            //if (Main.PieceOfShitLoadedElectricBoogaloo)
                //harm.CreateClassProcessor(typeof(MSUFix)).Patch();
        }
    }

    // on principle i will not put a pr out to fix this. nah.
    // also on principle i will add this hook to every mod i touch from now on
    // i dont fucking care. not even gonna import this.
    [HarmonyPatch("Moonstorm.BuffModuleBase", "OnBuffsChanged")]
    public class MSUFix
    {
        [HarmonyFinalizer]
        private static Exception Finalizer() => null;
    }

    [HarmonyPatch(typeof(Shoot), nameof(Shoot.Fire))]
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

    [HarmonyPatch(typeof(Slam), nameof(Slam.GetMinimumInterruptPriority))]
    public class RiftFlinch
    {
        [HarmonyPostfix]
        private static void Postfix(ref InterruptPriority __result)
        {
            __result = InterruptPriority.Pain;
        }
    }
}

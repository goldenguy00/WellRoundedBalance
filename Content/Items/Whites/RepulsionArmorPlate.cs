﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace WellRoundedBalance.Items.Whites
{
    public class RepulsionArmorPlate : ItemBase
    {
        public override string Name => ":: Items : Whites :: Repulsion Armor Plate";
        public override string InternalPickupToken => "repulsionArmorPlate";

        public override string PickupText => "Receive flat damage reduction from all attacks.";

        public override string DescText =>
            StackDesc(flatDamageReduction, flatDamageReductionStack, init => $"Reduce all <style=cIsDamage>incoming damage</style> by <style=cIsDamage>{init}</style>{{Stack}}. ", noop) + "Cannot be reduced below " +
            StackDesc(minimumDamage, minimumDamageStack, init => $"<style=cIsDamage>{init}</style>{{Stack}}", noop) +
            StackDesc(minimumPercentDamage, minimumPercentDamageStack, init => (minimumDamage > 0 || minimumDamageStack > 0 ? "or " : "") + $"<style=cIsDamage>{d(init)}</style>{{Stack}} of <style=cIsHealing>maximum health</style>", d) + ".";

        [ConfigField("Flat Damage Reduction", 5f)]
        public static float flatDamageReduction;

        [ConfigField("Flat Damage Reduction per Stack", 5f)]
        public static float flatDamageReductionStack;

        [ConfigField("Flat Damage Reduction is Hyperbolic", "Decimal, Max value. Set to 0 to make it linear.", 0f)]
        public static float flatDamageReductionIsHyperbolic;

        [ConfigField("Minimum Damage", 8f)]
        public static float minimumDamage;

        [ConfigField("Minimum Damage per Stack", 0f)]
        public static float minimumDamageStack;

        [ConfigField("Minimum Damage is Hyperbolic", "Decimal, Max value. Set to 0 to make it linear.", 0f)]
        public static float minimumDamageIsHyperbolic;

        [ConfigField("Minimum Percent Damage", "Decimal.", 0f)]
        public static float minimumPercentDamage;

        [ConfigField("Minimum Percent Damage per Stack", "Decimal.", 0f)]
        public static float minimumPercentDamageStack;

        [ConfigField("Minimum Percent Damage is Hyperbolic", "Decimal, Max value. Set to 0 to make it linear.", 0f)]
        public static float minimumPercentDamageIsHyperbolic;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public static void HealthComponent_TakeDamage(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(x => x.MatchLdfld<HealthComponent.ItemCounts>(nameof(HealthComponent.ItemCounts.armorPlate))) && c.TryGotoNext(x => x.MatchStloc(6)))
            {
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldloc, 6);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, HealthComponent, float>>((orig, self) => Mathf.Max(Mathf.Min(
                    StackAmount(minimumDamage, minimumDamageStack, self.itemCounts.armorPlate, minimumDamageIsHyperbolic),
                    self.fullHealth * StackAmount(minimumPercentDamage, minimumPercentDamageStack, self.itemCounts.armorPlate, minimumPercentDamageIsHyperbolic)),
                    StackAmount(flatDamageReduction, flatDamageReductionStack, self.itemCounts.armorPlate, flatDamageReductionIsHyperbolic)));
            }
            else Main.WRBLogger.LogError("Failed to apply Repulsion Armor Plate hook");
        }
    }
}
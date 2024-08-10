using System;
using R2API.Utils;

namespace WellRoundedBalance.Items.Whites
{
    public class ArmorPiercingRounds : ItemBase<ArmorPiercingRounds>
    {
        public override string Name => ":: Items : Whites :: Armor Piercing Rounds";
        public override ItemDef InternalPickup => RoR2Content.Items.BossDamageBonus;

        public override string PickupText => "Deal extra damage to bosses and champions.";

        public override string DescText => "Deal an additional <style=cIsDamage>" + d(bossChampionDamageBonus) + "</style> <style=cStack>(+" + d(bossChampionDamageBonus) + " per stack)</style> damage to bosses and champions.";

        [ConfigField("Boss and Champion Damage Bonus", "Decimal.", 0.15f)]
        public static float bossChampionDamageBonus;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdsfld("RoR2.RoR2Content/Items", "BossDamageBonus")) &&
                c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdcR4(0.2f)))
            {
                c.Remove();
                c.Emit(OpCodes.Ldsfld, typeof(ArmorPiercingRounds).GetFieldCached(nameof(bossChampionDamageBonus)));
            }
            else
            {
                Logger.LogError("Failed to apply Armor Piercing Rounds Deletion hook");
            }
        }
    }
}
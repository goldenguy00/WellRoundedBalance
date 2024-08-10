using System;
using WellRoundedBalance.Buffs;
using WellRoundedBalance.Gamemodes.Eclipse;
using WellRoundedBalance.Mechanics.Health;

namespace WellRoundedBalance.Elites.Special
{
    internal class Perfected : EliteBase<Perfected>
    {
        [ConfigField("Projectile Damage", "Decimal.", 1f)]
        public static float projectileDamage;

        [ConfigField("Projectile Fire Interval", "", 9f)]
        public static float projectileFireInterval;

        [ConfigField("Projectile Fire Interval E3+", "Only applies if you have Eclipse Changes enabled.", 7.5f)]
        public static float projectileFireIntervalE3;

        [ConfigField("Delay Between Projectiles", "", 0.5f)]
        public static float delayBetweenProjectiles;

        [ConfigField("Death Lunar Coin Drop Chance", "", 20f)]
        public static float lunarCoinDropChance;

        [ConfigField("Spawn On Loop", "", true)]
        public static bool spawnOnLoop;

        public override string Name => ":: Elites :: Perfected";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.CharacterBody.UpdateAffixLunar += CharacterBody_UpdateAffixLunar;
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;

            Changes();
        }

        [SystemInitializer(typeof(EliteCatalog))]
        public static void ChangeTier()
        {
            if (spawnOnLoop)
            {
                var perfectedEliteTierDef = EliteAPI.VanillaEliteTiers.Where(x => x.eliteTypes.Contains(RoR2Content.Elites.Lunar)).First();
                perfectedEliteTierDef.isAvailable = (rules) => Run.instance.loopClearCount > 0 && (rules == SpawnCard.EliteRules.Default || rules == SpawnCard.EliteRules.Lunar);
            }
        }

        private void HealthComponent_TakeDamage(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld("RoR2.RoR2Content/Buffs", "AffixLunar")))
            {
                c.Remove();
                c.Emit<Useless>(OpCodes.Ldsfld, nameof(Useless.uselessBuff));
            }
            else
            {
                Main.WRBLogger.LogError("Failed to apply Perfected Cripple hook");
            }
        }

        private void CharacterBody_UpdateAffixLunar(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.3f)))
            {
                c.Next.Operand = projectileDamage;
            }
            else
            {
                Main.WRBLogger.LogError("Failed to apply Perfected Projectile Damage hook");
            }

            c.Index = 0;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.1f)))
            {
                c.Next.Operand = delayBetweenProjectiles;
            }
            else
            {
                Main.WRBLogger.LogError("Failed to apply Perfected Projectile Interval hook");
            }

            c.Index = 0;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(10f)))
            {
                c.Index++;
                c.EmitDelegate<Func<float, float>>((orig) =>
                {
                    orig = Eclipse3.CheckEclipse() ? projectileFireIntervalE3 : projectileFireInterval;
                    return orig;
                });
            }
        }

        private void Changes()
        {
            var projectile = Utils.Paths.GameObject.LunarMissileProjectile.Load<GameObject>();

            var projectileSimple = projectile.GetComponent<ProjectileSimple>();
            projectileSimple.enableVelocityOverLifetime = true;
            projectileSimple.velocityOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 0f), new Keyframe(0.1f + Mathf.Epsilon, 1f), new Keyframe(1f, 1f));
            projectileSimple.desiredForwardSpeed = 120f;
        }
    }
}
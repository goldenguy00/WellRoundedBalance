using System.Collections;
using WellRoundedBalance.Buffs;
using WellRoundedBalance.Elites.All;
using WellRoundedBalance.Elites.Special;
using WellRoundedBalance.Gamemodes.Eclipse;

namespace WellRoundedBalance.Elites.Tier1
{
    internal class Blazing : EliteBase<Blazing>
    {
        public override string Name => ":: Elites : Blazing";

        [ConfigField("Death Pool Projectile Count", "", 4)]
        public static int deathPoolProjectileCount;

        [ConfigField("Death Pool Projectile Count E3+", "Only applies if you have Eclipse Changes enabled.", 6)]
        public static int deathPoolProjectileCountE3;

        [ConfigField("Fire Pool Damage Per Second", "Decimal.", 1.75f)]
        public static float firePoolDamagePerSecond;

        public static BuffDef lessDamage;

        public override void Init()
        {
            base.Init();
            lessDamage = ScriptableObject.CreateInstance<BuffDef>();
            lessDamage.isHidden = true;
            lessDamage.isDebuff = false;
            lessDamage.canStack = false;
            lessDamage.isCooldown = false;

            ContentAddition.AddBuffDef(lessDamage);
        }

        public override void Hooks()
        {
            IL.RoR2.CharacterBody.UpdateFireTrail += CharacterBody_UpdateFireTrail1;

            DelegateStuff.addBuff += CharacterBody_AddBuff;
            DelegateStuff.removeBuff += CharacterBody_RemoveBuff;
        }

        private void CharacterBody_RemoveBuff(CharacterBody self, BuffIndex buffType)
        {
            if (NetworkServer.active && buffType == RoR2Content.Buffs.AffixRed.buffIndex)
            {
                self.gameObject.RemoveComponent<BlazingController>();
            }
        }

        private void CharacterBody_AddBuff(CharacterBody self, BuffIndex buffType)
        {
            if (NetworkServer.active && buffType == RoR2Content.Buffs.AffixRed.buffIndex && !self.GetComponent<BlazingController>())
            {
                self.gameObject.AddComponent<BlazingController>();
            }
        }

        private void CharacterBody_UpdateFireTrail1(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld("RoR2.RoR2Content/Buffs", "AffixRed")))
            {
                c.Remove();
                c.Emit<Useless>(OpCodes.Ldsfld, nameof(Useless.uselessBuff));
            }
            else
            {
                Logger.LogError("Failed to apply Blazing Elite Fire Trail Deletion hook");
            }
        }
    }

    public class BlazingController : MonoBehaviour
    {
        public static float delay = 1f;
        public static float initialDelay = 4f;
        public static int passiveProjectileCount = 2;
        public static float passiveProjectileInterval = 11f;
        public static float passiveDelayBetweenProjectiles = 0.5f;
        public static float deathDelayBetweenProjectiles = 0.2f;

        private int deathProjectileCount;
        private float deathProjectileAngle;
        private float passiveProjectileAngle;
        private CharacterBody body;
        private HealthComponent hc;

        public void Start()
        {
            body = GetComponent<CharacterBody>();
            hc = body.healthComponent;

            deathProjectileCount = Eclipse3.CheckEclipse() ? Blazing.deathPoolProjectileCountE3 : Blazing.deathPoolProjectileCount;
            deathProjectileAngle = 360f / deathProjectileCount;
            passiveProjectileAngle = 360f / passiveProjectileCount;

            StartCoroutine(FireProjectiles());
        }

        public IEnumerator FireProjectiles()
        {
            yield return new WaitForSeconds(initialDelay);
            while (body && hc && hc.alive)
            {
                var position = body.corePosition + Vector3.up * 10f;
                var rotation = Quaternion.identity;

                for (var i = 0; i < passiveProjectileCount; i++)
                {
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                    {
                        owner = body.gameObject,
                        damage = body.damage * Blazing.firePoolDamagePerSecond * 0.2f,
                        crit = false,
                        position = position,
                        rotation = rotation,
                        projectilePrefab = Projectiles.MolotovBig.singlePrefab,
                        damageTypeOverride = DamageType.IgniteOnHit
                    });

                    rotation *= Quaternion.Euler(0f, passiveProjectileAngle, 0f);

                    yield return new WaitForSeconds(passiveDelayBetweenProjectiles);
                }
                yield return new WaitForSeconds(passiveProjectileInterval);
            }
            StartCoroutine(FireDeathProjectiles());
        }

        public IEnumerator FireDeathProjectiles()
        {
            yield return new WaitForSeconds(delay);

            var position = body.corePosition + Vector3.up * 6f;
            var rotation = Quaternion.identity;

            for (var i = 0; i < deathProjectileCount; i++)
            {
                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    owner = body.gameObject,
                    damage = body.damage * Blazing.firePoolDamagePerSecond * 0.2f,
                    crit = false,
                    position = position,
                    rotation = rotation,
                    projectilePrefab = Projectiles.Molotov.singlePrefab,
                    damageTypeOverride = DamageType.IgniteOnHit
                });

                rotation *= Quaternion.Euler(0f, deathProjectileAngle, 0f);

                yield return new WaitForSeconds(deathDelayBetweenProjectiles);
            }

            if (NetworkServer.active)
            {
                Destroy(this);
            }
        }
    }
}
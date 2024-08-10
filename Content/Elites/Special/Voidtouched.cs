using Inferno.Stat_AI;
using WellRoundedBalance.Buffs;
using WellRoundedBalance.Elites.Tier1;

namespace WellRoundedBalance.Elites
{
    public class Voidtouched : EliteBase<Voidtouched>
    {
        public override string Name => ":: Elites :: Voidtouched";

        public static GameObject MortarSmallPrefab;
        public static GameObject MortarDeathPrefab;
        public static GameObject MortarGhost;
        public static GameObject LaserPrefab;
        public static GameObject CrabRave;
        public static GameObject HitEffect;

        [ConfigField("Minimum Mortar Count (Skill)", 3)]
        public static int MinMortarCountSkill;
        [ConfigField("Maximum Mortar Count (Skill)", 9)]
        public static int MaxMortarCountSkill;
        [ConfigField("Minimum Mortar Count (Death)", 6)]
        public static int MinMortarCountDeath;
        [ConfigField("Maximum Mortar Count (Death)", 12)]
        public static int MaxMortarCountDeath;

        public override void Init()
        {
            base.Init();
            CrabRave = Utils.Paths.GameObject.VoidRaidCrabSpinBeamVFX.Load<GameObject>().InstantiateClone("Voidtouched Crab Rave Vfx");
            CrabRave.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            CrabRave.transform.localScale *= 0.2f;

            foreach (var light in CrabRave.GetComponentsInChildren<Light>())
            {
                light.range *= 0.5f;
                light.intensity *= 0.5f;
            }

            HitEffect = Utils.Paths.GameObject.VoidRaidCrabMultiBeamDotZoneImpact.Load<GameObject>();

            LaserPrefab = PrefabAPI.InstantiateClone(new("Voidtouched Beam"), "Voidtouched Laser");

            LaserPrefab.AddComponent<ProjectileController>();
            LaserPrefab.AddComponent<ProjectileDamage>();
            LaserPrefab.AddComponent<VoidtouchedLaserBehaviour>();

            PrefabAPI.RegisterNetworkPrefab(LaserPrefab);

            MortarGhost = PrefabAPI.InstantiateClone(Utils.Paths.GameObject.ClayPotProjectileGhost.Load<GameObject>(), "Voidtouched Mortar Ghost", false);

            var mat1 = Utils.Paths.Material.matVoidBarnacleBulletOverlay.Load<Material>();
            var mat2 = Utils.Paths.Material.matVoidBarnacleBullet.Load<Material>();
            var mat3 = Utils.Paths.Material.matVoidSurvivorCorruptOverlay.Load<Material>();

            foreach (Renderer renderer in MortarGhost.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.sharedMaterials = [mat1, mat2, mat3];
            }

            var spiky = Utils.Paths.GameObject.mdlArtifactSpikyBall.Load<GameObject>().GetComponentInChildren<MeshFilter>().mesh;

            MortarGhost.GetComponentInChildren<MeshFilter>().mesh = spiky;

            MortarSmallPrefab = PrefabAPI.InstantiateClone(Utils.Paths.GameObject.ClayPotProjectile.Load<GameObject>(), "Voidtouched Mortar Small");

            var simple = MortarSmallPrefab.GetComponent<ProjectileSimple>();
            simple.desiredForwardSpeed = 90f;
            simple.lifetime = 25f;

            MortarSmallPrefab.GetComponent<Rigidbody>().mass = 400f;
            MortarSmallPrefab.GetComponent<SphereCollider>().material = Utils.Paths.PhysicMaterial.physmatVoidSurvivorCrabCannon.Load<PhysicMaterial>();
            MortarSmallPrefab.RemoveComponent<ApplyTorqueOnStart>();

            var controller = MortarSmallPrefab.GetComponent<ProjectileController>();
            controller.allowPrediction = false;
            controller.ghostPrefab = MortarGhost;

            var impact = MortarSmallPrefab.GetComponent<ProjectileImpactExplosion>();
            impact.blastDamageCoefficient = 1f;
            impact.blastRadius = 0.5f;
            impact.lifetime = 25f;
            impact.impactEffect = Utils.Paths.GameObject.VoidSurvivorMegaBlasterExplosionCorrupted.Load<GameObject>();

            var damage = MortarSmallPrefab.GetComponent<ProjectileDamage>();
            damage.damageType = DamageType.Nullify;

            MortarDeathPrefab = PrefabAPI.InstantiateClone(MortarSmallPrefab, "Voidtouched Mortar Death");
            MortarDeathPrefab.transform.localScale *= 2f;
            MortarDeathPrefab.layer = LayerIndex.debris.intVal;

            var impactDeath = MortarDeathPrefab.GetComponent<ProjectileImpactExplosion>();
            impactDeath.childrenCount = 1;
            impactDeath.childrenProjectilePrefab = LaserPrefab;
            impactDeath.childrenDamageCoefficient = 1f;
            impactDeath.fireChildren = true;

            impact.childrenCount = 3;
            impact.childrenDamageCoefficient = 0.5f;
            impact.childrenProjectilePrefab = Utils.Paths.GameObject.NullifierPreBombProjectile.Load<GameObject>();
            impact.fireChildren = true;

            MortarDeathPrefab.GetComponent<ProjectileSimple>().desiredForwardSpeed = 35f;

            ContentAddition.AddProjectile(MortarSmallPrefab);
            ContentAddition.AddProjectile(MortarDeathPrefab);
            ContentAddition.AddProjectile(LaserPrefab);
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;

            DelegateStuff.addBuff += CharacterBody_AddBuff;
            DelegateStuff.removeBuff += CharacterBody_RemoveBuff;
    }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld(typeof(DLC1Content.Buffs), "EliteVoid")))
            {
                c.Remove();
                c.Emit<Useless>(OpCodes.Ldsfld, nameof(Useless.uselessBuff));
            }
            else
            {
                Logger.LogError("Failed to apply Voidtouched Elite Needletick hook");
            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            var body = damageReport.victimBody;

            if (body && body.HasBuff(DLC1Content.Buffs.EliteVoid))
            {
                var count = Mathf.RoundToInt(Util.Remap(body.baseMaxHealth, 1f, 2500f, MinMortarCountDeath, MaxMortarCountDeath));

                for (var i = 0; i < count; i++)
                {
                    var rad = 2 * Mathf.PI / count * i;
                    var direction = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) + Vector3.up;

                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
                    {
                        owner = body.gameObject,
                        projectilePrefab = MortarDeathPrefab,
                        position = body.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(direction),
                        damage = body.damage,
                        damageColorIndex = DamageColorIndex.Void
                    });
                }
            }
        }


        private void CharacterBody_AddBuff(CharacterBody self, BuffIndex buffType)
        {
            if (buffType == DLC1Content.Buffs.EliteVoid.buffIndex)
            {
                self.onSkillActivatedAuthority += Self_onSkillActivatedAuthority;
            }
        }

        private void CharacterBody_RemoveBuff(CharacterBody self, BuffIndex buffType)
        {
            if (buffType == DLC1Content.Buffs.EliteVoid.buffIndex)
            {
                self.onSkillActivatedAuthority -= Self_onSkillActivatedAuthority;
            }
        }

        private void Self_onSkillActivatedAuthority(GenericSkill skill)
        {
            if ((skill.baseRechargeInterval == 0f || skill.skillDef.stockToConsume == 0) && Util.CheckRoll(35f))
            {
                var count = Mathf.RoundToInt(Util.Remap(skill.baseRechargeInterval, 2f, 14f, MinMortarCountSkill, MaxMortarCountSkill));
                var body = skill.characterBody;

                for (var i = 0; i < count; i++)
                {
                    var rotation = Util.ApplySpread(body.inputBank.aimDirection, -25f, 25f, 1f, 1f);
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
                    {
                        owner = body.gameObject,
                        projectilePrefab = MortarSmallPrefab,
                        position = body.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(rotation),
                        damage = body.damage,
                        damageColorIndex = DamageColorIndex.Void,
                        speedOverride = 90f * UnityEngine.Random.Range(0.5f, 1.5f)
                    });
                }
            }
        }

        public class VoidtouchedLaserBehaviour : MonoBehaviour
        {
            public ProjectileController controller;
            public ProjectileDamage damage;
            public float stopwatch;
            public float damageStopwatch;
            public GameObject laserInstance;
            public float destructionStopwatch = 0f;
            public float bulletScale = 2f;
            public Vector3 scale = Vector3.zero;
            public bool markedForDestruction = false;
            public Vector3 scaleSubtrPerSec;
            public float scale2SubtrPerSec;
            public float y;

            public void Start()
            {
                controller = GetComponent<ProjectileController>();
                damage = GetComponent<ProjectileDamage>();

                laserInstance = GameObject.Instantiate(CrabRave, transform.position - new Vector3(0, 5f, 0), Quaternion.identity);
                scale = laserInstance.transform.localScale;
                y = scale.z;
                scale2SubtrPerSec = 2f;
                scaleSubtrPerSec = scale;
            }

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                damageStopwatch += Time.fixedDeltaTime;

                if (destructionStopwatch >= 0f)
                {
                    destructionStopwatch -= Time.fixedDeltaTime;
                    scale -= scaleSubtrPerSec * Time.fixedDeltaTime;
                    bulletScale -= scale2SubtrPerSec * Time.fixedDeltaTime;
                    laserInstance.transform.localScale = new(scale.x, scale.y, y);
                }

                if (destructionStopwatch <= 0f && markedForDestruction)
                {
                    Destroy(laserInstance);
                    Destroy(this.gameObject);
                }

                if (stopwatch >= 3f && !markedForDestruction)
                {
                    destructionStopwatch = 1f;
                    markedForDestruction = true;
                    return;
                }

                if (damageStopwatch >= 0.2f)
                {
                    damageStopwatch = 0f;

                    new BulletAttack()
                    {
                        owner = controller.owner,
                        weapon = gameObject,
                        origin = transform.position + new Vector3(0, 200f, 0),
                        aimVector = Vector3.down,
                        minSpread = 0f,
                        maxSpread = 0f,
                        damage = damage.damage * 5f * 0.2f,
                        force = 0f,
                        hitEffectPrefab = HitEffect,
                        isCrit = false,
                        procChainMask = default,
                        procCoefficient = 0f,
                        maxDistance = 200f,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.None,
                        stopperMask = LayerIndex.world.mask,
                        radius = bulletScale
                    }.Fire();
                }
            }

            public void OnDestroy()
            {
                if (laserInstance)
                    Destroy(laserInstance);
            }
        }
    }
}
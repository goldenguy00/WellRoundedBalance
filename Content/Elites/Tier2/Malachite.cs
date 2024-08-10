using WellRoundedBalance.Gamemodes.Eclipse;

namespace WellRoundedBalance.Elites.Tier2
{
    public class Malachite : EliteBase<Malachite>
    {
        public override string Name => ":: Elites ::: Malachite";

        [ConfigField("Turret Count", "", 2)]
        public static int TurretCount;

        [ConfigField("Turret Count Eclipse 3+", "Only applies if you have Eclipse Changes enabled.", 4)]
        public static int turretCountE3;

        [ConfigField("Safe Zone Radius", "", 50f)]
        public static float SafeZoneRadius;

        [ConfigField("Healing Multiplier", "", 0.5f)]
        public static float HealingMultiplier;

        internal static GameObject MalachiteTurret;
        internal static GameObject MalachiteDebuffZone;

        public override void Hooks()
        {
            On.RoR2.CharacterBody.UpdateAffixPoison += (orig, self, delta) =>
            {
                if (self.HasBuff(RoR2Content.Buffs.AffixPoison) && !self.GetComponent<TurretSpawner>())
                {
                    self.gameObject.AddComponent<TurretSpawner>();
                }

                if (!self.HasBuff(RoR2Content.Buffs.AffixPoison) && self.GetComponent<TurretSpawner>())
                {
                    self.gameObject.RemoveComponent<TurretSpawner>();
                }
            };

            MalachiteTurret = PrefabAPI.InstantiateClone(new("e"), "MalachiteTurret", false);
            MalachiteTurret.AddComponent<NetworkIdentity>();
            MalachiteTurret.AddComponent<TurretController>();
            var rb = MalachiteTurret.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 300;
            var turretMdl = Object.Instantiate(Utils.Paths.GameObject.mdlUrchinTurret.Load<GameObject>());
            turretMdl.transform.SetParent(MalachiteTurret.transform);
            turretMdl.transform.localScale *= 0.4f;
            turretMdl.transform.rotation = Quaternion.Euler(-90, 0, 0);
            var srenderer = turretMdl.GetComponentInChildren<SkinnedMeshRenderer>();
            var mrenderer = turretMdl.GetComponentInChildren<MeshRenderer>();
            if (srenderer)
            {
                srenderer.material = Utils.Paths.Material.matEliteUrchinCrown.Load<Material>();
            }
            if (mrenderer)
            {
                mrenderer.material = Utils.Paths.Material.matEliteUrchinCrown.Load<Material>();
            }
            MalachiteTurret.RegisterNetworkPrefab();

            MalachiteDebuffZone = Utils.Paths.GameObject.RailgunnerMineAltDetonated.Load<GameObject>().InstantiateClone("AntihealZone");
            var areaIndicator = MalachiteDebuffZone.transform.Find("AreaIndicator");
            var softGlow = areaIndicator.Find("SoftGlow");
            var sphere = areaIndicator.Find("Sphere");
            var light = areaIndicator.Find("Point Light");
            var core = areaIndicator.Find("Core");

            softGlow.gameObject.SetActive(false);
            light.gameObject.SetActive(false);
            core.gameObject.SetActive(false);

            var renderer = sphere.GetComponent<MeshRenderer>();
            var mats = renderer.sharedMaterials;
            mats[0] = Utils.Paths.Material.matElitePoisonOverlay.Load<Material>();
            mats[1] = Utils.Paths.Material.matElitePoisonAreaIndicator.Load<Material>();
            renderer.SetSharedMaterials(mats, 2);

            MalachiteDebuffZone.RemoveComponent<BuffWard>();

            var zone = MalachiteDebuffZone.AddComponent<SphereZone>();
            zone.rangeIndicator = areaIndicator;
            zone.isInverted = true;
            zone.radius = 30;

            MalachiteDebuffZone.RemoveComponent<SlowDownProjectiles>();

            MalachiteDebuffZone.AddComponent<ZoneController>();
        }

        internal class TurretSpawner : MonoBehaviour
        {
            private List<TurretController> activeTurrets = [];
            private float startTime;
            private CharacterBody body;
            private GameObject zoneInstance;
            private int turretCount;

            private void Start()
            {
                body = GetComponent<CharacterBody>();
                startTime = Run.instance.GetRunStopwatch();
                var e3 = Run.instance && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse3 && Eclipse3.instance.isEnabled;
                turretCount = e3 ? turretCountE3 : TurretCount;

                for (var i = 0; i < turretCount; i++)
                {
                    var turret = Instantiate(MalachiteTurret);
                    var controller = turret.GetComponent<TurretController>();
                    controller.owner = body;
                    activeTurrets.Add(controller);
                    NetworkServer.Spawn(turret);
                }

                zoneInstance = Instantiate(MalachiteDebuffZone, transform);
                NetworkServer.Spawn(zoneInstance);
            }

            private void FixedUpdate()
            {
                for (var i = 0; i < turretCount; i++)
                {
                    if (!activeTurrets[i].rb)
                    {
                        continue;
                    }

                    var elapsed = Run.instance.GetRunStopwatch() - startTime;
                    var plane1 = Vector3.up;
                    var plane2 = Vector3.forward;

                    var targetPosition = body.footPosition + new Vector3(0, 2, 0) + Quaternion.AngleAxis(360 / turretCount * i + elapsed / 10 * 360, plane1) * plane2 * 3;
                    var vel = body.isSprinting ? body.moveSpeed * body.sprintingSpeedMultiplier * 1.35f : body.moveSpeed * 1.35f;

                    var currentPos = activeTurrets[i].rb.position;
                    var lerpedPosition = Vector3.Lerp(currentPos, targetPosition, vel * Time.fixedDeltaTime);

                    activeTurrets[i].rb.MovePosition(lerpedPosition);
                }
            }

            private void OnDestroy()
            {
                for (var i = 0; i < activeTurrets.Count; i++)
                {
                    activeTurrets[i].Suicide();
                }

                if (zoneInstance)
                {
                    Destroy(zoneInstance);
                }
            }

            private void OnDisable()
            {
                for (var i = 0; i < activeTurrets.Count; i++)
                {
                    activeTurrets[i].Suicide();
                }

                if (zoneInstance)
                {
                    Destroy(zoneInstance);
                }
            }
        }

        internal class TurretController : MonoBehaviour
        {
            public HurtBox target;
            internal CharacterBody owner;
            internal Rigidbody rb;
            private float stopwatch = 0f;
            private float delay = 1.2f;

            private void Start()
            {
                rb = GetComponent<Rigidbody>();
            }

            private void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= delay)
                {
                    stopwatch = 0f;
                    RefreshTarget();
                    if (target)
                    {
                        var aim = (target.transform.position - transform.position).normalized;
                        if (Util.HasEffectiveAuthority(gameObject))
                        {
                            FireProjectileInfo info = new()
                            {
                                damage = owner.damage,
                                position = transform.position,
                                rotation = Util.QuaternionSafeLookRotation(aim),
                                owner = owner.gameObject,
                                projectilePrefab = Utils.Paths.GameObject.UrchinSeekingProjectile.Load<GameObject>()
                            };

                            ProjectileManager.instance.FireProjectile(info);
                        }

                        AkSoundEngine.PostEvent(Events.Play_elite_antiHeal_turret_shot, gameObject);
                    }
                }
            }

            private void RefreshTarget()
            {
                SphereSearch search = new();
                search.radius = 30f;
                search.origin = owner.footPosition;
                search.mask = LayerIndex.entityPrecise.mask;
                search.RefreshCandidates();
                search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(owner.teamComponent.teamIndex));
                search.OrderCandidatesByDistance();
                target = search.GetHurtBoxes().FirstOrDefault();
            }

            internal void Suicide()
            {
                if (gameObject) Destroy(gameObject);
            }
        }

        internal class ZoneController : MonoBehaviour
        {
            private SphereZone zone;

            private void Start()
            {
                zone = GetComponent<SphereZone>();
                On.RoR2.HealthComponent.Heal += Reduce;
            }

            private float Reduce(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask mask, bool regen)
            {
                if (zone && zone.IsInBounds(self.body.footPosition))
                {
                    amount *= HealingMultiplier;
                }
                return orig(self, amount, mask, regen);
            }

            private void OnDestroy()
            {
                On.RoR2.HealthComponent.Heal -= Reduce;
            }
        }
    }
}
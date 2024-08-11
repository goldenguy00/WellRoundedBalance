using System.Collections;
using WellRoundedBalance.Buffs;
using WellRoundedBalance.Gamemodes.Eclipse;

namespace WellRoundedBalance.Elites.Tier1
{
    internal class Overloading : EliteBase<Overloading>
    {
        public static BuffDef overloadingSpeedBuff;
        public override string Name => ":: Elites : Overloading";

        [ConfigField("Passive Movement Speed Gain", "Decimal.", 0.5f)]
        public static float passiveMovementSpeedGain;

        [ConfigField("Ally Buff Movement Speed Gain", "Decimal.", 0.5f)]
        public static float allyBuffMovementSpeedGain;

        [ConfigField("Ally Buff Movement Speed Gain Eclipse 3+", "Decimal. Only applies if you have Eclipse Changes enabled.", 0.75f)]
        public static float allyBuffMovementSpeedGainE3;

        [ConfigField("Teleport Cooldown", "", 6f)]
        public static float teleportCooldown;

        [ConfigField("Maximum Speed Aura Radius", "", 45f)]
        public static float maxSpeedAuraRadius;

        [ConfigField("Minimum Speed Aura Radius", "", 20f)]
        public static float minSpeedAuraRadius;

        private static GameObject SpeedAura;

        public static GameObject tpEffect;
        public static GameObject tpTracer;

        public static BuffDef overloadingSelfBuff;

        public override void Init()
        {
            var speedBuff = Utils.Paths.Texture2D.texBuffKillMoveSpeed.Load<Texture2D>();

            overloadingSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            overloadingSpeedBuff.isHidden = false;
            overloadingSpeedBuff.isDebuff = false;
            overloadingSpeedBuff.canStack = false;
            overloadingSpeedBuff.buffColor = new Color32(66, 98, 219, 255);
            overloadingSpeedBuff.iconSprite = Sprite.Create(speedBuff, new Rect(0f, 0f, speedBuff.width, speedBuff.height), new Vector2(0f, 0f));
            overloadingSpeedBuff.name = "Overloading Ally Speed Buff";

            ContentAddition.AddBuffDef(overloadingSpeedBuff);

            overloadingSelfBuff = ScriptableObject.CreateInstance<BuffDef>();
            overloadingSelfBuff.isHidden = true;
            overloadingSelfBuff.isDebuff = false;
            overloadingSelfBuff.canStack = false;
            overloadingSelfBuff.name = "Overloading Self Speed Buff";

            ContentAddition.AddBuffDef(overloadingSelfBuff);

            SpeedAura = Utils.Paths.GameObject.RailgunnerMineAltDetonated.Load<GameObject>().InstantiateClone("OverloadingSpeedAura");
            SpeedAura.RemoveComponent<SlowDownProjectiles>();
            var areaIndicator = SpeedAura.transform.Find("AreaIndicator");
            var softGlow = areaIndicator.Find("SoftGlow");
            var sphere = areaIndicator.Find("Sphere");
            var light = areaIndicator.Find("Point Light");
            var core = areaIndicator.Find("Core");

            softGlow.gameObject.SetActive(false);
            light.gameObject.SetActive(false);
            core.gameObject.SetActive(false);

            var renderer = sphere.GetComponent<MeshRenderer>();
            var mats = renderer.sharedMaterials;
            mats[0] = Utils.Paths.Material.matMoonbatteryCrippleRadius.Load<Material>();
            mats[1] = Utils.Paths.Material.matCrippleSphereIndicator.Load<Material>();
            renderer.SetSharedMaterials(mats, 2);

            var buffWard = SpeedAura.GetComponent<BuffWard>();
            buffWard.buffDef = overloadingSpeedBuff;
            buffWard.expires = false;
            buffWard.expireDuration = 10000;
            buffWard.invertTeamFilter = false;
            buffWard.buffDuration = 6f;

            var teamFilter = SpeedAura.AddComponent<TeamFilter>();
            teamFilter.defaultTeam = TeamIndex.None;

            SpeedAura.RegisterNetworkPrefab();

            tpEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Parent/ParentTeleportEffect.prefab").WaitForCompletion().InstantiateClone("LunarConstructTeleport", false);
            var particles = tpEffect.transform.GetChild(0);
            var ringParticle = particles.GetChild(0).GetComponent<ParticleSystemRenderer>();

            var moonRamp = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLunarWispFire.png").WaitForCompletion();

            var newRing = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Parent/matParentTeleportPortal.mat").WaitForCompletion());
            newRing.SetTexture("_RemapTex", moonRamp);

            ringParticle.sharedMaterial = newRing;

            particles.GetChild(1).gameObject.SetActive(false);

            var energyInitialParticle = particles.GetChild(3).GetComponent<ParticleSystemRenderer>();
            energyInitialParticle.sharedMaterial = newRing;
            energyInitialParticle.gameObject.transform.localScale = Vector3.one * 0.25f;

            var eps = particles.GetChild(3).GetComponent<ParticleSystem>().main;
            eps.duration = 0.17f;

            particles.GetChild(4).gameObject.SetActive(false);

            tpTracer = Utils.Paths.GameObject.VoidSurvivorBeamTracer.Load<GameObject>().InstantiateClone("OverloadingTracer", false);
            tpTracer.transform.GetChild(0).gameObject.SetActive(false);
            tpTracer.transform.GetChild(1).gameObject.SetActive(false);

            var lineRenderer = tpTracer.GetComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.33f;
            lineRenderer.numCapVertices = 10;

            var newMat = Object.Instantiate(Utils.Paths.Material.matVoidSurvivorBeamTrail.Load<Material>());
            newMat.SetTexture("_RemapTex", Utils.Paths.Texture2D.texRampLunarWardDecal.Load<Texture2D>());

            lineRenderer.material = newMat;

            var animateShaderAlpha = tpTracer.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 0.4f;

            ContentAddition.AddEffect(tpTracer);
            ContentAddition.AddEffect(tpEffect);

            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.GlobalEventManager.OnHitAll += GlobalEventManager_OnHitAll;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            DelegateStuff.addBuff += CharacterBody_AddBuff;
            DelegateStuff.removeBuff += CharacterBody_RemoveBuff;
        }

        private void CharacterBody_RemoveBuff(CharacterBody self, BuffIndex buffType)
        {
            if (NetworkServer.active && buffType == RoR2Content.Buffs.AffixBlue.buffIndex && !self.isPlayerControlled)
            {
                self.gameObject.RemoveComponent<OverloadingController>();
            }
        }

        private void CharacterBody_AddBuff(CharacterBody self, BuffIndex buffType)
        {
            if (NetworkServer.active && buffType == RoR2Content.Buffs.AffixBlue.buffIndex && !self.GetComponent<OverloadingController>() && !self.isPlayerControlled)
            {
                self.gameObject.AddComponent<OverloadingController>();
            }
        }

        private void GlobalEventManager_OnHitAll(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixBlue")))
            {
                c.Remove();
                c.Emit<Useless>(OpCodes.Ldsfld, nameof(Useless.uselessBuff));
            }
            else
            {
                Logger.LogError("Failed to apply Overloading Deletion 2 hook");
            }
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixBlue")))
            {
                c.Remove();
                c.Emit<Useless>(OpCodes.Ldsfld, nameof(Useless.uselessBuff));
            }
            else
            {
                Logger.LogError("Failed to apply Overloading Deletion 1 hook");
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var e3 = Run.instance && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse3 && Eclipse3.instance.isEnabled;
            if (sender)
            {
                if (sender.HasBuff(overloadingSpeedBuff) && !sender.HasBuff(RoR2Content.Buffs.AffixBlue))
                    args.moveSpeedMultAdd += e3 ? allyBuffMovementSpeedGainE3 : allyBuffMovementSpeedGain;
                if (sender.HasBuff(overloadingSelfBuff))
                {
                    args.moveSpeedMultAdd += passiveMovementSpeedGain;
                }
            }
        }

        private class OverloadingController : MonoBehaviour
        {
            public HealthComponent hc;
            public CharacterBody cb;
            public GameObject wardInstance;
            public float stopwatch = 0f;
            public Vector3 currentPosition;

            public void Start()
            {
                hc = GetComponent<HealthComponent>();
                cb = hc.body;
                
                StartCoroutine(Teleport());
            }

            public void OnDestroy()
            {
                if (wardInstance)
                {
                    Destroy(wardInstance);
                }
            }

            public IEnumerator Teleport()
            {
                yield return new WaitForSeconds(teleportCooldown);

                while (cb && hc && hc.alive)
                {
                    currentPosition = transform.position;

                    EffectManager.SpawnEffect(tpEffect, new EffectData
                    {
                        scale = 0.66f,
                        origin = currentPosition
                    }, true);

                    yield return new WaitForSeconds(0.33f);
                    if (!cb || !hc || !hc.alive)
                        break;

                    HandleTeleport();

                    yield return new WaitForSeconds(0.33f);
                    if (!cb || !hc || !hc.alive)
                        break;

                    EffectManager.SpawnEffect(tpEffect, new EffectData
                    {
                        scale = 0.66f,
                        origin = transform.position
                    }, true);

                    yield return new WaitForSeconds(teleportCooldown);
                }
            }

            public void HandleTeleport()
            {
                var nextPosition = PickTeleportPosition();
                nextPosition.y += 1;

                if (wardInstance != null)
                {
                    NetworkServer.Destroy(wardInstance);
                }

                wardInstance = Instantiate(SpeedAura, nextPosition, Quaternion.identity);
                wardInstance.GetComponent<BuffWard>().Networkradius = Util.Remap(cb.baseMaxHealth, 0f, 2100f, minSpeedAuraRadius, maxSpeedAuraRadius);
                wardInstance.GetComponent<TeamFilter>().teamIndex = cb.teamComponent.teamIndex;
                NetworkServer.Spawn(wardInstance);

                EffectManager.SpawnEffect(tpTracer, new EffectData
                {
                    start = currentPosition,
                    origin = nextPosition
                }, true);

                TeleportHelper.TeleportBody(cb, nextPosition);
            }

            public Vector3 PickTeleportPosition()
            {
                if (!SceneInfo.instance || !SceneInfo.instance.groundNodes)
                    return currentPosition;

                var nodes = SceneInfo.instance.groundNodes.nodes;

                List<Vector3> validPositions = [];
                foreach (var node in nodes)
                {
                    var distance = Vector3.SqrMagnitude(currentPosition - node.position);
                    if (distance is > 100 and < 900)
                    {
                        validPositions.Add(node.position);
                    }
                }

                if (validPositions.Any())
                    return validPositions.GetRandom();
                return currentPosition;
            }
        }
    }
}
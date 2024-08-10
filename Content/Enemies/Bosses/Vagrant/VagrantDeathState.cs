using System;
using System.Collections.Generic;
using System.Text;
using EntityStates.VagrantMonster;

namespace WellRoundedBalance.Enemies.Bosses.Vagrant
{
    public class VagrantDeathState : GenericCharacterDeath
    {
        private float duration;

        private GameObject chargeEffectInstance;
        private GameObject areaIndicatorInstance;

        private uint soundID;

        public override bool shouldAutoDestroy => false;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = ChargeMegaNova.baseDuration;
        }

        public override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
        {
            base.PlayCrossfade("Gesture, Override", "ChargeMegaNova", "ChargeMegaNova.playbackRate", this.duration, 0.3f);
        }

        public override void PlayDeathSound()
        {
            this.soundID = Util.PlayAttackSpeedSound(ChargeMegaNova.chargingSoundString, base.gameObject, base.attackSpeedStat);
        }

        public override void CreateDeathEffects()
        {
            var modelTransform = base.GetModelTransform();
            if (!modelTransform)
            {
                return;
            }

            var component = modelTransform.GetComponent<ChildLocator>();
            if (component)
            {
                var transform = component.FindChild("HullCenter");
                var transform2 = component.FindChild("NovaCenter");
                if (transform && ChargeMegaNova.chargingEffectPrefab)
                {
                    this.chargeEffectInstance = UnityEngine.Object.Instantiate(ChargeMegaNova.chargingEffectPrefab, transform.position, transform.rotation);
                    this.chargeEffectInstance.transform.localScale = new Vector3(ChargeMegaNova.novaRadius, ChargeMegaNova.novaRadius, ChargeMegaNova.novaRadius);
                    this.chargeEffectInstance.transform.parent = transform;
                    this.chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = this.duration;
                }
                if (transform2 && ChargeMegaNova.areaIndicatorPrefab)
                {
                    this.areaIndicatorInstance = UnityEngine.Object.Instantiate(ChargeMegaNova.areaIndicatorPrefab, transform2.position, transform2.rotation);
                    this.areaIndicatorInstance.transform.localScale = new Vector3(ChargeMegaNova.novaRadius * 2f, ChargeMegaNova.novaRadius * 2f, ChargeMegaNova.novaRadius * 2f);
                    this.areaIndicatorInstance.transform.parent = transform2;
                }
            }
        }

        private void Detonate()
        {
            var position = base.transform.position;
            Util.PlaySound(FireMegaNova.novaSoundString, base.gameObject);
            if (FireMegaNova.novaEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireMegaNova.novaEffectPrefab, base.gameObject, "NovaCenter", transmit: false);
            }
            var modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                var temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 3f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matVagrantEnergized");
                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
            if (NetworkServer.active)
            {
                new BlastAttack
                {
                    attacker = base.gameObject,
                    baseDamage = base.damageStat * FireMegaNova.novaDamageCoefficient * 4f,
                    baseForce = FireMegaNova.novaForce,
                    bonusForce = Vector3.zero,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    crit = base.characterBody.RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    inflictor = base.gameObject,
                    position = position,
                    procChainMask = default,
                    procCoefficient = 3f,
                    radius = ChargeMegaNova.novaRadius,
                    losType = BlastAttack.LoSType.NearestHit,
                    teamIndex = base.teamComponent.teamIndex,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(FireMegaNova.novaImpactEffectPrefab)
                }.Fire();
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration)
            {
                Detonate();
                base.DestroyModel();
                if (NetworkServer.active)
                {
                    base.DestroyBodyAsapServer();
                }
            }
        }
        public override void OnPreDestroyBodyServer()
        {
            if (this.chargeEffectInstance)
                EntityState.Destroy(this.chargeEffectInstance);
            if (this.areaIndicatorInstance)
                EntityState.Destroy(this.areaIndicatorInstance);
            if (base.isAuthority && DeathState.initialExplosion)
                EffectManager.SimpleImpactEffect(DeathState.initialExplosion, base.transform.position, Vector3.up, transmit: true);

        }
        public override void OnExit()
        {
            base.DestroyModel();
            if (base.modelLocator && base.modelLocator.modelBaseTransform)
                EntityState.Destroy(base.modelLocator.modelBaseTransform.gameObject);
            Util.PlaySound(DeathState.deathString, base.gameObject);
            AkSoundEngine.StopPlayingID(this.soundID);

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

}

namespace WellRoundedBalance.Enemies.Bosses.Vagrant
{
    public class ChainDashes : BaseSkillState
    {
        public int totalDashes;
        public int dashCount;
        public float dashDelay;
        public float dashDamage = 4f;
        public float dashForce = 50f;

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.SetAimTimer(0.2f);
            dashDelay = 0;
            totalDashes = base.healthComponent.isHealthLow ? 5 : 3;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!base.isAuthority)
            {
                return;
            }
            dashDelay -= Time.fixedDeltaTime;
            if (dashDelay <= 0f)
            {
                dashCount++;
                dashDelay = 4f / base.attackSpeedStat;
                base.characterBody.SetAimTimer(0.1f);
                base.rigidbodyDirection.aimDirection = base.inputBank.aimDirection;
                dashForce *= -1;
                base.rigidbodyMotor.ApplyForceImpulse(new PhysForceInfo()
                {
                    force = Vector3.Cross(Vector3.up, base.inputBank.aimDirection) * dashForce,
                    disableAirControlUntilCollision = false,
                    ignoreGroundStick = true,
                    massIsOne = true
                });

                for (var i = 0; i < totalDashes * 2; i++)
                {
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
                    {
                        owner = base.gameObject,
                        position = base.transform.position,
                        crit = base.RollCrit(),
                        rotation = Quaternion.LookRotation(UnityEngine.Random.onUnitSphere),
                        damage = base.damageStat * 1.5f,
                        projectilePrefab = WanderingVagrant.VagrantSeekerOrb
                    });
                }
                AkSoundEngine.PostEvent(Events.Play_vagrant_attack1_shoot, base.gameObject);
            }

            if (dashCount >= totalDashes)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
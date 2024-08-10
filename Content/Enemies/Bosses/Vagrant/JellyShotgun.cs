using System;
using EntityStates.VagrantMonster.Weapon;
using WellRoundedBalance.Gamemodes.Eclipse;

namespace WellRoundedBalance.Enemies.Bosses.Vagrant
{
    public class JellyShotgun : JellyBarrage
    {
        public static float BaseDuration = 2f;
        public static int BaseOrbs = 6;
        public static float BaseDamageCoefficient = 2.5f;
        public static float BaseOrbSpread = 4.5f;
        public int shotsFired;

        public static void ModifyBase()
        {
            JellyBarrage.maxSpread = BaseOrbSpread;
            JellyBarrage.damageCoefficient = BaseDamageCoefficient;
            JellyBarrage.baseDuration = BaseDuration;
            JellyBarrage.missileSpawnFrequency = 1f / Time.fixedDeltaTime;
            On.EntityStates.VagrantMonster.Weapon.JellyBarrage.FireBlob += (orig, self, v0, v1, v2) => 
            {
                if (self is JellyShotgun self2)
                {
                    self2.shotsFired++;
                    v0.origin += UnityEngine.Random.onUnitSphere * 6f;
                }
                PredictionUtils.PredictAimrayNew(v0, self.characterBody, JellyBarrage.projectilePrefab);
                orig(self, v0, v1, v2);
            };
        }

        public override void OnEnter()
        {
            base.OnEnter();
            shotsFired = 0;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (shotsFired >= BaseOrbs && missileStopwatch > 0)
            {
                missileStopwatch = -1f * baseDuration;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
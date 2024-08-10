﻿using RoR2.Skills;
using System;
using WellRoundedBalance.Enemies.Minibosses;

namespace WellRoundedBalance.Enemies.Standard
{
    internal class Geep : EnemyBase<Geep>
    {
        public override string Name => ":: Enemies :: Geep";

        [ConfigField("Base Max Health", "Disabled if playing Inferno.", 250f)]
        public static float baseMaxHealth;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
            Changes();
        }

        private void CharacterMaster_onStartGlobal(CharacterMaster master)
        {
            if (master.name is "GeepMaster(Clone)" && !Main.IsInfernoDef())
            {
                var spike = master.GetComponents<AISkillDriver>().First(ai => ai.customName == "Spike");
                spike.maxDistance = 45f;
            }
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (body.name is "GeepBody(Clone)" && !Main.IsInfernoDef())
            {
                body.baseMoveSpeed = 24f;
                if (!body.GetComponent<GupSpikesController>())
                    body.gameObject.AddComponent<GupSpikesController>();
            }
        }

        private void Changes()
        {
            var geep = Utils.Paths.GameObject.GeepBody12.Load<GameObject>();

            var geepBody = geep.GetComponent<CharacterBody>();
            geepBody.baseMaxHealth = baseMaxHealth;
            geepBody.levelMaxHealth = baseMaxHealth * 0.3f;

            var modelTransform = geep.transform.GetChild(0).GetChild(0);
            var spikes = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Spikes");
            var mainHitbox = spikes.hitBoxes[0].gameObject;
            mainHitbox.transform.localScale = new Vector3(6f, 6f, 2.5f);
            /*
            var hitboxGroup = modelTransform.GetComponent<HitBoxGroup>();
            hitboxGroup.hitBoxes[0] = spikes.hitBoxes[1];
            var newHitbox = hitboxGroup.hitBoxes[0];
            newHitbox.transform.localScale = new Vector3(2f, 2f, 1.4f);

            var contactDamage = geep.AddComponent<ContactDamage>();
            contactDamage.pushForcePerSecond = 500f;
            contactDamage.damagePerSecondCoefficient = 0.7f;
            contactDamage.hitBoxGroup = hitboxGroup;
            */

            var sd = Utils.Paths.SkillDef.GupSpikes.Load<SkillDef>();
            sd.baseRechargeInterval = 1.5f;
        }
    }
}
﻿using Inferno.Stat_AI;
using System;

namespace WellRoundedBalance.Enemies.Minibosses
{
    internal class Gup : EnemyBase<Gup>
    {
        public override string Name => ":: Minibosses :: Gup";

        [ConfigField("Base Max Health", "Disabled if playing Inferno.", 500f)]
        public static float baseMaxHealth;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.EntityStates.Gup.GupSpikesState.OnEnter += GupSpikesState_OnEnter;
            Changes();
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (Main.IsInfernoDef())
            {
                return;
            }
            switch (body.name)
            {
                case "GupBody(Clone)":
                    body.baseMoveSpeed = 19f;
                    break;
            }
        }

        private void GupSpikesState_OnEnter(On.EntityStates.Gup.GupSpikesState.orig_OnEnter orig, EntityStates.Gup.GupSpikesState self)
        {
            if (!Main.IsInfernoDef())
            {
                self.pushAwayForce = 3500f;
                self.damageCoefficient = 3.5f;
            }

            orig(self);
        }

        private void CharacterMaster_onStartGlobal(CharacterMaster master)
        {
            if (Main.IsInfernoDef())
            {
                return;
            }
            switch (master.name)
            {
                case "GupMaster(Clone)":
                    AISkillDriver spike = (from x in master.GetComponents<AISkillDriver>()
                                           where x.customName == "Spike"
                                           select x).First();
                    spike.maxDistance = 9.5f;
                    break;
            }
        }

        private void Changes()
        {
            var gup = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Gup/GupBody.prefab").WaitForCompletion();

            var gupBody = gup.GetComponent<CharacterBody>();
            gupBody.baseMaxHealth = baseMaxHealth;
            gupBody.levelMaxHealth = baseMaxHealth * 0.3f;

            var modelTransform = gup.transform.GetChild(0).GetChild(0);
            var spikes = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Spikes").hitBoxes[0].gameObject;
            spikes.transform.localScale = new Vector3(4f, 4f, 1.7f);
        }
    }
}
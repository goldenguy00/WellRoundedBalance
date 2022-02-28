﻿using EntityStates;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System.Linq;
using UnityEngine;

namespace UltimateCustomRun.Enemies.Bosses
{
    public class Grandparent : EnemyBase
    {
        public static bool AITweaks;
        public static bool SpeedTweaks;
        public static bool MovementTweaks;
        public static bool RockTweaks;
        public static bool NewSkill;
        public static AISkillDriver dr;
        public override string Name => ":::: Enemies ::: Grandparent";

        public override void Init()
        {
            AITweaks = ConfigOption(false, "Make Grandparent AI smarter?", "Vanilla is false.\nRecommended Value: True");
            SpeedTweaks = ConfigOption(false, "Make Grandparent faster?", "Vanilla is false.\nRecommended Value: True");
            MovementTweaks = ConfigOption(false, "Make Grandparent move?", "Vanilla is false.\nRecommended Value: True");
            RockTweaks = ConfigOption(false, "Make Rock Throw Better?", "Vanilla is false.\nRecommended Value: True");
            NewSkill = ConfigOption(false, "Add a New Skil to Grandparent?", "Vanilla is false.\nRecommended Value: True");
            base.Init();
        }

        public override void Hooks()
        {
            Buff();
        }

        public static void Buff()
        {
            var master = Resources.Load<CharacterMaster>("prefabs/charactermasters/GrandparentMaster").GetComponent<CharacterMaster>();
            GameObject masterbase = Resources.Load<GameObject>("prefabs/charactermasters/GrandparentMaster");
            var body = Resources.Load<CharacterBody>("prefabs/characterbodies/GrandparentBody").GetComponent<CharacterBody>();
            var bodybase = Resources.Load<GameObject>("prefabs/characterbodies/GrandparentBody");

            if (AITweaks)
            {
                masterbase.GetComponent<BaseAI>().aimVectorMaxSpeed = 180f;
            }

            if (SpeedTweaks)
            {
                body.baseMoveSpeed = 30f;

                var sun = Resources.Load<GameObject>("prefabs/networkedobjects/GrandParentSun").GetComponent<GrandParentSunController>();
                sun.maxDistance = 9999f;

                On.EntityStates.GrandParentBoss.FireSecondaryProjectile.OnEnter += (orig, self) =>
                {
                    self.baseDuration = 1.5f;
                    orig(self);
                };

                On.EntityStates.GrandParent.ChannelSunStart.OnEnter += (orig, self) =>
                {
                    EntityStates.GrandParent.ChannelSunStart.baseDuration = 1f;
                    orig(self);
                };
                On.EntityStates.GrandParent.ChannelSunEnd.OnEnter += (orig, self) =>
                {
                    EntityStates.GrandParent.ChannelSunEnd.baseDuration = 1f;
                    orig(self);
                };
            }

            if (RockTweaks)
            {
                var dwayne = Resources.Load<GameObject>("prefabs/projectiles/GrandparentBoulder").GetComponent<ProjectileImpactExplosion>();
                dwayne.falloffModel = BlastAttack.FalloffModel.None;
                dwayne.blastRadius = 10f;
                dwayne.childrenCount = 30;
                dwayne.minAngleOffset = new Vector3(-5f, 1f, -5f);
                dwayne.maxAngleOffset = new Vector3(5f, 1f, 5f);
            }

            if (MovementTweaks)
            {
                bodybase.AddComponent<CharacterMotor>();
                var a = bodybase.GetComponent<CharacterMotor>();
                a.characterDirection = bodybase.GetComponent<CharacterDirection>();
                a.muteWalkMotion = false;
                a.mass = 900f;
                a.airControl = 0.25f;
                a.disableAirControlUntilCollision = false;
                a.generateParametersOnAwake = true;
            }

            if (NewSkill)
            {
                var ai = (from x in masterbase.GetComponents<AISkillDriver>()
                          where x.customName == "FireSecondaryProjectile"
                          select x).First();

                AISkillDriver dr = masterbase.AddComponent<AISkillDriver>();

                dr.customName = "SkyLeap";
                dr.skillSlot = SkillSlot.Utility;

                dr.requiredSkill = ai.requiredSkill;
                dr.requireSkillReady = ai.requireSkillReady;
                dr.requireEquipmentReady = ai.requireEquipmentReady;

                dr.minUserHealthFraction = ai.minUserHealthFraction;
                dr.maxUserHealthFraction = ai.maxUserHealthFraction;
                dr.minTargetHealthFraction = ai.minTargetHealthFraction;
                dr.maxTargetHealthFraction = ai.maxTargetHealthFraction;

                dr.minDistance = ai.minDistance;
                dr.maxDistance = 50f;

                dr.selectionRequiresTargetLoS = ai.selectionRequiresTargetLoS;
                dr.selectionRequiresOnGround = ai.selectionRequiresOnGround;
                dr.selectionRequiresAimTarget = ai.selectionRequiresAimTarget;

                dr.moveTargetType = ai.moveTargetType;

                dr.activationRequiresTargetLoS = ai.activationRequiresTargetLoS;
                dr.activationRequiresAimTargetLoS = ai.activationRequiresAimTargetLoS;
                dr.activationRequiresAimConfirmation = ai.activationRequiresAimConfirmation;

                dr.movementType = ai.movementType;

                dr.moveInputScale = ai.moveInputScale;
                dr.aimType = ai.aimType;

                dr.ignoreNodeGraph = ai.ignoreNodeGraph;
                dr.shouldSprint = ai.shouldSprint;
                dr.shouldFireEquipment = ai.shouldFireEquipment;
                dr.shouldTapButton = ai.shouldTapButton;

                dr.buttonPressType = ai.buttonPressType;

                dr.driverUpdateTimerOverride = ai.driverUpdateTimerOverride;

                dr.resetCurrentEnemyOnNextDriverSelection = ai.resetCurrentEnemyOnNextDriverSelection;
                dr.noRepeat = ai.noRepeat;
                dr.nextHighPriorityOverride = ai.nextHighPriorityOverride;

                // this ai skill driver doesnt seem to work
                // :EYEBROW: :EYEBROW: aye WHY DOESNt IT WORKkkk
                // oh i think its at the bottom priority list so uh
                // yeah i gotta reorganize it somehow, cant at runtime unless i delete it, store it in an array and organize it :thonk:

                var sd = ScriptableObject.CreateInstance<SkillDef>();
                sd.skillName = "SkyLeap";
                sd.activationStateMachineName = "Body";
                sd.activationState = new SerializableEntityStateType(typeof(GrandparentShockwave));
                sd.interruptPriority = InterruptPriority.PrioritySkill;
                sd.baseRechargeInterval = 15f;
                sd.resetCooldownTimerOnUse = false;
                sd.dontAllowPastMaxStocks = false;
                sd.beginSkillCooldownOnSkillEnd = true;
                sd.forceSprintDuringState = false;
                sd.canceledFromSprinting = false;
                sd.mustKeyPress = false;

                Main.skillDefContent.Add(sd);
                Main.RegisterType(typeof(GrandparentShockwave));

                var family = Resources.Load<SkillFamily>("skilldefs/grandparentbody/GrandParentBodyUtilityFamily");
                family.variants[0] = new SkillFamily.Variant
                {
                    skillDef = sd,
                    unlockableDef = null,
                    viewableNode = new ViewablesCatalog.Node(sd.skillNameToken, false, null)
                };

                var ai2 = (from x in masterbase.GetComponents<AISkillDriver>()
                           where x.customName == "RotateToEnemyPriority"
                           select x).First();
                ai2.nextHighPriorityOverride = dr;
            }
        }

        public class GrandparentShockwave : EntityStates.BrotherMonster.ExitSkyLeap
        {
            public override void OnEnter()
            {
                waveProjectileDamageCoefficient = 0.1f;
                waveProjectileForce = 6000f;
                waveProjectileCount = 24;
                soundString = "Play_grandParent_spawn";
                base.OnEnter();
            }
        }
    }
}
using System.Reflection;
using HarmonyLib;
using WellRoundedBalance.Enemies.Bosses.Vagrant;

namespace WellRoundedBalance.Enemies.Bosses
{
    public class WanderingVagrant : EnemyBase<WanderingVagrant>
    {
        public override string Name => "::: Bosses ::: Wandering Vagrant";
        [ConfigField("Replace Primary?", "Replaces Vagrant's primary with a trispread of orbs.", true)]
        public static bool ReplacePrimary;
        [ConfigField("Nova Rework", "Reworks the Vagrant Nova to trigger once when the vagrant would normally die, granting it invincibility for the duration", true)]
        public static bool NovaTweak;
        [ConfigField("Change Vagrant Stats and Scale", "Tweaks the Wandering Vagrant to be much smaller and much more agile, highly recommended to leave on", true)]
        public static bool StatChanges;
        [ConfigField("Vagrant Chain Dash", "Gives Wandering Vagrant a chain dash which spews seeking orbs", true)]
        public static bool EnableChainDash;

        public static GameObject VagrantSeekerOrb;

        public override void Hooks()
        {
            TweakVagrantPrefab();
        }

        public void TweakVagrantPrefab()
        {
            var prefab = Utils.Paths.GameObject.VagrantBody15.Load<GameObject>();

            var body = prefab.GetComponent<CharacterBody>();
            var modelLoc = body.GetComponent<ModelLocator>();

            if (StatChanges)
            {
                body.baseMoveSpeed = 16f;
                body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                body.GetComponent<RigidbodyMotor>().canTakeImpactDamage = false;

                modelLoc.modelBaseTransform.localScale = new(4f, 4f, 4f);
            }

            if (EnableChainDash)
            {
                ApplySkillDef(prefab);
                TweakMaster();

                foreach (var p in body.GetComponents<QuaternionPID>())
                {
                    p.gain = 20;
                }
            }

            if (ReplacePrimary)
            {
                var skill = Utils.Paths.SkillDef.VagrantBodyJellyBarrage.Load<SkillDef>();
                skill.activationState = new(typeof(JellyShotgun));
                skill.beginSkillCooldownOnSkillEnd = true;
                skill.baseRechargeInterval = 4.75f;

                JellyShotgun.ModifyBase();
            }


            VagrantSeekerOrb = PrefabAPI.InstantiateClone(Utils.Paths.GameObject.VagrantCannon.Load<GameObject>(), "VagrantSeekerBolt");
            var ghost = PrefabAPI.InstantiateClone(Utils.Paths.GameObject.VagrantCannonGhost.Load<GameObject>(), "VagrantSeekerBolt");
            ghost.AddComponent<VagrantSeekerGhostController>();

            VagrantSeekerOrb.GetComponent<ProjectileController>().ghostPrefab = ghost;
            VagrantSeekerOrb.AddComponent<ProjectileTargetComponent>();
            VagrantSeekerOrb.AddComponent<VagrantSeekerController>();

            var renderer = ghost.AddComponent<LineRenderer>();
            renderer.startWidth = 0.5f;
            renderer.endWidth = 0.5f;
            renderer.material = Utils.Paths.Material.matCaptainTracerTrail.Load<Material>();

            var finder = VagrantSeekerOrb.AddComponent<ProjectileSphereTargetFinder>();
            finder.allowTargetLoss = false;
            finder.lookRange = 650f;
            finder.testLoS = false;
            finder.searchTimer = 0.1f;

            var simple = VagrantSeekerOrb.GetComponent<ProjectileSimple>();
            simple.updateAfterFiring = true;
            simple.enableVelocityOverLifetime = false;


            var expl = VagrantSeekerOrb.GetComponent<ProjectileImpactExplosion>();
            expl.blastRadius = 1.5f;
            expl.blastDamageCoefficient = 1f;

            ContentAddition.AddProjectile(VagrantSeekerOrb);
        }

        public void TweakMaster()
        {
            var master = Utils.Paths.GameObject.VagrantMaster.Load<GameObject>();

            var driver = master.AddComponent<AISkillDriver>();
            driver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            driver.activationRequiresAimConfirmation = false;
            driver.activationRequiresAimTargetLoS = false;
            driver.activationRequiresTargetLoS = false;
            driver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            driver.customName = "DashAtEnemy";
            driver.maxDistance = 500f;
            driver.minDistance = 100f;
            driver.skillSlot = SkillSlot.Utility;
            driver.requireSkillReady = true;
            driver.noRepeat = false;
            driver.aimType = AISkillDriver.AimType.MoveDirection;
            driver.moveInputScale = 2f;
            
            // correct way of reording components. fuck off with all the stupid fucking spawn hooks.
            // no wonder modded ror2 runs like shit. stop hooking dumb shit. jesus.
            var components = master.GetComponents<AISkillDriver>().ToArray();
            for (var i = 0; i < components.Length; i++)
            {
                var c = components[i];
                if (c.customName != "DashAtEnemy")
                {
                    if (c.nextHighPriorityOverride != null)
                        Logger.LogError("PRIORITY OVERRIDE MIGHT FUCK SHIT UP " + c.customName + " -> " +c.nextHighPriorityOverride.customName);
                    if (c.skillSlot != SkillSlot.Special)
                        master.AddComponentCopy(c);
                    Component.DestroyImmediate(c);
                }
            }
        }

        public void ApplySkillDef(GameObject prefab)
        {
            var slot = prefab.AddComponent<GenericSkill>();
            slot.skillName = "WR-B";

            var family = ScriptableObject.CreateInstance<SkillFamily>();
            (family as ScriptableObject).name = "VagrantChainDash";

            var def = ScriptableObject.CreateInstance<SkillDef>();
            def.activationStateMachineName = "Body";
            def.baseMaxStock = 1;
            def.beginSkillCooldownOnSkillEnd = true;
            def.skillNameToken = "LGM-A";
            def.skillDescriptionToken = "BAL-S";
            def.cancelSprintingOnActivation = true;
            def.canceledFromSprinting = false;
            def.isCombatSkill = true;
            def.stockToConsume = 1;
            def.baseRechargeInterval = 12f;
            def.activationState = new(typeof(ChainDashes));

            ContentAddition.AddSkillDef(def);
            family.variants =
            [
                new SkillFamily.Variant()
                {
                    skillDef = def,
                    unlockableDef = null,
                    viewableNode = new("j", false, null)
                }
            ];

            ContentAddition.AddSkillFamily(family);

            slot._skillFamily = family;
            prefab.GetComponent<SkillLocator>().utility = slot;
        }
    }
}
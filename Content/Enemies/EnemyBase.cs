using System;
using BepInEx.Configuration;
using EntityStates;
using RoR2.Skills;

namespace WellRoundedBalance.Enemies
{
    public abstract class EnemyBase<T> : EnemyBase where T : EnemyBase<T>
    {
        public static T instance { get; set; }

        public EnemyBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class EnemyBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBEnemyConfig;

        public override void Init()
        {
            base.Init();
        }

        public SkillDef CreateSkillDef<T>(float cooldown, string esm) where T : EntityState
        {
            var skill = ScriptableObject.CreateInstance<SkillDef>();
            skill.baseRechargeInterval = cooldown;
            skill.activationStateMachineName = esm;
            skill.activationState = new(typeof(T));

            ContentAddition.AddSkillDef(skill);
            return skill;
        }

        public void ReplaceSkill(SkillDef skill, GenericSkill slot)
        {
            slot._skillFamily.variants[0].skillDef = skill;
        }
    }
}
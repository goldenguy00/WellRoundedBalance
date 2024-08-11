using System;
using BepInEx.Configuration;

namespace WellRoundedBalance.Survivors
{
    public abstract class SurvivorBase<T> : SurvivorBase where T : SurvivorBase<T>
    {
        public static T instance { get; set; }

        public SurvivorBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class SurvivorBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBSurvivorConfig;

        public override void Init()
        {
            base.Init();
        }
    }
}
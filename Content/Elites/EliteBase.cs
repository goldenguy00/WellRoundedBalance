using System;
using BepInEx.Configuration;

namespace WellRoundedBalance.Elites
{
    public abstract class EliteBase<T> : EliteBase where T : EliteBase<T>
    {
        public static T instance { get; set; }

        public EliteBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class EliteBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBEliteConfig;

        public override void Init()
        {
            base.Init();
        }
    }
}
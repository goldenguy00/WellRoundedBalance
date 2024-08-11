using System;
using BepInEx.Configuration;

namespace WellRoundedBalance.Allies
{
    public abstract class AllyBase<T> : AllyBase where T : AllyBase<T>
    {
        public static T instance { get; set; }

        public AllyBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class AllyBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBAllyConfig;

        public override void Init()
        {
            base.Init();
        }
    }
}
using System;
using BepInEx.Configuration;

namespace WellRoundedBalance.Gamemodes
{
    public abstract class GamemodeBase<T> : GamemodeBase where T : GamemodeBase<T>
    {
        public static T instance { get; set; }

        public GamemodeBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class GamemodeBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBGamemodeConfig;

        public override void Init()
        {
            base.Init();
        }
    }
}
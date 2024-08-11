using System;
using BepInEx.Configuration;

namespace WellRoundedBalance.Artifacts.Vanilla
{
    public abstract class ArtifactEditBase<T> : ArtifactEditBase where T : ArtifactEditBase<T>
    {
        public static T instance { get; set; }

        public ArtifactEditBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class ArtifactEditBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBArtifactEditConfig;

        public override void Init()
        {
            base.Init();
        }
    }
}
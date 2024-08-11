using BepInEx.Configuration;
using System;

namespace WellRoundedBalance.Achievements
{
    public abstract class AchievementBase<T> : AchievementBase where T : AchievementBase<T>
    {
        public static T instance { get; set; }

        public AchievementBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class AchievementBase : SharedBase
    {
        public abstract string Token { get; }
        public abstract string Description { get; }
        public override ConfigFile Config => Main.WRBAchievementConfig;

        public static event Action onTokenRegister;

        //public static List<string> achievementList = [];

        public override void Init()
        {
            base.Init();
            onTokenRegister += SetToken;
            //achievementList.Add(Name);
        }

        [SystemInitializer(typeof(UnlockableCatalog))]
        public static void OnUnlockableInitialized() => onTokenRegister?.Invoke();

        public void SetToken()
        {
            if (Token != null)
            {
                var prefix = "ACHIEVEMENT_";
                var suffix = "_DESCRIPTION";
                LanguageAPI.Add(prefix + Token.ToUpper() + suffix, Description);
            };
        }
    }
}
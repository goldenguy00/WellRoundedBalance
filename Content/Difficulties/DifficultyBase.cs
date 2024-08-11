using BepInEx.Configuration;
using System;

namespace WellRoundedBalance.Difficulties
{
    public abstract class DifficultyBase<T> : DifficultyBase where T : DifficultyBase<T>
    {
        public static T instance { get; private set; }

        public DifficultyBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class DifficultyBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBDifficultyConfig;
        public virtual DifficultyIndex InternalDiff => DifficultyIndex.Invalid;
        public abstract float scaling { get; }
        public abstract string DescText { get; }

        public static event Action OnInit;

        public static List<string> difficultyList = [];

        public override void Init()
        {
            base.Init();
            OnInit += HandleInit;
            difficultyList.Add(Name);
        }

        [SystemInitializer(typeof(DifficultyCatalog))]
        public static void OnDifficultyInitialized() => OnInit?.Invoke();

        public void HandleInit()
        {
            if (InternalDiff == DifficultyIndex.Invalid)
                return;

            var def = DifficultyCatalog.GetDifficultyDef(InternalDiff);
            if (def != null)
            {
                def.scalingValue = scaling / 50f;
                LanguageAPI.Add(def.descriptionToken + "_WRB", DescText);
            }
        }
    }
}
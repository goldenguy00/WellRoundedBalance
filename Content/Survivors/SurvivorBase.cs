using BepInEx.Configuration;

namespace WellRoundedBalance.Survivors
{
    public abstract class SurvivorBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBSurvivorConfig;
        public static List<string> survivorList = [];

        public override void Init()
        {
            base.Init();
            survivorList.Add(Name);
        }
    }
}
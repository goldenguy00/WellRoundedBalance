namespace WellRoundedBalance.Achievements.Commando
{
    internal class Incorruptible : AchievementBase<Incorruptible>
    {
        
        public override string Token => "commandoNonLunarEndurance";

        public override string Description => "As Commando, clear 11 stages in a single run without picking up any Lunar items.";

        public override string Name => ":: Achievements :: Survivor :: Annihilator";

        public override void Hooks()
        {
        }

        [SystemInitializer(typeof(AchievementManager))]
        private static void OnAvailable()
        {
            RoR2.Achievements.Commando.CommandoNonLunarEnduranceAchievement.requirement = 11ul;
        }
    }
}
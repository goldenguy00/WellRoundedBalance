using RoR2.Achievements;

namespace WellRoundedBalance.Achievements.NonSpecific
{
    internal class WarHorn : AchievementBase<WarHorn>
    {
        public override string Token => "multiCombatShrine";

        public override string Description => "Complete 2 Combat Shrines in a single stage.";

        public override string Name => ":: Achievements : Non Specific :: Warmonger";

        public override void Hooks()
        {
            
        }

        [SystemInitializer(typeof(AchievementManager))]
        private static void MultiCombatShrineServerAchievement_Check()
        {
            MultiCombatShrineAchievement.MultiCombatShrineServerAchievement.requirement = 2;
        }
    }
}
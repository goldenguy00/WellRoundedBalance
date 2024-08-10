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
            RoR2.AchievementManager.availability.CallWhenAvailable(MultiCombatShrineServerAchievement_Check);
        }

        private void MultiCombatShrineServerAchievement_Check()
        {
            MultiCombatShrineAchievement.MultiCombatShrineServerAchievement.requirement = 2;
        }
    }
}
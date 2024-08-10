namespace WellRoundedBalance.Enemies.Bosses
{
    internal class Grandparent : EnemyBase<Grandparent>
    {
        public override string Name => "::: Bosses :: Grandparent";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            On.RoR2.GrandParentSunController.Start += GrandParentSunController_Start;
            Changes();
        }

        private void GrandParentSunController_Start(On.RoR2.GrandParentSunController.orig_Start orig, GrandParentSunController self)
        {
            self.bullseyeSearch.teamMaskFilter.RemoveTeam(self.teamFilter.teamIndex);
            orig(self);
        }

        private void Changes()
        {
            Utils.Paths.GameObject.GrandParentSun.LoadComponent<GrandParentSunController>().burnDuration = 0.4f;

            var master = Utils.Paths.GameObject.GrandparentMaster.Load<GameObject>();

            foreach (var skill in master.GetComponents<AISkillDriver>())
            {
                if (skill.customName is "ChannelSun")
                    skill.noRepeat = true;
            }
        }
    }
}
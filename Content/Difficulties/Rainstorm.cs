namespace WellRoundedBalance.Difficulties
{
    internal class Rainstorm : DifficultyBase<Rainstorm>
    {
        public override string Name => ":: Difficulties :: Rainstorm";
        public override DifficultyIndex InternalDiff => DifficultyIndex.Normal;

        public override string DescText => "The way the game is meant to be played. Test your abilities and skills against formidable foes." +
                                           (totalDifficultyScaling != 100f ? "<style=cStack>\n\n>Difficulty Scaling: <style=cIsHealth>+" + (totalDifficultyScaling - 100f) + "%</style></style>" : "");

        [ConfigField("Total Difficulty Scaling", "", 100f)]
        public static float totalDifficultyScaling;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        private void Changes()
        {
            var def = DifficultyCatalog.GetDifficultyDef(InternalDiff);
            if (def != null)
                def.scalingValue = totalDifficultyScaling / 50f;
        }
    }
}
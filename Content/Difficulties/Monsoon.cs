﻿using MonoMod.Cil;

namespace WellRoundedBalance.Difficulties
{
    internal class Monsoon : DifficultyBase<Monsoon>
    {
        public override string Name => ":: Difficulties ::: Monsoon";
        public override DifficultyIndex InternalDiff => DifficultyIndex.Hard;

        public override string DescText => "For hardcore players. Every bend introduces pain and horrors of the planet. You will die.<style=cStack>\n\n" +
                                           (percentRegenDecrease > 0 ? ">Player Health Regeneration: <style=cIsHealth>-" + d(percentRegenDecrease) + "</style> \n" : "") +
                                           ">Difficulty Scaling: <style=cIsHealth>+" + (totalDifficultyScaling - 100f) + "%</style></style>";

        [ConfigField("Percent Regen Decrease", "Decimal.", 0.25f)]
        public static float percentRegenDecrease;

        [ConfigField("Total Difficulty Scaling", "", 133f)]
        public static float totalDifficultyScaling;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            Changes();
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(72),
                x => x.MatchLdcR4(0.4f)))
            {
                c.Index += 1;
                c.Next.Operand = percentRegenDecrease;
            }
            else
            {
                Logger.LogError("Failed to apply Drizzle Regen hook");
            }
        }

        private void Changes()
        {
            var def = DifficultyCatalog.GetDifficultyDef(InternalDiff);
            if (def != null)
                def.scalingValue = totalDifficultyScaling / 50f;
        }
    }
}
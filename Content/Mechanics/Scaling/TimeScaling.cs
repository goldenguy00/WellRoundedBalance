﻿using R2API.Utils;
using System;

namespace WellRoundedBalance.Mechanics.Scaling
{
    public class TimeScaling : MechanicBase<TimeScaling>
    {
        public static float timer;
        public static float interval = 180f;
        public static float vanillaStandardScaling;
        public static float vanillaLinearScaling;
        public static float ambientLevel;
        public static float vanillaStandardAmbientLevel;
        public static float vanillaLinearAmbientLevel;

        public override string Name => ":: Mechanics : Time Scaling";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            ChangeBehavior();
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private void RoR2Application_onFixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= interval && Run.instance)
            {
                ChatMessage.Send("Current ambient level: " + ambientLevel);
                ChatMessage.Send("Vanilla ambient level would be: " + vanillaStandardAmbientLevel);
                ChatMessage.Send("Vanilla linear ambient level would be: " + vanillaLinearAmbientLevel);
                timer = 0f;
            }
        }

        public static void ChangeBehavior()
        {
            On.RoR2.Run.RecalculateDifficultyCoefficentInternal += (orig, self) =>
            {
                int playerCount = self.participatingPlayerCount;
                float Time = self.GetRunStopwatch() * 0.016666668f; // stupid vanilla workaround

                var playerfactorbase = 0.7f;
                var playercountmultiplier = 0.3f;
                var playercountexponent = 0.2f;
                var timefactormultiplier = 0.0506f;

                DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(self.selectedDifficulty);

                float playerFactor = playerfactorbase + playerCount * playercountmultiplier;
                float timeFactor = Time * timefactormultiplier * (1.2f * Mathf.Sqrt(difficultyDef.scalingValue));
                float playerScalar = (float)Math.Pow(playerCount, playercountexponent);

                float customTimeFactor = Mathf.Sqrt(Time) * 0.42f * difficultyDef.scalingValue;

                float customFactor = 1f + 0.31f * customTimeFactor * playerScalar;

                float finalDifficulty = (playerFactor + timeFactor * playerScalar) * customFactor;
                self.compensatedDifficultyCoefficient = finalDifficulty;
                self.difficultyCoefficient = finalDifficulty;
                self.ambientLevel = Mathf.Min(3f * (finalDifficulty - playerFactor) + 1f, Run.ambientLevelCap);

                Run.ambientLevelCap = int.MaxValue;
                int ambientLevelFloor = self.ambientLevelFloor;
                self.ambientLevelFloor = Mathf.FloorToInt(self.ambientLevel);
                if (ambientLevelFloor != self.ambientLevelFloor && ambientLevelFloor != 0 && self.ambientLevelFloor > ambientLevelFloor)
                {
                    self.OnAmbientLevelUp();
                }

                float stageFactor = Mathf.Pow(1.15f, self.stageClearCount);
                float vanillaTimeFactor = Time * timefactormultiplier * difficultyDef.scalingValue;

                vanillaStandardScaling = (playerFactor + vanillaTimeFactor * playerScalar) * stageFactor;

                vanillaLinearScaling = playerFactor + vanillaTimeFactor * playerScalar;

                ambientLevel = self.ambientLevel;
                vanillaStandardAmbientLevel = 3 * (vanillaStandardScaling - playerFactor) + 1;
                vanillaLinearAmbientLevel = 3 * (vanillaLinearScaling - playerFactor) + 1;
            };
        }
    }
}
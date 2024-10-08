﻿using R2API.Utils;
using System;

namespace WellRoundedBalance.Mechanics.Scaling
{
    public class TimeScaling : MechanicBase<TimeScaling>
    {
        public static float vanillaStandardScaling;
        public static float vanillaLinearScaling;
        public static float ambientLevel;
        public static float vanillaStandardAmbientLevel;
        public static float vanillaLinearAmbientLevel;

        public override string Name => ":: Mechanics : Time Scaling";

        [ConfigField("Time Scaling", "Formula for difficulty coefficient: ((Player Factor Base + Player Count * Player Count Multiplier) + (Time in minutes * Time Factor Multiplier * (Square Root Multiplier * Square Root(DifficultyDef Scaling Value (1.5 for WRB Drizzle, 2 for Rainstorm, 3 for Monsoon)))) * (Player Count ^ Player Count Exponent 1)) * (Custom Factor Add + Custom Time Factor Multiplier * (Square Root(Time in minutes) * Scaling Value Multiplier * DifficultyDef Scaling Value) * Player Count ^ Player Count Exponent 2)\nFormula for ambient level: ", 0.75f)]
        public static float duhDoesNothing;

        [ConfigField("Scaling Debug Keybind", "Writes vanilla time scaling for comparison", ";")]
        public static string scalingDebugKeybind;

        [ConfigField("Player Factor Base", "", 0.7f)]
        public static float playerFactorBase;

        [ConfigField("Player Count Multiplier", "", 0.3f)]
        public static float playerCountMultiplier;

        [ConfigField("Player Count Exponent 1", "", 0.13f)]
        public static float playerCountExponent;

        [ConfigField("Player Count Exponent 2", "", 0.07f)]
        public static float playerCountExponent2;

        [ConfigField("Time Factor Multiplier", "", 0.0506f)]
        public static float timeFactorMultiplier;

        [ConfigField("Square Root Multiplier", "", 1.2f)]
        public static float squareRootMultiplier;

        [ConfigField("Scaling Value Multiplier", "", 0.42f)]
        public static float scalingValueMultiplier;

        [ConfigField("Custom Factor Add", "", 1f)]
        public static float customFactorAdd;

        [ConfigField("Custom Time Factor Multiplier", "", 0.31f)]
        public static float customTimeFactorMultiplier;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            ChangeBehavior();
            //RoR2Application.onUpdate += RoR2Application_onUpdate;
        }

        private void RoR2Application_onUpdate()
        {
            if (Input.GetKeyDown(scalingDebugKeybind) && Run.instance)
            {
                ChatMessage.Send("\n");
                ChatMessage.Send("Current ambient level: " + ambientLevel);
                ChatMessage.Send("Vanilla ambient level would be: " + vanillaStandardAmbientLevel);
                ChatMessage.Send("Vanilla linear ambient level would be: " + vanillaLinearAmbientLevel);
                ChatMessage.Send("\n");
            }
        }

        public static void ChangeBehavior()
        {
            On.RoR2.Run.RecalculateDifficultyCoefficentInternal += (orig, self) =>
            {
                float playerCount = self.participatingPlayerCount;
                var Time = self.GetRunStopwatch() * 0.016666668f; // stupid vanilla workaround

                var playerfactorbase = playerFactorBase;
                var playercountmultiplier = playerCountMultiplier;
                var playercountexponent = playerCountExponent;
                var playercountexponent2 = playerCountExponent2;
                var timefactormultiplier = timeFactorMultiplier;

                var difficultyDef = DifficultyCatalog.GetDifficultyDef(self.selectedDifficulty);

                var playerFactor = playerfactorbase + playerCount * playercountmultiplier;
                var timeFactor = Time * timefactormultiplier * (squareRootMultiplier * Mathf.Sqrt(difficultyDef.scalingValue));
                var playerScalar = (float)Math.Pow(playerCount, playercountexponent);
                var playerScalar2 = (float)Math.Pow(playerCount, playercountexponent2);

                var customTimeFactor = Mathf.Sqrt(Time) * scalingValueMultiplier * difficultyDef.scalingValue;

                var customFactor = customFactorAdd + customTimeFactorMultiplier * customTimeFactor * playerScalar2;

                //
                var finalDifficulty = (playerFactor + timeFactor * playerScalar) * customFactor;
                //

                self.compensatedDifficultyCoefficient = finalDifficulty;
                self.difficultyCoefficient = finalDifficulty;
                self.ambientLevel = Mathf.Min(3f * (finalDifficulty - playerFactor) + 1f, Run.ambientLevelCap);

                Run.ambientLevelCap = int.MaxValue;
                var ambientLevelFloor = self.ambientLevelFloor;
                self.ambientLevelFloor = Mathf.FloorToInt(self.ambientLevel);
                if (ambientLevelFloor != self.ambientLevelFloor && ambientLevelFloor != 0 && self.ambientLevelFloor > ambientLevelFloor)
                {
                    self.OnAmbientLevelUp();
                }

                var stageFactor = Mathf.Pow(1.15f, self.stageClearCount);
                var vanillaTimeFactor = Time * timefactormultiplier * difficultyDef.scalingValue;

                vanillaStandardScaling = (playerFactor + vanillaTimeFactor * playerScalar) * stageFactor;

                vanillaLinearScaling = playerFactor + vanillaTimeFactor * playerScalar;

                ambientLevel = self.ambientLevel;
                vanillaStandardAmbientLevel = 3 * (vanillaStandardScaling - playerFactor) + 1;
                vanillaLinearAmbientLevel = 3 * (vanillaLinearScaling - playerFactor) + 1;
            };
        }
    }
}
﻿using MonoMod.Cil;
using System;

namespace WellRoundedBalance.Artifacts.Vanilla
{
    internal class Sacrifice : ArtifactEditBase<Sacrifice>
    {
        public override string Name => ":: Artifacts :::::::::::: Sacrifice";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            IL.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += SacrificeArtifactManager_OnServerCharacterDeath;
        }

        [ConfigField("Base Drop Chance", "", 4f)]
        public static float baseDropChance;

        [ConfigField("Swarm Drop Chance", "", 2f)]
        public static float swarmDropChance;

        [ConfigField("Max Drop Chance", "", 7f)]
        public static float maxDropChance;

        [ConfigField("Max Swarm Drop Chance", "", 3.5f)]
        public static float maxSwarmDropChance;

        private void SacrificeArtifactManager_OnServerCharacterDeath(ILContext il)
        {
            ILCursor c = new(il);

            //Change base drop chance
            c.GotoNext(MoveType.After,
                x => x.MatchLdcR4(5f)
                );
            c.EmitDelegate<Func<float, float>>(orig =>
            {
                return RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef) ? swarmDropChance : baseDropChance;
            });

            //Clamp final drop chance
            c.GotoNext(
                x => x.MatchStloc(0) //Called after GetExpAdjustedDropChancePercent
                );
            c.EmitDelegate<Func<float, float>>(orig =>
            {
                var finalDropChance = orig;

                if (orig > 0f)
                {
                    var swarmsEnabled = RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef);

                    var baseChance = baseDropChance;
                    var maxChance = maxDropChance;

                    if (swarmsEnabled)
                    {
                        baseChance = swarmDropChance;
                        maxChance = maxSwarmDropChance;
                    }

                    if (finalDropChance < baseChance)
                    {
                        finalDropChance = baseChance;
                    }

                    if (finalDropChance > maxChance)
                    {
                        finalDropChance = maxChance;
                    }
                }

                return finalDropChance;
            });
        }
    }
}
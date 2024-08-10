using MonoMod.RuntimeDetour;
using R2API.Utils;
using System.Runtime.CompilerServices;
using System;
using HarmonyLib;

namespace WellRoundedBalance.Mechanics.Allies
{
    internal class UpdatePlayerCount : MechanicBase<UpdatePlayerCount>
    {
        public override string Name => ":: Mechanics ::::::::::::::: Update Player Count";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            new Hook(AccessTools.PropertyGetter(typeof(Run), nameof(Run.participatingPlayerCount)), 
                typeof(UpdatePlayerCount).GetMethodCached(nameof(Run_participatingPlayerCount)));
        }

        private static int Run_participatingPlayerCount(Run _)
        {
            int players = PlayerCharacterMasterController.instances.Count(pc => pc.isConnected);

            if (Main.WildbookMultitudesLoaded)
                players = ApplyMultitudes(players);

            if (Main.ZetArtifactsLoaded)
                players = ApplyZetMultitudesArtifact(players);

            return players;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static int ApplyMultitudes(int origPlayerCount)
        {
            return origPlayerCount * Multitudes.Multitudes.Multiplier;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static int ApplyZetMultitudesArtifact(int origPlayerCount)
        {
            if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(TPDespair.ZetArtifacts.ZetArtifactsContent.Artifacts.ZetMultifact) && TPDespair.ZetArtifacts.ZetMultifact.Enabled)
            {
                return origPlayerCount * Math.Max(2, TPDespair.ZetArtifacts.ZetArtifactsPlugin.MultifactMultiplier.Value); //GetMultiplier is private so I copypasted the code.
            }
            else
            {
                return origPlayerCount;
            }
        }
    }
}
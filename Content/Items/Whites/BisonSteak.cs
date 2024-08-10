namespace WellRoundedBalance.Items.Whites
{
    public class BisonSteak : ItemBase<BisonSteak>
    {
        public override string Name => ":: Items : Whites :: Bison Steak";
        public override ItemDef InternalPickup => RoR2Content.Items.FlatHealth;

        public override string PickupText => "Gain bonus max health.";

        public override string DescText =>
            StackDesc(maximumHealthGain, maximumHealthGainStack, init => $"Increases <style=cIsHealing>maximum health</style> by <style=cIsHealing>{init}</style>{{Stack}}" + (levelHealthGain > 0 ? " and <style=cIsHealing>health per level</style> by <style=cIsHealing>" + levelHealthGain + "</style> <style=cStack>(+" + levelHealthGain + " per stack)</style>." : "."));

        [ConfigField("Maximum Health Gain", 40f)]
        public static float maximumHealthGain;

        [ConfigField("Maximum Health Gain per Stack", 40f)]
        public static float maximumHealthGainStack;

        [ConfigField("Maximum Health Gain is Hyperbolic", "Decimal, Max value. Set to 0 to make it linear.", 0f)]
        public static float maximumHealthGainIsHyperbolic;

        [ConfigField("Health Per Level Gain", 1f)]
        public static float levelHealthGain;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                var count = sender.inventory.GetItemCount(InternalPickup);
                if (count > 0)
                {
                    args.baseHealthAdd += StackAmount(maximumHealthGain, maximumHealthGainStack, count, maximumHealthGainIsHyperbolic);
                    args.baseHealthAdd += StackAmount(levelHealthGain, levelHealthGain, sender.level);
                }
            }
        }
    }
}
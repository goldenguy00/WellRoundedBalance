using System;

namespace WellRoundedBalance.Elites.All
{
    public class StatChanges : EliteBase<StatChanges>
    {

        public override string Name => ":: Elites : Stat & Drop Rate Changes";

        [ConfigField("Tier 0 Cost Multiplier", "Applies to Perfected elites.", 18f)]
        public static float tier0CostMultiplier;

        [ConfigField("Tier 1 Cost Multiplier", "", 5f)]
        public static float tier1CostMultiplier;

        [ConfigField("Tier 1 Health Multiplier", "", 2.5f)]
        public static float tier1HealthMultiplier;

        [ConfigField("Tier 1 Damage Multiplier", "", 1.5f)]
        public static float tier1DamageMultiplier;

        [ConfigField("Tier 2 Cost Multiplier", "Applies to Artifact of Honor", 26f)]
        public static float tier2CostMultiplier;

        [ConfigField("Tier 2 Health Multiplier", "Applies to Artifact of Honor", 6f)]
        public static float tier2HealthMultiplier;

        [ConfigField("Tier 2 Damage Multiplier", "Applies to Artifact of Honor", 1f)]
        public static float tier2DamageMultiplier;

        [ConfigField("Tier 1 Honor Cost Multiplier", "", 3.5f)]
        public static float tier1HonorCostMultiplier;

        [ConfigField("Tier 1 Honor Health Multiplier", "", 2.5f)]
        public static float tier1HonorHealthMultiplier;

        [ConfigField("Tier 1 Honor Damage Multiplier", "", 1f)]
        public static float tier1HonorDamageMultiplier;

        [ConfigField("Aspect Chance", "Decimal.", 0.001f)]
        public static float aspectChance;

        [ConfigField("Enable Aspect Inheritance?", "Makes all minions gain the most recent aspect.", true)]
        public static bool aspectInheritance;

        public static HashSet<EquipmentIndex> aspects = [];

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            if (aspectInheritance)
                CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody bpdy)
        {
            if (!bpdy.isPlayerControlled || !bpdy.master || !bpdy.master.inventory)
                return;

            var equipment = bpdy.master.inventory.currentEquipmentIndex;
            if (aspects.Contains(equipment))
            {
                var eliteDef = EquipmentCatalog.GetEquipmentDef(equipment);
                if (eliteDef && eliteDef.passiveBuffDef)
                {
                    foreach (var tc in TeamComponent.GetTeamMembers(TeamIndex.Player))
                    {
                        var body = tc.body;
                        if (!body)
                            continue;

                        body.AddBuff(eliteDef.passiveBuffDef);
                    }
                }
            }
        }

        [SystemInitializer([typeof(CombatDirector)])]
        private static void CombatDirector_Init()
        {
            Main.WRBLogger.LogError("combat director init pre orig ran");

            var honorTier = EliteAPI.VanillaEliteOnlyFirstTierDef;
            honorTier.costMultiplier = tier1HonorCostMultiplier;

            foreach (var eliteDef in honorTier.eliteTypes)
            {
                eliteDef.damageBoostCoefficient = tier1HonorDamageMultiplier;
                eliteDef.healthBoostCoefficient = tier1HonorHealthMultiplier;
            }

            var tier1 = EliteAPI.VanillaFirstTierDef;
            tier1.costMultiplier = tier1CostMultiplier;

            foreach (var eliteDef in tier1.eliteTypes)
            {
                eliteDef.damageBoostCoefficient = tier1DamageMultiplier;
                eliteDef.healthBoostCoefficient = tier1HealthMultiplier;
            }


            foreach (var eliteTierDef in CombatDirector.eliteTiers)
            {
                if (eliteTierDef?.eliteTypes.Any() == true && eliteTierDef != honorTier && eliteTierDef != tier1)
                {
                    bool isLunar = false, isT2 = false;
                    foreach (var eliteDef in eliteTierDef.eliteTypes)
                    {
                        if (eliteDef != null)
                        {
                            if (eliteDef == RoR2Content.Elites.Lunar)
                            {
                                isLunar = true;
                                break;
                            }
                            else if (eliteDef == RoR2Content.Elites.Poison)
                            {
                                isT2 = true;
                                break;
                            }
                        }
                    }

                    if (isLunar)
                    {
                        eliteTierDef.costMultiplier = tier0CostMultiplier;
                    }
                    else if (isT2)
                    {
                        eliteTierDef.costMultiplier = tier2CostMultiplier;

                        foreach (var eliteDef in eliteTierDef.eliteTypes)
                        {
                            eliteDef.damageBoostCoefficient = tier2DamageMultiplier;
                            eliteDef.healthBoostCoefficient = tier2HealthMultiplier;
                        }
                    }
                }
            }

            for (var i = 0; i < EliteCatalog.eliteDefs.Length; i++)
            {
                var index = EliteCatalog.eliteDefs[i];
                if (index.eliteEquipmentDef)
                {
                    index.eliteEquipmentDef.dropOnDeathChance = aspectChance;
                    aspects.Add(index.eliteEquipmentDef.equipmentIndex);
                }
            }
        }
    }
}
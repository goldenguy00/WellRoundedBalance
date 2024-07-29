using System.Collections.Generic;
using System.Linq;
using MiscMods.Artifacts;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MiscMods
{
    public class Utils
    {
        public static bool IsValid(EliteDef ed)
        {
            return ed && ed.IsAvailable()
                && ed.eliteEquipmentDef
                && ed.eliteEquipmentDef.passiveBuffDef
                && ed.eliteEquipmentDef.passiveBuffDef.isElite
                && !Cruelty.BlacklistedElites.Contains(ed.eliteEquipmentDef);
        }

        public static bool GetRandom(float availableCredits, DirectorCard card, Xoroshiro128Plus rng, List<BuffIndex> exclude, out EliteDef def, out float cost)
        {
            def = null;
            cost = 0;

            var tiers = EliteAPI.GetCombatDirectorEliteTiers();
            if (tiers == null || tiers.Length == 0)
                return false;

            List<(EliteDef, float)> availableDefs = [];
            for (int j = 0; j < tiers.Length; j++)
            {
                var etd = tiers[j];
                if (etd != null && !etd.canSelectWithoutAvailableEliteDef &&
                   (card == null || etd.CanSelect(card.spawnCard.eliteRules)))
                {
                    float eliteCost = card?.cost ?? 0f;
                    bool canAfford = eliteCost > 0f && availableCredits >= eliteCost * etd.costMultiplier;

                    if (canAfford)
                    {
                        for (int i = 0; i < etd.eliteTypes.Length; i++)
                        {
                            var ed = etd.eliteTypes[i];
                            if (IsValid(ed) && !exclude.Contains(ed.eliteEquipmentDef.passiveBuffDef.buffIndex))
                                availableDefs.Add((ed, eliteCost));
                        }
                    }
                }
            }

            if (availableDefs.Any())
            {
                var d = rng.NextElementUniform(availableDefs);
                def = d.Item1;
                cost = d.Item2;
                return true;
            }
            return false;
        }

        public static void MultiplyHealth(Inventory inventory, float multAdd)
        {
            int itemCount = (int)((multAdd - 1f) * 10);
            if (itemCount > 0)
                inventory.GiveItem(RoR2Content.Items.BoostHp, itemCount);
        }

        public static void MultiplyDamage(Inventory inventory, float multAdd)
        {
            int itemCount = (int)((multAdd - 1f) * 10);
            if (itemCount > 0 && inventory)
                inventory.GiveItem(RoR2Content.Items.BoostDamage, itemCount);
        }
    }
}

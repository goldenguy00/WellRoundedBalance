using BepInEx.Configuration;

using PATH = WellRoundedBalance.Utils.Paths.ItemDef;

namespace WellRoundedBalance.Items.ConsistentCategories
{
    public static class BetterItemCategories
    {
        /* Makes item categories more consistent and differentiates them a bit more, mainly:
           - Attack speed items being damage
           - Cooldown reduction being both damage and utility
           - Ally items being both damage and defense
           - Non-damaging status effects being utility, etc
           - Defense category is defensive vanilla items tagged as Utility and or Healing
        */

        // also changes AI blacklist to accomodate for item changes

        public static ConfigEntry<bool> enable { get; set; }
        public static ItemTag defenseTag = (ItemTag)(-1);

        [SystemInitializer(typeof(ItemCatalog))]
        private static void BetterAIBlacklist()
        {
            Main.WRBLogger.LogDebug("Calling BetterAIBlacklist init");
            foreach (var itemDef in ItemCatalog.itemDefs)
            {
                if (!itemDef.tags.Contains(ItemTag.AIBlacklist) &&
                    itemDef.tags.Any(t => t
                    is ItemTag.OnKillEffect
                    or ItemTag.InteractableRelated
                    or ItemTag.InteractableRelated
                    or ItemTag.SprintRelated
                    or ItemTag.EquipmentRelated
                    or ItemTag.HoldoutZoneRelated
                    or ItemTag.OnStageBeginEffect))
                {
                    itemDef.tags = [.. itemDef.tags, ItemTag.AIBlacklist];
                    if (Main.enableLogging.Value)
                        Main.WRBLogger.LogDebug("Added AI Blacklist to " + Language.GetString(itemDef.nameToken));
                }
            }
        }

        public static void Init()
        {
            defenseTag = ItemAPI.AddItemTag("Defense");

            // general changes
            PATH.AlienHead.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.AutoCastEquipment.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.EquipmentRelated];
            PATH.Bandolier.Load<ItemDef>().tags = [ItemTag.Utility, ItemTag.OnKillEffect];
            PATH.BeetleGland.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.CannotCopy];
            PATH.Clover.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.IceRing.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.EquipmentMagazine.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.EquipmentRelated];
            PATH.Icicle.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.OnKillEffect];
            PATH.KillEliteFrenzy.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.OnKillEffect];
            PATH.Knurl.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.LunarBadLuck.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.LunarSecondaryReplacement.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CannotSteal];
            PATH.NovaOnHeal.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.AIBlacklist];
            PATH.RoboBallBuddy.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.CannotCopy];
            PATH.SecondarySkillMagazine.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.ShockNearby.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.AIBlacklist];
            PATH.Talisman.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.OnKillEffect, ItemTag.EquipmentRelated];
            PATH.UtilitySkillMagazine.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.WarCryOnMultiKill.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.OnKillEffect];
            PATH.WardOnLevel.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.CannotCopy, ItemTag.AIBlacklist];
            PATH.EquipmentMagazineVoid.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility];
            PATH.FragileDamageBonus.Load<ItemDef>().tags = [ItemTag.Damage];
            PATH.LunarSun.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];
            PATH.RandomEquipmentTrigger.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.EquipmentRelated];
            PATH.ParentEgg.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.CannotCopy, ItemTag.BrotherBlacklist];
            PATH.GoldOnHurt.Load<ItemDef>().tags = [ItemTag.Utility, ItemTag.CannotDuplicate, ItemTag.OnStageBeginEffect];
            PATH.FlatHealth.Load<ItemDef>().tags = [ItemTag.Utility];
            PATH.PrimarySkillShuriken.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist];
            PATH.MoveSpeedOnKill.Load<ItemDef>().tags = [ItemTag.Utility, ItemTag.OnKillEffect];
            PATH.LunarTrinket.Load<ItemDef>().tags = [ItemTag.Healing, ItemTag.Utility, ItemTag.ObliterationRelated];

            // special cases
            PATH.ShinyPearl.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.WorldUnique, defenseTag];
            PATH.Phasing.Load<ItemDef>().tags = [ItemTag.Utility, ItemTag.LowHealth, defenseTag];
            PATH.SiphonOnLowHealth.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.BrotherBlacklist, defenseTag];
            PATH.Squid.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.InteractableRelated, defenseTag];
            PATH.TitanGoldDuringTP.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.WorldUnique, ItemTag.CannotSteal, ItemTag.CannotCopy, ItemTag.HoldoutZoneRelated, defenseTag];
            PATH.ImmuneToDebuff.Load<ItemDef>().tags = [ItemTag.Utility, defenseTag];
            PATH.MinorConstructOnKill.Load<ItemDef>().tags = [ItemTag.Damage, defenseTag];
            PATH.PermanentDebuffOnHit.Load<ItemDef>().tags = [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, defenseTag];
            PATH.VoidMegaCrabItem.Load<ItemDef>().tags = [ItemTag.Damage, defenseTag];
            PATH.Medkit.Load<ItemDef>().tags = [ItemTag.AIBlacklist, defenseTag];
            PATH.ExtraLife.Load<ItemDef>().tags = [ItemTag.AIBlacklist, defenseTag];
            PATH.Bear.Load<ItemDef>().tags = [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, defenseTag];

            if (Greens.HarvestersScythe.instance?.isEnabled == true)
                PATH.HealOnCrit.Load<ItemDef>().tags = [ItemTag.Damage, defenseTag];

            ItemAPI.ApplyTagToItem(defenseTag, PATH.EnergizedOnEquipmentUse.Load<ItemDef>());
            ItemAPI.ApplyTagToItem(defenseTag, PATH.MissileVoid.Load<ItemDef>());

            // removals and defense additions
            ReplaceWithDefense(PATH.ArmorPlate.Load<ItemDef>()); // Repulsion Armor Plate
            ReplaceWithDefense(PATH.BarrierOnKill.Load<ItemDef>()); // Topaz Brooch
            ReplaceWithDefense(PATH.BarrierOnOverHeal.Load<ItemDef>()); // Aegis
            ReplaceWithDefense(PATH.BeetleGland.Load<ItemDef>()); // Queen's Gland
            ReplaceWithDefense(PATH.CaptainDefenseMatrix.Load<ItemDef>()); // Defensive Microbots
            ReplaceWithDefense(PATH.FlatHealth.Load<ItemDef>()); // Bison Steak
            ReplaceWithDefense(PATH.GhostOnKill.Load<ItemDef>()); // Happiest Mask
            ReplaceWithDefense(PATH.HeadHunter.Load<ItemDef>()); // Wake of Vultures
            ReplaceWithDefense(PATH.HealWhileSafe.Load<ItemDef>()); // Cautious Slug
            ReplaceWithDefense(PATH.IncreaseHealing.Load<ItemDef>()); // Rejuvenation Rack
            ReplaceWithDefense(PATH.Infusion.Load<ItemDef>());
            ReplaceWithDefense(PATH.Mushroom.Load<ItemDef>()); // Bustling Fungus
            ReplaceWithDefense(PATH.Pearl.Load<ItemDef>());
            ReplaceWithDefense(PATH.PersonalShield.Load<ItemDef>()); // Personal Shield Generator
            ReplaceWithDefense(PATH.Plant.Load<ItemDef>()); // Interstellar Desk Plant
            ReplaceWithDefense(PATH.RepeatHeal.Load<ItemDef>()); // Corpsebloom
            ReplaceWithDefense(PATH.RoboBallBuddy.Load<ItemDef>()); // Empathy Cores
            ReplaceWithDefense(PATH.Seed.Load<ItemDef>()); // Leeching Seed
            ReplaceWithDefense(PATH.ShieldOnly.Load<ItemDef>()); // Transcendence
            ReplaceWithDefense(PATH.SprintArmor.Load<ItemDef>()); // Rose Buckler
            ReplaceWithDefense(PATH.Tooth.Load<ItemDef>()); // Monster Tooth
            ReplaceWithDefense(PATH.TPHealingNova.Load<ItemDef>()); // Lepton Daisy
            ReplaceWithDefense(PATH.BearVoid.Load<ItemDef>()); // Safer Spaces
            ReplaceWithDefense(PATH.ExtraLifeVoid.Load<ItemDef>()); // Pluripotent Larva
            ReplaceWithDefense(PATH.HealingPotion.Load<ItemDef>()); // Power Elixir
            ReplaceWithDefense(PATH.MushroomVoid.Load<ItemDef>()); // Weeping Fungus
            ReplaceWithDefense(PATH.OutOfCombatArmor.Load<ItemDef>()); // Oddly-shaped Opal

            // category chest changes
            Utils.Paths.BasicPickupDropTable.dtSmallChestHealing.Load<BasicPickupDropTable>().requiredItemTags = [defenseTag];
            LanguageAPI.Add("CATEGORYCHEST_HEALING_NAME", "Chest - Defense");
            LanguageAPI.Add("CATEGORYCHEST_HEALING_CONTEXT", "Open Chest - Defense");

            Utils.Paths.BasicPickupDropTable.dtCategoryChest2Healing.Load<BasicPickupDropTable>().requiredItemTags = [defenseTag];
            LanguageAPI.Add("CATEGORYCHEST2_HEALING_NAME", "Large Chest - Defense");
            LanguageAPI.Add("CATEGORYCHEST2_HEALING_CONTEXT", "Open Large Chest - Defense");

            var newMat = Object.Instantiate(Utils.Paths.Material.matCategoryChestHealing.Load<Material>());
            newMat.SetTexture("_MainTex", Main.wellroundedbalance.LoadAsset<Texture2D>("Assets/WellRoundedBalance/texDefenseChest.png"));

            var defenseChest = Utils.Paths.GameObject.CategoryChestHealing.Load<GameObject>();
            var smr = defenseChest.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>();
            smr.material = newMat;

            var defenseChest2 = Utils.Paths.GameObject.CategoryChest2HealingVariant.Load<GameObject>();
            var smr2 = defenseChest2.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>();
            smr2.material = newMat;
        }

        private static void ReplaceWithDefense(ItemDef def)
        {
            var tags = def.tags.ToList();
            if (tags.Remove(ItemTag.Healing) | tags.Remove(ItemTag.Utility))
            {
                def.tags = [.. tags, defenseTag];
            }
            else
            {
                Main.WRBLogger.LogError(def.nameToken + " tried to assign Defense category, but doesn't have Healing or Utility tag!");
            }
        }
    }
}
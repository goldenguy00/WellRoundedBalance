namespace WellRoundedBalance.Items.Reds
{
    public class PluripotentLarva : ItemBase<PluripotentLarva>
    {
        public override string Name => ":: Items :::::: Voids :: Pluripotent Larva";
        public override ItemDef InternalPickup => DLC1Content.Items.ExtraLifeVoid;

        public override string PickupText => "Shuffle your inventory, and get a <style=cIsVoid>corrupted</style> extra life. Consumed on use. <style=cIsVoid>Corrupts all Dio's Best Friends.</style>.";

        public override string DescText => "<style=cIsUtility>Shuffle your inventory</style>. <style=cIsUtility>Upon death</style>, this item will be <style=cIsUtility>consumed</style> and you will <style=cIsHealing>return to life</style> with <style=cIsHealing>3 seconds of invulnerability</style>, and all of your items that can be <style=cIsUtility>corrupted</style> will be. <style=cIsVoid>Corrupts all Dio's Best Friends</style>.";

        public override void Init()
        {
            LanguageAPI.Add("PLURI_CORRUPTED", "<style=cWorldEvent>{0} has been... corrupted.</color>");
            LanguageAPI.Add("PLURI_CORRUPTED_2P", "<style=cWorldEvent>You have been... corrupted.</color>"); // me
            base.Init();
        }

        public override void Hooks()
        {
            RoR2.Inventory.onServerItemGiven += Inventory_onServerItemGiven;
        }

        private void Inventory_onServerItemGiven(Inventory self, ItemIndex itemIndex, int count)
        {
            if (itemIndex == DLC1Content.Items.ExtraLifeVoid.itemIndex)
            {
                List<ItemIndex> tier1Indices = [];
                List<int> stacks = [];

                // Store the indices and stack counts of all Tier 1 items in the inventory
                for (var i = 0; i < self.itemAcquisitionOrder.Count; i++)
                {
                    var index = self.itemAcquisitionOrder[i];
                    var itemDef = ItemCatalog.GetItemDef(index);

                    if (itemDef.tier == ItemTier.Tier1 || itemDef.deprecatedTier == ItemTier.Tier1)
                    {
                        tier1Indices.Add(index);
                        stacks.Add(self.GetItemCount(index));
                    }
                }

                // Shuffle the stack counts using Fisher-Yates shuffle algorithm
                var n = stacks.Count;
                while (n > 1)
                {
                    n--;
                    var k = Random.Range(0, n + 1);
                    var temp = stacks[k];
                    stacks[k] = stacks[n];
                    stacks[n] = temp;
                }

                // Assign the shuffled stack counts to the Tier 1 items
                for (var i = 0; i < tier1Indices.Count; i++)
                {
                    var index = tier1Indices[i];
                    var stackCount = stacks[i];
                    self.RemoveItem(index, self.GetItemCount(index));
                    self.GiveItem(index, stackCount);
                }

                var body = self.gameObject.GetComponent<CharacterMaster>()?.GetBody();

                if (NetworkServer.active)
                {
                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = body,
                        baseToken = "PLURI_CORRUPTED"
                    });
                }
            }
        }
    }
}
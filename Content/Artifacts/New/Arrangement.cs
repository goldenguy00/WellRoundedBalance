﻿namespace WellRoundedBalance.Artifacts.New
{
    internal class Arrangement : ArtifactAddBase<Arrangement>
    {
        public override string ArtifactName => "Artifact of Arrangement";

        public override string ArtifactLangTokenName => "ARRANGEMENT";

        public override string ArtifactDescription => "Category chests are much more common and have unique spawn rates from one another.";

        public override Sprite ArtifactEnabledIcon => Main.wellroundedbalance.LoadAsset<Sprite>("texBuffHappiestMaskReady.png");

        public override Sprite ArtifactDisabledIcon => Main.wellroundedbalance.LoadAsset<Sprite>("texBuffDelicateWatchIcon.png");

        public override string Name => ":: Artifacts ::::::::::::::::: Arrangement";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private void Run_onRunStartGlobal(Run run)
        {
            if (ArtifactEnabled)
            {
                On.RoR2.ClassicStageInfo.Start += ClassicStageInfo_Start;
            }
            else
            {
                On.RoR2.ClassicStageInfo.Start -= ClassicStageInfo_Start;
            }
        }

        private void ClassicStageInfo_Start(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                var categories = self.interactableCategories.categories;
                var chest = Utils.Paths.InteractableSpawnCard.iscChest1.Load<InteractableSpawnCard>();
                var chestWeight = 1f;
                var categoryChestsFound = 0;
                var totalCategoryChests = GetTotalCategoryChestCount(categories);

                for (var i = 0; i < categories.Length && categoryChestsFound < totalCategoryChests; i++)
                {
                    var categoryIndex = categories[i];
                    for (var j = 0; j < categoryIndex.cards.Length && categoryChestsFound < totalCategoryChests; j++)
                    {
                        var cardIndex = categoryIndex.cards[j];
                        if (cardIndex.spawnCard == chest)
                        {
                            chestWeight = cardIndex.selectionWeight;
                        }
                        if (cardIndex.spawnCard.name.Contains("CategoryChest"))
                        {
                            // Logger.LogError("Found CategoryChest " + cardIndex.spawnCard.name);

                            categoryChestsFound++;
                            if (categoryChestsFound % 2 == 0)
                            {
                                cardIndex.selectionWeight = Mathf.RoundToInt(cardIndex.selectionWeight * chestWeight * Run.instance.treasureRng.RangeFloat(2.5f, 4f));
                            }
                            else
                            {
                                cardIndex.selectionWeight = Mathf.RoundToInt(cardIndex.selectionWeight * chestWeight * Run.instance.treasureRng.RangeFloat(6f, 9f));
                            }
                        }
                    }
                }
            }
        }

        private int GetTotalCategoryChestCount(DirectorCardCategorySelection.Category[] categories)
        {
            var count = 0;
            for (var i = 0; i < categories.Length; i++)
            {
                var categoryIndex = categories[i];
                for (var j = 0; j < categoryIndex.cards.Length; j++)
                {
                    var cardIndex = categoryIndex.cards[j];
                    if (cardIndex.spawnCard.name.Contains("CategoryChest"))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
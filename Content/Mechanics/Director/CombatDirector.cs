﻿namespace WellRoundedBalance.Mechanics.Director
{
    internal class CombatDirector : MechanicBase<CombatDirector>
    {
        public override string Name => ":: Mechanics :::: Combat Director";

        [ConfigField("Minimum Reroll Spawn Interval Multiplier", "", 1.65f)]
        public static float minimumRerollSpawnIntervalMultiplier;

        [ConfigField("Credit Multiplier", "", 1.35f)]
        public static float creditMultiplier;

        [ConfigField("Elite Bias Multiplier", "", 0.9f)]
        public static float eliteBiasMultiplier;

        [ConfigField("Credit Multiplier for each Mountain Shrine", "", 1.05f)]
        public static float creditMultiplierForEachMountainShrine;

        [ConfigField("Gold and Experience Multiplier for each Mountain Shrine", "", 0.9f)]
        public static float goldAndExperienceMultiplierForEachMountainShrine;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            On.RoR2.CombatDirector.OnEnable += CombatDirector_OnEnable;
            On.RoR2.CombatDirector.OnDisable += CombatDirector_OnDisable;
            //On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;

            // enemy variety
            On.RoR2.CombatDirector.Simulate += CombatDirector_Simulate;
            On.RoR2.Chat.SendBroadcastChat_ChatMessageBase += ChangeMessage;
            On.RoR2.BossGroup.UpdateBossMemories += UpdateTitle;
        }
        private void CombatDirector_Simulate(On.RoR2.CombatDirector.orig_Simulate orig, RoR2.CombatDirector self, float deltaTime)
        {
            orig(self, deltaTime);

            if (self.currentMonsterCard != null)
            {
                float monsterSpawnTimer = self.monsterSpawnTimer;
                int spawnCountInCurrentWave = self.spawnCountInCurrentWave;

                if (self == TeleporterInteraction.instance?.bossDirector)
                    self.SetNextSpawnAsBoss();
                else if (self.finalMonsterCardsSelection != null && self.finalMonsterCardsSelection.Count > 0)
                    self.PrepareNewMonsterWave(self.finalMonsterCardsSelection.Evaluate(self.rng.nextNormalizedFloat));

                self.monsterSpawnTimer = monsterSpawnTimer;
                self.spawnCountInCurrentWave = spawnCountInCurrentWave;
            }
        }


        private void ChangeMessage(On.RoR2.Chat.orig_SendBroadcastChat_ChatMessageBase orig, ChatMessageBase message)
        {
            if (message is Chat.SubjectFormatChatMessage chat && chat.paramTokens?.Any() is true && chat.baseToken is "SHRINE_COMBAT_USE_MESSAGE")
                chat.paramTokens[0] = Language.GetString("LOGBOOK_CATEGORY_MONSTER").ToLower();

            // Replace with generic message since shrine will have multiple enemy types
            orig(message);
        }

        private void UpdateTitle(On.RoR2.BossGroup.orig_UpdateBossMemories orig, BossGroup self)
        {
            orig(self);

            var health = new Dictionary<(string, string), float>();
            float maximum = 0;

            for (int i = 0; i < self.bossMemoryCount; ++i)
            {
                var body = self.bossMemories[i].cachedBody;
                if (!body) 
                    continue;

                var component = body.healthComponent;
                if (!component || !component.alive)
                    continue;

                string name = Util.GetBestBodyName(body.gameObject);
                string subtitle = body.GetSubtitle();

                var key = (name, subtitle);
                if (!health.ContainsKey(key))
                    health[key] = 0;

                health[key] += component.combinedHealth + component.missingCombinedHealth * 4;

                // Use title for enemy with the most total health and damage received
                if (health[key] > maximum)
                    maximum = health[key];
                else
                    continue;

                if (string.IsNullOrEmpty(subtitle))
                    subtitle = Language.GetString("NULL_SUBTITLE");

                self.bestObservedName = name;
                self.bestObservedSubtitle = $"<sprite name=\"CloudLeft\" tint=1> {subtitle} <sprite name=\"CloudRight\" tint=1>";
            }
        }

        private bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, RoR2.CombatDirector self,
            SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance,
            bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            self.monsterCredit += (0.3f * Mathf.Sqrt(Run.instance.difficultyCoefficient)) + ((spawnCard.directorCreditCost - 50) / 50);
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }

        private void CombatDirector_OnDisable(On.RoR2.CombatDirector.orig_OnDisable orig, RoR2.CombatDirector self)
        {
            self.minRerollSpawnInterval *= minimumRerollSpawnIntervalMultiplier;
            self.maxRerollSpawnInterval *= minimumRerollSpawnIntervalMultiplier;
            orig(self);
        }

        private void CombatDirector_OnEnable(On.RoR2.CombatDirector.orig_OnEnable orig, RoR2.CombatDirector self)
        {
            self.maximumNumberToSpawnBeforeSkipping = 4;
            self.maxConsecutiveCheapSkips = 4;
            self.minRerollSpawnInterval /= minimumRerollSpawnIntervalMultiplier;
            self.maxRerollSpawnInterval /= minimumRerollSpawnIntervalMultiplier;
            self.creditMultiplier *= creditMultiplier;
            self.eliteBias *= eliteBiasMultiplier;
            var teleporter = TeleporterInteraction.instance;
            if (teleporter != null)
            {
                for (var i = 0; i < teleporter.shrineBonusStacks; i++)
                {
                    self.creditMultiplier *= creditMultiplierForEachMountainShrine;// * Mathf.Pow(Run.instance.participatingPlayerCount, 0.05f);
                    self.expRewardCoefficient *= goldAndExperienceMultiplierForEachMountainShrine;
                    self.goldRewardCoefficient *= goldAndExperienceMultiplierForEachMountainShrine;
                }
            }
            orig(self);
        }
    }
}
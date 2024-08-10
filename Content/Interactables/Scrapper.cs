using EntityStates.Scrapper;
using RoR2.Hologram;

namespace WellRoundedBalance.Interactables
{
    public class Scrapper : InteractableBase<Scrapper>
    {
        public override string Name => ":: Interactables ::::: Scrapper";

        [ConfigField("Max Spawns Per Stage", "", 1)]
        public static int maxSpawnsPerStage;

        [ConfigField("Max Uses", "", 3)]
        public static int maxUses;

        [ConfigField("Max Scrap Count Per Use", "", 1)]
        public static int maxScrapCountPerUse;

        [ConfigField("Weight Multiplier", "", 0.5f)]
        public static float weightMultiplier;

        public static Dictionary<GameObject, int> uses;

        public static InteractableSpawnCard scrapper = Utils.Paths.InteractableSpawnCard.iscScrapper.Load<InteractableSpawnCard>();

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            var scrapper = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset").WaitForCompletion();
            scrapper.maxSpawnsPerStage = maxSpawnsPerStage;
            scrapper.directorCreditCost = 0;

            var scrapperGO = Utils.Paths.GameObject.Scrapper.Load<GameObject>();
            var counter = scrapperGO.AddComponent<ScrapperUseCounter>();
            counter.useCount = maxUses;
            var hologram = scrapperGO.AddComponent<HologramProjector>();
            hologram.displayDistance = 15f;
            hologram.hologramPivot = scrapperGO.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0); // head.end heheheha
            hologram.hologramPivot.transform.localScale *= 2f;
            hologram.hologramPivot.transform.localPosition += new Vector3(0f, 1f, 0f);
            hologram.disableHologramRotation = false;
            var hologram2 = scrapperGO.AddComponent<ScrapperHologram>();

            uses = [];

            Stage.onServerStageComplete += Stage_onServerStageComplete;
            On.EntityStates.Scrapper.ScrapperBaseState.OnEnter += ScrapperBaseState_OnEnter;
            On.EntityStates.Scrapper.Scrapping.OnEnter += Scrapping_OnEnter;
            On.RoR2.ScrapperController.Start += ScrapperController_Start;
            On.RoR2.ClassicStageInfo.Start += ClassicStageInfo_Start;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            var scrappers = GameObject.FindObjectsOfType<ScrapperController>();
            if (scrappers?.Any() == true)
            {
                foreach (var controller in scrappers)
                {
                    if (controller && controller.TryGetComponent<ScrapperUseCounter>(out var counter))
                    {
                        counter.useCount = maxUses * Run.instance.participatingPlayerCount;
                    }
                }
            }
        }

        private void ClassicStageInfo_Start(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                var categories = self.interactableCategories.categories;
                for (var i = 0; i < categories.Length; i++)
                {
                    var categoryIndex = categories[i];
                    for (var j = 0; j < categoryIndex.cards.Length; j++)
                    {
                        var cardIndex = categoryIndex.cards[j];
                        if (cardIndex.spawnCard == scrapper)
                        {
                            cardIndex.selectionWeight = Mathf.RoundToInt(cardIndex.selectionWeight * weightMultiplier);
                            break;
                        }
                    }
                }
            }

        }

        private void ScrapperController_Start(On.RoR2.ScrapperController.orig_Start orig, ScrapperController self)
        {
            self.maxItemsToScrapAtATime = maxScrapCountPerUse;

            orig(self);
        }

        private void Scrapping_OnEnter(On.EntityStates.Scrapper.Scrapping.orig_OnEnter orig, Scrapping self)
        {
            if (self.outer)
            {
                var scrapper = self.outer.gameObject;
                if (scrapper != null && uses.ContainsKey(scrapper))
                {
                    uses[scrapper]--;
                    var counter = scrapper.GetComponent<ScrapperUseCounter>();
                    if (counter)
                    {
                        counter.useCount--;
                    }
                }
            }
            orig(self);
        }

        private void Stage_onServerStageComplete(Stage stage)
        {
            uses.Clear();
        }

        private void ScrapperBaseState_OnEnter(On.EntityStates.Scrapper.ScrapperBaseState.orig_OnEnter orig, ScrapperBaseState self)
        {
            GameObject scrapper = null;
            if (self.outer)
            {
                scrapper = self.outer.gameObject;
                if (!uses.ContainsKey(scrapper))
                {
                    uses.Add(scrapper, maxUses * Run.instance.livingPlayerCount);
                }
            }

            orig(self);

            if (scrapper && uses[scrapper] <= 0 && self.outer.TryGetComponent<PickupPickerController>(out var ppc))
            {
                ppc.SetAvailable(false);
            }
        }
    }

    public class ScrapperUseCounter : MonoBehaviour
    {
        public int useCount;
        public float timer;
        public float explosionInterval = 0.7f;
        public float deleteInterval = 0.8f;

        private void FixedUpdate()
        {
            if (useCount <= 0 && NetworkServer.active)
            {
                timer += Time.fixedDeltaTime;
                if (timer >= explosionInterval)
                {
                    EffectManager.SpawnEffect(Utils.Paths.GameObject.ExplosionVFX.Load<GameObject>(), new EffectData
                    {
                        origin = transform.position,
                        scale = 3f
                    }, true);
                }
                if (timer >= deleteInterval)
                {
                    NetworkServer.Destroy(base.gameObject);
                }
            }
        }
    }

    public class ScrapperHologram : MonoBehaviour, IHologramContentProvider
    {
        public ScrapperUseCounter counter;

        private void Start()
        {
            counter = GetComponent<ScrapperUseCounter>();
        }

        GameObject IHologramContentProvider.GetHologramContentPrefab()
        {
            return PlainHologram.hologramContentPrefab;
        }

        bool IHologramContentProvider.ShouldDisplayHologram(GameObject viewer)
        {
            if (!viewer)
                return false;

            var distance = Vector3.Distance(viewer.transform.position, gameObject.transform.position);
            return distance <= 15f;
        }

        void IHologramContentProvider.UpdateHologramContent(GameObject self)
        {
            if (self && counter && self.TryGetComponent<PlainHologram.PlainHologramContent>(out var hologram))
            {
                hologram.text = counter.useCount + (counter.useCount == 1 ? " use left" : " uses left");
                hologram.color = Color.white;
            }
        }
    }
}
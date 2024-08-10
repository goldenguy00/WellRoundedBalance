using MonoMod.Cil;
using System;
using RoR2.UI;
using UnityEngine.UI;

namespace WellRoundedBalance.Interactables
{
    internal class VoidCradle : InteractableBase<VoidCradle>
    {
        public override string Name => ":: Interactables :::: Void Cradle";
        public CostTypeIndex costTypeIndex = (CostTypeIndex)19;
        public CostTypeDef def;
        public GameObject optionPanel;
        public static InteractableSpawnCard vradle;

        [ConfigField("Curse Gain", "Decimal.", 0.1f)]
        public static float curseGain;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            var VoidCradle = Utils.Paths.GameObject.VoidChest.Load<GameObject>();
            var interaction = VoidCradle.GetComponent<PurchaseInteraction>();
            interaction.costType = costTypeIndex;
            interaction.cost = 0;
            interaction.contextToken = "WRB_VOIDCHEST_CONTEXT";
            VoidCradle.RemoveComponent<ChestBehavior>();
            VoidCradle.AddComponent<NetworkUIPromptController>();
            VoidCradle.AddComponent<PickupIndexNetworker>();
            var controller = VoidCradle.AddComponent<PickupPickerController>();
            controller.cutoffDistance = 10;
            optionPanel = Utils.Paths.GameObject.OptionPickerPanel.Load<GameObject>().InstantiateClone("VoidCradleOptionPicker", false);
            var bg = optionPanel.transform.Find("MainPanel").Find("Juice").Find("BG, Colored");
            var bgCenter = bg.Find("BG, Colored Center");
            bg.GetComponent<Image>().color = new Color32(237, 127, 205, 255);
            bgCenter.GetComponent<Image>().color = new Color32(237, 127, 205, 255);
            var label = optionPanel.transform.Find("MainPanel").Find("Juice").Find("Label");
            label.GetComponent<HGTextMeshProUGUI>().text = "Awaiting Transmutation...";
            controller.panelPrefab = optionPanel;
            LanguageAPI.Add("WRB_VOIDCHEST_CONTEXT", "Open?");
            VoidCradle.AddComponent<CradleManager>();
            VoidCradle.RemoveComponent<ScriptedCombatEncounter>();

            vradle = Utils.Paths.InteractableSpawnCard.iscVoidChest.Load<InteractableSpawnCard>();

            On.RoR2.CostTypeCatalog.Init += On_CostTypeCatalog_Init;
            IL.RoR2.CostTypeCatalog.Init += IL_CostTypeCatalog_Init;
            On.RoR2.UI.PickupPickerPanel.OnCreateButton += this.PickupPickerPanel_OnCreateButton;

            On.RoR2.SceneDirector.SelectCard += SceneDirector_SelectCard;

            On.RoR2.PickupPickerController.OnInteractionBegin += this.PickupPickerController_OnInteractionBegin;
        }

        private void PickupPickerController_OnInteractionBegin(On.RoR2.PickupPickerController.orig_OnInteractionBegin orig, PickupPickerController self, Interactor activator)
        {
            // dont run this method on cradles since cradlemanager implements its own version
            if (!self.gameObject.name.Contains("VoidChest"))
                orig(self, activator);
        }

        private void PickupPickerPanel_OnCreateButton(On.RoR2.UI.PickupPickerPanel.orig_OnCreateButton orig, PickupPickerPanel self, int index, MPButton button)
        {
            orig(self, index, button);

            if (!self.gameObject.name.Contains("VoidChest"))
                return;

            var pickupDef = PickupCatalog.GetPickupDef(self.pickerController.options[index].pickupIndex);
            if (pickupDef is null)
                return;

            var def = ItemCatalog.GetItemDef(GetCorruption(pickupDef.itemIndex));
            if (!def)
                return;

            var tp = button.gameObject.AddComponent<TooltipProvider>();
            tp.SetContent(new TooltipContent
            {
                bodyColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem),
                titleColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItemDark),
                overrideTitleText = "Transmutes into: " + Language.GetString(def.nameToken),
                bodyToken = def.descriptionToken,
                titleToken = "gdfgdfgdfghgh"
            });
        }

        private void IL_CostTypeCatalog_Init(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(15)))
            {
                c.EmitDelegate<Func<int, int>>((c) => 20);
            }
            else
            {
                Logger.LogError("Failed to apply CostTypeCatalog IL hook");
            }
        }

        private void On_CostTypeCatalog_Init(On.RoR2.CostTypeCatalog.orig_Init orig)
        {
            orig();
            CostTypeCatalog.Register(costTypeIndex, new()
            {
                buildCostString = (def, c) => c.stringBuilder.Append("<style=cDeath>10% Curse</style>"),
                isAffordable = (def, c) => HasAtLeastOneItem(c.activator.GetComponent<CharacterBody>().inventory),
                payCost = (def, c) => { }
            });
        }

        private DirectorCard SceneDirector_SelectCard(On.RoR2.SceneDirector.orig_SelectCard orig, SceneDirector self, WeightedSelection<DirectorCard> deck, int max)
        {
            DirectorCard card = null;
            for (var i = 0; i < 10; i++)
            {
                var next = orig(self, deck, max);
                if (next != null && next.spawnCard && next.spawnCard == vradle && ShouldBlockCradles())
                {
                    // Main.WRBLogger.LogError("No players have corruptible items, blocking vradle spawn");
                    continue;
                }
                card = next;
            }

            return card == null ? orig(self, deck, max) : card; // failsafe in the event cradles are the literal only thing it can afford (eg. void locus)
        }

        public static bool ShouldBlockCradles()
        {
            foreach (var pmc in PlayerCharacterMasterController.instances)
            {
                if (pmc.master && HasAtLeastOneItem(pmc.master.inventory))
                {
                    // Main.WRBLogger.LogError("Should Block Cradles returned false");
                    return false;
                }
            }

            return true;
        }

        public static bool HasAtLeastOneItem(Inventory inventory)
        {
            foreach (var index in inventory.itemAcquisitionOrder)
            {
                if (IsCorruptible(index))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsCorruptible(ItemIndex index)
        {
            if (ItemCatalog.GetItemDef(index).tier == ItemTier.Boss)
            {
                // boss items cant be selected by vradles so dont return true
                // Main.WRBLogger.LogError("ItemTier was boss");
                return false;
            }
            var item = RoR2.Items.ContagiousItemManager.GetTransformedItemIndex(index);
            return item != ItemIndex.None;
        }

        public static ItemIndex GetCorruption(ItemIndex index)
        {
            return RoR2.Items.ContagiousItemManager.GetTransformedItemIndex(index);
        }

        private class CradleManager : MonoBehaviour
        {
            public float timer;
            public float interval = 1f;
            public bool wasDisabled = false;
            public PurchaseInteraction interaction => GetComponent<PurchaseInteraction>();
            public PickupPickerController controller => GetComponent<PickupPickerController>();
            public List<PickupPickerController.Option> options = [];
            public bool hasSet = false;

            private void Start()
            {
                interaction.onPurchase.AddListener(OnPurchase);
                controller.onPickupSelected.AddListener(Corrupt);
            }

            public void Corrupt(int i)
            {
                PickupIndex index = new(i);
                var def = index.itemIndex;
                var interactor = interaction.lastActivator;
                var body = interactor.GetComponent<CharacterBody>();
                var c = body.inventory.GetItemCount(def);
                body.inventory.RemoveItem(def, c);
                body.inventory.GiveItem(GetCorruption(def), c);
                CharacterMasterNotificationQueue.PushItemTransformNotification(body.master, def, GetCorruption(def), CharacterMasterNotificationQueue.TransformationType.ContagiousVoid);
                interaction.SetAvailable(false);
                var amount = body.healthComponent.fullCombinedHealth * curseGain;
                float curse = Mathf.RoundToInt(amount / body.healthComponent.fullCombinedHealth * 100f);
                controller.networkUIPromptController.SetParticipantMaster(null);

                for (var j = 0; j < curse; j++)
                {
                    body.AddBuff(RoR2Content.Buffs.PermanentCurse);
                }

                var machine = GetComponent<EntityStateMachine>();
                if (machine)
                {
                    machine.SetNextState(new EntityStates.Barrel.Opening());
                }
            }

            public void OnPurchase(Interactor interactor)
            {
                if (interactor.GetComponent<CharacterBody>())
                {
                    // Main.WRBLogger.LogError("Running OnPurchase");
                    var body = interactor.GetComponent<CharacterBody>();
                    var c = 0;
                    for (var i = 0; i < options.Count; i++)
                    {
                        var opt = options[i];
                        if (body.inventory.GetItemCount(opt.pickupIndex.itemIndex) <= 0)
                        {
                            options.Remove(opt);
                        }
                    }
                    if (options.Count == 0)
                    {
                        // Main.WRBLogger.LogError("Options count 0, regenerating.");
                        hasSet = false;
                    }
                    foreach (var index in body.inventory.itemAcquisitionOrder.OrderBy(x => UnityEngine.Random.value))
                    {
                        if (hasSet)
                        {
                            continue;
                        }
                        if (IsCorruptible(index))
                        {
                            var def = ItemCatalog.GetItemDef(index);
                            if (def.tier == ItemTier.Boss || c >= 3)
                            {
                                continue;
                            }
                            options.Add(new PickupPickerController.Option
                            {
                                pickupIndex = PickupCatalog.FindPickupIndex(index),
                                available = true
                            });
                            c++;
                        }
                    }

                    if (options.Count >= 1)
                    {
                        hasSet = true;
                        // Debug.Log("starting UI");
                        controller.SetOptionsInternal([.. options]);
                        controller.SetOptionsServer([.. options]);
                        controller.onServerInteractionBegin.Invoke(interactor);
                        controller.networkUIPromptController.SetParticipantMasterFromInteractor(interactor);
                    }
                    interaction.SetAvailableTrue();
                }
            }

            public void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if (timer >= interval)
                {
                    var teleporter = TeleporterInteraction.instance;
                    if (teleporter && teleporter.activationState == TeleporterInteraction.ActivationState.Charged && !wasDisabled)
                    {
                        EffectManager.SpawnEffect(Utils.Paths.GameObject.ExplodeOnDeathVoidExplosionEffect.Load<GameObject>(), new EffectData
                        {
                            origin = transform.position,
                            scale = 3f
                        }, true);

                        gameObject.SetActive(false);

                        wasDisabled = true;
                    }
                }
            }
        }
    }
}
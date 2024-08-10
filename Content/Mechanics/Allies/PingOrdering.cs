using EntityStates.AI;
using RoR2.UI;
using System;
using BepInEx.Configuration;

namespace WellRoundedBalance.Mechanics.Allies
{
    internal class PingOrdering : MechanicBase<PingOrdering>
    {
        [ConfigField("Order Button", "", KeyCode.Mouse3)]
        public static KeyCode keyCode;

        public static Dictionary<CharacterMaster, List<AwaitOrders>> subordinateDict = [];

        public static GameObject PingPrefab;

        public static KeyboardShortcut button;

        public override string Name => ":: Mechanics :::::::::::::: Ping Ordering";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            //button = new KeyboardShortcut(keyCode);
            //IL.RoR2.UI.PingIndicator.RebuildPing += PingIndicator_RebuildPing;
            //On.RoR2.Highlight.GetColor += Highlight_GetColor;
            //On.RoR2.PlayerCharacterMasterController.CheckPinging += PlayerCharacterMasterController_CheckPinging;
        }

        private void PlayerCharacterMasterController_CheckPinging(On.RoR2.PlayerCharacterMasterController.orig_CheckPinging orig, PlayerCharacterMasterController self)
        {
            orig(self);
            if (button.IsPressed())
            {
                var minionGroup = MinionOwnership.MinionGroup.FindGroup(self.master.netId);
                if (minionGroup != null)
                {
                    foreach (var minion in minionGroup.members)
                    {
                        if (minion && minion.TryGetComponent<EntityStateMachine>(out var stateMachine))
                        {
                            var state = new AwaitOrders();
                            if (subordinateDict.TryGetValue(self.master, out var val))
                                val.Add(state);
                            else
                                subordinateDict.Add(self.master, [state]);

                            stateMachine.SetState(state);
                        }
                    }
                }
            }
        }

        private Color Highlight_GetColor(On.RoR2.Highlight.orig_GetColor orig, Highlight self)
        {
            var ret = orig(self);
            if (ret == Color.magenta && self.highlightColor == (Highlight.HighlightColor)(669))
            {
                return Color.cyan + new Color(0.01f, 0, 0);
            }
            return ret;
        }

        private void PingIndicator_RebuildPing(ILContext il)
        {
            ILCursor c = new(il);

            c.GotoNext(x => x.MatchLdstr("PLAYER_PING_ENEMY"));

            c.FindNext(out var cList, x => x.MatchBr(out _));
            var label = cList[0].MarkLabel();

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((PingIndicator self) =>
            {
                var ownerMaster = self.pingOwner.GetComponent<CharacterMaster>();
                var targetMaster = self.pingTarget.GetComponent<CharacterBody>().master;

                if (subordinateDict.TryGetValue(ownerMaster, out var orderList) && orderList.Any())
                {
                    var isEnemy = TeamManager.IsTeamEnemy(ownerMaster.teamIndex, targetMaster.teamIndex);

                    foreach(var order in orderList)
                    {
                        order.SubmitOrder(isEnemy ? AwaitOrders.Orders.Attack : AwaitOrders.Orders.Assist, self.pingTarget);
                    }
                    orderList.Clear();
                    subordinateDict.Remove(ownerMaster);
                    //Chat.AddMessage(string.Format(Language.GetString("PING_ORDER_ENEMY"),self.pingText.text,Util.GetBestBodyName(subordinateDict[ownerMaster].characterBody),Util.GetBestBodyName(targetMaster.characterBody));
                    self.pingDuration = 1f;

                    return true;
                }
                
                if (targetMaster.TryGetComponent<BaseAI>(out var ai) && ai.leader.characterBody?.master == ownerMaster)
                {
                    var pctrl = self.pingOwner.GetComponent<PingerController>();
                    pctrl.pingIndicator = null;
                    pctrl.pingStock++;

                    var orders = new AwaitOrders(self);
                    subordinateDict[ownerMaster] = [orders];
                    targetMaster.GetComponent<EntityStateMachine>().SetState(orders);

                    self.pingColor = Color.cyan;
                    self.pingDuration = float.PositiveInfinity;
                    self.enemyPingGameObjects[0].GetComponent<SpriteRenderer>().color = Color.cyan;
                    self.pingHighlight.highlightColor = (Highlight.HighlightColor)669;

                    return true;
                }

                return false;
            });

            c.Emit(OpCodes.Brtrue, label);

            c.Index = 0;
            c.GotoNext(x => x.MatchLdstr("PLAYER_PING_DEFAULT"));

            c.FindNext(out cList, x => x.MatchBr(out _));
            label = cList[0].MarkLabel();

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((PingIndicator self) =>
            {
                var ownerMaster = self.pingOwner.GetComponent<CharacterMaster>();
                if (subordinateDict.TryGetValue(ownerMaster, out var ordersList))
                {
                    foreach (var order in ordersList)
                    {
                        order.SubmitOrder(AwaitOrders.Orders.Move, null, self.pingOrigin);
                    }
                    subordinateDict.Remove(ownerMaster);
                    //Chat.AddMessage(string.Format(Language.GetString("PING_ORDER_ENEMY"),self.pingText.text,Util.GetBestBodyName(subordinateDict[ownerMaster].characterBody),Util.GetBestBodyName(targetMaster.characterBody));
                    self.pingDuration = 1f;

                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
    }

    public class AwaitOrders(PingIndicator pInd = null) : BaseAIState
    {
        public enum Orders
        {
            None,
            Move,
            Attack,
            Assist
        }

        public Orders order;

        public Vector3? targetPosition;

        public GameObject target;

        public float sprintThreshold;

        public PingIndicator ping = pInd;

        public override void OnEnter()
        {
            base.OnEnter();
            if (!ping)
            {
                ping = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/PingIndicator")).GetComponent<PingIndicator>();
                ping.pingOwner = characterMaster.minionOwnership?.ownerMaster?.gameObject;
                ping.pingOrigin = body.transform.position;
                ping.pingNormal = Vector3.zero;
                ping.pingTarget = body.gameObject;
                ping.transform.position = body.transform.position;
                ping.positionIndicator.targetTransform = body.transform;
                ping.positionIndicator.defaultPosition = body.transform.position;
                ping.targetTransformToFollow = body.coreTransform;
                ping.pingDuration = float.PositiveInfinity;
                ping.fixedTimer = float.PositiveInfinity;
                ping.pingColor = Color.cyan;
                ping.pingText.color = ping.textBaseColor * ping.pingColor;
                ping.pingText.text = Util.GetBestMasterName(characterMaster.minionOwnership?.ownerMaster);
                ping.pingObjectScaleCurve.enabled = false;
                ping.pingObjectScaleCurve.enabled = true;
                ping.pingHighlight.highlightColor = (Highlight.HighlightColor)(669);
                ping.pingHighlight.targetRenderer = body.modelLocator?.modelTransform?.GetComponentInChildren<CharacterModel>()?.baseRendererInfos?.First((r) => !r.ignoreOverlays).renderer;
                ping.pingHighlight.strength = 1f;
                ping.pingHighlight.isOn = true;
                foreach (var gameObject in ping.enemyPingGameObjects)
                {
                    gameObject.SetActive(true);
                    var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer)
                    {
                        spriteRenderer.color = Color.cyan;
                    }
                    var particleSystem = gameObject.GetComponent<ParticleSystem>();
                    if (particleSystem)
                    {
                        var main = particleSystem.main;
                        var startColor = main.startColor;
                        startColor.colorMax = Color.cyan;
                        startColor.colorMin = Color.cyan;
                        startColor.color = Color.cyan;
                    }
                }
            }
            sprintThreshold = ai.skillDrivers.FirstOrDefault((drive) => drive.shouldSprint)?.minDistanceSqr ?? float.PositiveInfinity;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!target && !targetPosition.HasValue)
                AimAt(ref bodyInputs, ai.leader);

            switch (order)
            {
                case Orders.Attack:
                {
                    ai.currentEnemy.gameObject = target;
                    ai.enemyAttention = ai.enemyAttentionDuration;
                    outer.SetNextState(new EntityStates.AI.Walker.Combat());
                    break;
                }
                case Orders.Move:
                {
                    if (!body || body.moveSpeed == 0)
                        outer.SetNextStateToMain();

                    var agent = ai.broadNavigationAgent;
                    agent.currentPosition = ai.body.footPosition;
                    
                    ai.SetGoalPosition(targetPosition);
                    ai.localNavigator.targetPosition = agent.output.nextPosition ?? ai.localNavigator.targetPosition;

                    if (!agent.output.targetReachable)
                    {
                        agent.InvalidatePath();
                    }
                    ai.localNavigator.Update(cvAIUpdateInterval.value);
                    bodyInputs.moveVector = ai.localNavigator.moveVector;
                    var sqrMagnitude = (base.body.footPosition - targetPosition.Value).sqrMagnitude;
                    bodyInputs.pressSprint = sqrMagnitude > sprintThreshold;
                    if (ai.localNavigator.wasObstructedLastUpdate)
                        base.ModifyInputsForJumpIfNeccessary(ref bodyInputs);
                    var num = base.body.radius * base.body.radius * 4;
                    if (sqrMagnitude < num)
                        outer.SetNextStateToMain();
                    break;
                }
                case Orders.Assist:
                {
                    ai.buddy.gameObject = target;
                    ai.customTarget.gameObject = target;
                    outer.SetNextState(new EntityStates.AI.Walker.Combat());
                    break;
                }
            };
        }

        public override void OnExit()
        {
            base.OnExit();
            if (ping)
            {
                ping.fixedTimer = 0f;
            }
        }

        public void SubmitOrder(Orders command, GameObject target, Vector3? targetPosition = null)
        {
            order = command;
            this.target = target;
            this.targetPosition = targetPosition;
            if (targetPosition.HasValue)
            {
                var agent = ai.broadNavigationAgent;
                agent.goalPosition = targetPosition;
                agent.InvalidatePath();
            }
        }
    }
}
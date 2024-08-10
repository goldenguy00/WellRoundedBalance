namespace WellRoundedBalance.Mechanics.Bosses
{
    public class OnKill : MechanicBase<OnKill>
    {
        public override string Name => ":: Mechanics ::::: Boss On Kill Thresholds";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            RoR2.BodyCatalog.availability.CallWhenAvailable(PopulateAcceptableBodies);
        }

        private void PopulateAcceptableBodies()
        {
            Utils.Paths.GameObject.BrotherBody.Load<GameObject>().AddComponent<OnKillThresholdManager>();
            Utils.Paths.GameObject.BrotherHauntBody.Load<GameObject>().AddComponent<OnKillThresholdManager>();

            Utils.Paths.GameObject.VoidRaidCrabBody.Load<GameObject>().AddComponent<OnKillThresholdManager>();

            Utils.Paths.GameObject.MiniVoidRaidCrabBodyBase.Load<GameObject>().AddComponent<OnKillThresholdManager>();
            Utils.Paths.GameObject.MiniVoidRaidCrabBodyPhase1.Load<GameObject>().AddComponent<OnKillThresholdManager>();
            Utils.Paths.GameObject.MiniVoidRaidCrabBodyPhase2.Load<GameObject>().AddComponent<OnKillThresholdManager>();
            Utils.Paths.GameObject.MiniVoidRaidCrabBodyPhase3.Load<GameObject>().AddComponent<OnKillThresholdManager>();

            Utils.Paths.GameObject.ScavLunar1Body.Load<GameObject>().AddComponent<OnKillThresholdManager>();
            Utils.Paths.GameObject.ScavLunar2Body.Load<GameObject>().AddComponent<OnKillThresholdManager>();
            Utils.Paths.GameObject.ScavLunar3Body.Load<GameObject>().AddComponent<OnKillThresholdManager>();
            Utils.Paths.GameObject.ScavLunar4Body.Load<GameObject>().AddComponent<OnKillThresholdManager>();
        }

        private class OnKillThresholdManager : MonoBehaviour, IOnTakeDamageServerReceiver
        {
            private HealthComponent healthComponent;
            private CharacterBody characterBody;

            private List<float> activeThresholds;

            private void Start()
            {
                healthComponent = base.GetComponent<HealthComponent>();
                characterBody = base.GetComponent<CharacterBody>();

                activeThresholds =
                [
                    0.8f,
                    0.65f,
                    0.5f,
                    0.35f,
                    0.2f,
                    0.05f
                ];

                // reset this since it only checks on hc awake
                healthComponent.onTakeDamageReceivers = base.GetComponents<IOnTakeDamageServerReceiver>();
            }

            public void OnTakeDamageServer(DamageReport report)
            {
                if (report.victimBody == characterBody && report.attackerBody)
                {
                    List<float> completed = [];

                    foreach (var threshold in activeThresholds)
                    {
                        if (healthComponent.health < (healthComponent.fullCombinedHealth * threshold))
                        {
                            completed.Add(threshold);
                        }
                    }

                    foreach (var threshold in completed)
                    {
                        activeThresholds.Remove(threshold);
                        TriggerKill(report);
                    }
                }
            }

            private void TriggerKill(DamageReport report)
            {
                if (!NetworkServer.active || !GlobalEventManager.instance)
                {
                    return;
                }

                DamageInfo info = new()
                {
                    attacker = report.attacker,
                    crit = report.damageInfo.crit,
                    damage = report.damageDealt,
                    damageType = report.damageInfo.damageType,
                    position = characterBody.corePosition,
                    damageColorIndex = report.damageInfo.damageColorIndex,
                    procCoefficient = report.damageInfo.procCoefficient,
                };

                // no happiest mask
                info.procChainMask.AddProc((ProcType)12096721);

                GlobalEventManager.instance.OnCharacterDeath(new(info, healthComponent, info.damage, healthComponent.combinedHealth));
            }
        }
    }
}
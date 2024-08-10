using BepInEx.Configuration;
using Mono.Cecil.Cil;
using Newtonsoft.Json.Utilities;
using R2API.MiscHelpers;
using RoR2;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using WellRoundedBalance.Achievements;
using WellRoundedBalance.Allies;
using WellRoundedBalance.Artifacts.New;
using WellRoundedBalance.Artifacts.Vanilla;
using WellRoundedBalance.Difficulties;
using WellRoundedBalance.Elites;
using WellRoundedBalance.Enemies;
using WellRoundedBalance.Enemies.All;
using WellRoundedBalance.Equipment;
using WellRoundedBalance.Gamemodes;
using WellRoundedBalance.Interactables;
using WellRoundedBalance.Items;
using WellRoundedBalance.Items.ConsistentCategories;
using WellRoundedBalance.Items.NoTier;
using WellRoundedBalance.Mechanics;
using WellRoundedBalance.Mechanics.Health;
using WellRoundedBalance.Misc;
using WellRoundedBalance.Projectiles;
using WellRoundedBalance.Survivors;

namespace WellRoundedBalance
{
    public static class Initialize
    {
        /*
        public static JobHandle handle;
        public static NativeArray<AchievementBase> result;

        public struct AchievementJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<AchievementBase> result = new(1, Allocator.TempJob);

            public void Execute()
            {
                Type type = result[0];

                AchievementBase based = (AchievementBase)Activator.CreateInstance(type);
                if (Validate(based))
                {
                    try
                    {
                        based.Init();
                    }
                    catch (Exception ex)
                    {
                        Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}");
                    }
                }
            }
        }
        */

        // bruh what the fuck is this shit I hate jobs I hate unity what the

        public static void Init()
        {
            var stopwatch = Stopwatch.StartNew();
            // Main.WRBLogger.LogError("init called");

            var types = typeof(Initialize).Assembly.GetTypes();

            FunnyLabel.Init();
            // Useless.Create();
            Buffs.Useless.Init();
            VoidBall.Init();
            BlazingProjectileVFX.Init();
            Molotov.Init();
            MolotovBig.Init();
            DucleusLaser.Init();
            TitanFist.Init();
            EarthQuakeWave.Init();
            GupSpike.Init();

            BetterItemCategories.Init();

            /*
            object achievementLock = new();
            object allyLock = new();
            object artifactAddLock = new();
            object artifactEditLock = new();
            object difficultyLock = new();
            object eliteLock = new();
            object enemyLock = new();
            object equipmentLock = new();
            object gamemodeLock = new();
            object interactableLock = new();
            object itemLock = new();
            object mechanicLock = new();
            object survivorLock = new();
            */

            if (Main.enableAchievements.Value)
            {
                var achievement = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(AchievementBase)));

                Main.WRBLogger.LogInfo("==+----------------==ACHIEVEMENTS==----------------+==");

                foreach (var type in achievement)
                {
                    var based = (AchievementBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }

                /*
                result = new NativeArray<AchievementBase>(1, Allocator.TempJob);

                AchievementJob achievementJob = new()
                {
                    result = result
                };

                handle = achievementJob.Schedule();

                // Sometime later in the frame, wait for the job to complete before accessing the results. bruh can I not do it outside a monobehaviour or something
                handle.Complete();

                result.Dispose();
                */
                // bruh
            }

            if (Main.enableAllies.Value)
            {
                var ally = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(AllyBase)));

                Main.WRBLogger.LogInfo("==+----------------==ALLIES==----------------+==");

                foreach (var type in ally)
                {
                    var based = (AllyBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableArtifactAdds.Value)
            {
                /*
                IEnumerable<Type> artifactAdd = Assembly.GetExecutingAssembly().GetTypes()
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactAddBase)));

                Main.WRBLogger.LogInfo("==+----------------==ARTIFACT ADDS==----------------+==");

                foreach (Type type in artifactAdd)
                {
                    ArtifactAddBase based = (ArtifactAddBase)Activator.CreateInstance(type);
                    if (ValidateArtifactAdd(based))
                    {
                        based.Init();
                        // disabled until icon is done
                    }
                }
                */
            }

            if (Main.enableArtifactEdits.Value)
            {
                var artifactEdit = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactEditBase)));

                Main.WRBLogger.LogInfo("==+----------------==ARTIFACT EDITS==----------------+==");

                foreach (var type in artifactEdit)
                {
                    var based = (ArtifactEditBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableDifficulties.Value)
            {
                var difficulty = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DifficultyBase)));

                Main.WRBLogger.LogInfo("==+----------------==DIFFICULTIES==----------------+==");

                foreach (var type in difficulty)
                {
                    var based = (DifficultyBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableElites.Value)
            {
                var elite = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteBase)));

                Main.WRBLogger.LogInfo("==+----------------==ELITES==----------------+==");

                foreach (var type in elite)
                {
                    var based = (EliteBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableEnemies.Value)
            {
                var enemy = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EnemyBase)));

                Main.WRBLogger.LogInfo("==+----------------==ENEMIES==----------------+==");

                foreach (var type in enemy)
                {
                    var based = (EnemyBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableEquipment.Value)
            {
                var equipment = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

                Main.WRBLogger.LogInfo("==+----------------==EQUIPMENT==----------------+==");

                foreach (var type in equipment)
                {
                    var based = (EquipmentBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableGamemodes.Value)
            {
                var gamemode = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(GamemodeBase)));

                Main.WRBLogger.LogInfo("==+----------------==GAMEMODES==----------------+==");

                foreach (var type in gamemode)
                {
                    var based = (GamemodeBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableInteractables.Value)
            {
                var interactable = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(InteractableBase)));

                Main.WRBLogger.LogInfo("==+----------------==INTERACTABLES==----------------+==");

                foreach (var type in interactable)
                {
                    var based = (InteractableBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableItems.Value)
            {
                var item = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

                Main.WRBLogger.LogInfo("==+----------------==ITEMS==----------------+==");

                foreach (var type in item)
                {
                    var based = (ItemBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }

                if (Items.Whites.PowerElixir.instance != null)
                    EmptyBottle.Init();
            }

            if (Main.enableMechanics.Value)
            {
                var mechanic = types
                                                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(MechanicBase)));

                Main.WRBLogger.LogInfo("==+----------------==MECHANICS==----------------+==");

                foreach (var type in mechanic)
                {
                    var based = (MechanicBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            if (Main.enableSurvivors.Value)
            {
                var survivor = types
                                                    .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SurvivorBase)));

                Main.WRBLogger.LogInfo("==+----------------==SURVIVORS==----------------+==");

                foreach (var type in survivor)
                {
                    var based = (SurvivorBase)Activator.CreateInstance(type);
                    if (Validate(based))
                    {
                        // try { based.Init(); } catch (Exception ex) { Main.WRBLogger.LogError($"Failed to initialize {type.Name}: {ex}"); }
                        based.Init();
                    }
                }
            }

            // FamilyEvents.Init();
            BleedCapInit.Init();

            Main.WRBLogger.LogDebug("==+----------------==INFO==----------------+==");
            Main.WRBLogger.LogDebug("Initialized " + SharedBase.initList.Count + " abstract classes");
            Main.WRBLogger.LogDebug("Initialized mod in " + stopwatch.ElapsedMilliseconds + "ms");
            Main.WRBLogger.LogDebug("Lotussy");
        }

        public static bool Validate<T>(T obj) where T : SharedBase
        {
            if (obj.isEnabled)
            {
                // Main.WRBLogger.LogError("validating T: " + obj);
                var enabledfr = GetConfigForType<T>().Bind(obj.Name, "Enable Changes?", true, "Vanilla is false").Value;
                if (enabledfr) return true;
                else ConfigManager.ConfigChanged = true;
            }
            return false;
        }

        private static ConfigFile GetConfigForType<T>() where T : SharedBase
        {
            return typeof(T).Name switch
            {
                nameof(AchievementBase) => Main.WRBAchievementConfig,
                nameof(AllyBase) => Main.WRBAllyConfig,
                nameof(ArtifactAddBase) => Main.WRBArtifactAddConfig,
                nameof(ArtifactEditBase) => Main.WRBArtifactEditConfig,
                nameof(DifficultyBase) => Main.WRBDifficultyConfig,
                nameof(EliteBase) => Main.WRBEliteConfig,
                nameof(EnemyBase) => Main.WRBEnemyConfig,
                nameof(EquipmentBase) => Main.WRBEquipmentConfig,
                nameof(GamemodeBase) => Main.WRBGamemodeConfig,
                nameof(InteractableBase) => Main.WRBInteractableConfig,
                nameof(ItemBase) => Main.WRBItemConfig,
                nameof(MechanicBase) => Main.WRBMechanicConfig,
                nameof(SurvivorBase) => Main.WRBSurvivorConfig,
                _ => throw new NotSupportedException($"Config not supported for type {typeof(T).Name}"),
            };
        }
    }
}
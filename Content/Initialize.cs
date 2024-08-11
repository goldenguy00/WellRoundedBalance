using BepInEx.Configuration;
using Mono.Cecil.Cil;
using Newtonsoft.Json.Utilities;
using R2API.MiscHelpers;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Networking.Match;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        // bruh what the fuck is this shit I hate jobs I hate unity what the

        private static List<SharedBase> Taska(object obj)
        {
            List<SharedBase> result = [];
            foreach (var type in obj as IEnumerable<Type>)
            {
                var based = Activator.CreateInstance(type) as SharedBase;
                if (Validate(based))
                {
                    ConfigManager.HandleConfigAttributes(based.GetType(), based.Name, based.Config);
                    result.Add(based);
                }
            }
            Main.WRBLogger.LogWarning("--------- FINISHED ------------");
            return result;
        }

        public static Task<Task<T>>[] Interleaved<T>(IEnumerable<Task<T>> tasks)
        {
            var inputTasks = tasks.ToList();

            var buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
            var results = new Task<Task<T>>[buckets.Length];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new TaskCompletionSource<Task<T>>();
                results[i] = buckets[i].Task;
            }

            int nextTaskIndex = -1;
            Action<Task<T>> continuation = completed =>
            {
                var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
                bucket.TrySetResult(completed);
            };

            foreach (var inputTask in inputTasks)
                inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return results;
        }

        public static void Init()
        {
            var stopwatch = Stopwatch.StartNew();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract);

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

            List<Task<List<SharedBase>>> t = [];
            Main.WRBLogger.LogWarning("Begin Tasks --------------------");

            if (Main.enableAchievements.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(AchievementBase)))));
            }
            if (Main.enableAllies.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(AllyBase)))));
            }
            if (Main.enableArtifactEdits.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(ArtifactEditBase)))));
            }
            if (Main.enableDifficulties.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(DifficultyBase)))));
            }
            if (Main.enableElites.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(EliteBase)))));
            }
            if (Main.enableEnemies.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(EnemyBase)))));
            }
            if (Main.enableEquipment.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(EquipmentBase)))));
            }
            if (Main.enableGamemodes.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(GamemodeBase)))));
            }
            if (Main.enableInteractables.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(InteractableBase)))));
            }
            if (Main.enableItems.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(ItemBase)))));
            }
            if (Main.enableMechanics.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(MechanicBase)))));
            }
            if (Main.enableSurvivors.Value)
            {
                t.Add(Task.Factory.StartNew(Taska, types.Where(t => t.IsSubclassOf(typeof(SurvivorBase)))));
            }
            foreach (var bucket in Interleaved(t))
            {
                var r = bucket.GetAwaiter().GetResult();

                foreach (var o in r.GetAwaiter().GetResult())
                {
                    o.Init();
                }
            }
            stopwatch.Stop();

            BleedCapInit.Init();

            Main.WRBLogger.LogDebug("==+----------------==INFO==----------------+==");
            Main.WRBLogger.LogDebug("Initialized " + SharedBase.initList.Count + " abstract classes");
            Main.WRBLogger.LogDebug("Initialized mod in " + stopwatch.Elapsed.Seconds + "m");
            Main.WRBLogger.LogDebug("Lotussy");
        }

        public static bool Validate<T>(T obj) where T : SharedBase
        {
            var cfg = obj.Config.Bind(obj.Name, "Enable Changes?", true, "Vanilla is false");
            cfg.SettingChanged += (sender, args) => obj.isEnabled = (bool)(args as SettingChangedEventArgs).ChangedSetting.BoxedValue;
            obj.isEnabled = cfg.Value;
            return obj.isEnabled;
        }
    }
}
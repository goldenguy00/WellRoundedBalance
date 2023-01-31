﻿using BepInEx.Configuration;
using System;
using UnityEngine;

namespace WellRoundedBalance.Mechanics.Monsters
{
    internal class BetterScaling
    {
        public static ConfigEntry<bool> enable { get; set; }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.teamComponent.teamIndex != TeamIndex.Player)
                args.armorAdd += 150 - 150 / Mathf.Pow(1 + 0.055f, Run.instance.stageClearCount);
        }

        [SystemInitializer(typeof(BodyCatalog))]
        public static void NerfHealthScaling()
        {
            enable = Main.WRBMechanicConfig.Bind(":: Mechanics ::::::::::::: Monster Armor Buff", "Enable?", true, "Vanilla is false");
            if (enable.Value)
            {
                RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
                foreach (CharacterBody body in BodyCatalog.allBodyPrefabBodyBodyComponents)
                {
                    if (SurvivorCatalog.FindSurvivorDefFromBody(body.gameObject) == null)
                    {
                        body.baseMaxHealth *= 0.75f;
                        body.levelMaxHealth *= 0.75f;
                    }
                }
            }
        }
    }
}
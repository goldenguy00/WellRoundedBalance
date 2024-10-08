﻿using BepInEx.Configuration;
using BepInEx.Logging;
using System;

namespace WellRoundedBalance.Utils
{
    public abstract class SharedBase
    {
        public abstract string Name { get; }
        public virtual bool isEnabled { get; set; } = true;
        public static ManualLogSource Logger => Main.WRBLogger;
        public abstract ConfigFile Config { get; }

        public static List<string> initList = [];

        public abstract void Hooks();

        public virtual void Init()
        {
            //ConfigManager.HandleConfigAttributes(GetType(), Name, Config);
            Hooks();
            //Main.WRBLogger.LogWarning("Initialized " + Name);
            initList.Add(Name);
        }

        public string d(float f) => (f * 100f).ToString() + "%";

        public string m(float f) => f + "m";

        public string s(float f, string suffix) => f + (suffix.StartsWith("{Stack}") ? "" : " ") + suffix + (Mathf.Abs(f) > 1 ? "s" : string.Empty);

        public static string StackDesc(float init, float stack, Func<float, string> initFn)
        {
            return StackDesc(init, stack, initFn, f => f.ToString());
        }

        public static string StackDesc(float init, float stack, Func<float, string> initFn, Func<float, string> stackFn)
        {
            if (init == 0 && stack == 0) return string.Empty;
            var ret = initFn(init);
            if (stack != 0) ret = ret.Replace("{Stack}", " <style=cStack>(" + (stack > 0 ? "+" : string.Empty) + stackFn(stack) + " per stack)</style>");
            else ret = ret.Replace("{Stack}", "");
            return ret;
        }

        public static float StackAmount(float init, float stack, float count)
        {
            if (count <= 0)
                return 0;

            return init + (stack * (count - 1));
        }

        public static float StackAmount(float init, float stack, float count, float isHyperbolic)
        {
            if (count <= 0)
                return 0;
            
            var ret = init + (stack * (count - 1));
            if (isHyperbolic != 0) 
                return GetHyperbolic(init, isHyperbolic, ret);
            
            return ret;
        }

        public static float GetHyperbolic(float firstStack, float cap, float chance) // Util.ConvertAmplificationPercentageIntoReductionPercentage but Better :zanysoup:
        {
            if (firstStack >= cap)
                return cap * (chance / firstStack); // should not happen, but failsafe

            var count = chance / firstStack;
            var coeff = 100 * firstStack / (cap - firstStack); // should be good

            return cap * (1 - (100 / ((count * coeff) + 100)));
        }
    }
}
﻿using System;
using BepInEx.Configuration;

namespace WellRoundedBalance.Interactables
{
    public abstract class InteractableBase<T> : InteractableBase where T : InteractableBase<T>
    {
        public static T instance { get; set; }

        public InteractableBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class InteractableBase : SharedBase
    {
        public override ConfigFile Config => Main.WRBInteractableConfig;

        public override void Init()
        {
            base.Init();
        }
    }
}
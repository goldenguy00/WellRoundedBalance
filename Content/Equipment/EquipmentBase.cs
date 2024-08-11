using BepInEx.Configuration;
using System;

namespace WellRoundedBalance.Equipment
{
    public abstract class EquipmentBase<T> : EquipmentBase where T : EquipmentBase<T>
    {
        public static T instance { get; set; }

        public EquipmentBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class EquipmentBase : SharedBase
    {
        public virtual EquipmentDef InternalPickup { get; }
        public abstract string PickupText { get; }
        public abstract string DescText { get; }
        public override ConfigFile Config => Main.WRBEquipmentConfig;

        public static event Action onTokenRegister;


        public override void Init()
        {
            base.Init();
            onTokenRegister += SetToken;
        }

        [SystemInitializer(typeof(EquipmentCatalog))]
        public static void OnEquipmentInitialized() => onTokenRegister?.Invoke();

        public void SetToken()
        {
            if (InternalPickup != null)
            {
                InternalPickup.pickupToken += "_WRB";
                InternalPickup.descriptionToken += "_WRB";
                LanguageAPI.Add(InternalPickup.pickupToken, PickupText);
                LanguageAPI.Add(InternalPickup.descriptionToken, DescText);
            };
        }
    }
}
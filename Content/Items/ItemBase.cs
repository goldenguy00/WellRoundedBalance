using BepInEx.Configuration;
using System;

namespace WellRoundedBalance.Items
{
    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; set; }

        public ItemBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class " + typeof(T).Name + " was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class ItemBase : SharedBase
    {
        public virtual ItemDef InternalPickup { get; }
        public abstract string PickupText { get; }
        public abstract string DescText { get; }
        public override ConfigFile Config => Main.WRBItemConfig;

        public static event Action onTokenRegister;

        public static int GetItemLoc(ILCursor c, string item) // modify this on compat update
        {
            var ret = -1;
            if (c.TryGotoNext(x => x.MatchLdsfld(typeof(RoR2Content.Items), item), x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)), x => x.MatchStloc(out ret))) c.Index--;
            else if (c.TryGotoNext(x => x.MatchLdsfld(typeof(DLC1Content.Items), item), x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)), x => x.MatchStloc(out ret))) c.Index--;
            return ret;
        }

        public override void Init()
        {
            base.Init();
            onTokenRegister += SetToken;
        }

        [SystemInitializer(typeof(ItemCatalog))]
        public static void OnItemInitialized() => onTokenRegister?.Invoke();

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

        public string GetToken(string addressablePath)
        {
            var def = Addressables.LoadAssetAsync<ItemDef>(addressablePath).WaitForCompletion();
            var token = def.nameToken;
            token = token.Replace("ITEM_", string.Empty);
            token = token.Replace("_NAME", string.Empty);
            return token;
        }
    }
}
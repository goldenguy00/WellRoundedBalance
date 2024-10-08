using System.Reflection;

namespace WellRoundedBalance.Utils
{
    public static class Extensions
    {
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            var type = comp.GetType();
            if (type != other.GetType()) 
                return null; // type mis-match

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            var finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T AddComponentCopy<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd);
        }

        public static PickupIndex GetPickupIndex(this ItemDef def)
        {
            return PickupCatalog.FindPickupIndex(def.itemIndex);
        }

        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var component))
            {
                MonoBehaviour.Destroy(component);
            }
        }

        public static void RemoveComponents<T>(this GameObject go) where T : Component
        {
            var coms = go.GetComponents<T>();
            for (var i = 0; i < coms.Length; i++)
            {
                GameObject.Destroy(coms[i]);
            }
        }

        public static bool IsAboveFraction(this HealthComponent healthComponent, float fraction)
        {
            var newFraction = fraction * 0.01f;
            var health = healthComponent.fullHealth * newFraction;
            return healthComponent.combinedHealth > health;
        }

        public static T GetRandom<T>(this List<T> list, Xoroshiro128Plus rng = null)
        {
            if (list.Count == 0)
            {
                return default(T);
            }
            if (rng == null)
            {
                return list[UnityEngine.Random.RandomRangeInt(0, list.Count)];
            }
            else
            {
                return list[rng.RangeInt(0, list.Count)];
            }
        }

        public static T GetRandom<T>(this T[] array)
        {
            var index = UnityEngine.Random.Range(0, array.Length);
            return array[index];
        }

        public static string ToPercentage(this float self)
        {
            return (self * 100).ToString() + "%";
        }

        public static bool CheckLoS(Vector3 victimPosition, Vector3 attackerPosition, float maxRange)
        {
            var vector = victimPosition - attackerPosition;
            if (vector.magnitude >= maxRange) return false; // < 120m + LoS check
            return !Physics.Raycast(victimPosition, vector, out var raycastHit, vector.magnitude, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
        }

        public static bool HasEquipment(Inventory inventory, EquipmentDef equipmentDef)
        {
            return inventory.GetEquipment(inventory.activeEquipmentSlot).equipmentDef == equipmentDef;
        }

        public static bool HasEquipment(CharacterBody characterBody, EquipmentDef equipmentDef)
        {
            var inventory = characterBody.inventory;
            if (inventory)
            {
                return inventory.GetEquipment(inventory.activeEquipmentSlot).equipmentDef == equipmentDef;
            }
            return false;
        }
    }
}
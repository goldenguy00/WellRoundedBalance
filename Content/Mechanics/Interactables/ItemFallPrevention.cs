﻿using RoR2.Navigation;
using UnityEngine;

namespace WellRoundedBalance.Mechanics.Interactables
{
    internal class ItemFallPrevention : MechanicBase<ItemFallPrevention>
    {
        public override string Name => ":: Mechanics :::::::::::: Item Fall Prevention";

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            On.RoR2.MapZone.TryZoneStart += MapZone_TryZoneStart;
            // do this better later
        }

        private void MapZone_TryZoneStart(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
        {
            if (self.zoneType == MapZone.ZoneType.OutOfBounds)
            {
                if (other.GetComponent<PickupDropletController>() ||
                    other.GetComponent<GenericPickupController>() ||
                    other.GetComponent<PickupPickerController>())
                {

                    var spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
                    spawnCard.hullSize = HullClassification.Human;
                    spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
                    spawnCard.prefab = LegacyResourcesAPI.Load<GameObject>("SpawnCards/HelperPrefab");

                    var directorPlacementRule = new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                        position = other.transform.position
                    };

                    var clone = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, directorPlacementRule, RoR2Application.rng));

                    if (clone)
                    {
                        TeleportHelper.TeleportGameObject(other.gameObject, clone.transform.position);
                        Object.Destroy(clone);
                    }

                    Object.Destroy(spawnCard);
                }
            }
            orig(self, other);
        }
    }
}
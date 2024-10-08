﻿using EntityStates.Missions.Moon;

namespace WellRoundedBalance.Mechanics.HoldoutZone
{
    internal class PillarRewards : MechanicBase<PillarRewards>
    {
        public override string Name => ":: Mechanics :::::: Pillar Rewards";

        public override void Hooks()
        {
            On.EntityStates.Missions.Moon.MoonBatteryComplete.OnEnter += Completed;
        }

        private void Completed(On.EntityStates.Missions.Moon.MoonBatteryComplete.orig_OnEnter orig, MoonBatteryComplete self)
        {
            orig(self);

            if (!NetworkServer.active)
            {
                return;
            }

            var count = Run.instance.participatingPlayerCount;
            var angle = 360f / count;
            var offset = 3f;
            var vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            var quaternion = Quaternion.AngleAxis(angle, Vector3.up);

            for (var i = 0; i < count; i++)
            {
                GenericPickupController.CreatePickupInfo info = new()
                {
                    position = self.transform.position + new Vector3(0, offset, 0),
                    prefabOverride = Utils.Paths.GameObject.OptionPickup.Load<GameObject>(),
                    rotation = Quaternion.identity,
                    pickupIndex = PickupCatalog.FindPickupIndex(ItemTier.Lunar),
                    pickerOptions = GenerateOptions()
                };

                PickupDropletController.CreatePickupDroplet(info, vector);
                vector = quaternion * vector;
            }
        }

        private PickupPickerController.Option[] GenerateOptions()
        {
            PickupPickerController.Option pearl = new()
            {
                available = true,
                pickupIndex = Run.instance.treasureRng.RangeFloat(0, 10) > 2 ? RoR2Content.Items.Pearl.GetPickupIndex() : RoR2Content.Items.ShinyPearl.GetPickupIndex()
            };

            PickupPickerController.Option lunar = new()
            {
                available = true,
                pickupIndex = Run.instance.availableLunarItemDropList[Run.instance.treasureRng.RangeInt(0, Run.instance.availableLunarItemDropList.Count - 1)]
            };

            PickupPickerController.Option green = new()
            {
                available = true,
                pickupIndex = Run.instance.availableTier2DropList[Run.instance.treasureRng.RangeInt(0, Run.instance.availableTier2DropList.Count - 1)]
            };

            return [lunar, pearl, green];
        }
    }
}
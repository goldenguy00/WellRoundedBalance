using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine.UIElements;

namespace WellRoundedBalance.Items.Reds
{
    public class WakeOfVultures : ItemBase<WakeOfVultures>
    {
        public override string Name => ":: Items ::: Reds :: Wake Of Vultures";
        public override ItemDef InternalPickup => RoR2Content.Items.HeadHunter;

        public override string PickupText => "Gain the powers of slain elites. Become resistant to elites.";

        public override string DescText => "Killing an elite <style=cIsUtility>grants you their power</style>. You can have <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> affix out at once. Take <style=cIsDamage>" + d(damageReduction) + "</style> <style=cStack>(+" + d(damageReduction) + " per stack)</style> reduced damage from elites of the same type as you.";

        [ConfigField("Damage Reduction", "Decimal. ", 0.2f)]
        public static float damageReduction;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            //IL.RoR2.GlobalEventManager.OnCharacterDeath += DisableVanilla;
            //GlobalEventManager.onCharacterDeathGlobal += Killed;
            //On.RoR2.HealthComponent.TakeDamage += ReduceDamage;
            //On.RoR2.Inventory.RemoveItem_ItemIndex_int += Inventory_RemoveItem_ItemIndex_int;
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);
            if (itemIndex == RoR2Content.Items.HeadHunter.itemIndex)
            {
                var master = self.GetComponent<CharacterMaster>();
                if (master)
                {
                    var body = master.GetBody();
                    if (body)
                    {
                        for (var i = 0; i < body.activeBuffsList.Length; i++)
                        {
                            var buff = body.activeBuffsList[i];
                            if (BuffCatalog.eliteBuffIndices.Contains(buff))
                            {
                                var indexToRemove = Array.IndexOf(body.activeBuffsList, buff);
                                if (indexToRemove != -1)
                                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.activeBuffsList, indexToRemove, 1);
                            }
                        }
                    }
                }
            }
        }

        private void ReduceDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damage)
        {
            if (NetworkServer.active)
            {
                if (self.body && damage.attacker)
                {
                    var inventory = self.body.inventory;
                    var attacker = damage.attacker.GetComponent<CharacterBody>();
                    if (attacker && attacker.isElite && inventory)
                    {
                        var stacks = inventory.GetItemCount(RoR2Content.Items.HeadHunter);
                        if (stacks > 0)
                        {
                            List<BuffIndex> currentEliteBuffs = [];
                            foreach (var buff in attacker.activeBuffsList)
                            {
                                if (BuffCatalog.eliteBuffIndices.Contains(buff))
                                {
                                    currentEliteBuffs.Add(buff);
                                }
                            }

                            List<BuffIndex> currentEliteBuffsVictim = [];
                            foreach (var buff in self.body.activeBuffsList)
                            {
                                if (BuffCatalog.eliteBuffIndices.Contains(buff))
                                {
                                    currentEliteBuffsVictim.Add(buff);
                                }
                            }

                            var hasAtLeastOne = false;

                            foreach (var index in currentEliteBuffsVictim)
                            {
                                if (currentEliteBuffs.Contains(index))
                                {
                                    hasAtLeastOne = true;
                                    break;
                                }
                            }

                            var mult = Mathf.Pow(1 - damageReduction, stacks);
                            // Debug.Log(mult);
                            if (hasAtLeastOne)
                            {
                                damage.damage *= mult;
                            }
                        }
                    }
                }
            }
            orig(self, damage);
        }

        private void DisableVanilla(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld(typeof(RoR2Content.Items), "HeadHunter")))
            {
                c.Remove();
                c.Emit<Useless>(OpCodes.Ldsfld, nameof(Useless.uselessItem));
            }
            else
            {
                Main.WRBLogger.LogError("Failed to apply Wake of Vultures Deletion hook");
            }
        }

        private void Killed(DamageReport report)
        {
            if (NetworkServer.active)
            {
                if (report.victimIsElite && report.attackerBody)
                {
                    // Debug.Log("killed elite");
                    var stack = report.attackerBody.inventory.GetItemCount(RoR2Content.Items.HeadHunter);
                    if (stack > 0)
                    {
                        List<BuffIndex> currentEliteBuffs = [];
                        foreach (var buff in report.attackerBody.activeBuffsList)
                        {
                            if (BuffCatalog.eliteBuffIndices.Contains(buff))
                            {
                                currentEliteBuffs.Add(buff);
                            }
                        }

                        BuffIndex eliteIndex = 0;
                        foreach (var buff in report.victimBody.activeBuffsList)
                        {
                            if (BuffCatalog.eliteBuffIndices.Contains(buff) && !currentEliteBuffs.Contains(buff))
                            {
                                // Debug.Log("giving elite buff");
                                eliteIndex = buff;
                                break;
                            }
                        }

                        if (eliteIndex != 0)
                        {
                            report.attackerBody.AddBuff(eliteIndex);
                        }

                        if (currentEliteBuffs.Count > stack)
                        {
                            // Debug.Log("has too many elite buffs");
                            for (var i = 0; i < currentEliteBuffs.Count - stack; i++)
                            {
                                report.attackerBody.RemoveBuff(currentEliteBuffs[i]);
                                // Debug.Log("removing buff");
                            }
                        }
                    }
                }
            }
        }
    }
}
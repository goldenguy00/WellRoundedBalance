using System;
using EntityStates.BeetleGuardMonster;
using EntityStates.Bell.BellWeapon;
using EntityStates.ClayBoss;
using EntityStates.ClayBoss.ClayBossWeapon;
using EntityStates.GravekeeperBoss;
using EntityStates.GreaterWispMonster;
using EntityStates.ImpBossMonster;
using EntityStates.ImpMonster;
using EntityStates.LemurianBruiserMonster;
using EntityStates.LemurianMonster;
using EntityStates.RoboBallBoss.Weapon;
using EntityStates.ScavMonster;
using EntityStates.Vulture.Weapon;
using HarmonyLib;

namespace WellRoundedBalance.Gamemodes.Eclipse
{
    internal class Eclipse2 : GamemodeBase<Eclipse2>
    {
        public override string Name => ":: Gamemode : Eclipse 2";

        public override void Init() => base.Init();

        public override void Hooks()
        {
            // special cases
            IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate += (il) => FireSunder_FixedUpdate(new(il));
            IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate += (il) => ChargeTrioBomb_FixedUpdate(new(il));

            // generic type T
            IL.EntityStates.GenericProjectileBaseState.FireProjectile += (il) => FireProjectile<GenericProjectileBaseState>(new(il));
            IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter += (il) => FireProjectile<FireCannons>(new(il));
            IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate += (il) => FireProjectile<FireEyeBlast>(new(il));
            IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += (il) => FireProjectile<FireWindblade>(new(il));
            IL.EntityStates.GravekeeperBoss.FireHook.OnEnter += (il) => FireProjectile<FireHook>(new(il));
            IL.EntityStates.LemurianMonster.FireFireball.OnEnter += (il) => FireProjectile<FireFireball>(new(il));
            IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate += (il) => FireProjectile<FireMegaFireball>(new(il));
            IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter += (il) => FireProjectile<FireEnergyCannon>(new(il));
            IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade += (il) => FireProjectile<FireBombardment>(new(il));
            IL.EntityStates.ClayBoss.FireTarball.FireSingleTarball += (il) => FireProjectile<FireTarball>(new(il));
            IL.EntityStates.ImpMonster.FireSpines.FixedUpdate += (il) => FireProjectile<FireSpines>(new(il));

            //fire many
            IL.EntityStates.ImpBossMonster.FireVoidspikes.FixedUpdate += (il) => FireProjectileGroup<FireVoidspikes>(new(il));

            // original wrb stuff
            IL.RoR2.HoldoutZoneController.FixedUpdate += HoldoutZoneController_FixedUpdate;

            // old e5
            CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
        }

        private void CharacterMaster_onStartGlobal(CharacterMaster master)
        {
            if (Run.instance?.selectedDifficulty >= DifficultyIndex.Eclipse2 && master.TryGetComponent<BaseAI>(out var baseAI))
            {
                baseAI.fullVision = true;
                baseAI.aimVectorMaxSpeed = 250f;
                baseAI.aimVectorDampTime = 0.05f;
            }
        }

        #region Generic Prediction IL
        /// <summary>
        /// aimRay must be on the stack before calling this!
        /// </summary>
        private static void EmitPredictAimray(ILCursor c, Type type, string prefabName = "projectilePrefab")
        {
            //this.characterbody
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(EntityState), nameof(EntityState.characterBody)));

            // this.projectilePrefab
            // - or -
            // {TYPE}.{PREFAB_NAME}
            var fieldInfo = AccessTools.Field(type, prefabName);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldarg_0);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldfld, fieldInfo);
            else c.Emit(OpCodes.Ldsfld, fieldInfo);

            // Utils.PredictAimRay(aimRay, characterBody, projectilePrefab);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PredictionUtils), nameof(PredictionUtils.PredictAimrayNew)));
        }

        private static void FireProjectile<T>(ILCursor c, string prefabName)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T), prefabName);
            else Logger.LogError("Failed to apply Eclipse 2 Generic BaseState.GetAimRay IL Hook");
        }

        private static void FireProjectile<T>(ILCursor c)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T));
            else Logger.LogError("Failed to apply Eclipse 2 Generic BaseState.GetAimRay IL Hook");
        }

        private static void FireProjectileGroup<T>(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T));
        }
        #endregion

        private static void FireSunder_FixedUpdate(ILCursor c)
        {
            var loc = 0;

            if (c.TryFindNext(out _,
                    x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay)),
                    x => x.MatchStloc(out loc)) &&
                c.TryGotoNext(
                    x => x.MatchCall(AccessTools.PropertyGetter(typeof(ProjectileManager), nameof(ProjectileManager.instance)))))
            {
                c.Emit(OpCodes.Ldloc, loc);
                EmitPredictAimray(c, typeof(FireSunder));
                c.Emit(OpCodes.Stloc, loc);
            }
            else Logger.LogError("Failed to apply Eclipse 2 BeetleGuardMonster.FireSunder.FixedUpdate IL Hook");
        }

        private static void ChargeTrioBomb_FixedUpdate(ILCursor c)
        {
            int rayLoc = 0, transformLoc = 0;

            if (c.TryFindNext(out _,
                    x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay)),
                    x => x.MatchStloc(out rayLoc),
                    x => x.MatchCall<ChargeTrioBomb>(nameof(ChargeTrioBomb.FindTargetChildTransformFromBombIndex)),
                    x => x.MatchStloc(out transformLoc)) &&
                c.TryGotoNext(MoveType.Before,
                    x => x.MatchCall(AccessTools.PropertyGetter(typeof(ProjectileManager), nameof(ProjectileManager.instance)))))
            {
                // set origin
                c.Emit(OpCodes.Ldloc, rayLoc);
                c.Emit(OpCodes.Ldloc, transformLoc);
                c.EmitDelegate<Action<Ray, Transform>>((ray, transform) => ray.origin = transform.position);

                // call prediction utils
                c.Emit(OpCodes.Ldloc, rayLoc);
                EmitPredictAimray(c, typeof(ChargeTrioBomb), "bombProjectilePrefab");
                c.Emit(OpCodes.Stloc, rayLoc);
            }
            else Logger.LogError("AccurateEnemies: EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate IL Hook failed");
        }

        private void HoldoutZoneController_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.5f)))
            {
                c.Next.Operand = 1f;
            }
            else
            {
                Logger.LogError("Failed to apply Eclipse 2 hook");
            }
        }
    }
}
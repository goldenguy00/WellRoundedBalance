namespace WellRoundedBalance.Projectiles
{
    public static class DucleusLaser
    {
        public static GameObject prefab;

        public static void Init()
        {
            prefab = PrefabAPI.InstantiateClone(Utils.Paths.GameObject.LaserMajorConstruct.Load<GameObject>(), "DefenseNucleusLaser", false);
            prefab.AddComponent<EffectComponent>();
            prefab.AddComponent<VFXAttributes>();

            var objectScaleCurve = prefab.AddComponent<ObjectScaleCurve>();
            objectScaleCurve.time = 5f;
            objectScaleCurve.overallCurve = new AnimationCurve
            {
                preWrapMode = WrapMode.Clamp,
                postWrapMode = WrapMode.Clamp,
                keys = [new Keyframe(0f, 1f), new Keyframe(4.7f, 1f), new Keyframe(5f, 0f)]
            };
            
            var destroyOnTimer = prefab.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 5.1f;

            ContentAddition.AddEffect(prefab);
        }
    }
}
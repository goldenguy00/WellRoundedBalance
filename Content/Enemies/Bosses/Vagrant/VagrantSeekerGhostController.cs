namespace WellRoundedBalance.Enemies.Bosses.Vagrant
{
    public class VagrantSeekerGhostController : MonoBehaviour
    {
        public static List<Color> colors =
        [
            Color.red, Color.yellow, Color.green, Color.cyan
        ];

        public ProjectileTargetComponent component;
        public VagrantSeekerController owner;
        public LineRenderer lr;
        public Color color;

        public void Start()
        {
            color = colors.GetRandom();

            lr = GetComponent<LineRenderer>();
            lr.startColor = color;
            lr.endColor = color;

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer != lr)
                {
                    renderer.material.SetColor("_Color", color);
                    renderer.material.SetInt("_FEON", 0);
                    renderer.material.SetInt("_FlowmapOn", 0);
                    renderer.material.SetShaderKeywords([]);
                }
            }
        }

        public void FixedUpdate()
        {
            if (!component.target || owner.begunRam)
            {
                lr.widthMultiplier = 0f;
                return;
            }

            lr.SetPosition(0, base.transform.position);
            var ray = new Ray(base.transform.position, (component.target.position - base.transform.position).normalized);
            lr.SetPosition(1, ray.GetPoint(400));

            lr.widthMultiplier = 1f - ((1.5f - owner.duration) / 1.5f);

            if (lr.widthMultiplier <= 0.05f)
            {
                lr.widthMultiplier = 1.5f;
                lr.startColor = Color.white;
                lr.endColor = Color.white;
            }
        }
    }
}

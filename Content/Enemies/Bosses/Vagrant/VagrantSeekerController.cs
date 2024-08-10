namespace WellRoundedBalance.Enemies.Bosses.Vagrant
{
    public class VagrantSeekerController : MonoBehaviour
    {
        public ProjectileSimple simple;
        public ProjectileTargetComponent targetComp;
        public float duration = 1.5f;
        public float ramSpeed = 165f;
        public float initialSpeed = 40f;
        public float speedDecPerSec;
        public bool begunRam = false;
        public Vector3 forward;

        public void Start()
        {
            simple = GetComponent<ProjectileSimple>();
            initialSpeed = 60f * Random.Range(0.75f, 2f);
            targetComp = GetComponent<ProjectileTargetComponent>();
            simple.lifetime = 20;
            simple.desiredForwardSpeed = initialSpeed;
            speedDecPerSec = initialSpeed / (duration - 1f);
            forward = base.transform.forward;

            var controller = GetComponent<ProjectileController>();
            controller.ghost.GetComponent<VagrantSeekerGhostController>().component = targetComp;
            controller.ghost.GetComponent<VagrantSeekerGhostController>().owner = this;
        }

        public void FixedUpdate()
        {
            if (duration >= 0f)
            {
                initialSpeed -= speedDecPerSec * Time.fixedDeltaTime;
                simple.desiredForwardSpeed = Mathf.Max(initialSpeed, 0f);
                base.transform.forward = forward;
                duration -= Time.fixedDeltaTime;
            }

            if (duration <= 0f && !begunRam)
            {
                begunRam = true;

                if (targetComp.target)
                {
                    var facing = (targetComp.target.position - base.transform.position).normalized;
                    base.transform.forward = facing;
                    simple.desiredForwardSpeed = ramSpeed;
                }
                else
                {
                    simple.lifetime = 0f;
                }
            }
        }
    }
}

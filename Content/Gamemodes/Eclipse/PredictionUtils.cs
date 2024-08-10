using System.Diagnostics;

namespace WellRoundedBalance.Gamemodes.Eclipse
{
    internal class PredictionUtils
    {

        private static bool ShouldPredict(CharacterBody body)
        {
            return body && body.teamComponent.teamIndex != TeamIndex.Player &&
                Run.instance?.selectedDifficulty >= DifficultyIndex.Eclipse2;
        }

        private static readonly float zero = 0.1f * Time.fixedDeltaTime;
        private static long max = 0;
        public static Ray PredictAimrayNew(Ray aimRay, CharacterBody body, GameObject projectilePrefab)
        {
            if (!ShouldPredict(body) || !projectilePrefab)
                return aimRay;
            var s = Stopwatch.StartNew();
            // lil bit of wiggle room cuz floats are fun
            var projectileSpeed = 0f;
            if (projectilePrefab.TryGetComponent<ProjectileSimple>(out var ps))
            {
                if (ps.rigidbody && !ps.rigidbody.useGravity)
                    projectileSpeed = GetProjectileSimpleModifiers(ps.desiredForwardSpeed);
                else
                    projectileSpeed = ps.desiredForwardSpeed;
            }

            if (projectilePrefab.TryGetComponent<ProjectileCharacterController>(out var pcc))
                projectileSpeed = Mathf.Max(projectileSpeed, pcc.velocity);

            if (projectileSpeed > zero && GetTargetHurtbox(body, out var targetBody))
            {
                //Velocity shows up as 0 for clients due to not having authority over the CharacterMotor
                //Less accurate, but it works online.
                var targetVelocity = (targetBody.transform.position - targetBody.previousPosition) / Time.fixedDeltaTime;
                var motor = targetBody.characterMotor;

                // compare the two options since big number = better number of course
                if (motor && targetVelocity.sqrMagnitude < motor.velocity.sqrMagnitude)
                    targetVelocity = motor.velocity;

                if (targetVelocity.sqrMagnitude > zero * zero) //Dont bother predicting stationary targets
                {
                    aimRay = GetRay(aimRay, projectileSpeed, targetBody.transform.position, targetVelocity);
                }
            }
            s.Stop();
            if (s.ElapsedTicks > max)
            {
                max = s.ElapsedTicks;
                Main.WRBLogger.LogDebug($"prediction ticks {s.ElapsedTicks} | ms {s.ElapsedMilliseconds}");

            }
            return aimRay;
        }

        private static float GetProjectileSimpleModifiers(float speed)
        {
            if (Main.InfernoLoaded) 
                speed *= Main.GetInfernoProjectileSpeedMult();

            if (Main.RiskyArtifactsLoaded)
                speed *= Main.GetRiskyArtifactsWarfareProjectileSpeedMult();

            return speed;
        }

        private static Ray GetRay(Ray aimRay, float v, Vector3 y, Vector3 dy)
        {
            // dont question it man im so bad at math
            // but its really fucking fast and really fucking accurate
            // might want to integrate acceleration at some point to
            // cut down on overshooting decelerating targets
            // edit: never fuckign mind i hate math im not gonna solve a fucking quartic equation what the hell
            // https://gamedev.stackexchange.com/questions/77749/predicted-target-location
            //https://gamedev.stackexchange.com/questions/149327/projectile-aim-prediction-with-acceleration
            var yx = y - aimRay.origin;

            var a = (v * v) - Vector3.Dot(dy, dy);
            var b = -2 * Vector3.Dot(dy, yx);
            var c = -1 * Vector3.Dot(yx, yx);

            var d = (b * b) - (4 * a * c);
            if (d > 0)
            {
                d = Mathf.Sqrt(d);
                var t1 = (-b + d) / (2 * a);
                var t2 = (-b - d) / (2 * a);
                var t = Mathf.Max(t1, t2);
                if (t > 0)
                {
                    var newA = (dy * t + yx) / t;
                    aimRay = new Ray(aimRay.origin, newA.normalized);
                }
            }
            return aimRay;
        }

        private static bool GetTargetHurtbox(CharacterBody body, out CharacterBody target)
        {
            var aiComponents = body.master.aiComponents;
            for (var i = 0; i < aiComponents.Length; i++)
            {
                var ai = aiComponents[i];
                if (ai && ai.hasAimTarget)
                {
                    var aimTarget = ai.skillDriverEvaluation.aimTarget;
                    if (aimTarget.characterBody && aimTarget.healthComponent && aimTarget.healthComponent.alive )
                    {
                        target = aimTarget.characterBody;
                        return true;
                    }
                }
            }
            target = null;
            return false;
        }
    }
}
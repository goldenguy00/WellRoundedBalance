﻿namespace WellRoundedBalance.Misc
{
    public class DestroyStuckObject : MonoBehaviour // FUCK im too stupid to make this work
    {
        public float noMovementThreshold = 0.0001f;
        public const int noMovementFrames = 3;
        public Vector3[] previousLocations = new Vector3[noMovementFrames];
        public float timer;
        public float interval = 0.5f;

        private void Awake()
        {
            //For good measure, set the previous locations
            for (int i = 0; i < previousLocations.Length; i++)
            {
                previousLocations[i] = Vector3.zero;
            }
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= interval)
            {
                //Store the newest vector at the end of the list of vectors
                for (int i = 0; i < previousLocations.Length - 1; i++)
                {
                    previousLocations[i] = previousLocations[i + 1];
                }
                previousLocations[previousLocations.Length - 1] = transform.position;

                //Check the distances between the points in your previous locations
                //If for the past several updates, there are no movements smaller than the threshold,
                //you can most likely assume that the object is not moving
                for (int i = 0; i < previousLocations.Length - 1; i++)
                {
                    if (Vector3.Distance(previousLocations[i], previousLocations[i + 1]) >= noMovementThreshold)
                    {
                        Main.WRBLogger.LogError("above not moving threshold");
                        break;
                    }
                    else
                    {
                        Main.WRBLogger.LogError("BELOW    not moving threshold");
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
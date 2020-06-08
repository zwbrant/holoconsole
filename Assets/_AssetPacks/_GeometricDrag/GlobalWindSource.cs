using UnityEngine;

namespace GeometricDrag
{
    public class GlobalWindSource : MonoBehaviour
    {
        public float Strength = 10f;
        public Vector3 Direction;
        public bool EnableFalloff = true;
        public float Range = 10f;

        public int ForceIndex { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            ForceIndex = DragSolver.AddGlobalAirForce(new AirForce(Strength, Direction, transform.position, Range));
        }

        // Update is called once per frame
        void Update()
        {
            if (EnableFalloff)
                DragSolver.SetGlobalAirForce(ForceIndex, new AirForce(Strength, Direction, transform.position, Range));
            else
                DragSolver.SetGlobalAirForce(ForceIndex, new AirForce(Strength, Direction));

        }
    }
}

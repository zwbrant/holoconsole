using UnityEngine;


namespace Valorem.HoloHands
{
    public abstract class HandManager : MonoBehaviour
    {
        public enum HandState
        {
            Ready,
            Tapped,
            Hold,
        }
        public class Hand
        {
            public bool Active = false;
            public bool IsPrimary = false;
            public HandState State = HandState.Ready;
            public bool DoubleTapped = false;
            public uint Key;
            public float LastTapTime;
            public Vector3 Position;
            public Vector3 TappedPosition;
        }
        public Hand PrimaryHand;
        public Hand SecondaryHand;

        public static HandManager Instance;

        public float MaxTappedTime = .333f;
        public float MaxTappedDistance = .0333f;
        public float DoubleTapTime = .333f;

        public delegate void AirTapData(bool doubleTapped, bool isPrimary);
        public event AirTapData OnAirTapStart;
        public event AirTapData OnAirTapEnd;
        public event AirTapData OnAirHoldStart;
        public event AirTapData OnAirHoldEnd;
        public event AirTapData OnTwoHandStart;
        public event AirTapData OnTwoHandEnd;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy((gameObject));
        }

        void Start()
        {
            PrimaryHand = new Hand
            {
                IsPrimary = true
            };
            SecondaryHand = new Hand();
        }

        protected void EventOnAirTapStart(bool doubleTapped, bool isPrimary)
        {
            if (OnAirTapStart != null)
            {
                OnAirTapStart(doubleTapped, isPrimary);
            }
        }
        protected void EventOnAirTapEnd(bool doubleTapped, bool isPrimary)
        {
            if (OnAirTapEnd != null)
            {
                OnAirTapEnd(doubleTapped, isPrimary);
            }
        }
        protected void EventOnAirHoldStart(bool doubleTapped, bool isPrimary)
        {
            if (OnAirHoldStart != null)
            {
                OnAirHoldStart(doubleTapped, isPrimary);
            }
        }
        protected void EventOnAirHoldEnd(bool doubleTapped, bool isPrimary)
        {
            if (OnAirHoldEnd != null)
            {
                OnAirHoldEnd(doubleTapped, isPrimary);
            }
        }
        protected void EventOnTwoHandStart(bool doubleTapped, bool isPrimary)
        {
            if (OnTwoHandStart != null)
            {
                OnTwoHandStart(doubleTapped, isPrimary);
            }
        }
        protected void EventOnTwoHandEnd(bool doubleTapped, bool isPrimary)
        {
            if (OnTwoHandEnd != null)
            {
                OnTwoHandEnd(doubleTapped, isPrimary);
            }
        }


    }
}

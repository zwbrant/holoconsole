using System;
using UnityEngine;
using UnityEngine.UI;
using Valorem.HoloConsole.CustomHoloHands;

namespace Valorem.HoloConsole
{
    public class ScrollHandler : MonoBehaviour
    {
        public HoloHold HoloHold;
        public ScrollRect ScrollRect;

        // Use this for initialization
        void Start()
        {
            HoloHold.MovementUpdate += HandleScroll;
        }

        private void HandleScroll(Vector3 movementDelta)
        {
            if (movementDelta.y > 0)
                ScrollRect.verticalNormalizedPosition += (float)Math.Pow(movementDelta.y * .3f, 2f);
            else
                ScrollRect.verticalNormalizedPosition -= (float)Math.Pow(movementDelta.y * .3f, 2f);

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

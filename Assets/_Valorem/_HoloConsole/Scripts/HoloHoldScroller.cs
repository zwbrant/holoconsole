using System;
using UnityEngine;
using Valorem.HoloConsole.CustomHoloHands;

namespace Valorem.HoloConsole
{
    public class HoloHoldScroller : MonoBehaviour
    {
        public const float ScollMultiplier = .2f;
        public HoloHold SourceHoloHold;


        // Use this for initialization
        void Start()
        {
            SourceHoloHold.MovementUpdate += MovementUpdateHandler;
        }

        private void MovementUpdateHandler(Vector3 movementDelta)
        {
            float scrollMagnitude = (float)Math.Pow(movementDelta.y * ScollMultiplier, 2f);
            if (movementDelta.y > 0)
            {
                LogManager.Instance.Scroll(-scrollMagnitude);

            }
            else
            {

                LogManager.Instance.Scroll(scrollMagnitude);
            }
        }


    }
}

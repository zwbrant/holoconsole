using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using Valorem.HoloConsole.CustomHoloHands;

namespace Valorem.HoloConsole
{
    public class Scaler : MonoBehaviour
    {
        public RectTransform ScaleTarget;
        public HoloHold HoloHold;

        private float _multiplier;

        public void OnManipulationCanceled(ManipulationEventData eventData)
        {
            //Debug.LogWarning("Manip canceled");

        }

        public void OnManipulationCompleted(ManipulationEventData eventData)
        {
            //Debug.LogWarning("Manip complete");
        }


        //public void OnManipulationUpdated(ManipulationEventData eventData)
        //{
        //    if (_held)
        //        ScaleTarget.sizeDelta = new Vector2(ScaleTarget.sizeDelta.x + eventData.CumulativeDelta.x * 150f, ScaleTarget.sizeDelta.y + eventData.CumulativeDelta.y * 150f);
        //}

        // Use this for initialization
        void Start()
        {
            //GestureRecognizer gr = new GestureRecognizer();
            //gr.ManipulationStartedEvent += OnManipulationStarted;
            //gr.ManipulationStartedEvent += OnManipulationUpdated;
            HoloHold.MovementUpdate += HoldMovementUpdate;
            _multiplier = LogManager.Instance.ScaleSensitivity * .022f;
        }

        public void HoldMovementUpdate(Vector3 movementDelta)
        {
            LogManager.Instance.ResizeConsole((Vector2)movementDelta * _multiplier);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnManipulationStarted(ManipulationEventData eventData)
        {
            //Debug.LogWarning("Manip started");
        }

        //private bool _held = false; 
        //public void OnHoldStarted(HoldEventData eventData)
        //{
        //    print("Hold Started");
        //    _held = true;
        //}

        //public void OnHoldCompleted(HoldEventData eventData)
        //{
        //    print("Hold Complete");
        //    _held = false;

        //}

        //public void OnHoldCanceled(HoldEventData eventData)
        //{
        //    print("Hold Canceled");
        //    _held = false;

        //}
    }
}

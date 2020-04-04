using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Valorem.HoloHands
{
    public class PlayerHands : UnityEngine.Object
    {
        private static PlayerHands _instance;
        public static PlayerHands Instance
        {
            get { return _instance ?? (_instance = new PlayerHands()); }
        }

        public Transform HeadTransform
        {
            get { return Camera.main.transform; }
        }

        public Vector3 HeadPosition
        {
            get { return HeadTransform.position; }
        }

        public Vector3 HeadDirection
        {
            get { return HeadTransform.forward; }
        }

        public Ray HeadRay
        {
            get
            {
                if (IsHandDetected)
                {
                    return CurrentHand.headRay;
                }
                return new Ray(HeadTransform.position, HeadTransform.forward);
            }
        }


        public bool IsHandDetected { get; private set; }

        public UnityEngine.XR.WSA.Input.InteractionSourceState CurrentHand { get; private set; }

        public Vector3 HandVelocity
        {
            get
            {
                if (IsHandDetected)
                {
                    Vector3 vel;
                    if (CurrentHand.properties.sourcePose.TryGetVelocity(out vel))
                    {
                        return vel;
                    }
                }
                return Vector3.zero;
            }
        }

        public Vector3 HandPosition
        {
            get
            {
                if (IsHandDetected)
                {
                    Vector3 pos;
                    if (CurrentHand.properties.sourcePose.TryGetPosition(out pos))
                    {
                        return pos;
                    }
                }
                return Vector3.zero;
            }
        }

        public HashSet<UnityEngine.XR.WSA.Input.InteractionSourceKind> PressedSources { get; private set; }

        public Dictionary<uint, UnityEngine.XR.WSA.Input.InteractionSourceState> VisibleHands = new Dictionary<UInt32, UnityEngine.XR.WSA.Input.InteractionSourceState>();

        public PlayerHands()
        {
            //InteractionManager.SourceLost += InteractionManager_SourceLost;
            //InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
            //InteractionManager.SourcePressed += InteractionManager_SourcePressed;
            //InteractionManager.SourceReleased += InteractionManager_SourceReleased;

            PressedSources = new HashSet<UnityEngine.XR.WSA.Input.InteractionSourceKind>();
        }

        ~PlayerHands()
        {
            //InteractionManager.SourceLost -= InteractionManager_SourceLost;
            //InteractionManager.SourceUpdated -= InteractionManager_SourceUpdated;
            //InteractionManager.SourcePressed -= InteractionManager_SourcePressed;
            //InteractionManager.SourceReleased -= InteractionManager_SourceReleased;
        }

        public Vector3 WorldToViewportPoint(Vector3 position)
        {
            return Camera.main.WorldToViewportPoint(position);
        }

        private void InteractionManager_SourceUpdated(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
            if (state.source.kind == UnityEngine.XR.WSA.Input.InteractionSourceKind.Hand)
            {
                IsHandDetected = true;

                if (VisibleHands.ContainsKey(state.source.id))
                {
                    VisibleHands[state.source.id] = state;
                }
                else
                {
                    VisibleHands.Add(state.source.id, state);
                }
                CurrentHand = state;
            }
        }

        private void InteractionManager_SourceLost(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
            if (state.source.kind == UnityEngine.XR.WSA.Input.InteractionSourceKind.Hand)
            {
                IsHandDetected = false;
                if (VisibleHands.ContainsKey(state.source.id))
                {
                    VisibleHands.Remove(state.source.id);
                }
                CurrentHand = IsHandDetected ? VisibleHands.FirstOrDefault().Value : default(UnityEngine.XR.WSA.Input.InteractionSourceState);
            }
        }

        private void InteractionManager_SourcePressed(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
            PressedSources.Add(state.source.kind);
        }

        private void InteractionManager_SourceReleased(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
            PressedSources.Remove(state.source.kind);
        }
    }
}
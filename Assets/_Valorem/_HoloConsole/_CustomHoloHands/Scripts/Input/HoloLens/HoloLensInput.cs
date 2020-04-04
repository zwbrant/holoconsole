using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valorem.HoloHands
{
    public class HoloLensInput : HandManager
    {

        void Update()
        {
            // If either hand is inactive
            if (PrimaryHand.Active == false ||
                SecondaryHand.Active == false)
            {
                // check missing time and look for a pressed hand
                SearchForNewHands();
            }
            // if primary is active check for it's state
            if (PrimaryHand.Active)
            {
                CheckHand(PrimaryHand);
            }
            if (SecondaryHand.Active)
            {
                CheckHand(SecondaryHand);
            }
        }
        private void SearchForNewHands()
        {
            Dictionary<uint, UnityEngine.XR.WSA.Input.InteractionSourceState>.KeyCollection keys = PlayerHands.Instance.VisibleHands.Keys;
            foreach (uint key in keys)
            {
                // if new key is not already in use
                if (key != PrimaryHand.Key && key != SecondaryHand.Key)
                {
                    // make sure the new hand is in the ready position
                    if (PlayerHands.Instance.VisibleHands[key].selectPressed == false)
                    {
                        if (PrimaryHand.Active == false)
                        {
                            FoundNewHand(PrimaryHand, key);
                            PrimaryHand.IsPrimary = true;
                        }
                        else if (SecondaryHand.Active == false)
                        {
                            FoundNewHand(SecondaryHand, key);
                        }
                    }
                }
            }
        }
        private void FoundNewHand(Hand hand, uint key)
        {
            hand.Active = true;
            hand.Key = key;
        }
        private Vector3 GetHandPosition(uint key)
        {
            Vector3 position;
            PlayerHands.Instance.VisibleHands[key].properties.location.TryGetPosition(out position);
            return position;
        }
        private bool IsHandStillActive(Hand checkedHand)
        {
            Dictionary<uint, UnityEngine.XR.WSA.Input.InteractionSourceState>.KeyCollection keys = PlayerHands.Instance.VisibleHands.Keys;
            bool isActive = false;
            uint checkedKey = checkedHand.Key;
            foreach (uint key in keys)
            {
                if (key == checkedKey)
                {
                    isActive = true;
                }
            }
            return isActive;
        }
        private void DeactivateHand(Hand hand)
        {
            if (hand.State == HandState.Hold)
            {
                EventOnAirHoldEnd(hand.DoubleTapped, hand.IsPrimary);
                //if (OnAirHoldEnd != null)
                //{
                //    OnAirHoldEnd(hand.DoubleTapped, hand.IsPrimary);
                //}
            }
            // check if it was part of a two hand gesture;
            if (PrimaryHand.State == HandState.Hold &&
            SecondaryHand.State == HandState.Hold)
            {
                EventOnTwoHandEnd(false, true);
                //if (OnTwoHandEnd != null)
                //{
                //    OnTwoHandEnd(false, true);
                //}
            }
            if (hand.IsPrimary)
            {
                if (SecondaryHand.Active &&
                    SecondaryHand.State == HandState.Ready)
                {
                    SwitchHands();
                    SecondaryHand = new Hand();
                }
                else
                {
                    PrimaryHand = new Hand
                    {
                        IsPrimary = true
                    };
                    SecondaryHand = new Hand();
                }
            }
            else
            {
                SecondaryHand = new Hand();
            }
        }
        private void StartAirTap(Hand hand)
        {
            //check if this is a double tap
            if (hand.LastTapTime >= Time.time - MaxTappedTime)
            {
                hand.DoubleTapped = true;
            }
            hand.TappedPosition = hand.Position;
            hand.LastTapTime = Time.time;
            hand.State = HandState.Tapped;
            if (hand.IsPrimary == false &&
                PrimaryHand.State == HandState.Ready)
            {
                //switch hands
                SwitchHands();
                hand = PrimaryHand;
            }
            EventOnAirTapStart(hand.DoubleTapped, hand.IsPrimary);
            //if (OnAirTapStart != null)
            //{
            //    OnAirTapStart(hand.DoubleTapped, hand.IsPrimary);
            //}

        }
        private void CheckHand(Hand hand)
        {
            // see if hand is lost
            if (!IsHandStillActive(hand))
            {
                DeactivateHand(hand);
            }
            else // hand is still there so run it though it's state check
            {
                hand.Position = GetHandPosition(hand.Key);
                switch (hand.State)
                {
                    case HandState.Ready:
                        CheckReadyHand(hand);
                        break;
                    case HandState.Tapped:
                        CheckTappedHand(hand);
                        break;
                    case HandState.Hold:
                        CheckHoldHand(hand);
                        break;
                }
            }
        }
        private void SwitchHands()
        {
            bool activeP = PrimaryHand.Active;
            HandState stateP = PrimaryHand.State;
            bool doubleTappedP = PrimaryHand.DoubleTapped;
            uint keyP = PrimaryHand.Key;
            float lastTapTimeP = PrimaryHand.LastTapTime;
            Vector3 positionP = PrimaryHand.Position;
            Vector3 tappedPositionP = PrimaryHand.TappedPosition;
            bool activeS = SecondaryHand.Active;
            HandState stateS = SecondaryHand.State;
            bool doubleTappedS = SecondaryHand.DoubleTapped;
            uint keyS = SecondaryHand.Key;
            float lastTapTimeS = SecondaryHand.LastTapTime;
            Vector3 positionS = SecondaryHand.Position;
            Vector3 tappedPositionS = SecondaryHand.TappedPosition;
            PrimaryHand.Active = activeS;
            PrimaryHand.IsPrimary = true;
            PrimaryHand.State = stateS;
            PrimaryHand.DoubleTapped = doubleTappedS;
            PrimaryHand.Key = keyS;
            PrimaryHand.LastTapTime = lastTapTimeS;
            PrimaryHand.Position = positionS;
            PrimaryHand.TappedPosition = tappedPositionS;
            SecondaryHand.Active = activeP;
            SecondaryHand.IsPrimary = false;
            SecondaryHand.State = stateP;
            SecondaryHand.DoubleTapped = doubleTappedP;
            SecondaryHand.Key = keyP;
            SecondaryHand.LastTapTime = lastTapTimeP;
            SecondaryHand.Position = positionP;
            SecondaryHand.TappedPosition = tappedPositionP;
        }
        private void CheckReadyHand(Hand hand)
        {
            // check to see if the hand is pressed
            if (PlayerHands.Instance.VisibleHands[hand.Key].selectPressed)
            {
                // start a tap
                StartAirTap(hand);
            }
        }
        private void CheckTappedHand(Hand hand)
        {
            // find out if it is released
            if (PlayerHands.Instance.VisibleHands[hand.Key].selectPressed == false)
            {
                // release tap
                EventOnAirTapEnd(hand.DoubleTapped, hand.IsPrimary);
                //if (OnAirTapEnd != null)
                //{
                //    OnAirTapEnd(hand.DoubleTapped, hand.IsPrimary);
                //}
                hand.State = HandState.Ready;
                //check if this is primary hand and if the secondary is tapped;
                if (hand.IsPrimary &&
                    SecondaryHand.State == HandState.Tapped)
                {
                    SwitchHands();
                }
            }
            else // it is still pressed, check if it is ready to be a hold
            {
                var tappedDistance = Vector3.Distance(hand.TappedPosition, hand.Position);
                //see if hand reached max tapped time or
                // if hand reached max tapped distance
                if (hand.LastTapTime < Time.time - MaxTappedTime ||
                    tappedDistance > MaxTappedDistance)
                {
                    hand.State = HandState.Hold;
                    if (hand.IsPrimary == false &&
                        PrimaryHand.State == HandState.Ready)
                    {
                        SwitchHands();
                        //hand = primaryHand;
                    }
                    else if (hand.IsPrimary == false &&
                        PrimaryHand.State == HandState.Tapped)
                    {
                        //start a two handed gesture
                        PrimaryHand.State = HandState.Hold;
                    }
                    else if (hand.IsPrimary &&
                        SecondaryHand.State == HandState.Tapped)
                    {
                        //start a two handed gesture
                        SecondaryHand.State = HandState.Hold;
                    }
                    // if both hands are now set to hold, start two hand gesture event
                    if (PrimaryHand.State == HandState.Hold &&
                        SecondaryHand.State == HandState.Hold)
                    {
                        EventOnTwoHandStart(false, true);
                        //if (OnTwoHandStart != null)
                        //{
                        //    OnTwoHandStart(false, true);
                        //}
                    }
                    else // if only one hand is now a hold, start a hold event
                    {
                        EventOnAirHoldStart(hand.DoubleTapped, hand.IsPrimary);
                        //if (OnAirHoldStart != null)
                        //{
                        //    OnAirHoldStart(hand.DoubleTapped, hand.IsPrimary);
                        //}
                    }
                }
            }
        }
        private void CheckHoldHand(Hand hand)
        {
            // find out if it is released
            if (PlayerHands.Instance.VisibleHands[hand.Key].selectPressed == false)
            {
                EventOnAirHoldEnd(hand.DoubleTapped, hand.IsPrimary);
                //if (OnAirHoldEnd != null)
                //{
                //    OnAirHoldEnd(hand.DoubleTapped, hand.IsPrimary);
                //}
                // release hold and deactivate
                if (hand.IsPrimary)
                {
                    if (SecondaryHand.State == HandState.Hold)
                    {
                        EventOnTwoHandEnd(false, true);
                        //if (OnTwoHandEnd != null)
                        //{
                        //    OnTwoHandEnd(false, true);
                        //}
                    }
                    PrimaryHand = new Hand
                    {
                        IsPrimary = true
                    };
                    SecondaryHand = new Hand();
                }
                else
                {
                    EventOnAirHoldEnd(hand.DoubleTapped, hand.IsPrimary);
                    //if (OnAirHoldEnd != null)
                    //{
                    //    OnAirHoldEnd(hand.DoubleTapped, hand.IsPrimary);
                    //}
                    if (PrimaryHand.State == HandState.Hold)
                    {
                        EventOnTwoHandEnd(false,true);
                        //if (OnTwoHandEnd != null)
                        //{
                        //    OnTwoHandEnd(false, true);
                        //}
                    }
                    SecondaryHand = new Hand();
                }
            }
        }
    }
}

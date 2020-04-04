using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valorem.HoloHands;

public class MouseInput : HandManager {
	
	// Update is called once per frame
	void Update () {
        
	    if (Input.GetMouseButton(0))
	    {
	        if (PrimaryHand.Active)
	        {
	            UpdateHand(PrimaryHand);
	        }
	        else
	        {
	            AvtivateHand(PrimaryHand);
	        }
	    }
	    else
	    {
	        if (PrimaryHand.Active)
	        {
	            DeactivateHand(PrimaryHand);
                PrimaryHand = new Hand();
	            PrimaryHand.IsPrimary = true;
	        }
	    }
    }

    void AvtivateHand(Hand hand)
    {
        hand.Active = true;
        hand.State = HandState.Tapped;
        hand.TappedPosition = Camera.main.transform.position + Camera.main.transform.forward;
        hand.Position = hand.TappedPosition;
        hand.LastTapTime = Time.time;
        EventOnAirTapStart(hand.DoubleTapped, hand.IsPrimary);
    }

    void DeactivateHand(Hand hand)
    {
        if (hand.State == HandState.Tapped)
        {
            EventOnAirTapEnd(hand.DoubleTapped, hand.IsPrimary);
        }
        else if (hand.State == HandState.Hold)
        {
            EventOnAirHoldEnd(hand.DoubleTapped, hand.IsPrimary);
        }
    }
    void UpdateHand(Hand hand)
    {
        hand.Position = Camera.main.transform.position + Camera.main.transform.forward;
        if (hand.State == HandState.Tapped)
        {
            if (Time.time - hand.LastTapTime > MaxTappedTime)
            {
                hand.State = HandState.Hold;
                EventOnAirHoldStart(hand.DoubleTapped,hand.IsPrimary);
            }
            else if(Vector3.Distance(hand.TappedPosition,hand.Position) > MaxTappedDistance)
            {
                hand.State = HandState.Hold;
                EventOnAirHoldStart(hand.DoubleTapped, hand.IsPrimary);
            }
        }
    }
}

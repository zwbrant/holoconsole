//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.EventSystems;
using Valorem.HoloConsole;

namespace Valve.VR.Extras
{
    public class SteamVR_LaserPointer : MonoBehaviour
    {
        public SteamVR_Behaviour_Pose pose;

        public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");
        public Transform LaserHit;
        [Range(-10f, 10f)]
        public float LaserHitGap = .03f;
        public bool SetLaserColor = true;

        public bool active = true;
        public Color color;
        public float thickness = 0.002f;
        public Color clickColor = Color.green;
        public GameObject holder;
        public GameObject pointer;
        bool isActive = false;
        public bool addRigidBody = false;
        public Transform reference;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;

        public event PointerEventHandler PointerDown;
        public event PointerEventHandler PointerUp;
        public event PointerDragHandler PointerDrag;

        [Range(0f, 1000f)]
        public float ThrowForce = 100f;
        public GameObject DraggingPoint;
        public Transform DraggedObject = null;
        public Vector3 LocalDragContactPoint;
        Scaler currScaler;

        Transform previousContact = null;


        private void Start()
        {
            if (pose == null)
                pose = this.GetComponent<SteamVR_Behaviour_Pose>();
            if (pose == null)
                Debug.LogError("No SteamVR_Behaviour_Pose component found on this object", this);

            if (interactWithUI == null)
                Debug.LogError("No ui interaction action has been set on this component.", this);


            holder = new GameObject("holder");
            holder.transform.parent = this.transform;
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.identity;

            DraggingPoint = new GameObject("DragContactPoint");
            DraggingPoint.transform.parent = holder.transform;
            DraggingPoint.transform.localPosition = Vector3.zero;
            DraggingPoint.transform.localRotation = Quaternion.identity;
            DraggingPoint.SetActive(false);

            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    Object.Destroy(collider);
                }
            }
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;

            if (LaserHit != null && SetLaserColor)
            {
                var laserColor = new Color(color.r, color.g, color.b, 1f);

                LaserHit.GetComponent<SpriteRenderer>().color = laserColor;

                var lights = LaserHit.GetComponentsInChildren<Light>();
                for (int i = 0; i < lights.Length; i++)
                    lights[i].color = laserColor;
            }
        }

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            if (PointerIn != null)
                PointerIn(this, e);

            ExecuteEvents.Execute<IFocusable>(e.target.gameObject, null, (x, y) => x.OnFocusEnter());

        }

        public virtual void OnPointerOut(PointerEventArgs e)
        {
            if (PointerOut != null)
                PointerOut(this, e);
            ExecuteEvents.Execute<IFocusable>(e.target.gameObject, null, (x, y) => x.OnFocusExit());

        }


        bool rBodyWasKinematic = false;
        public virtual void OnPointerDown(PointerEventArgs e)
        {
            if (PointerDown != null)
                PointerDown(this, e);

            // start dragging if the target isn't a button
            if (e.target != null && !ExecuteEvents.CanHandleEvent<IInputClickHandler>(e.target.gameObject))
            {
                DraggedObject = (e.target.name == "TranslateSlider") ? e.target.parent : e.target;

                DraggingPoint.transform.position = e.contactPoint;
                LocalDragContactPoint = DraggedObject.InverseTransformPoint(e.contactPoint);
                DraggingPoint.SetActive(true);

                var rBody = DraggedObject.GetComponent<Rigidbody>();
                if (rBody != null)
                {
                    rBodyWasKinematic = rBody.isKinematic;
                    rBody.isKinematic = true;
                }

                // save the scaler if we have one, and start tracking the dragger
                currScaler = DraggedObject.GetComponent<Scaler>();
                lastDraggerPosition = DraggingPoint.transform.position;

            }
        }

        public virtual void OnPointerUp(PointerEventArgs e)
        {
            if (PointerUp != null)
                PointerUp(this, e);

            // release dragged object
            if (DraggedObject != null)
            {
                var rBody = DraggedObject.GetComponent<Rigidbody>();
                if (rBody != null)
                {
                    if (rBodyWasKinematic)
                        rBody.isKinematic = true;
                    else
                    {
                        rBody.isKinematic = false;
                        rBody.AddForce(dragDelta * ThrowForce);
                    }
                    rBodyWasKinematic = false;
                }

                LocalDragContactPoint = Vector3.zero;
                DraggingPoint.SetActive(false);
                DraggedObject = null;
            }

            if (e.target != null)
            {
                var args = new InputClickedEventData(EventSystem.current);
                ExecuteEvents.Execute<IInputClickHandler>(e.target.gameObject, null, (x, y) => x.OnInputClicked(args));
            }

            currScaler = null;
        }

        Vector3 lastDraggerPosition;
        Vector3 dragDelta;
        public virtual void OnDrag(PointerEventArgs pArgs, PointerDragArgs dArgs)
        {
            if (PointerDrag != null)
                PointerDrag(this, pArgs, dArgs);

            dragDelta = DraggingPoint.transform.position - lastDraggerPosition;

            if (currScaler != null)
            {
                currScaler.HoldMovementUpdate(currScaler.transform.InverseTransformVector(Vector3.Normalize(dragDelta)));
            }
            else
            {
                // manually translate the target with the pointer
                var movementPoint = DraggedObject.TransformPoint(LocalDragContactPoint);
                Vector3 globalizedOffset = DraggedObject.TransformVector(LocalDragContactPoint);

                Vector3 newPosition = Vector3.Lerp(movementPoint, DraggingPoint.transform.position, .5f * Time.deltaTime * 20f);
                newPosition -= globalizedOffset;

                DraggedObject.position = newPosition;

                //Debug.DrawLine(holder.transform.position, movementPoint);
                //Debug.DrawLine(DraggedObject.position, DraggedObject.position + globalizedOffset, Color.green);
            }
        }

        public void UpdateLaserHit(bool bHit, RaycastHit hit)
        {
            if (bHit)
                LaserHit.gameObject.SetActive(true);
            else
            {
                LaserHit.gameObject.SetActive(false);
                return;
            }

            LaserHit.position = hit.point + (hit.normal * LaserHitGap);
            LaserHit.LookAt(hit.point + hit.normal);
        }

        private void Update()
        {
            if (!isActive)
            {
                isActive = true;
                this.transform.GetChild(0).gameObject.SetActive(true);
            }

            float dist = 100f;

            Ray raycast = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            bool bHit = Physics.Raycast(raycast, out hit);



            if (previousContact && previousContact != hit.transform)
            {
                PointerEventArgs args = new PointerEventArgs();
                args.fromInputSource = pose.inputSource;
                args.distance = 0f;
                args.flags = 0;
                args.target = previousContact;
                OnPointerOut(args);
                previousContact = null;
            }
            if (bHit && previousContact != hit.transform)
            {
                PointerEventArgs argsIn = new PointerEventArgs();
                argsIn.fromInputSource = pose.inputSource;
                argsIn.distance = hit.distance;
                argsIn.flags = 0;
                argsIn.target = hit.transform;
                OnPointerIn(argsIn);
                previousContact = hit.transform;
            }
            if (!bHit)
            {
                previousContact = null;
            }
            if (bHit && hit.distance < 100f)
            {
                dist = hit.distance;
            }



            if (interactWithUI.GetStateUp(pose.inputSource))
            {
                PointerEventArgs argsUp = new PointerEventArgs();
                argsUp.fromInputSource = pose.inputSource;
                argsUp.flags = 0;
                if (bHit)
                {
                    argsUp.distance = hit.distance;
                    argsUp.target = hit.transform;
                }
                OnPointerUp(argsUp);
            }

            if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
            {
                pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
                pointer.GetComponent<MeshRenderer>().material.color = clickColor;
            }
            else
            {
                pointer.transform.localScale = new Vector3(thickness, thickness, dist);
                pointer.GetComponent<MeshRenderer>().material.color = color;
            }
            pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);

            // laser hit
            if (LaserHit != null)
                UpdateLaserHit(bHit, hit);

            if (interactWithUI.GetStateDown(pose.inputSource))
            {
                PointerEventArgs argsDown = new PointerEventArgs();
                argsDown.fromInputSource = pose.inputSource;
                argsDown.distance = hit.distance;
                argsDown.flags = 0;
                argsDown.target = hit.transform;
                argsDown.contactPoint = hit.point;
                OnPointerDown(argsDown);
            }

            if (DraggedObject != null)
            {

                PointerEventArgs args = new PointerEventArgs();
                args.fromInputSource = pose.inputSource;
                args.flags = 0;
                if (bHit)
                {
                    args.distance = hit.distance;
                    args.target = hit.transform;
                }

                PointerDragArgs argsDrag = new PointerDragArgs();
                argsDrag.draggedObject = DraggedObject;
                //argsDrag.movementDelta = 
                OnDrag(args, argsDrag);
            }


        }
    }



    public struct PointerEventArgs
    {
        public SteamVR_Input_Sources fromInputSource;
        public uint flags;
        public float distance;
        public Transform target;
        public Vector3 contactPoint;
    }

    public struct PointerDragArgs
    {
        public Transform draggedObject;
        public Vector3 movementDelta;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);
    public delegate void PointerDragHandler(object sender, PointerEventArgs pArgs, PointerDragArgs dArgs);
}
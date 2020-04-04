using UnityEngine;
using Valorem.HoloHands;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Events;

namespace Valorem.HoloConsole.CustomHoloHands
{
    public class HoloHold : MonoBehaviour
    {
        //public Renderer moveRenderer;
        [Tooltip("Set the 'Move Transform' to the Transform you want to Hold, This script should be on it's Collider")]
        public Transform HoldTransform;

        public enum OneHanded
        {
            Translate,
            Spin,
            None,
            LocalTranslate
        }
        [SerializeField]

        [Header("One Handed/Two Handed Settings")]

        public OneHanded OneHandedMode = OneHanded.Translate;
        public bool TwoHandedTranslate = true;
        public bool TwoHandedRotate = true;
        public bool TwoHandedScale = false;

        [Header("Translate Settings")]
        public Vector2 XTranslateLimits = new Vector2(Mathf.NegativeInfinity,Mathf.Infinity);
        public Vector2 YTranslateLimits = new Vector2(Mathf.NegativeInfinity, Mathf.Infinity);
        public Vector2 ZTranslateLimits = new Vector2(Mathf.NegativeInfinity, Mathf.Infinity);

        [Header("Scale Settings")]

        public Vector2 ScaleLimits = new Vector2(0, Mathf.Infinity);

        [Header("Speed/Smooth Settings")]

        public float TranslateSpeed = .9f; // 0 (slow) to 1 (fast)
        public float RotateSpeed = .9f;
        public float SpinSpeed = -2f;


        private bool _twoHanded = false;
        private Vector3 _headPosition;
        private Vector3 _shoulderPosition;
        private Vector3 _primaryHandPosition;
        private Vector3 _secondaryHandPosition;
        private Vector3 _holdDelta; // x angle delta from shoulder to hand/transform, y angle delta from sholder to hand/transform, z distance mulitplier from shoulder to hand/transform
        private Vector3 _targetPosition;
        private float _rotateDeltaAngle;
        private float _targetRotationY;
        private float _spinDeltaAngle;
        private float _targetSpinAngle;
        private float _startSpingAngle;
        private Vector3 _lastHandPosition;

        //Scaling Variables
        private Vector3 _scaleMultiplier;
        private Vector3 _targetScale;

        public float ScaleSpeed = .9f;

        private bool _isHeld = false;
        private long _localId;
        private long _userId;

        [Header("Network Settings")]
        public bool IsNetworked = false;
        private long _networkId = -1;

        [Header("Air Tap Events")]

        public UnityEvent TappedEvents;

        [Header("UI Settings")]
        public bool ShowHighlight = true;
        public bool ShowGizmos = true;

        //highlight variables 
        private bool _isHighlight = false;
        private bool _translateGizmoOn = false;
        private bool _rotateGizmoOn = false;
        private bool _scaleGizmoOn = false;

        //injected variables
        private HandManager _handManager;
        private HoloHoldController _holoSelect;

        void Start()
        {
            if (HoldTransform == null) HoldTransform = transform;
            if (GetComponent<Rigidbody>() != null)
            {
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        private void OnEnable()
        {
            if (_holoSelect == null || _handManager == null)
            {
                HoloHoldController holoHoldController = Camera.main.GetComponent<HoloHoldController>();
                SetHandManager(holoHoldController.HandManager);
                SetHoloSelect(holoHoldController);
            }
        }

        public void OnHoldEnable()
        {
            if (!_isHeld) // if it's not already being held,(going from one hadn to two hand will trigger OnHoldEnable() twice)
            {
                
            }
            _isHeld = true;
            if (HoldTransform.GetComponent<Rigidbody>() != null)
            {
                HoldTransform.GetComponent<Rigidbody>().isKinematic = true;
            }

            GetPlayerData();
            switch (OneHandedMode)
            {
                case OneHanded.Translate:
                    CalcMovementDeltas();
                    break;
                case OneHanded.Spin:
                    CalcSpinDelta();
                    break;
                case OneHanded.LocalTranslate:
                    _lastHandPosition = _primaryHandPosition;
                    _targetPosition = HoldTransform.position;
                    break;
            }
        }
        void OnHoldDisable()
        {
            //_gizmoFadeIn = 0;
            _isHeld = false;
            if (HoldTransform.GetComponent<Rigidbody>() != null)
            {
                HoldTransform.GetComponent<Rigidbody>().isKinematic = false;
            }

            _twoHanded = false;
        }
        void Update()
        {
            if (_isHeld)
            {
                GetPlayerData();

                if (_twoHanded) // move and rotate with both hands
                {
                    if (TwoHandedTranslate)
                    {
                        SetTargetPosition();
                        PositionTransform();
                    }
                    if (TwoHandedRotate)
                    {
                        SetTargetRotation();
                        RotateTransform();
                    }
                    if (TwoHandedScale)
                    {
                        SetTargetScale();
                        ScaleTransform();
                    }
                }
                else
                {
                    if (OneHandedMode == OneHanded.Translate)// move the object with one hand
                    {
                        SetTargetPosition();
                        PositionTransform();
                    }
                    else if (OneHandedMode == OneHanded.Spin) // spin the object with one hand
                    {
                        SetTargetSpin();
                        SpinTransform();
                    }
                    else if (OneHandedMode == OneHanded.LocalTranslate)
                    {
                        Vector3 deltaPosition = _primaryHandPosition - _lastHandPosition;
                        _targetPosition += deltaPosition;
                        PositionTransform();
                        _lastHandPosition = _primaryHandPosition;
                    }
                }
            }
        }
        void GetPlayerData()
        {        
            if (_handManager == null)
            {
                Debug.LogError("HandManager not found for " + name);
                enabled = false;
                return;
            }           

            _headPosition = Camera.main.transform.position;
            _shoulderPosition = Camera.main.transform.position + (Vector3.up * -.15f);
            _primaryHandPosition = _handManager.PrimaryHand.Position;
            if (_twoHanded) _secondaryHandPosition = _handManager.SecondaryHand.Position;
        }
        void SetTargetRotation()
        {
            Vector3 twoHandDelta = _secondaryHandPosition - _primaryHandPosition;
            _targetRotationY = Vector2.Angle(HoldTransform.up, new Vector2(twoHandDelta.x, twoHandDelta.z));
            if (twoHandDelta.x < 0f)
            {
                _targetRotationY *= -1f;
            }
        }
        void SetTargetSpin()
        {
            Vector3 spinDelta = _primaryHandPosition - _headPosition;
            var spinAngY = Vector2.Angle(HoldTransform.up, new Vector2(spinDelta.x, spinDelta.z));
            if (spinDelta.x < 0)
            {
                spinAngY *= -1;
            }
            _targetSpinAngle = spinAngY + _spinDeltaAngle - _startSpingAngle;
        }
        void SetTargetPosition()
        {
            Vector3 handPosition;
            if (_twoHanded)
            {
                handPosition = (_primaryHandPosition + _secondaryHandPosition) / 2f;
            }
            else
            {
                handPosition = _primaryHandPosition;
            }
            Vector3 handDelta = handPosition - _shoulderPosition;
            var handAngleY = Vector2.Angle(Vector2.up, new Vector2(handDelta.x, handDelta.z));
            if (handDelta.x < 0)
            {
                handAngleY *= -1;
            }
            var handDist2D = Mathf.Sqrt(Mathf.Pow(handDelta.x, 2) + Mathf.Pow(handDelta.z, 2));
            var handAngleX = Vector2.Angle(Vector2.right, new Vector2(handDist2D, handDelta.y));
            if (handDelta.y > 0)
            {
                handAngleX *= -1;
            }
            var handDistance = Vector3.Distance(_shoulderPosition, handPosition);
            var objectRotation = HoldTransform.rotation;
            var objectPosition = HoldTransform.position;
            HoldTransform.position = _shoulderPosition;
            HoldTransform.eulerAngles = new Vector3(handAngleX + _holdDelta.x, handAngleY + _holdDelta.y, 0f);
            HoldTransform.Translate(0, 0, handDistance * _holdDelta.z);
            _targetPosition = HoldTransform.position;
            HoldTransform.position = objectPosition;
            HoldTransform.rotation = objectRotation;


        }
        void SetTargetScale()
        {
            float handDistance = Vector3.Distance(_primaryHandPosition, _secondaryHandPosition);
            _targetScale = _scaleMultiplier * handDistance;
        }

        public delegate void MovementUpdateHandler(Vector3 movement);
        public event MovementUpdateHandler MovementUpdate;

        void PositionTransform()
        {
            Vector3 tempPos = HoldTransform.localPosition;
            _translateGizmoOn = true;
            float lag = Time.deltaTime * 60.0f;
            float speed = 60f - TranslateSpeed * 60f;
            HoldTransform.position = (HoldTransform.position * speed + lag * _targetPosition) / (speed + lag);

            //Constrain To Limits
            HoldTransform.localPosition = new Vector3(
                Mathf.Clamp(HoldTransform.localPosition.x, XTranslateLimits.x, XTranslateLimits.y),
                Mathf.Clamp(HoldTransform.localPosition.y, YTranslateLimits.x, YTranslateLimits.y),
                Mathf.Clamp(HoldTransform.localPosition.z, ZTranslateLimits.x, ZTranslateLimits.y));

            if (MovementUpdate != null)
                MovementUpdate(HoldTransform.localPosition - tempPos);
        }
        void SpinTransform()
        {
            _rotateGizmoOn = true;
            HoldTransform.eulerAngles = new Vector3(0f, _targetSpinAngle * SpinSpeed + _startSpingAngle, 0f);
        }
        void RotateTransform()
        {
            _rotateGizmoOn = true;
            HoldTransform.eulerAngles = new Vector3(0f, _targetRotationY + _rotateDeltaAngle, 0f);
        }
        void ScaleTransform()
        {
            _scaleGizmoOn = true;
            float lag = Time.deltaTime * 60.0f;
            float speed = 60f - ScaleSpeed * 60f;
            HoldTransform.localScale = (HoldTransform.localScale * speed + lag * _targetScale) / (speed + lag);

            //Constrain Scale to Limits
            HoldTransform.localScale = Vector3.one * Mathf.Clamp(HoldTransform.localScale.x, ScaleLimits.x, ScaleLimits.y);
        }
        void CalcMovementDeltas()
        {
            Vector3 handPosition;
            if (_twoHanded)
            {
                handPosition = (_primaryHandPosition + _secondaryHandPosition) / 2f;
            }
            else
            {
                handPosition = _primaryHandPosition;
            }
            Vector3 handDelta = handPosition - _shoulderPosition;
            Vector3 objectDelta = HoldTransform.position - _shoulderPosition;

            float objectDist = Vector3.Distance(_shoulderPosition, HoldTransform.position);
            float handDist = Vector3.Distance(_shoulderPosition, handPosition);

            _holdDelta.z = objectDist / handDist;

            float objectAngY = Vector2.Angle(Vector2.up, new Vector2(objectDelta.x, objectDelta.z));
            if (objectDelta.x < 0)
            {
                objectAngY *= -1;
            }
            float handAngY = Vector2.Angle(Vector2.up, new Vector2(handDelta.x, handDelta.z));
            if (handDelta.x < 0)
            {
                handAngY *= -1;
            }

            float objectDist2D = Mathf.Sqrt(Mathf.Pow(objectDelta.x, 2) + Mathf.Pow(objectDelta.z, 2));
            float handDist2D = Mathf.Sqrt(Mathf.Pow(handDelta.x, 2) + Mathf.Pow(handDelta.z, 2));

            float objectAngX = Vector2.Angle(Vector2.right, new Vector2(objectDist2D, objectDelta.y));
            if (objectDelta.y > 0)
            {
                objectAngX *= -1;
            }
            float handAngX = Vector2.Angle(Vector2.right, new Vector2(handDist2D, handDelta.y));
            if (handDelta.y > 0)
            {
                handAngX *= -1;
            }
            float deltaXAngle = Mathf.DeltaAngle(handAngX, objectAngX);
            float deltaYAngle = Mathf.DeltaAngle(handAngY, objectAngY);
            _holdDelta = new Vector3(deltaXAngle, deltaYAngle, _holdDelta.z);
        }
        void CalcRotationalDelta()
        {
            Vector3 twoHandDelta = _secondaryHandPosition - _primaryHandPosition;
            var twoHandAngY = Vector2.Angle(Vector2.up, new Vector2(twoHandDelta.x, twoHandDelta.z));
            if (twoHandDelta.x < 0)
            {
                twoHandAngY *= -1;
            }
            _rotateDeltaAngle = Mathf.DeltaAngle(twoHandAngY, HoldTransform.eulerAngles.y);
        }
        void CalcScaleMultiplier()
        {
            float handDistance = Vector3.Distance(_primaryHandPosition, _secondaryHandPosition);
            Vector3 startingScale = HoldTransform.localScale;
            _scaleMultiplier = startingScale / handDistance;
        }
        void CalcSpinDelta()
        {
            Vector3 spinDelta = _primaryHandPosition - _headPosition;
            var spinAngY = Vector2.Angle(Vector2.up, new Vector2(spinDelta.x, spinDelta.z));
            if (spinDelta.x < 0)
            {
                spinAngY *= -1;
            }
            _spinDeltaAngle = Mathf.DeltaAngle(spinAngY, HoldTransform.eulerAngles.y);
            _startSpingAngle = HoldTransform.eulerAngles.y;
        }
        public void TwoHandStart(bool doubleTapped, bool isPrimary)
        {
            if (!TwoHandedTranslate && !TwoHandedRotate && !TwoHandedScale) return;
            _twoHanded = true;
            GetPlayerData();
            CalcMovementDeltas();
            CalcRotationalDelta();
            //get Y axis rotational delta
            //if scaling get scale multiplier
            CalcScaleMultiplier();
        }
        void EndInteraction()
        {
            _holoSelect.DropTransform(true);
            OnHoldDisable();
        }
        public void AirTap(bool doubleTapped, bool isPrimary)
        {
            TappedEvents.Invoke();
        }
        public void EndHold()
        {
            EndInteraction();
        }
        public void SetHighlight(bool isHighlight)
        {
            _isHighlight = isHighlight;
        }
        public bool IsGazed()
        {
            bool returnBool = _isHighlight;
            return returnBool;
        }
        public bool IsHeld()
        {
            bool returnBool = _isHeld;
            return returnBool;
        }
        public bool TranslateToolActive()
        {
            bool returnBool = _translateGizmoOn;
            if (!ShowGizmos) returnBool = false;
            return returnBool;
        }
        public bool RotateToolActive()
        {
            bool returnBool = _rotateGizmoOn;
            if (!ShowGizmos) returnBool = false;
            return returnBool;
        }
        public bool ScaleToolActive()
        {
            bool returnBool = _scaleGizmoOn;
            if (!ShowGizmos) returnBool = false;
            return returnBool;
        }
        public bool TwoHandedActive()
        {
            bool returnBool = _twoHanded;
            return returnBool;
        }
        public void SetHandManager(HandManager hManager)
        {
            _handManager = hManager;
        }
        public void SetHoloSelect(HoloHoldController hSelect)
        {
            _holoSelect = hSelect;
        }
        public void SetLocalId(long id)
        {
            _localId = id;
        }
        public long GetLocalId()
        {
            return _localId;
        }
        public void SetNetworkedId(long id)
        {
            _networkId = id;
        }
        public long GetNetworkedId()
        {
            return _networkId;
        }
        public void SetUserId(long id)
        {
            _userId = id;
        }
        public long GetUserId()
        {
            return _userId;
        }
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (XTranslateLimits.x == -Mathf.Infinity &&
                XTranslateLimits.y == Mathf.Infinity &&
                YTranslateLimits.x == -Mathf.Infinity &&
                YTranslateLimits.y == Mathf.Infinity &&
                ZTranslateLimits.x == -Mathf.Infinity &&
                ZTranslateLimits.y == Mathf.Infinity)
            {
                return;
            }


            Vector3 center;
            Vector3 size;
            Transform par = null;
            if (HoldTransform != null && HoldTransform.parent != null)
            {
                par = HoldTransform.parent;
            }
            else if (HoldTransform == null && transform.parent != null)
            {
                par = transform.parent;
            }
            size = new Vector3(
            XTranslateLimits.y - XTranslateLimits.x,
            YTranslateLimits.y - YTranslateLimits.x,
            ZTranslateLimits.y - ZTranslateLimits.x);
            center = new Vector3(
                (XTranslateLimits.x + XTranslateLimits.y)/2f,
                (YTranslateLimits.x + YTranslateLimits.y) / 2f,
                (ZTranslateLimits.x + ZTranslateLimits.y)/ 2f);
            if (par != null)
            {
                //center = par.TransformPoint(center);
                //center += par.position;
                Gizmos.matrix = par.localToWorldMatrix;
            }
            
            Gizmos.color = new Color(1, .75f, 0, .25f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(1, .75f, 0, .75f);
            Gizmos.DrawWireCube(center, size);


        }
#endif
    }
}

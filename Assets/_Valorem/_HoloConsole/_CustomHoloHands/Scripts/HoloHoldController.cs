using UnityEngine;
using Valorem.HoloHands;

namespace Valorem.HoloConsole.CustomHoloHands
{
    public class HoloHoldController : MonoBehaviour
    {
        public HandManager HandManager { get { return _handManager;  } }
        private HandManager _handManager;
        public HoloLensInput HoloLensInput;
        public MixedRealityInput MixedRealityInput;
        public MouseInput MouseInput;
        public bool CursorEnabled = true;
        public bool GizmosEnabled = true;
        //public Color GizmoColor = new Color(.75f, .75f, .75f);

        public bool GlobalOverrideControl;
        public Transform GlobalTransform;
        public enum HoloHoldState
        {
            Null,
            Gazed,
            Highlighted,
            Held
        }
        //protected HoloHoldState holoHoldState = HoloHoldState.Null;
        public class GizmoStats
        {
            public HoloHoldState HhState = HoloHoldState.Null;

            public float GazeFadeIn;
            public float HighlightFadeIn;
            public float HoldFadeIn;

            public bool TranslateOn;
            public bool RotateOn;
            public bool ScaleOn;

            public float TranslateFadeIn;
            public float RotateFadeIn;
            public float ScaleFadeIn;

            //Hands
            public byte HandsReady;

            public byte HandsHeld;

            public float HandReadyFi1;
            public float HandREadyFi2;
            public float HandHeldFi1;
            public float HandHeldFi2;

            public long HoloHoldId = -1;
            public long NetworkHoldId = -1;

            public Color MainColor = new Color(.75f, .75f, .75f);
        }
        protected GizmoStats MyGizmo;
        protected HoloHold[] Holds;
        protected HoloHold[] NetworkedHolds;
        protected HoloHold CurrentHoloHold;
        protected float GazeDistance;
        protected Quaternion GazeRotation;
        private Quaternion _gazeRotationInterpolation = Quaternion.identity;
        static Material _lineMaterial;

        private bool _lookForNewHoloHolds;

        public LayerMask IgnoreLayer;
        protected virtual void Start()
        {
            NetworkedHolds = new HoloHold[0];
            MyGizmo = new GizmoStats();
            HandGizmos.Initialize();
            AssignHoloHoldIDs();
            if (GlobalOverrideControl) //if there is only one object that is being moved so no need for gaze
            {
                CurrentHoloHold = GlobalTransform.GetComponent<HoloHold>();
            }
            CreateLineMaterial();
            GazeRotation = transform.rotation;
        }

        void Awake()
        {
#if UNITY_EDITOR
            if (HandManager.Instance == null)
            {
                _handManager = Instantiate(MouseInput);
            }

#else
            //if HoloLens
            if (HandManager.Instance == null)
            {
                _handManager = Instantiate(HoloLensInput);
            }
            //if MR Headset
            //if (HandManager.Instance == null)
            //{
            //    _handManager = Instantiate(MixedRealityInput);
            //}
#endif
        }
        void OnEnable()
        {
            HandManager.OnAirTapEnd += OnTapEnd;
            HandManager.OnAirHoldStart += OnHoldStart;
            HandManager.OnTwoHandStart += OnTwoHoldStart;
            HandManager.OnAirHoldEnd += OnHoldEnd;
        }
        void OnDisable()
        {
            HandManager.OnAirTapEnd -= OnTapEnd;
            HandManager.OnAirHoldStart -= OnHoldStart;
            HandManager.OnTwoHandStart -= OnTwoHoldStart;
            HandManager.OnAirHoldEnd -= OnHoldEnd;
        }
        static void CreateLineMaterial()
        {
            if (!_lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                // Turn on alpha blending
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                _lineMaterial.SetInt("_ZWrite", 0);
            }
        }
        public void NewHoloHoldsEnabled()
        {
            _lookForNewHoloHolds = true;
        }
        void AssignHoloHoldIDs()
        {
            HoloHold[] holds = FindObjectsOfType(typeof(HoloHold)) as HoloHold[];
            long networkedHoldsCount = 0;
            foreach (HoloHold t in holds)
            {
                if (holds[networkedHoldsCount].IsNetworked)
                {
                    networkedHoldsCount++;
                }

            }

            Holds = new HoloHold[holds.Length];
            NetworkedHolds = new HoloHold[networkedHoldsCount];

            long index = 0;
            long netIndex = 0;
            foreach (HoloHold t in holds)
            {

                Holds[index] = holds[index];
                Holds[index].SetLocalId(index);
                Holds[index].SetHandManager(HandManager);
                Holds[index].SetHoloSelect(this);
                
                if (holds[index].IsNetworked)
                {
                    NetworkedHolds[netIndex] = holds[index];
                    NetworkedHolds[netIndex].SetNetworkedId(netIndex);
                    netIndex++;
                }
                else
                {
                    Holds[index].SetNetworkedId(-1);
                }
                index++;
            }
            _lookForNewHoloHolds = false;
        }
        // UPDATE
        protected virtual void Update()
        {
            if (_lookForNewHoloHolds)
            {
                AssignHoloHoldIDs();
            }
            //if (Input.GetMouseButtonDown(1))
            //{
            //    OnTapEnd(false, true);
            //}
            GetGazeRotation();
            if (CurrentHoloHold)
            {
                UpdateGizmosStates();
                UpdateGizmosFadeIns(MyGizmo);
            }
            
            if (MyGizmo.HhState != HoloHoldState.Held &&
                !GlobalOverrideControl)
            {
                FindGazedTransform();
            }

            SendHeadAndCursor();
        }
        void OnTapEnd(bool doubleTapped, bool isPrimary)
        {
            if (MyGizmo.HhState != HoloHoldState.Null &&
                MyGizmo.HhState != HoloHoldState.Held)
            {
                CurrentHoloHold.AirTap(doubleTapped, isPrimary);
            }
        }
        void OnHoldStart(bool doubleTapped, bool isPrimary)
        {
            StartHoloHold(false);
        }
        void OnTwoHoldStart(bool doubleTapped, bool isPrimary)
        {
            StartHoloHold(true);
        }
        void OnHoldEnd(bool doubleTapped, bool isPrimary)
        {
            if (CurrentHoloHold != null)
            {
                CurrentHoloHold.EndHold();
            }
        }
        void StartHoloHold(bool twoHanded)
        {
            if (GlobalOverrideControl) //if there is only one object that is being moved so no need for gaze
            {
                if (MyGizmo.HhState != HoloHoldState.Held &&
                    GlobalTransform != null)
                {
                    //heldTransform = soloTransform;
                    CurrentHoloHold = GlobalTransform.GetComponent<HoloHold>();
                    //holoHoldTransform = holoHold.holdTransform;
                    //holoHold.handManager = handManager;

                    MyGizmo.HhState = HoloHoldState.Held;
                    // holoHold.enabled = true;
                    if (twoHanded)
                    {
                        CurrentHoloHold.OnHoldEnable();
                        CurrentHoloHold.TwoHandStart(false, true);
                    }
                    else
                    {
                        CurrentHoloHold.OnHoldEnable();
                    }
                }
            }
            else if (MyGizmo.HhState != HoloHoldState.Held && //if there is no object held yet
                CurrentHoloHold != null && // make sure there is smething gazed or highlighted though
                CurrentHoloHold.GetUserId() == 0 && // make sure no one else is holding this
                CurrentHoloHold.OneHandedMode != HoloHold.OneHanded.None)
            {
                //heldTransform = gazedTransform;
                //holoHold = gazedTransform.GetComponent<HoloHold>();
                //holoHoldTransform = holoHold.holdTransform;

                MyGizmo.HhState = HoloHoldState.Held;

                //holoHold = heldTransform.GetComponent<HoloHold>();
                //holoHold.handManager = handManager;
                //holoHold.selector = this;
                //holoHold.enabled = true;
                if (twoHanded)
                {
                    CurrentHoloHold.OnHoldEnable();
                    CurrentHoloHold.TwoHandStart(false, true);
                }
                else
                {
                    CurrentHoloHold.OnHoldEnable();
                }
            }
            else if (MyGizmo.HhState == HoloHoldState.Held && //if there is a object held 
                twoHanded &&
                CurrentHoloHold != null) //and you add a second hand
            {
                CurrentHoloHold.TwoHandStart(false, true);
            }
            if (MyGizmo.HhState == HoloHoldState.Held && 
                CurrentHoloHold != null)
            {
                //broadcast grab state of 0
                if (CurrentHoloHold.IsNetworked) SendHoldTransform(CurrentHoloHold.GetNetworkedId(), 0, CurrentHoloHold.transform);
                //CustomMessages.Instance.SendHoldTransform(heldID, 0, heldTransform.localPosition, heldTransform.localRotation, heldTransform.localScale);
            }
        }
        public void DropTransform(bool dropped)
        {
            EndHighlight();
            if (CurrentHoloHold.IsNetworked) SendHoldTransform(CurrentHoloHold.GetNetworkedId(), 2, CurrentHoloHold.transform);
            //CustomMessages.Instance.SendHoldTransform(heldID, 2, heldTransform.localPosition, heldTransform.localRotation, heldTransform.localScale);
            //holoHoldTransform = null;
            CurrentHoloHold = null;
        }
        private void EndHighlight() //reset all fade ins and turn highlight off HoloHold
        {
            SetGaze(false);
            MyGizmo = new GizmoStats();
        }
        void FindGazedTransform()
        {
            GazeDistance = 2f;
            RaycastHit hitInfo;
            LayerMask lMask = ~IgnoreLayer;
            //if (Physics.Raycast(transform.position, transform.forward, out hitInfo, Mathf.Infinity, lMask))
            if (Physics.Raycast(transform.position, GazeRotation * Vector3.forward, out hitInfo, Mathf.Infinity, lMask))
                {
                if (hitInfo.transform.GetComponent<HoloHold>())
                {
                    if (CurrentHoloHold != hitInfo.transform.GetComponent<HoloHold>()) // if its a new holoHold
                    {
                        //reset all fade ins and turn highlight off HoloHold
                        EndHighlight();
                        CurrentHoloHold = hitInfo.transform.GetComponent<HoloHold>();
                        MyGizmo.HoloHoldId = CurrentHoloHold.GetLocalId();
                        if (CurrentHoloHold.IsNetworked) MyGizmo.NetworkHoldId = CurrentHoloHold.GetNetworkedId();
                        SetGaze(true);
                    }

                    MyGizmo.HhState = HandManager.PrimaryHand.Active ? HoloHoldState.Highlighted : HoloHoldState.Gazed;
                }
                else
                {
                    if (CurrentHoloHold)
                    {
                        EndHighlight();
                        CurrentHoloHold = null;
                    }
                    
                }
                GazeDistance = hitInfo.distance - .04f ;
            }
            else if (CurrentHoloHold)
            {
                EndHighlight();
                CurrentHoloHold = null;
            }
        }
        protected virtual void SendHoloHoldState()
        {

        }
        void SetGaze(bool isHighlight)
        {
            /*
            if (gazedTransform.GetComponent<HoloGaze>() != null)
            {
                gazedTransform.GetComponent<HoloGaze>().enabled = isGazed;
            }
            */
            if (CurrentHoloHold) CurrentHoloHold.SetHighlight(isHighlight);
        }
        protected virtual void OnPostRender()
        {
            _lineMaterial.SetPass(0);
            GL.PushMatrix();
            if (CursorEnabled && MyGizmo.HhState != HoloHoldState.Held)
            {
                DrawCursor();
            }
            if (GizmosEnabled)
            {
                if (MyGizmo.HoloHoldId >= 0)
                {
                    Vector3 radialPosiiton = CurrentHoloHold.HoldTransform.position;
                    Quaternion radialRotation = Quaternion.LookRotation(transform.position - radialPosiiton, transform.up);
                    Vector3 scale = CurrentHoloHold.HoldTransform.lossyScale;
                    float radius = Mathf.Sqrt(scale.x * scale.x + scale.y * scale.y + scale.z * scale.z)/2f;
                    //float ditamce = Mathf.Clamp(Vector3.Distance(Camera.main.transform.position, radialPosiiton)- radius,0f,Mathf.Infinity);
                    //HandGizmos.DrawGazeRing(radialPosiiton, radialRotation, radius, myGizmo);
                    //HandGizmos.DrawHoldDots(radialPosiiton, radialRotation, radius, myGizmo);
                    if (CurrentHoloHold.ShowHighlight)
                    {
                        HandGizmos.DrawHighlight(radialPosiiton, radialRotation, radius, MyGizmo);
                    }
                    if (MyGizmo.TranslateOn)
                    {
                        //myGizmo.TranslateFadeIn = 1f;
                        HandGizmos.DrawTranslate(CurrentHoloHold,transform.position,transform.rotation,scale.x,MyGizmo);
                    }
                    if (MyGizmo.RotateOn)
                    {
                        //myGizmo.RotateFadeIn = 1f;
                        HandGizmos.DrawRotate(CurrentHoloHold, transform.position, transform.rotation, radius, MyGizmo);
                    }
                }
                DrawRemoteGizmos();
            }
            GL.PopMatrix();
        }
        void GetGazeRotation()
        {
            //_gazeRotation = Quaternion.Slerp(_gazeRotation, transform.rotation, Time.deltaTime * 5f);

            Quaternion diff = Quaternion.Inverse(GazeRotation) * transform.rotation;
            //_gazeRotationInterpolation = Quaternion.Slerp(_gazeRotationInterpolation, diff, Time.deltaTime * 10f);
            //_gazeRotationInterpolation = Quaternion.Slerp(_gazeRotationInterpolation, Quaternion.identity, Time.deltaTime * 30f);
            _gazeRotationInterpolation = Quaternion.Slerp(_gazeRotationInterpolation, diff, 1f/6f);
            _gazeRotationInterpolation = Quaternion.Slerp(_gazeRotationInterpolation, Quaternion.identity, .5f);
            GazeRotation = Quaternion.Slerp(GazeRotation, GazeRotation*_gazeRotationInterpolation, Time.deltaTime * 120f);

        }
        void DrawCursor()
        {
            bool hitHh = MyGizmo.HhState == HoloHoldState.Gazed ||
                MyGizmo.HhState == HoloHoldState.Highlighted;
            HandGizmos.DrawCursor(transform.position, GazeRotation, GazeDistance, hitHh, HandManager.PrimaryHand.Active, HandManager.SecondaryHand.Active, MyGizmo.MainColor);
        }
        protected virtual void SendHoldTransforms()
        {
            //foreach (var hold in _holds)
            //{
            //    CustomMessages.Instance.SendHoldTransform(hold.id, 1, hold.holdTransform.localPosition, hold.holdTransform.localRotation, hold.holdTransform.localScale);
            //}
        }
        protected virtual void SendHoldTransform(long sendHoldId, long holdState, Transform sendTransform)
        {

        }
        protected virtual void SendHeadAndCursor()
        {
            
        }
        protected virtual void DrawRemoteGizmos()
        {

        }
        //public virtual void SetGizmoColor(ColorButton colorButton)
        //{
        //    Color newColor = colorButton.currentColor;
        //    MyGizmo.MainColor = new Color(newColor.r * .75f, newColor.g * .75f, newColor.b * .75f, 1f);
        //}
        public virtual void ToggleMyGizmos(bool translateOn, bool rotateOn, bool scaleOn)
        {
            if (!MyGizmo.TranslateOn && translateOn) MyGizmo.TranslateFadeIn = 0f;
            if (!MyGizmo.RotateOn && rotateOn) MyGizmo.RotateFadeIn = 0f;
            if (!MyGizmo.ScaleOn && scaleOn) MyGizmo.ScaleFadeIn = 0f;
            MyGizmo.TranslateOn = translateOn;
            MyGizmo.RotateOn = rotateOn;
            MyGizmo.ScaleOn = scaleOn;
        }
        private void UpdateGizmosStates()
        {
            //set gizmo bools
            if (MyGizmo.HhState == HoloHoldState.Held)
            {
                MyGizmo.TranslateOn = CurrentHoloHold.TranslateToolActive();
                MyGizmo.RotateOn = CurrentHoloHold.RotateToolActive();
                MyGizmo.ScaleOn = CurrentHoloHold.ScaleToolActive();
            }
            else
            {
                MyGizmo.TranslateOn = false;
                MyGizmo.RotateOn = false;
                MyGizmo.ScaleOn = false;
            }
            //set number of ready hands
            if (HandManager.PrimaryHand.Active)
            {
                if (HandManager.SecondaryHand.Active)
                {
                    MyGizmo.HandsReady = 2;
                }
                else
                {
                    MyGizmo.HandsReady = 1;
                }
            }
            else
            {
                MyGizmo.HandsReady = 0;
            }
            // set number of held hands
            if (MyGizmo.HhState == HoloHoldState.Held)
            {
                if (CurrentHoloHold.TwoHandedActive())
                {
                    MyGizmo.HandsHeld = 2;
                }
                else
                {
                    MyGizmo.HandsHeld = 1;
                }
            }
            else
            {
                MyGizmo.HandsHeld = 0;
            }
        }
        protected void UpdateGizmosFadeIns(GizmoStats gizmoStats)
        {
            float fadeInSpeed = 1f;
            float handSpeed = 3f;
            // gaze fadein
            gizmoStats.GazeFadeIn += Time.deltaTime*fadeInSpeed;
            gizmoStats.GazeFadeIn = Mathf.Clamp(gizmoStats.GazeFadeIn, 0f, 1f);
            // highlight and hold fadein
            switch (gizmoStats.HhState)
            {
                case HoloHoldState.Gazed:
                    gizmoStats.HighlightFadeIn -= Time.deltaTime * fadeInSpeed;
                    gizmoStats.HoldFadeIn -= Time.deltaTime * fadeInSpeed;
                    break;
                case HoloHoldState.Highlighted:
                    gizmoStats.HighlightFadeIn += Time.deltaTime * fadeInSpeed;
                    gizmoStats.HoldFadeIn -= Time.deltaTime * fadeInSpeed;
                    break;
                case HoloHoldState.Held:
                    gizmoStats.HighlightFadeIn += Time.deltaTime * fadeInSpeed;
                    gizmoStats.HoldFadeIn += Time.deltaTime * fadeInSpeed;
                    break;
            }
            gizmoStats.HighlightFadeIn = Mathf.Clamp(gizmoStats.HighlightFadeIn, 0f, 1f);
            gizmoStats.HoldFadeIn = Mathf.Clamp(gizmoStats.HoldFadeIn, 0f, 1f);
            // gizmos
            if (gizmoStats.TranslateOn)
            {
                gizmoStats.TranslateFadeIn += Time.deltaTime * fadeInSpeed;
                gizmoStats.TranslateFadeIn = Mathf.Clamp(gizmoStats.TranslateFadeIn, 0f, 1f);
            }
            else gizmoStats.TranslateFadeIn = 0;
            if (gizmoStats.RotateOn)
            {
                gizmoStats.RotateFadeIn += Time.deltaTime * fadeInSpeed;
                gizmoStats.RotateFadeIn = Mathf.Clamp(gizmoStats.RotateFadeIn, 0f, 1f);
            }
            else gizmoStats.RotateFadeIn = 0;
            if (gizmoStats.ScaleOn)
            {
                gizmoStats.ScaleFadeIn += Time.deltaTime * fadeInSpeed;
                gizmoStats.ScaleFadeIn = Mathf.Clamp(gizmoStats.ScaleFadeIn, 0f, 1f);
            }
            else gizmoStats.ScaleFadeIn = 0;
            // hands ready
            switch (gizmoStats.HandsReady)
            {
                case 0:
                    gizmoStats.HandReadyFi1 -= Time.deltaTime * handSpeed;
                    gizmoStats.HandREadyFi2 -= Time.deltaTime * handSpeed;
                    break;
                case 1:
                    gizmoStats.HandReadyFi1 += Time.deltaTime * handSpeed;
                    gizmoStats.HandREadyFi2 -= Time.deltaTime * handSpeed;
                    break;
                case 2:
                    gizmoStats.HandReadyFi1 += Time.deltaTime * handSpeed;
                    gizmoStats.HandREadyFi2 += Time.deltaTime * handSpeed;
                    break;
            }
            gizmoStats.HandReadyFi1 = Mathf.Clamp(gizmoStats.HandReadyFi1, 0f, 1f);
            gizmoStats.HandREadyFi2 = Mathf.Clamp(gizmoStats.HandREadyFi2, 0f, 1f);
            // hands held
            switch (gizmoStats.HandsHeld)
            {
                case 0:
                    gizmoStats.HandHeldFi1 -= Time.deltaTime * handSpeed;
                    gizmoStats.HandHeldFi2 -= Time.deltaTime * handSpeed;
                    break;
                case 1:
                    gizmoStats.HandHeldFi1 += Time.deltaTime * handSpeed;
                    gizmoStats.HandHeldFi2 -= Time.deltaTime * handSpeed;
                    break;
                case 2:
                    gizmoStats.HandHeldFi1 += Time.deltaTime * handSpeed;
                    gizmoStats.HandHeldFi2 += Time.deltaTime * handSpeed;
                    break;
            }
            gizmoStats.HandHeldFi1 = Mathf.Clamp(gizmoStats.HandHeldFi1, 0f, 1f);
            gizmoStats.HandHeldFi2 = Mathf.Clamp(gizmoStats.HandHeldFi2, 0f, 1f);
        }
    }
}

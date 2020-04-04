using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valorem.HoloConsole.CustomHoloHands;

namespace Valorem.HoloConsole
{
    // The data model of a log. The actual gameobject is tied together by LogItem. 
    public struct Log
    {
        public int index;
        public string condition;
        public string stackTrace;
        public LogType type;
    }

    /// <summary>
    /// The core of the HoloConsole. This class manages all logging functions and input.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class LogManager : Singleton<LogManager>
    {
        [Header("Settings")]
        public bool EnabledAtStart = true;
        public Vector2 AttachedSize = new Vector2(960, 640);
        public bool FollowCamAtStart = true;
        public bool ShowLogs = true, ShowWarnings = true, ShowErrors = true, ShowExceptions = true;
        public Color LogColor = Color.white;
        public Color DarkerColor { get; private set; }
        public Color HighlightedColor = Color.yellow;
        [Range(0, 1)]
        public float ItemContrast = .12f;
        [Range(.25f, 1.75f)]
        public float ScrollSensitivity = 1f;
        [Range(.25f, 1.75f)]
        public float ScaleSensitivity = 1f;

        [Header("References")]
        public GameObject LogItemPF;
        public LogItem PlaceholderItem;
        public ExpandedLogItem ExpandedItem;
        public RectTransform Content;
        public RectTransform ViewPort;
        public TrackCamera CamTracker;
        public Billboard Billboard;
        public HoloHold Translator;
        public Scaler Scaler;
        public Transform ScrollDummyTransform;
        public ColorStrobe TopBttn;
        public HoloHoldController HoloHoldController;
        public TabController TabSystem;
        public GameObject PerformanceView;
        public GameObject ConsoleView;

        [Header("Log Type Sprites")]
        public Sprite LogSprite;
        public Sprite ErrorSprite;
        public Sprite WarningSprite;

        public RectTransform RectTransform { get; private set; }

        private List<Log> _logs = new List<Log>();
        private LinkedList<LogItem> _enabledItems;
        private Queue<LogItem> _disabledItems;
        private Vector2 _dettachedSize;
        private int _enabledItemsCount;
        private float _consoleViewPortHeightDifference;
        private bool _mouseScrollEnabled = false;
        private bool _resizing = false;
        private float _previousConsoleWidth;

        #region Constants
        public readonly float ItemHeight = 64f;
        private readonly float TopMargin = 50f;
        private readonly float BottomMargin = 50f;
        private readonly float MinWidth = 520f;
        private readonly float MinHeight = 380f;
        private readonly float MaxWidth = 1500f;
        private readonly float MaxHeight = 1800f;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(EnabledAtStart);
            _enabledItemsCount = 0;
            Application.logMessageReceived += LogReceivedHandler;
            InitializeReferences();
        }

        // if they haven't been set in editor, try to setup necessary components
        private void InitializeReferences()
        {
            RectTransform = GetComponent<RectTransform>();
            if (Billboard == null) { Billboard = GetComponent<Billboard>(); }
            if (Billboard.TargetTransform == null) Billboard.TargetTransform = Camera.main.transform;
            if (CamTracker == null) { CamTracker = GetComponent<TrackCamera>(); }
            if (Scaler == null) { Scaler = GetComponentInChildren<Scaler>(); }
            if (TopBttn == null) { TopBttn = GetComponentInChildren<ColorStrobe>(); }
            if (HoloHoldController == null)
            {
                HoloHoldController = Camera.main.GetComponent<HoloHoldController>();
                if (HoloHoldController == null) { Debug.LogError("No HoloHoldController found on main camera"); }
            }
            if (TabSystem == null) { TabSystem = GetComponentInChildren<TabController>(); }
            if (PerformanceView == null) { PerformanceView = GameObject.Find("PerformanceView"); }
            if (ConsoleView == null) { ConsoleView = GameObject.Find("ConsoleView"); }
        }

        protected void Start()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            _mouseScrollEnabled = true;
#endif

            _enabledItems = new LinkedList<LogItem>();
            _disabledItems = new Queue<LogItem>();

            RectTransform.sizeDelta = AttachedSize;
            _previousConsoleWidth = RectTransform.rect.width;
            Content.sizeDelta = new Vector2(0f, ViewPort.rect.height);
            _dettachedSize = AttachedSize;

            FollowCamera = FollowCamAtStart;

            _consoleViewPortHeightDifference = RectTransform.rect.height - ViewPort.rect.height;

            // set darker color (for every-other log)
            float contrastMulti = (1f - ItemContrast);
            DarkerColor = new Color(
                    LogColor.r * contrastMulti,
                    LogColor.g * contrastMulti,
                    LogColor.b * contrastMulti);
        }

        // Update is called once per frame
        void Update()
        {
            if (RectTransform.rect.width == _previousConsoleWidth && _resizing) {
                SetItemTextComponentsEnabled(true);
                _resizing = false;
            } 
            if (_mouseScrollEnabled) { Scroll(Input.GetAxis("Mouse ScrollWheel")); }

            // activating and deactivating the scroll-to-top button
            if (!FollowCamera && Content.anchoredPosition.y > 100f && Content.sizeDelta.y > ViewPort.rect.height)
            {
                if (!TopBttn.gameObject.activeSelf) { TopBttn.gameObject.SetActive(true); }
            }
            else if (TopBttn.gameObject.activeSelf)
            {
                TopBttn.StopStrobe();
                TopBttn.gameObject.SetActive(false);
            }
        }

        #region Public functions and properties
        public bool FollowCamera
        {
            get { return _followCamera; }
            set
            {
                _followCamera = value;
                RectTransform.pivot = (_followCamera) ? new Vector2(0f, 1f) : new Vector2(.5f, .5f);
                Scaler.gameObject.SetActive(!_followCamera);
                Translator.gameObject.SetActive(!_followCamera);
                TopBttn.gameObject.SetActive(!_followCamera);
                if (!_followCamera) { TabSystem.ShowTabs(); }
                else { TabSystem.HideTabs(); }

                CamTracker.enabled = _followCamera;
                Billboard.enabled = !_followCamera;

                if (!_followCamera)
                {
                    if (RectTransform.sizeDelta != _dettachedSize) RectTransform.sizeDelta = _dettachedSize;
                    StartCoroutine(SlerpToDetached(Camera.main.transform.TransformPoint(Vector3.forward * 7f)));
                }
                else
                {
                    if (_dettachedSize != Vector2.zero)
                        _dettachedSize = RectTransform.sizeDelta;
                    if (RectTransform.sizeDelta != AttachedSize) RectTransform.sizeDelta = AttachedSize;
                    UpdateContentHeight();
                    StopAllCoroutines();
                }
            }
        }
        private bool _followCamera;

        public void ToggleFollowCamera() { FollowCamera = !FollowCamera; }

        public void ToggleEnabled() { gameObject.SetActive(!gameObject.activeSelf); }

        public void SwitchToPerformanceView() {
            if (TabSystem.gameObject.activeSelf) { TabSystem.SetSelectedTab(1); }
            else
            {
                ConsoleView.SetActive(false);
                PerformanceView.SetActive(true);
            }

        }
        public void SwitchToConsoleView() {
            if (TabSystem.gameObject.activeSelf) { TabSystem.SetSelectedTab(0); }
            else {
                ConsoleView.SetActive(false);
                PerformanceView.SetActive(true);
            }
        }

        public void ResizeConsole(Vector2 sizeChange)
        {
            if (sizeChange.magnitude > .001f)
            {
                if (!_resizing)
                {
                    SetItemTextComponentsEnabled(false);
                    _resizing = true;
                }
                float x = Mathf.Clamp(RectTransform.sizeDelta.x + sizeChange.x, MinWidth, MaxWidth);
                float y = Mathf.Clamp(RectTransform.sizeDelta.y - sizeChange.y, MinHeight, MaxHeight);
                RectTransform.sizeDelta = new Vector2(x, y);
                _previousConsoleWidth = RectTransform.rect.width;
                UpdateContentHeight();
                CheckBounderies();

            }
        }

        public void ClearLog()
        {
            var CurrItem = _enabledItems.First;
            while (CurrItem != null)
            {
                Destroy(CurrItem.Value.gameObject);
                CurrItem = CurrItem.Next;
            }
            _enabledItems.Clear();
            _logs.Clear();

            Content.sizeDelta = new Vector2(0f, ViewPort.rect.height);
            ScrollToTop();
        }

        public void Scroll(float verticalAxis)
        {
            if (verticalAxis == 0f) { return; }
            Vector2 newPos = Content.anchoredPosition;
            newPos.y = Mathf.Clamp(newPos.y + (-verticalAxis * 140f), 0, Content.sizeDelta.y - ViewPort.rect.height);
            Content.anchoredPosition = newPos;
            CheckBounderies();
        }

        public void ScrollToTop() { StartCoroutine(LerpToTop()); }

        public void TestLog() { Debug.Log("LogManager.TestLog()"); }
        #endregion

        private void LogReceivedHandler(string condition, string stackTrace, LogType type)
        {
            // are we showing this type of item? 
            if ((type == LogType.Log && !ShowLogs) || (type == LogType.Warning && !ShowWarnings) || (type == LogType.Error && !ShowErrors))
                return;
            // get rid of the initial placeholder item, if we have one
            if (PlaceholderItem != null && PlaceholderItem.gameObject.activeSelf) PlaceholderItem.gameObject.SetActive(false);
            // make the go-to-top bttn strobe if the content is scrolled down
            if (Content.anchoredPosition.y > 100f && !TopBttn.IsStrobing) TopBttn.StartStrobe();

            Log newLog = new Log() { condition = condition, stackTrace = stackTrace, type = type, index = _logs.Count };

            if (_enabledItems.Count == 0 || GetBottomItemGap() >= ItemHeight)
            {
                LogItem newItem = CreateLogItem(newLog, _logs.Count, true);
                ShiftActiveItemsDown();
                AddItemOnTop(newItem);
                _logs.Add(newLog);
            }
            else
            {
                _logs.Add(newLog);
                CycleBottomToTop();
                ShiftActiveItemsDown();
                CheckBounderies();
            }

            UpdateContentHeight();
        }

        private LogItem CreateLogItem(string condition, string stackTrace, LogType type, int indexInLog, bool isActive)
        {
            LogItem newItem = Instantiate(LogItemPF).GetComponent<LogItem>();
            newItem.transform.SetParent(Content.transform, false);
            newItem.Initialize();

            // setup HoloHold component with existing HandManager
            HoloHold holoHold = newItem.GetComponent<HoloHold>();
            holoHold.SetHandManager(HoloHoldController.HandManager);
            holoHold.SetHoloSelect(HoloHoldController);

            newItem.gameObject.SetActive(isActive);
            newItem.indexInLog = indexInLog;
            newItem.gameObject.name = ("Item " + indexInLog);
            newItem.ItemType = type;
            newItem.SetText(condition, stackTrace);
            newItem.BackgroundImage.color = (indexInLog % 2 == 0) ? LogColor : DarkerColor;

            return newItem;
        }

        private LogItem CreateLogItem(Log log, int indexInLog, bool isActive)
        {
            return CreateLogItem(log.condition, log.stackTrace, log.type, indexInLog, isActive);
        }

        #region Item manipulation functions
        private void AddItemOnTop(LogItem item)
        {
            float localY = (_enabledItems.First == null) ? -(ItemHeight / 2) : _enabledItems.First.Value.RectTransform.localPosition.y + ItemHeight;
            item.RectTransform.localPosition = new Vector3(
                item.RectTransform.localPosition.x,
                localY,
                item.RectTransform.localPosition.z);
            item.gameObject.SetActive(true);
            _enabledItems.AddFirst(item);
        }

        private void AddItemOnBottom(LogItem item)
        {
            float localY = (_enabledItems.Last == null) ? -(ItemHeight / 2) : _enabledItems.Last.Value.RectTransform.localPosition.y - ItemHeight;
            item.RectTransform.localPosition = new Vector3(
                item.RectTransform.localPosition.x,
                localY,
                item.RectTransform.localPosition.z);
            item.gameObject.SetActive(true);
            _enabledItems.AddLast(item);
        }

        private void ShiftActiveItemsDown()
        {
            LinkedListNode<LogItem> currItem = _enabledItems.First;
            while (currItem != null)
            {
                currItem.Value.ShiftDown();
                currItem = currItem.Next;
            }
        }

        private void RecycleBottomToTop(Log newLog)
        {
            LogItem bottomItem = _enabledItems.Last.Value;
            bottomItem.SetLog(newLog);

            float newY = _enabledItems.First.Value.RectTransform.localPosition.y + bottomItem.Height;

            _enabledItems.RemoveLast();
            _enabledItems.AddFirst(bottomItem);

            bottomItem.RectTransform.localPosition = new Vector3(bottomItem.RectTransform.localPosition.x, newY, bottomItem.RectTransform.localPosition.z);
        }

        private void CycleTopToBottom()
        {
            // abort if we're at the bottom
            if (_enabledItems.Last.Value.indexInLog < 1) { return; }
            LogItem topItem = _enabledItems.First.Value;

            Log nextLogDown = _logs[_enabledItems.Last.Value.indexInLog - 1];
            topItem.SetLog(nextLogDown);

            _enabledItems.RemoveFirst();

            topItem.RectTransform.localPosition = new Vector3(topItem.RectTransform.localPosition.x,
                _enabledItems.Last.Value.RectTransform.localPosition.y - topItem.Height,
                topItem.RectTransform.localPosition.z);
            topItem.indexInLog = _enabledItems.Last.Value.indexInLog - 1;

            _enabledItems.AddLast(topItem);
        }

        private void CycleBottomToTop()
        {
            // abort if we're at the top
            if (_enabledItems.First.Value.indexInLog == _logs.Count - 1) { return; }
            LogItem bottomItem = _enabledItems.Last.Value;

            Log nextLogUp = _logs[_enabledItems.First.Value.indexInLog + 1];
            bottomItem.SetLog(nextLogUp);

            _enabledItems.RemoveLast();

            bottomItem.RectTransform.localPosition = new Vector3(
                bottomItem.RectTransform.localPosition.x,
                _enabledItems.First.Value.RectTransform.localPosition.y + ItemHeight,
                bottomItem.RectTransform.localPosition.z);

            bottomItem.indexInLog = _enabledItems.First.Value.indexInLog + 1;
            _enabledItems.AddFirst(bottomItem);
        }
        #endregion

        private void CheckBounderies()
        {
            if (_enabledItems.Count > 0)
            {
                float topGap = GetTopItemGap();
                float bottomGap = GetBottomItemGap();
                if ((topGap > 0 && topGap < ItemHeight) && bottomGap > 0 && bottomGap < ItemHeight) { return; }

                if (topGap < 0)
                {
                    if (bottomGap >= ItemHeight)
                    {
                        CycleTopToBottom();
                    }
                    else
                    {
                        // no room on bottom, so disable top item
                        _enabledItems.First.Value.gameObject.SetActive(false);
                        _disabledItems.Enqueue(_enabledItems.First.Value);
                        _enabledItems.RemoveFirst();
                    }
                    return;
                }
                else if (topGap >= ItemHeight && _enabledItems.First.Value.indexInLog != _logs.Count - 1)
                {
                    int logIndex = _enabledItems.First.Value.indexInLog + 1;
                    int freeItemSpaces = (int)(topGap / ItemHeight);
                    // start at the index after the bottom item, go down from there (earlier logs)
                    LogItem itemToBeAdded;
                    // use cached items if available
                    if (_disabledItems.Count > 0)
                    {
                        itemToBeAdded = _disabledItems.Dequeue();
                        itemToBeAdded.SetLog(_logs[logIndex]);
                    }
                    else
                    {
                        // make new item, leave disabled until finished
                        itemToBeAdded = CreateLogItem(_logs[logIndex], logIndex, false);
                    }
                    AddItemOnTop(itemToBeAdded);
                }

                // if bottom item is outside the console
                if (bottomGap < 0)
                {
                    // if enough space to fit one on top
                    if (topGap >= ItemHeight)
                    {
                        CycleBottomToTop();
                    }
                    else
                    {
                        // no room on top, so disable bottom item
                        _enabledItems.Last.Value.gameObject.SetActive(false);
                        _disabledItems.Enqueue(_enabledItems.Last.Value);
                        _enabledItems.RemoveLast();
                    }
                }
                // if there's room for more item(s) on the bottom and we're not at the bottom of the list
                else if (bottomGap >= ItemHeight && _enabledItems.Last.Value.indexInLog > 0)
                {
                    int logIndex = _enabledItems.Last.Value.indexInLog - 1;
                    int freeItemSpaces = (int)(bottomGap / ItemHeight);
                    // start at the index after the bottom item, go down from there (earlier logs)
                    LogItem itemToBeAdded;
                    // use cached items if available
                    if (_disabledItems.Count > 0)
                    {
                        itemToBeAdded = _disabledItems.Dequeue();
                        itemToBeAdded.SetLog(_logs[logIndex]);
                    }
                    else
                    {
                        // make new item, leave disabled until finished
                        itemToBeAdded = CreateLogItem(_logs[logIndex], logIndex, false);
                    }
                    AddItemOnBottom(itemToBeAdded);
                }
            }
        }

        private float GetMinViewPortHeight()
        {
            return RectTransform.rect.height - _consoleViewPortHeightDifference;
        }

        private void UpdateContentHeight()
        {
            float contentRectHeight = Mathf.Clamp(_logs.Count * ItemHeight, GetMinViewPortHeight(), _logs.Count * ItemHeight);
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, contentRectHeight);
        }

        // Gap between bottom of bottom log item and bottom of Console. Negative values indicate item is outside bounds of console.
        private float GetBottomItemGap()
        {
            if (_enabledItems == null || _enabledItems.Count < 1) { return 0f; }
            // have to use LogContainer because pivot of Console changes
            float bottomLogYPos = ViewPort.InverseTransformPoint(_enabledItems.Last.Value.RectTransform.position).y;
            float bottomGap = (ViewPort.rect.height / 2) + (bottomLogYPos - (ItemHeight / 2));
            // add the difference between the viewport and the Console
            bottomGap += ((RectTransform.rect.height - ViewPort.rect.height) / 2);

            return bottomGap;
        }

        private float GetTopItemGap()
        {
            if (_enabledItems == null || _enabledItems.Count < 1) { return 0f; }
            // have to use LogContainer because pivot of Console changes
            float topLogYPos = ViewPort.InverseTransformPoint(_enabledItems.First.Value.RectTransform.position).y;
            float topGap = (ViewPort.rect.height / 2) - (topLogYPos + (ItemHeight / 2));
            // add the difference between the viewport and the Console
            topGap += ((RectTransform.rect.height - ViewPort.rect.height) / 2);

            return topGap;
        }

        private void SetItemTextComponentsEnabled(bool enabled)
        {
            LinkedListNode<LogItem> currItem = _enabledItems.First;
            while (currItem != null)
            {
                currItem.Value.SetTextComponentEnabled(enabled);
                currItem = currItem.Next;
            }

            if (ExpandedItem.gameObject.activeSelf)
            {
                ExpandedItem.BodyText.enabled = enabled;
                ExpandedItem.CollapsedText.enabled = enabled;
            }
        }

        private IEnumerator SlerpToDetached(Vector3 SlerpTarget)
        {
            float _slerpTimer = 0f;

            while (_slerpTimer < 1.5f)
            {
                _slerpTimer += Time.deltaTime;

                transform.position = Vector3.Slerp(transform.position, SlerpTarget, _slerpTimer / 1f);
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator LerpToTop()
        {
            float _lerpTimer = 0f;

            while (_lerpTimer < 1f)
            {
                _lerpTimer += Time.deltaTime;
                float newY = Mathf.Lerp(Content.anchoredPosition.y, 0f, _lerpTimer / 1f);
                Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, newY);
                CheckBounderies();
                yield return new WaitForEndOfFrame();
            }
        }
    }
}

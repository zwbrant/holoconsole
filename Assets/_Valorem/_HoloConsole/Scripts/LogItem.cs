using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;
using UnityEngine.UI;
using Valorem.HoloConsole.CustomHoloHands;

namespace Valorem.HoloConsole
{
    public class LogItem : MonoBehaviour, IInputClickHandler, IFocusable/*, IManipulationHandler*/
    {
        [Header("Component References")]
        public Image TypeImage;
        public Text CollapsedText;

        [HideInInspector]
        public int indexInLog;

        public RectTransform RectTransform { get { return _rectTransform; } }
        protected RectTransform _rectTransform;

        public LogType ItemType
        {
            get { return _itemType; }
            set
            {
                switch (value)
                {
                    case LogType.Error:
                        TypeImage.sprite = LogManager.Instance.ErrorSprite;
                        break;
                    case LogType.Exception:
                        TypeImage.sprite = LogManager.Instance.ErrorSprite;
                        break;
                    case LogType.Warning:
                        TypeImage.sprite = LogManager.Instance.WarningSprite;
                        break;
                    default:
                        TypeImage.sprite = LogManager.Instance.LogSprite;
                        break;
                }
                _itemType = value;
            }
        }
        private LogType _itemType = LogType.Log;

        // maybe make this a global variable (static?) 
        public float Height { get { return _height; } }
        private float _height;

        private bool _collapsed;

        public string Condition
        {
            get { return _condition; }
            set
            {

                _condition = value;
            }
        }
        protected string _condition;

        public string StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }
        protected string _stackTrace;


        public Image BackgroundImage { get; private set; }

        protected void Awake()
        {

        }

        internal void Initialize()
        {
            _rectTransform = GetComponent<RectTransform>();
            _height = _rectTransform.sizeDelta.y;
            _collapsed = true;
            if (String.IsNullOrEmpty(Condition) && String.IsNullOrEmpty(StackTrace))
                SetText(CollapsedText.text, "");
            BackgroundImage = GetComponent<Image>();
            GetComponent<HoloHold>().HoldTransform = LogManager.Instance.ScrollDummyTransform;
        }

        // Use this for initialization
        protected void Start()
        {

            // REMOVE
            //Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            //float normalizedYPos = (LogManager.Instance.ContentRectTrans.sizeDelta.y - Math.Abs(RectTransform.anchoredPosition.y)) / LogManager.Instance.ContentRectTrans.sizeDelta.y;
            //normalizedYPos *= 1.05f;

        }

        public void ShiftUp()
        {
            _rectTransform.anchoredPosition = (new Vector2(0f, _rectTransform.anchoredPosition.y + _height));
        }

        public void ShiftDown()
        {
            _rectTransform.anchoredPosition = (new Vector2(0f, _rectTransform.anchoredPosition.y - _height));
        }

        public void SetText(string condition, string stackTrace)
        {
            _condition = condition;
            _stackTrace = stackTrace;

            if (_collapsed)
            {
                CollapsedText.text = condition + Environment.NewLine + stackTrace;
            }
        }

        public void SetLog(Log log)
        {
            gameObject.name = "Log " + log.index;
            BackgroundImage.color = (log.index % 2 == 0) ? LogManager.Instance.LogColor : LogManager.Instance.DarkerColor;
            SetText(log.condition, log.stackTrace);
            ItemType = log.type;
            indexInLog = log.index;
        }

        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            LogManager.Instance.ExpandedItem.ExpandItem(this);
        }

        private bool _focused = false;
        private Color _initialColor;
        public virtual void OnFocusEnter()
        {
            if (!_focused)
            {
                _focused = true;
                _initialColor = BackgroundImage.color;
                BackgroundImage.color = LogManager.Instance.HighlightedColor;
            }
        }

        public virtual void OnFocusExit()
        {
            if (_focused)
            {
                _focused = false;
                BackgroundImage.color = _initialColor;
            }
        }

        public void SetTextComponentEnabled(bool enabled)
        {
            CollapsedText.enabled = enabled;
        }
    }
}

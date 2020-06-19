using HoloToolkit.Unity.InputModule;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Valorem.HoloConsole
{
    public class ExpandedLogItem : LogItem
    {
        public Text BodyText;
        private const float expansionTime = 1f;
        public bool IsExpanded { get { return _isExpanded; } }
        private bool _isExpanded;
        private BoxCollider _boxCollider;

        protected void Awake()
        {
            base.Awake();
            _isExpanded = false;
        }

        protected void Start()
        {
            base.Start();
            _boxCollider = GetComponent<BoxCollider>();
            _initialSize = RectTransform.sizeDelta;

        }

        void Update()
        {

        }

        public override void OnInputClicked(InputClickedEventData eventData)
        {
            if (IsExpanded)
            {
                StartCoroutine(Collapse());
            }
        }

        public override void OnFocusEnter() { }
        public override void OnFocusExit() { }

        private Vector2 _collapsedOffsetMin;
        private Vector2 _collapsedOffsetMax;
        private bool _wasTopBttnActive;

        public void ExpandItem(LogItem item)
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            gameObject.SetActive(true);
            SetText(item);
            //AlignWithItem(item);
            ItemType = item.ItemType;

            //_rectTransform.offsetMax = new Vector2(_rectTransform.offsetMax.x, -(LogManager.Instance.RectTransform.sizeDelta.y - 200f) / 2f);
            //_rectTransform.offsetMin = new Vector2(_rectTransform.offsetMin.x, (LogManager.Instance.RectTransform.sizeDelta.y - 200f) / 2f);

            //_collapsedOffsetMax = _rectTransform.offsetMax;
            //_collapsedOffsetMin = _rectTransform.offsetMin;

            _wasTopBttnActive = LogManager.Instance.gameObject.activeSelf;
            if (_wasTopBttnActive) LogManager.Instance.TopBttn.gameObject.SetActive(false);
            StartCoroutine(Expand());
        }

        private void SetText(LogItem item)
        {
            CollapsedText.text = item.Condition;
            BodyText.text = item.StackTrace;
        }

        public void AlignWithItem(LogItem item)
        {

            float offsetMinY = item.RectTransform.anchoredPosition.y - (item.Height / 2);
            float offsetMaxY = (LogManager.Instance.RectTransform.sizeDelta.y - (-LogManager.Instance.ViewPort.offsetMax.y + LogManager.Instance.ViewPort.offsetMin.y)) - (item.RectTransform.anchoredPosition.y + (item.Height / 2));

            _rectTransform.offsetMin = new Vector2(_rectTransform.offsetMin.x, offsetMinY);
            _rectTransform.offsetMax = new Vector2(_rectTransform.offsetMax.x, -offsetMaxY);
        }

        private Vector2 _initialSize;
        public IEnumerator Expand()
        {
            BodyText.enabled = false;
            CollapsedText.enabled = true;

            float _lerpTimer = 0f;
            while (_lerpTimer < .8f)
            {
                _lerpTimer += Time.deltaTime;
                if (_lerpTimer > .2f && !TypeImage.enabled)
                {
                    TypeImage.enabled = true;
                }
                float height = Mathf.Lerp(RectTransform.sizeDelta.y, 0, _lerpTimer / .8f);
                RectTransform.sizeDelta = new Vector2(0, height);
                yield return new WaitForEndOfFrame();
            }

            BodyText.enabled = true;
            //CollapsedText.enabled = true;
            _boxCollider.size = new Vector3(
                RectTransform.rect.width,
                RectTransform.rect.height,
                20f);
            _isExpanded = true;
        }

        public IEnumerator Collapse()
        {
            BodyText.enabled = false;


            float _lerpTimer = 0f;
            while (_lerpTimer < .8f)
            {
                _lerpTimer += Time.deltaTime;
                if (_lerpTimer < .4f && TypeImage.enabled)
                {
                    TypeImage.enabled = false;
                    CollapsedText.enabled = false;
                }
                float height = Mathf.Lerp(RectTransform.sizeDelta.y, _initialSize.y - 90f, _lerpTimer / .8f);
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, height);
                yield return new WaitForEndOfFrame();
            }

            if (_wasTopBttnActive) LogManager.Instance.TopBttn.gameObject.SetActive(true);
            gameObject.SetActive(false);
            _isExpanded = false;
        }
    }
}

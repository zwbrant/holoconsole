using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Valorem.HoloConsole
{
    public class TabController : MonoBehaviour
    {
        [System.Serializable]
        public struct Tab
        {
            public string Label;
            public UnityEvent SelectedClick;
            public UnityEvent DeselectedClick;
            internal TabButton _tabButton;
        }

        [SerializeField]
        private Tab[] Tabs;
        public int InitiallySelectedTabIndex = 0;
        [HideInInspector]
        public Color SelectedColor;
        [HideInInspector]
        public Color SelectedLabelColor;
        public Color DeselectedColor;
        public Color DeselectedLabelColor;
        public Color HighlightedColor = Color.yellow;
        public RectTransform RectTrans
        {
            get
            {
                if (_rectTrans == null) { _rectTrans = GetComponent<RectTransform>(); }
                return _rectTrans;
            }
            private set
            {
                _rectTrans = value;
            }
        }
        private RectTransform _rectTrans;

        private GameObject _templateTab;


        // Use this for initialization
        void Start()
        {
            if (transform.GetChild(0) == null)
            {
                Debug.LogError("The tab container needs a child to use as a template for tabs. Aborting tab creation.");
                return;
            }
            else if (Tabs.Length > 0)
            {
                // setup the pre-made tab and get it's attributes to use globally
                _templateTab = transform.GetChild(0).gameObject;
                SelectedColor = _templateTab.GetComponent<Image>().color;
                SelectedLabelColor = _templateTab.GetComponentInChildren<Text>().color;
                SetupTab(_templateTab, ref Tabs[0]);

                for (int i = 1; i < Tabs.Length; i++)
                {
                    GameObject newTab = Instantiate(_templateTab, this.transform);
                    RectTransform rectTrans = _templateTab.GetComponent<RectTransform>();
                    newTab.GetComponent<RectTransform>().localPosition = new Vector3(
                        rectTrans.localPosition.x + rectTrans.rect.width * i,
                        rectTrans.localPosition.y,
                        rectTrans.localPosition.z);
                    SetupTab(newTab, ref Tabs[i]);
                }

                SetSelectedTab(Tabs[InitiallySelectedTabIndex]._tabButton);
            }
        }

        public void SetSelectedTab(TabButton selectedTab)
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                Tabs[i]._tabButton.Selected = (Tabs[i]._tabButton == selectedTab);
            }
        }

        public void SetSelectedTab(int selectedTab)
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                Tabs[i]._tabButton.Selected = (i == selectedTab);
            }
        }

        void SetupTab(GameObject tabGameObject, ref Tab tab)
        {
            tab._tabButton = tabGameObject.GetComponent<TabButton>();
            tab._tabButton.Initialize(tab, this);
        }

        public void ShowTabs()
        {
            StartCoroutine(LerpTabsVisible());
        }
        public IEnumerator LerpTabsVisible()
        {
            float _lerpTimer = 0f;

            while (_lerpTimer < 1f)
            {
                _lerpTimer += Time.deltaTime;

                float newY = Mathf.Lerp(RectTrans.anchoredPosition.y, -0.2f, _lerpTimer / 1f);
                RectTrans.anchoredPosition = new Vector2(RectTrans.anchoredPosition.x, newY);

                yield return new WaitForEndOfFrame();
            }
        }

        public void HideTabs()
        {
            StartCoroutine(LerpTabsHidden());
        }
        public IEnumerator LerpTabsHidden()
        {
            float _lerpTimer = 0f;

            while (_lerpTimer < 1f)
            {
                _lerpTimer += Time.deltaTime;

                float newY = Mathf.Lerp(RectTrans.anchoredPosition.y, -50f, _lerpTimer / 1f);
                RectTrans.anchoredPosition = new Vector2(RectTrans.anchoredPosition.x, newY);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}

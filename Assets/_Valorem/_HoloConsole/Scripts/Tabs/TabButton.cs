using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.UI;

namespace Valorem.HoloConsole
{
    [RequireComponent(typeof(Text), typeof(Image))]
    public class TabButton : MonoBehaviour, IInputClickHandler, IFocusable
    {
        public UnityEvent SelectedClick;
        public UnityEvent DeselectedClick;

        private TabController _tabController;
        private Image _backgroundImg;
        private Text _label;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                _backgroundImg.color = (_selected) ? _tabController.SelectedColor : _tabController.DeselectedColor;
                _label.color = (_selected) ? _tabController.SelectedLabelColor : _tabController.DeselectedLabelColor;
                if (_selected && SelectedClick != null)
                {
                    SelectedClick.Invoke();
                }
                else if (DeselectedClick != null)
                {
                    DeselectedClick.Invoke();
                }
            }
        }
        private bool _selected = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (!Selected)
            {
                _tabController.SetSelectedTab(this);
                OnFocusExit();
            }
        }

        public void Initialize(TabController.Tab tab, TabController tabController)
        {
            _backgroundImg = GetComponent<Image>();
            _label = GetComponentInChildren<Text>();

            _tabController = tabController;
            _label.text = tab.Label;
            SelectedClick = tab.SelectedClick;
            DeselectedClick = tab.DeselectedClick;

            Selected = false;
        }

        private bool _focused = false;
        public virtual void OnFocusEnter()
        {
            if (!_focused && !Selected)
            {
                _focused = true;
                _backgroundImg.color = _tabController.HighlightedColor;
            }
        }

        public virtual void OnFocusExit()
        {
            if (_focused)
            {
                _focused = false;
                _backgroundImg.color = (Selected) ? _tabController.SelectedColor : _tabController.DeselectedColor;
            }
        }
    }
}

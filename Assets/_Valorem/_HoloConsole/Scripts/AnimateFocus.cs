using System.Collections;
using UnityEngine;

namespace Valorem.HoloConsole
{
    public class AnimateFocus : MonoBehaviour, IFocusable
    {

        public float AnimateTime = .5f;
        public float AnimateScaleMulti = 1.5f;
        private Vector3 _initialScale;
        private float _lerpTimer = 0f;

        private bool _isShrinking = false;
        private bool _isGrowing = false;

        private bool _focused = false;

        public void OnFocusEnter()
        {
            //print("Focused");
            _focused = true;
            if (!_isShrinking)
            {
                _isGrowing = true;
                StartCoroutine(ScaleUp());
            }
        }

        public void OnFocusExit()
        {
            //print("Un-Focused");

            _focused = false;
            if (!_isGrowing)
            {
                _isShrinking = true;
                StartCoroutine(ScaleDown());
            }
        }

        private IEnumerator ScaleUp()
        {
            _isShrinking = false;
            while (transform.localScale != _initialScale * AnimateScaleMulti)
            {
                IterateLerpTime();
                transform.localScale = Vector3.Lerp(transform.localScale, _initialScale * AnimateScaleMulti, _lerpTimer / AnimateTime);
                yield return null;
            }
            _isShrinking = false;

            _isGrowing = false;
            _lerpTimer = AnimateTime;
            if (!_focused)
                StartCoroutine(ScaleDown());
        }

        private IEnumerator ScaleDown()
        {
            _isGrowing = false;

            while (transform.localScale != _initialScale)
            {
                IterateLerpTimeBackwards();
                transform.localScale = Vector3.Lerp(transform.localScale, _initialScale, (AnimateTime - _lerpTimer) / AnimateTime);
                yield return null;
            }
            _isShrinking = false;
            _isGrowing = false;

            _lerpTimer = 0f;

            if (_focused)
                StartCoroutine(ScaleUp());
        }

        private void IterateLerpTime()
        {
            _lerpTimer += Time.deltaTime;
            if (_lerpTimer > AnimateTime) _lerpTimer = AnimateTime;
        }

        private void IterateLerpTimeBackwards()
        {
            _lerpTimer -= Time.deltaTime;
            if (_lerpTimer < 0f) _lerpTimer = 0f;
        }

        // Use this for initialization
        void Start()
        {
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_focused)
            {
                OnFocusExit();
            }
        }
    }
}

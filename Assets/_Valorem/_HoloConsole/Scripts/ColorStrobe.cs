using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valorem.HoloConsole
{
    public class ColorStrobe : MonoBehaviour
    {
        public Color StrobeColor;
        public float StrobeTime = .5f;

        private Image _image;
        private Color _initialColor;
        [HideInInspector]
        public bool IsStrobing = false;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _initialColor = _image.color;
        }

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartStrobe()
        {
            IsStrobing = true;
            StartCoroutine(Strobe());
        }

        public void StopStrobe()
        {
            IsStrobing = false;
            StopCoroutine(Strobe());
            _image.color = _initialColor;
        }

        bool _flipStrobe = false;
        private IEnumerator Strobe()
        {
            float timer = 0f;
            while (2 == 2)
            {
                timer += Time.deltaTime;
                if (!_flipStrobe)
                    _image.color = Color.Lerp(_initialColor, StrobeColor, timer / StrobeTime);
                else
                    _image.color = Color.Lerp(StrobeColor, _initialColor, timer / StrobeTime);

                if (timer > StrobeTime)
                {
                    timer = 0f;
                    _flipStrobe = !_flipStrobe;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}

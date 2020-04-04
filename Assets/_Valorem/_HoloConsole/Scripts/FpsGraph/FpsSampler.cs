using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valorem.Utilities
{
    public class FpsSampler : MonoBehaviour
    {
        [Header("Sample Parameters")]
        [Tooltip("Time between each sample")]
        public float SamplingIntervalSeconds = 1;
        [Tooltip("Number of samples to record")]
        public int SampleRecordSize = 10;

        public float CurrentFpsSample { get; private set; }
        public float AvgFps { get; private set; }


        public LinkedList<float> Samples
        {
            get; protected set;
        }

        // Sum of frame times since last interval
        private float _sampleFrameTimeSum;
        // Count of frames since last interval
        private int _sampleFrameCount;
        // Timer that resets after every interval
        // private float _sampleIntervalTimer;
        // Sum and count of ALL frames & times, not just in the last sample (for avg)
        private int _frameCount = 0;
        private float _frameTimeSum;

        // Use this for initialization
        void Start()
        {
            Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            SampleFps();
        }

        void SampleFps()
        {
            _sampleFrameCount++;
            _frameCount++;
            _frameTimeSum += Time.unscaledDeltaTime;
            _sampleFrameTimeSum += Time.unscaledDeltaTime;
            //_sampleIntervalTimer += Time.unscaledDeltaTime;

            // Check if interval is finished
            if (_sampleFrameTimeSum >= SamplingIntervalSeconds)
            {
                if (Samples.Count >= SampleRecordSize)
                    Samples.RemoveFirst();

                CurrentFpsSample = 1f / (_sampleFrameTimeSum / _sampleFrameCount);
                AvgFps = 1f / (_frameTimeSum / _frameCount);
                Samples.AddLast(CurrentFpsSample);

                //_sampleIntervalTimer = 0f;
                _sampleFrameTimeSum = 0f;
                _sampleFrameCount = 0;
            }
        }

        private void Initialize()
        {
            Samples = new LinkedList<float>();
            _sampleFrameTimeSum = 0f;
            //_sampleIntervalTimer = 0f;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valorem.Utilities
{

    [RequireComponent(typeof(RectTransform))]
    public class FpsGraphRenderer : MonoBehaviour
    {
        [Header("Graph Parameters")]
        public int MaxYValue = 100;
        public Color LineColor = Color.yellow;
        public Color AxisColor = Color.blue;
        public Color AxisIncrementColor = Color.white;

        [Header("References")]
        public FpsSampler FpsSampler;
        public Text MaxYText;
        public Text CurrentFpsText;
        private RectTransform _currentFpsRectTrans;
        public Text AvgFpsText;
        private RectTransform _avgFpsRectTrans;

        [SerializeField]
        private Material _lineMaterial;
        private RectTransform _rectTrans;
        // Corners of graph
        private Vector3 topLeft;
        private Vector3 topRight;
        private Vector3 bottomLeft;
        private Vector3 bottomRight;

        private float xIntervalSpace;
        private float yIntervalSpace;
        private float graphHeight;
        private float graphWidth;

        // Use this for initialization
        void Start()
        {
            _rectTrans = GetComponent<RectTransform>();
            topLeft = _rectTrans.TransformPoint(Vector3.zero);
            xIntervalSpace = 0f;
            yIntervalSpace = 0f;
            graphHeight = 0f;
            graphWidth = 0f;

            if (MaxYText != null) { MaxYText.text = MaxYValue.ToString(); }
            if (CurrentFpsText != null) { _currentFpsRectTrans = CurrentFpsText.GetComponent<RectTransform>(); }
            if (AvgFpsText != null) { _avgFpsRectTrans = CurrentFpsText.GetComponent<RectTransform>(); }
        }

        // Update is called once per frame
        void Update()
        {
            graphHeight = _rectTrans.rect.height;
            graphWidth = _rectTrans.rect.width;
            xIntervalSpace = graphWidth / (FpsSampler.SampleRecordSize - 1);
            yIntervalSpace = graphHeight / (5);
        }

        //static void CreateLineMaterial()
        //{
        //    if (!_lineMaterial)
        //    {
        //        // Unity has a built-in shader that is useful for drawing
        //        // simple colored things.
        //        Shader shader = Shader.Find("Hidden/Internal-Colored");
        //        _lineMaterial = new Material(shader)
        //        {
        //            hideFlags = HideFlags.HideAndDontSave
        //        };
        //        // Turn on alpha blending
        //        _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //        _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //        //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        //        //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //        // Turn backface culling off
        //        _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        //        // Turn off depth writes
        //        _lineMaterial.SetInt("_ZWrite", 0);
        //    }
        //}

        private void OnRenderObject()
        {
            topLeft = _rectTrans.TransformPoint(0, graphHeight, 0);
            topRight = _rectTrans.TransformPoint(graphWidth, graphHeight, 0);
            bottomLeft = _rectTrans.TransformPoint(Vector3.zero);
            bottomRight = _rectTrans.TransformPoint(graphWidth, 0, 0);

            _lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(AxisColor);
            DrawAxis();
            GL.Color(AxisIncrementColor);
            DrawAxisIncrements();
            GL.Color(LineColor);
            DrawFpsData();

            GL.End();
            GL.PopMatrix();

            UpdateTextReadouts();

        }

        private void UpdateTextReadouts()
        {
            float yPos = Mathf.Lerp(_currentFpsRectTrans.anchoredPosition.y, TransformFpsToYValue(FpsSampler.CurrentFpsSample), .02f);
            _currentFpsRectTrans.anchoredPosition = new Vector2(0f, yPos);
            CurrentFpsText.text = ((int)FpsSampler.CurrentFpsSample).ToString();

            AvgFpsText.text = "Avg FPS: " + (int)FpsSampler.AvgFps;
        }

        private void DrawFpsData()
        {
            if (FpsSampler != null && FpsSampler.Samples.Count > 1)
            {
                var currSample = FpsSampler.Samples.First;
                // Index of the current X axis interval (from 0 to the FpsSampler.SampleRecordSize)
                int xIndex = FpsSampler.SampleRecordSize - FpsSampler.Samples.Count;
                while (currSample.Next != null)
                {
                    GL.Vertex(transform.TransformPoint(xIndex * xIntervalSpace, TransformFpsToYValue(currSample.Value), 0));
                    GL.Vertex(transform.TransformPoint((xIndex + 1) * xIntervalSpace, TransformFpsToYValue(currSample.Next.Value), 0));
                    xIndex++;
                    currSample = currSample.Next;
                }
            }
        }

        // The local Y coordinate (height) of a given FPS. This will be negative because we assume origin to be top left of graph. 
        private float TransformFpsToYValue(float fps)
        {
            return Mathf.Clamp((fps / MaxYValue) * graphHeight, 0f, graphHeight);
        }

        private void DrawAxis()
        {
            GL.Vertex(topLeft);
            GL.Vertex(bottomLeft);
            GL.Vertex(bottomLeft);
            GL.Vertex(bottomRight);
        }

        private void DrawAxisIncrements()
        {
            float bigIncrementLength = 25f;
            float littleIncrementLength = 20f;

            // Big increments (at start and ends of axis)
            GL.Vertex(bottomLeft);
            GL.Vertex(transform.TransformPoint(0, -bigIncrementLength, 0));

            GL.Vertex(bottomLeft);
            GL.Vertex(transform.TransformPoint(-bigIncrementLength, 0, 0));

            GL.Vertex(topLeft);
            GL.Vertex(transform.TransformPoint(-bigIncrementLength, graphHeight, 0));

            //GL.Vertex(bottomRight);
            //GL.Vertex(transform.TransformPoint(graphWidth, -bigIncrementLength, 0));

            // Little increments 
            for (int i = 1; i < FpsSampler.SampleRecordSize - 1; i++)
            {
                GL.Vertex(transform.TransformPoint(i * xIntervalSpace, 0, 0));
                GL.Vertex(transform.TransformPoint(i * xIntervalSpace, -littleIncrementLength, 0));
            }

            for (int i = 1; i <= 4; i++)
            {
                GL.Vertex(transform.TransformPoint(0, yIntervalSpace * i, 0));
                GL.Vertex(transform.TransformPoint(-littleIncrementLength, yIntervalSpace * i, 0));
            }
        }

    }
}

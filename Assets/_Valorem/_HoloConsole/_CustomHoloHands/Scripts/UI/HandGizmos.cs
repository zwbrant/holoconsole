using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valorem.HoloConsole.CustomHoloHands;

namespace Valorem.HoloHands
{
    public class HandGizmos : MonoBehaviour
    {
        static Vector3[] _highlightVertices;
        static Vector3[] _gazeRingVertices;
        static Vector3[] _holdDotsVertices;
        static Vector3[] _dotVerticies; // small dots to be rendered over and over
        static AnimationCurve _easeInCurve;
        static AnimationCurve _easeOutCuve;
        static AnimationCurve _easeInAndOutCurve;
        static AnimationCurve _bumpCurve;
        static Keyframe[] _keys;

        public static void Initialize()
        {
            SetUpAnimationCurves();
            SetUpHighlightVerticies();
            SetUpGazeRing();
            
        }
        private static void SetUpAnimationCurves()
        {
            _easeInCurve = new AnimationCurve();
            _easeOutCuve = new AnimationCurve();
            _easeInAndOutCurve = new AnimationCurve();
            _bumpCurve = new AnimationCurve();
            
            _keys = new Keyframe[2];
            _keys[0] = new Keyframe(0, 0, 0f, 0f);
            _keys[1] = new Keyframe(1, 1, 1.5f, 0f);
            _easeInCurve.keys = _keys;
            _keys[0] = new Keyframe(0, 0, 0f, 1.5f);
            _keys[1] = new Keyframe(1, 1, 0f, 0f);
            _easeOutCuve.keys = _keys;
            _keys[0] = new Keyframe(0, 0, 0f, 0f);
            _keys[1] = new Keyframe(1, 1, 0f, 0f);
            _easeInAndOutCurve.keys = _keys;
            _keys[0] = new Keyframe(0, 0, 0f, 1.5f);
            _keys[1] = new Keyframe(1, 1, -1.5f, 0f);
            _bumpCurve.keys = _keys;
        }
        private static void SetUpGazeRing()
        {
            int numOfVs = 45;
            _gazeRingVertices = new Vector3[numOfVs + 1];
            float angle = 360f / (numOfVs);
            for (int v = 0; v < numOfVs + 1; v++)
            {
                angle *= v;
                float radian = Mathf.Deg2Rad*angle;
                float x = Mathf.Sin(radian);
                float y = Mathf.Cos(radian);
                float z = 0;
                
                _gazeRingVertices[v] = new Vector3(x, y, z);
            }
        }
        private static void SetUpDotVerticies()
        {
            Vector3 rightDir = Camera.main.transform.right;
            Vector3 upDir = Camera.main.transform.up;
            _dotVerticies = new Vector3[8];
            float angle = 360f / 8f;
            for (int v = 0; v < 8; v++)
            {
                angle *= v;
                float radian = Mathf.Deg2Rad * angle;
                float x = Mathf.Sin(radian);
                float y = Mathf.Cos(radian);
                //float z = 0;

                _dotVerticies[v] = x * rightDir + y * upDir;
            }
        }
        public static void StartDots()
        {
            SetUpDotVerticies();
            GL.Begin(GL.QUADS);
        }
        public static void DrawDot(Vector3 dotPosiiton, float size)
        {
            int vNum = 0;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 1;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 2;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 3;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);

            vNum = 0;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 3;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 4;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 7;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);

            vNum = 7;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 4;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 5;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
            vNum = 6;
            GL.Vertex3(_dotVerticies[vNum].x * size + dotPosiiton.x, _dotVerticies[vNum].y * size + dotPosiiton.y, _dotVerticies[vNum].z * size + dotPosiiton.z);
        }
        public static void EndDots()
        {
            GL.End();
        }
        public static void DrawWireBox(Vector3 position, Quaternion rotation, float width, float height, float length, Color color)
        {
            Vector3 rightDir = rotation * Vector3.right * width * .5f;
            Vector3 upDir = rotation * Vector3.up * height * .5f;
            Vector3 forwardDir = rotation * Vector3.forward * length * .5f;
            Vector3[] verts = new Vector3[8];
            verts[0] = -rightDir + upDir + forwardDir + position;
            verts[1] = rightDir + upDir + forwardDir + position;
            verts[2] = rightDir + upDir - forwardDir + position;
            verts[3] = - rightDir + upDir - forwardDir + position;
            verts[4] = - rightDir - upDir + forwardDir + position;
            verts[5] = rightDir - upDir + forwardDir + position;
            verts[6] = rightDir - upDir - forwardDir + position;
            verts[7] = -rightDir - upDir - forwardDir + position;
            GL.Begin(GL.LINES);
            GL.Color(color);
            //top
            GL.Vertex(verts[0]);
            GL.Vertex(verts[1]);
            GL.Vertex(verts[1]);
            GL.Vertex(verts[2]);
            GL.Vertex(verts[2]);
            GL.Vertex(verts[3]);
            GL.Vertex(verts[3]);
            GL.Vertex(verts[0]);
            //bottom
            GL.Vertex(verts[4]);
            GL.Vertex(verts[5]);
            GL.Vertex(verts[5]);
            GL.Vertex(verts[6]);
            GL.Vertex(verts[6]);
            GL.Vertex(verts[7]);
            GL.Vertex(verts[7]);
            GL.Vertex(verts[4]);
            //top to bottom edges
            GL.Vertex(verts[0]);
            GL.Vertex(verts[4]);
            GL.Vertex(verts[1]);
            GL.Vertex(verts[5]);
            GL.Vertex(verts[2]);
            GL.Vertex(verts[6]);
            GL.Vertex(verts[3]);
            GL.Vertex(verts[7]);

            GL.End();
        }
        public static void DrawArcLine(Vector3 archPosition, Quaternion faceDirection, float radius, float centerAngle, float archWidth, Color arcColor)
        {
            float currentAngle = centerAngle - archWidth / 2f;
            float angleLeft = archWidth;
            Vector3 upDir = faceDirection * Vector3.up * radius;
            Vector3 rightDir = faceDirection * Vector3.right * radius;
            GL.Begin(GL.LINE_STRIP);
            GL.Color(arcColor);
            float radians = currentAngle * Mathf.Deg2Rad;
            GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir + Mathf.Cos(radians) * upDir);
            while (angleLeft > 0)
            {

                if (angleLeft >= 5)
                {
                    angleLeft -= 5f;
                    currentAngle += 5f;
                }
                else
                {
                    currentAngle += angleLeft;
                    angleLeft = 0;
                }
                radians = currentAngle * Mathf.Deg2Rad;
                GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir + Mathf.Cos(radians) * upDir);
            }
            GL.End();
        }
        public static void DrawArcLine(Vector3 archPosition, Quaternion faceDirection, float radius, float centerAngle, float archWidth, Color arcColor, float degreesPerLine)
        {

            float currentAngle = centerAngle - archWidth / 2f;
            float angleLeft = archWidth;
            Vector3 upDir = faceDirection * Vector3.up * radius;
            Vector3 rightDir = faceDirection * Vector3.right * radius;
            GL.Begin(GL.LINE_STRIP);
            GL.Color(arcColor);
            float radians = currentAngle * Mathf.Deg2Rad;
            GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir + Mathf.Cos(radians) * upDir);
            while (angleLeft > 0f)
            {

                if (angleLeft >= degreesPerLine)
                {
                    angleLeft -= degreesPerLine;
                    currentAngle += degreesPerLine;
                }
                else
                {
                    currentAngle += angleLeft;
                    angleLeft = 0f;
                }
                radians = currentAngle * Mathf.Deg2Rad;
                GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir + Mathf.Cos(radians) * upDir);
            }
            GL.End();
        }
        public static void DrawArcQuads(Vector3 archPosition, Quaternion faceDirection, float innerRadius, float outerRadius, float centerAngle, float archWidth, Color arcColor)
        {
            float currentAngle = centerAngle - archWidth / 2f;
            float angleLeft = archWidth;
            Vector3 upDir = faceDirection * Vector3.up;
            Vector3 rightDir = faceDirection * Vector3.right;
            GL.Begin(GL.QUADS);
            GL.Color(arcColor);
            float radians = currentAngle * Mathf.Deg2Rad;
            //GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir + Mathf.Cos(radians) * upDir);
            while (angleLeft > 0)
            {
                GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir * innerRadius + Mathf.Cos(radians) * upDir * innerRadius);
                GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir * outerRadius + Mathf.Cos(radians) * upDir * outerRadius);

                if (angleLeft >= 5)
                {
                    angleLeft -= 5f;
                    currentAngle += 5f;
                }
                else
                {
                    currentAngle += angleLeft;
                    angleLeft = 0;
                }
                radians = currentAngle * Mathf.Deg2Rad;
                
                GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir * outerRadius + Mathf.Cos(radians) * upDir * outerRadius);
                GL.Vertex(archPosition + Mathf.Sin(radians) * rightDir * innerRadius + Mathf.Cos(radians) * upDir * innerRadius);
            }
            GL.End();
        }
        private static void SetUpHighlightVerticies()
        {
            _highlightVertices = new Vector3[8 * 6];
            for (int v = 0; v < _highlightVertices.Length; v++)
            {
                _highlightVertices[v] = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f));
            }
            
        }
        public static void DrawHighlight()
        {
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            for (int v = 0; v < _highlightVertices.Length; v++)
            {
                GL.Vertex(_highlightVertices[v]);
            }
            GL.End();
        }
        public static void DrawHighlight(Vector3 position, Quaternion rotation, float radius, HoloHoldController.GizmoStats gizmoStats)
        {

            //Quaternion ROT = Quaternion.FromToRotation(Vector3.zero, Vector3.forward);
            Quaternion rot = rotation;

            Vector3 forwardDir = rot * Vector3.forward;

            Color arcColor = gizmoStats.MainColor * _easeOutCuve.Evaluate( gizmoStats.GazeFadeIn*2f) *.5f;
            Color dimColor = gizmoStats.MainColor * _bumpCurve.Evaluate(gizmoStats.GazeFadeIn * 2.5f) * .25f;

            float size = _bumpCurve.Evaluate(gizmoStats.GazeFadeIn*3f);
            float width = _easeOutCuve.Evaluate(gizmoStats.GazeFadeIn*3f-1f);
            float spin = _easeInAndOutCurve.Evaluate(gizmoStats.GazeFadeIn * 3f - 2f);
            //Vector3 forwardDir = rotation * Vector3.forward * radius;

            Vector3 quadArcPosition = position + forwardDir * width * radius * .05f;

            DrawArcQuads(quadArcPosition, rot, (size * .5f + .5f) * radius, (size * .525f + .5f) * radius, 45f + 30f * spin, 30f * (.25f + .75f * width), arcColor);
            DrawArcQuads(quadArcPosition, rot, (size * .5f + .5f) * radius, (size * .525f + .5f) * radius, 135f - 30f * spin, 30f * (.5f + .5f * width), arcColor);

            DrawArcQuads(quadArcPosition, rot, (size * .5f + .5f) * radius, (size * .525f + .5f) * radius, 315f - 30f * spin, 30f * (.25f + .75f * width), arcColor);
            DrawArcQuads(quadArcPosition, rot, (size * .5f + .5f) * radius, (size * .525f + .5f) * radius, 225f + 30f * spin, 30f * (.5f + .5f * width), arcColor);

            size = _bumpCurve.Evaluate(gizmoStats.GazeFadeIn * 2.5f);
            DrawArcLine(position - forwardDir * width * radius * .05f, rot, radius * 1.1f * (size * .25f + .75f), 0f, 360f, dimColor);

            if (gizmoStats.HandReadyFi1 > 0)
            {
                size = _bumpCurve.Evaluate(gizmoStats.HandReadyFi1);
                width = _easeOutCuve.Evaluate(gizmoStats.HandReadyFi1);
                spin = _easeInAndOutCurve.Evaluate(gizmoStats.HandHeldFi1);
                DrawArcQuads(quadArcPosition, rot, (size * .5f + .65f) * radius, (size * .525f + .75f) * radius, 45f + 40f * spin, 2f * (.1f + .9f * width), arcColor);
                DrawArcQuads(quadArcPosition, rot, (size * .5f + .65f) * radius, (size * .525f + .75f) * radius, 135f - 40f * spin, 2f * (.1f + .9f * width), arcColor);
                DrawArcQuads(quadArcPosition, rot, (size * .5f + .65f) * radius, (size * .525f + .75f) * radius, 225f + 40f * spin, 2f * (.1f + .9f * width), arcColor);
                DrawArcQuads(quadArcPosition, rot, (size * .5f + .65f) * radius, (size * .525f + .75f) * radius, 315f - 40f * spin, 2f * (.1f + .9f * width), arcColor);

                if (gizmoStats.HandREadyFi2 > 0)
                {
                    size = _bumpCurve.Evaluate(gizmoStats.HandREadyFi2);
                    width = _easeOutCuve.Evaluate(gizmoStats.HandREadyFi2);
                    spin = _easeInAndOutCurve.Evaluate(gizmoStats.HandHeldFi2);
                    DrawArcQuads(quadArcPosition, rot, (size * .5f + .825f) * radius, (size * .525f + .835f) * radius, 45f + 40f * spin, 2f * (.1f + .9f * width), arcColor);
                    DrawArcQuads(quadArcPosition, rot, (size * .5f + .825f) * radius, (size * .525f + .835f) * radius, 135f - 40f * spin, 2f * (.1f + .9f * width), arcColor);
                    DrawArcQuads(quadArcPosition, rot, (size * .5f + .825f) * radius, (size * .525f + .835f) * radius, 225f + 40f * spin, 2f * (.1f + .9f * width), arcColor);
                    DrawArcQuads(quadArcPosition, rot, (size * .5f + .825f) * radius, (size * .525f + .835f) * radius, 315f - 40f * spin, 2f * (.1f + .9f * width), arcColor);
                }
            }
        }
        public static void DrawGazeRing(Vector3 position, Quaternion rotation, float radius, HoloHoldController.GizmoStats gizmoStats)
        {
            GL.Begin(GL.LINES);
            GL.Color(new Color(
                gizmoStats.GazeFadeIn * gizmoStats.MainColor.r * .333f * (1f - gizmoStats.HoldFadeIn * .5f),
                gizmoStats.GazeFadeIn * gizmoStats.MainColor.g * .333f * (1f - gizmoStats.HoldFadeIn * .5f),
                gizmoStats.GazeFadeIn * gizmoStats.MainColor.b * .333f * (1f - gizmoStats.HoldFadeIn * .5f)));
            float size = 1;// .1f + Mathf.Pow(1.0f - gizmoStats.GazeFadeIn, 2) - gizmoStats.HoldFadeIn / 10f;
            for (int v = 0; v < _gazeRingVertices.Length - 1; v++)
            {
                //if (GazeRingVerticies[v].y < .1f && GazeRingVerticies[v].y > -.1f)
                //{
                    GL.Vertex(position + rotation * _gazeRingVertices[v] * radius * size);
                    GL.Vertex(position + rotation * _gazeRingVertices[v + 1] * radius * size);
                //}

            }
            GL.End();
            GL.Begin(GL.QUADS);
            GL.Color(new Color(
    gizmoStats.GazeFadeIn * gizmoStats.MainColor.r * .333f * (1f - gizmoStats.HoldFadeIn * .5f),
    gizmoStats.GazeFadeIn * gizmoStats.MainColor.g * .333f * (1f - gizmoStats.HoldFadeIn * .5f),
    gizmoStats.GazeFadeIn * gizmoStats.MainColor.b * .333f * (1f - gizmoStats.HoldFadeIn * .5f)));
            SetUpDotVerticies();
            GL.End();
        }
        public static void DrawHoldDots(Vector3 position, Quaternion rotation, float radius, HoloHoldController.GizmoStats gizmoStats)
        {
            if (gizmoStats.HhState != HoloHoldController.HoloHoldState.Gazed)
            {
                GL.Begin(GL.LINES);
                GL.Color(new Color(
                    gizmoStats.MainColor.r * .5f + gizmoStats.HoldFadeIn * .5f,
                    gizmoStats.MainColor.g * .5f + gizmoStats.HoldFadeIn * .5f,
                    gizmoStats.MainColor.b * .5f + gizmoStats.HoldFadeIn * .5f));

                for (float d = .5f; d < gizmoStats.HighlightFadeIn * 7; d++)
                {
                    float angle = d * (5.0f + 2.5f * gizmoStats.HoldFadeIn) + (1.0f - gizmoStats.HoldFadeIn) * 25f;
                    float radian = Mathf.Deg2Rad * angle;
                    Vector3 dotPos1A = new Vector3(Mathf.Cos(radian) * (.9f - gizmoStats.HoldFadeIn / 20f), Mathf.Sin(radian) * (.9f - gizmoStats.HoldFadeIn / 20f), .025f);
                    Vector3 dotPos1B = new Vector3(dotPos1A.x, dotPos1A.y, -dotPos1A.z);

                    Vector3 dotPos2A = new Vector3(-dotPos1A.x, dotPos1A.y, dotPos1A.z);
                    Vector3 dotPos2B = new Vector3(-dotPos1A.x, dotPos1A.y, -dotPos1A.z);

                    Vector3 dotPos3A = new Vector3(-dotPos1A.x, -dotPos1A.y, dotPos1A.z);
                    Vector3 dotPos3B = new Vector3(-dotPos1A.x, -dotPos1A.y, -dotPos1A.z);

                    Vector3 dotPos4A = new Vector3(dotPos1A.x, -dotPos1A.y, dotPos1A.z);
                    Vector3 dotPos4B = new Vector3(dotPos1A.x, -dotPos1A.y, -dotPos1A.z);

                    GL.Vertex(position + rotation * dotPos1A * radius);
                    GL.Vertex(position + rotation * dotPos1B * radius);
                    GL.Vertex(position + rotation * dotPos2A * radius);
                    GL.Vertex(position + rotation * dotPos2B * radius);
                    GL.Vertex(position + rotation * dotPos3A * radius);
                    GL.Vertex(position + rotation * dotPos3B * radius);
                    GL.Vertex(position + rotation * dotPos4A * radius);
                    GL.Vertex(position + rotation * dotPos4B * radius);
                }
                GL.End();
            }
        }
        public static void DrawCursor(Vector3 headPosition, Quaternion headRotation, float cursorDistance, bool hittingHoloHold, bool handReady, bool twoHandReady, Color cursorColor)
        {
            Color brightColor = new Color(
                cursorColor.r * .95f,
                cursorColor.g * .95f, 
                cursorColor.b * .95f);
            Color dimlightColor = new Color(
                cursorColor.r * .6f,
                cursorColor.g * .6f,
                cursorColor.b * .6f);

            // Draw lines
            GL.Begin(GL.LINES);
            GL.Color(dimlightColor);

            Vector3 forward = headRotation * Vector3.forward;
            Vector3 up = headRotation * Vector3.up;
            Vector3 right = headRotation * Vector3.right;

            float cursorSize = .005f * cursorDistance;

            if (!handReady)
            {

                GL.Color(brightColor);

                for (float a = 0; a < 360; a += 15)
                {
                    GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * .5f * Mathf.Sin(a * Mathf.Deg2Rad) + up * cursorSize * .5f * Mathf.Cos(a * Mathf.Deg2Rad));
                    GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * .5f * Mathf.Sin((a + 15) * Mathf.Deg2Rad) + up * cursorSize * .5f * Mathf.Cos((a + 15) * Mathf.Deg2Rad));
                }
            }
            else 
            {

                GL.Color(dimlightColor);

                GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * .7f + up * cursorSize * .7f);
                GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * .7f + up * cursorSize * .7f);
                GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * .7f - up * cursorSize * .7f);
                GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * .7f - up * cursorSize * .7f);

                GL.Color(brightColor);

                for (float a = 45; a < 135; a += 15)
                {
                    GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * Mathf.Sin(a * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos(a * Mathf.Deg2Rad));
                    GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * Mathf.Sin((a + 15) * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos((a + 15) * Mathf.Deg2Rad));

                    GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * Mathf.Sin(a * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos(a * Mathf.Deg2Rad));
                    GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * Mathf.Sin((a + 15) * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos((a + 15) * Mathf.Deg2Rad));
                }
                if (twoHandReady)
                {
                    GL.Color(dimlightColor);

                    for (float a = 45; a < 135; a += 15)
                    {
                        GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * Mathf.Sin(a * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos(a * Mathf.Deg2Rad) + right * cursorSize * .25f);
                        GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * Mathf.Sin((a + 15) * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos((a + 15) * Mathf.Deg2Rad) + right * cursorSize * .25f);

                        GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * Mathf.Sin(a * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos(a * Mathf.Deg2Rad) - right * cursorSize * .25f);
                        GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * Mathf.Sin((a + 15) * Mathf.Deg2Rad) + up * cursorSize * Mathf.Cos((a + 15) * Mathf.Deg2Rad) - right * cursorSize * .25f);
                    }
                }
            }




            if (hittingHoloHold)
            {

                GL.Color(brightColor);
                GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * .1f);
                GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * .1f);

                GL.Vertex(headPosition + forward * cursorDistance - up * cursorSize * .1f);
                GL.Vertex(headPosition + forward * cursorDistance + up * cursorSize * .1f);

                GL.End();
                GL.Begin(GL.QUADS);
                GL.Color(brightColor);
                GL.Vertex(headPosition + forward * cursorDistance + right * cursorSize * .1f);
                GL.Vertex(headPosition + forward * cursorDistance - up * cursorSize * .1f);
                GL.Vertex(headPosition + forward * cursorDistance - right * cursorSize * .1f);
                GL.Vertex(headPosition + forward * cursorDistance + up * cursorSize * .1f);
            }

            GL.End();
        }
        public static void DrawRemoteCursor(Vector3 headPosition, Vector3 cursorPosition, Color cursorColor)
        {
            GL.Begin(GL.LINES);
            GL.Color(cursorColor);
            GL.Vertex(cursorPosition);
            GL.Color(Color.black);
            GL.Vertex((headPosition + cursorPosition)/2f);
            GL.End();
        }
        public static void DrawTranslate(HoloHold holoHold, Vector3 headPosition, Quaternion headRotation, float scale, HoloHoldController.GizmoStats gizmoStats)
        {
            Transform holdTransform = holoHold.HoldTransform;
            float quadSize = (scale) / 100f;

            GL.Begin(GL.QUADS);
            GL.Color(gizmoStats.MainColor);

            Vector3 quadNormal = holdTransform.forward;
            //Vector3 quadUp = selector.transform.up;
            //Vector3 quadRight = selector.transform.right;

            Vector3 quadUp = Camera.main.transform.up;
            Vector3 quadRight = Camera.main.transform.right;

            Vector3 axisRight = Vector3.right;
            Vector3 axisUp = Vector3.up;
            Vector3 axisForward = Vector3.forward;

            //float limit = 0;
            float limitPosition = 0;
            int axisDirection = 1;

            if (holdTransform.parent != null)
            {
                axisRight = holdTransform.parent.right;
                axisUp = holdTransform.parent.up;
                axisForward = holdTransform.parent.forward;
            }

            for (int c = 0; c < 6; c++)
            {
                switch (c)
                {
                    case 0: // right
                        quadNormal = axisRight;
                        limitPosition = holdTransform.localPosition.x;
                        //limit = holoHold.xTranslateLimits.y;
                        axisDirection = 1;
                        //quadUp = transform.up;
                        //quadRight = transform.right;
                        break;
                    case 1: // left
                        quadNormal = -axisRight;
                        limitPosition = holdTransform.localPosition.x;
                        //limit = holoHold.xTranslateLimits.x;
                        axisDirection = -1;
                        //quadUp = transform.up;
                        //quadRight = transform.right;
                        break;
                    case 2: //up
                        quadNormal = axisUp;
                        limitPosition = holdTransform.localPosition.y;
                        //limit = holoHold.yTranslateLimits.y;
                        axisDirection = 1;
                        //quadUp = transform.up;
                        //quadRight = transform.forward;
                        break;
                    case 3: // down
                        quadNormal = -axisUp;
                        limitPosition = holdTransform.localPosition.y;
                        //limit = holoHold.yTranslateLimits.x;
                        axisDirection = -1;
                        //quadUp = transform.up;
                        //quadRight = transform.forward;
                        break;
                    case 4: // forward
                        quadNormal = axisForward;
                        limitPosition = holdTransform.localPosition.z;
                        //limit = holoHold.zTranslateLimits.y;
                        axisDirection = 1;
                        //quadUp = transform.forward;
                        //quadRight = transform.right;
                        break;
                    case 5: // backward
                        quadNormal = -axisForward;
                        limitPosition = holdTransform.localPosition.z;
                        //limit = holoHold.zTranslateLimits.x;
                        axisDirection = -1;
                        //quadUp = transform.forward;
                        //quadRight = transform.right;
                        break;
                }

                float quadSpace = scale / 4f;

                for (int q = 1; q < 5; q++)
                {
                    float normalDistance = -((limitPosition / quadSpace) % 1f) * quadSpace * axisDirection;
                    if (axisDirection == -1) normalDistance = -quadSpace + normalDistance;
                    normalDistance += q * quadSpace;

                    //float brightness = (2f - Mathf.Abs(normalDistance - 3f)) * _gizmoFadeIn;
                    float brightness = Mathf.Clamp(1f - normalDistance / (quadSpace * 5f), 0f, 1f);
                    GL.Color(new Color(.5f * brightness, .5f * brightness, .5f * brightness, 1f));
                    GL.Vertex(holdTransform.position + quadNormal * scale * .5f + quadNormal.normalized * normalDistance + quadUp * quadSize + quadRight * quadSize);
                    GL.Vertex(holdTransform.position + quadNormal * scale * .5f + quadNormal.normalized * normalDistance + quadUp * quadSize - quadRight * quadSize);
                    GL.Vertex(holdTransform.position + quadNormal * scale * .5f + quadNormal.normalized * normalDistance - quadUp * quadSize - quadRight * quadSize);
                    GL.Vertex(holdTransform.position + quadNormal * scale * .5f + quadNormal.normalized * normalDistance - quadUp * quadSize + quadRight * quadSize);
                }

            }
            GL.End();
        }
        public static void DrawRotate(HoloHold holoHold, Vector3 headPosition, Quaternion headRotation, float radius, HoloHoldController.GizmoStats gizmoStats)
        {
            Transform holdTransform = holoHold.HoldTransform;
            GL.Begin(GL.LINES);
            GL.Color(gizmoStats.MainColor);
            //float radius = radius / 2;
            Vector3 localRight = Vector3.right;
            Vector3 localForward = Vector3.forward;
            if (holdTransform.parent)
            {
                localRight = holdTransform.parent.right;
                localForward = holdTransform.parent.forward;
            }
            for (int a = 0; a < 360; a += 5)
            {
                float size = 1f;
                if (a % 90 == 0) size = 1f;
                else if (a % 45 == 0) size = .75f;
                else if (a % 30 == 0) size = .5f;
                else size = .25f;
                GL.Vertex(holdTransform.position + (localRight * Mathf.Sin(a * Mathf.Deg2Rad) + localForward * Mathf.Cos(a * Mathf.Deg2Rad)) * radius * (1f + .2f * gizmoStats.RotateFadeIn));
                GL.Vertex(holdTransform.position + (localRight * Mathf.Sin(a * Mathf.Deg2Rad) + localForward * Mathf.Cos(a * Mathf.Deg2Rad)) * radius * (1f + (.2f - size * .1f) * gizmoStats.RotateFadeIn));
            }
            GL.End();
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valorem.HoloConsole
{
    public class TrackCamera : MonoBehaviour
    {
        public Vector3 LocalCamPositionTarget = new Vector3(-2.55f, 1.38f, 9.4f);
        [Range(0f, 10f)]
        public float SlerpSpeed = 6.5f;

        // Update is called once per frame
        void Update()
        {
            float slerpFraction = Time.deltaTime * SlerpSpeed;

            Vector3 targetPos = Camera.main.transform.TransformPoint(LocalCamPositionTarget);
            transform.position = Vector3.Slerp(transform.position, targetPos, slerpFraction);

            transform.rotation = Quaternion.Slerp(transform.rotation, Camera.main.transform.rotation, slerpFraction);
        }
    }
}

using System.Collections;
using UnityEngine;

namespace Aryzon
{
    public class RotateWithCamera : MonoBehaviour
    {
        public bool rotate = true;
        private bool rotating = false;

        [HideInInspector]
        public float _maxRotationY = 180f;
        [HideInInspector]
        public float _deltaRotationY = 360f;
        public float _minimumAngleY = 60f;

        [Tooltip("Angular speed in radians per second.")]
        public float speed;

        private void OnEnable()
        {
            StartCoroutine(StartWhenReady());
        }

        IEnumerator StartWhenReady()
        {
            while (AryzonSettings.Instance == null || AryzonSettings.Instance.aryzonManager == null)
            {
                yield return null;
            }
            AryzonSettings.Instance.aryzonManager.PoseUpdate += UpdatePose;
        }

        public void UpdatePose(AryzonPose pose)
        {
            if (rotate)
            {
                float fromRotation = gameObject.transform.rotation.eulerAngles.y;
                float toRotation = pose.rotation.eulerAngles.y;
                float deltaRotation = toRotation - fromRotation;

                if (deltaRotation > _maxRotationY)
                {
                    deltaRotation -= _deltaRotationY;
                }
                else if (deltaRotation < -_maxRotationY)
                {
                    deltaRotation += _deltaRotationY;
                }

                float minAngle = _minimumAngleY;

                if (!rotating && Mathf.Abs(deltaRotation) > minAngle)
                {
                    rotating = true;
                } else if (rotating)
                {
                    // Using else if here to make rotation start in the next frame to solve a glitch when rotation changes a lot for only a single frame.
                    float newRotation = fromRotation + deltaRotation / 20f;
                    gameObject.transform.rotation = Quaternion.Euler(new Vector3(gameObject.transform.rotation.eulerAngles.x, newRotation, gameObject.transform.rotation.eulerAngles.z));
                    if (Mathf.Abs(deltaRotation) < 1f)
                    {
                        rotating = false;
                    }
                }
                gameObject.transform.position = pose.position;
            }
        }

        private void OnDisable()
        {
            if (AryzonSettings.Instance != null && AryzonSettings.Instance.aryzonManager != null)
            {
                AryzonSettings.Instance.aryzonManager.PoseUpdate -= UpdatePose;
            }
        }
    }
}
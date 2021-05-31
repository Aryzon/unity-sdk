using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aryzon {

    public class AryzonPoseDriver : MonoBehaviour
    {
        [SerializeField] private bool _applyHeadModel = true;
        public bool applyHeadModel
        {
            get { return _applyHeadModel; }
            set { _applyHeadModel = value; }
        }

        [SerializeField] private bool _applyRotationModel = true;
        public bool applyRotationModel
        {
            get { return _applyRotationModel; }
            set { _applyRotationModel = value; }
        }

        private Vector3 headModel;

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
            AryzonSettings.Instance.UpdateLayout += UpdateLayout;

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            int m = 1;
            if (!AryzonSettings.Headset.landscapeLeft) { m = -1; }

            headModel = new Vector3(AryzonSettings.Calibration.XShift + m * (AryzonSettings.Phone.xShift + 0.04f), AryzonSettings.Calibration.YShift + m * (AryzonSettings.Phone.yShift - 0.03f), -AryzonSettings.Calibration.EyeToLens);
        }

        public void UpdatePose(AryzonPose pose)
        {
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;

            if (applyHeadModel)
            {
                transform.localPosition = transform.TransformPoint(headModel);
            }
            if (applyRotationModel)
            {
                transform.Rotate(Vector3.right, AryzonSettings.Headset.xRotation);
            }
        }

        private void OnDisable()
        {
            if (AryzonSettings.Instance != null) {
                AryzonSettings.Instance.UpdateLayout -= UpdateLayout;
                if (AryzonSettings.Instance.aryzonManager != null)
                {
                    AryzonSettings.Instance.aryzonManager.PoseUpdate -= UpdatePose;
                }
            } 
        }
    }
}
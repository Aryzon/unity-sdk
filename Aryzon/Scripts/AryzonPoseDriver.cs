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

            headModel = new Vector3(AryzonSettings.Calibration.XShift, AryzonSettings.Calibration.YShift, -AryzonSettings.Headset.eyeToLens);
        }

        private void UpdateLayout()
        {
             headModel = new Vector3(AryzonSettings.Calibration.XShift, AryzonSettings.Calibration.YShift, -AryzonSettings.Headset.eyeToLens);
        }

        public void UpdatePose(AryzonPose pose)
        {
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;

            if (applyHeadModel)
            {
                transform.localPosition = transform.TransformPoint(headModel);
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
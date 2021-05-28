using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

using Google.XR.Cardboard;

#if ARYZON_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
#endif

namespace Aryzon
{
    public class AryzonCameraHandler
    {
        private List<SavedCameraSettings> savedCameraSettings = new List<SavedCameraSettings>();
        private List<MonoBehaviour> cameraBackgroundComponents = new List<MonoBehaviour>();
        private List<GameObject> cameraBackgroundObjects = new List<GameObject>();

        public void UpdateCamerasForStereoscopicMode()
        {
            savedCameraSettings.Clear();

            cameraBackgroundComponents.Clear();
            cameraBackgroundObjects.Clear();

            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.stereoTargetEye == StereoTargetEyeMask.Both)
                {
                    cam.stereoSeparation = AryzonSettings.Calibration.IPD;

                    SavedCameraSettings savedCameraSetting = new SavedCameraSettings();
                    savedCameraSetting.camera = cam;
                    savedCameraSetting.bgColor = cam.backgroundColor;
                    savedCameraSetting.clearFlags = cam.clearFlags;
                    savedCameraSettings.Add(savedCameraSetting);

                    if (AryzonSettings.Instance.aryzonManager.blackBackgroundInStereoscopicMode)
                    {
                        DisableBackgroundRenderer(cam);
                    }
#if ARYZON_ARFOUNDATION
                    ARPoseDriver arPoseDriver = cam.GetComponent<ARPoseDriver>();
                    if (arPoseDriver && arPoseDriver.enabled)
                    {
                        savedCameraSetting.poseDrivers.Add(arPoseDriver);
                    }
#endif
                    TrackedPoseDriver trackedPoseDriver = cam.GetComponent<TrackedPoseDriver>();
                    if (trackedPoseDriver && trackedPoseDriver.enabled)
                    {
                        savedCameraSetting.poseDrivers.Add(trackedPoseDriver);
                    }
                    if (savedCameraSetting.poseDrivers.Count > 0)
                    {
                        AryzonPoseDriver aryzonPoseDriver = cam.GetComponent<AryzonPoseDriver>();

                        if (!aryzonPoseDriver)
                        {
                            aryzonPoseDriver = cam.gameObject.AddComponent<AryzonPoseDriver>();
                        }
                        aryzonPoseDriver.applyHeadModel = true;
                        aryzonPoseDriver.enabled = true;

                        foreach (MonoBehaviour poseDriver in savedCameraSetting.poseDrivers)
                        {
                            poseDriver.enabled = false;
                        }
                    }
                }
            }
        }

        public void ResetCameras()
        {
            EnableBackgroundRenderers();

            foreach (SavedCameraSettings savedCameraSetting in savedCameraSettings)
            {
                Camera cam = savedCameraSetting.camera;
                if (cam)
                {
                    cam.clearFlags = savedCameraSetting.clearFlags;
                    cam.backgroundColor = savedCameraSetting.bgColor;
                }
                foreach (MonoBehaviour poseDriver in savedCameraSetting.poseDrivers)
                {
                    AryzonPoseDriver aryzonPoseDriver = cam.GetComponent<AryzonPoseDriver>();
                    if (aryzonPoseDriver)
                    {
                        aryzonPoseDriver.enabled = false;
                    }
                    if (poseDriver)
                    {
                        poseDriver.enabled = true;
                    }
                }
            }
        }

        public void DisableBackgroundRenderer(Camera cam)
        {
#if ARYZON_ARFOUNDATION
            if (AryzonSettings.Instance.aryzonManager.trackingEngine == TrackingEngine.ARFoundation)
            {
                ARCameraBackground cameraBackground = cam.GetComponent<ARCameraBackground>();

                if (cameraBackground && cameraBackground.enabled)
                {
                    cameraBackgroundComponents.Add(cameraBackground);
                }
            }
#endif
            foreach (MonoBehaviour background in cameraBackgroundComponents)
            {
                if (background)
                {
                    background.enabled = false;
                }
            }

            foreach (GameObject background in cameraBackgroundObjects)
            {
                if (background)
                {
                    background.SetActive(false);
                }
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        public void EnableBackgroundRenderers()
        {
            foreach (MonoBehaviour background in cameraBackgroundComponents)
            {
                if (background)
                {
                    background.enabled = true;
                }
            }

            foreach (GameObject background in cameraBackgroundObjects)
            {
                if (background)
                {
                    background.SetActive(true);
                }
            }
        }
    }



    public class SavedCameraSettings
    {
        public CameraClearFlags clearFlags;
        public Color bgColor;
        public Camera camera;
        public List<MonoBehaviour> poseDrivers = new List<MonoBehaviour>();
    }
}
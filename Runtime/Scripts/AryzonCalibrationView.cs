using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aryzon
{
    public class AryzonCalibrationView : MonoBehaviour, IAryzonEventHandler
    {
        public Canvas calibrationCanvas;

        public Text values;

        private bool hasChanges = false;

        private void UpdateValues()
        {
            UpdateText();
            SavePressed();
        }

        private void UpdateText()
        {
            values.text = (AryzonSettings.Calibration.XShift * 100).ToString("0.##")
                + "\n" + (AryzonSettings.Calibration.YShift * 100).ToString("0.##")
                + "\n" + (AryzonSettings.Calibration.EyeToLens * 100).ToString("0.##")
                + "\n" + (AryzonSettings.Calibration.IPD * 100).ToString("0.##")
                + "\n" + (AryzonSettings.Calibration.ILD * 100).ToString("0.##");
        }

        private void OnEnable()
        {
            AryzonSettings.Instance.RegisterEventHandler(this);
        }

        private void OnDisable()
        {
            if (AryzonSettings.Instance)
            {
                AryzonSettings.Instance.UnregisterEventHandler(this);
            }
        }

        public void SavePressed()
        {
            if (hasChanges)
            {
                AryzonSettings.Instance.SaveCalibration();
            }
        }

        public void UpPressed()
        {
            AryzonSettings.Calibration.YShift -= 0.01f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
        }

        public void DownPressed()
        {
            AryzonSettings.Calibration.YShift += 0.01f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
        }

        public void RightPressed()
        {
            AryzonSettings.Calibration.XShift -= 0.01f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
        }

        public void LeftPressed()
        {
            AryzonSettings.Calibration.XShift += 0.01f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
        }

        public void WiderIPDPressed()
        {
            AryzonSettings.Calibration.IPD += 0.0005f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
            AryzonCardboardSubsystemLoader.ReloadDeviceParams();
        }

        public void SmallerIPDPressed()
        {
            AryzonSettings.Calibration.IPD -= 0.0005f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
            AryzonCardboardSubsystemLoader.ReloadDeviceParams();
        }

        public void WiderILDPressed()
        {
            AryzonSettings.Calibration.ILD += 0.0005f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
            AryzonCardboardSubsystemLoader.ReloadDeviceParams();
        }

        public void SmallerILDPressed()
        {
            AryzonSettings.Calibration.ILD -= 0.0005f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
            AryzonCardboardSubsystemLoader.ReloadDeviceParams();
        }

        public void FurtherAwayPressed()
        {
            AryzonSettings.Calibration.EyeToLens += 0.01f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
        }

        public void CloserByPressed()
        {
            AryzonSettings.Calibration.EyeToLens -= 0.01f;
            AryzonSettings.Instance.Apply();
            hasChanges = true;
            UpdateValues();
        }

        public void OnStopStereoscopicMode(AryzonModeEventArgs e)
        {
            calibrationCanvas.gameObject.SetActive(false);
        }

        public void OnStartStereoscopicMode(AryzonModeEventArgs e)
        {
            if (AryzonSettings.Calibration.showCalibrate) {
                calibrationCanvas.worldCamera = AryzonSettings.Instance.aryzonManager.XRCamera;
                calibrationCanvas.gameObject.SetActive(true);
                UpdateText();
            }
        }

        public void OnStopAryzonMode(AryzonModeEventArgs e)
        {
            
        }

        public void OnStartAryzonMode(AryzonModeEventArgs e)
        {
            
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aryzon {

    public class AryzonSettingsUI : MonoBehaviour
    {
        public Toggle showCalibration;
        public Toggle predictMovement;

        public Slider rotationPredictSlider;
        public Slider positionPredictSlider;

        public Text rotationSliderStatus;
        public Text positionSliderStatus;

        void Start()
        {
            showCalibration.isOn = AryzonSettings.Calibration.showCalibrate;
            predictMovement.isOn = AryzonSettings.Phone.predict;

            rotationPredictSlider.value = AryzonSettings.Phone.rotationPredictSteps;
            positionPredictSlider.value = AryzonSettings.Phone.positionPredictSteps;

            rotationSliderStatus.text = "Predict rotation " + AryzonSettings.Phone.rotationPredictSteps + " frame";
            positionSliderStatus.text = "Predict position " + AryzonSettings.Phone.positionPredictSteps + " frame";

            if (AryzonSettings.Phone.rotationPredictSteps > 1)
            {
                rotationSliderStatus.text += "s";
            }

            if (AryzonSettings.Phone.positionPredictSteps > 1)
            {
                positionSliderStatus.text += "s";
            }
        }

        public void ShowCalibrationToggleChanged ()
        {
            AryzonSettings.Calibration.showCalibrate = showCalibration.isOn;
            AryzonSettings.Instance.Save();
        }

        public void PredictMovementToggleChanged()
        {
            AryzonSettings.Phone.predict = predictMovement.isOn;
            AryzonSettings.Instance.Save();
        }

        public void RotationPredictSliderChanged()
        {
            AryzonSettings.Phone.rotationPredictSteps = (int)rotationPredictSlider.value;
            rotationSliderStatus.text = "Predict rotation " + AryzonSettings.Phone.rotationPredictSteps + " frame";
            if (AryzonSettings.Phone.rotationPredictSteps > 1)
            {
                rotationSliderStatus.text += "s";
            }
            AryzonSettings.Instance.Save();
        }

        public void PositionPredictSliderChanged()
        {
            AryzonSettings.Phone.positionPredictSteps = (int)positionPredictSlider.value;
            positionSliderStatus.text = "Predict position " + AryzonSettings.Phone.positionPredictSteps + " frame";
            if (AryzonSettings.Phone.positionPredictSteps > 1)
            {
                positionSliderStatus.text += "s";
            }
            AryzonSettings.Instance.Save();
        }
    }
}
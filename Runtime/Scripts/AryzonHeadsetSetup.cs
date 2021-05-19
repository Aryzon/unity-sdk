using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Aryzon;
using System.Text;

public class AryzonHeadsetSetup : MonoBehaviour
{
    public Dropdown headsetSelectDropdown;
    public Text statusText;
    public Toggle showCalibrateToggle;
    private HeadsetDatas headsets = new HeadsetDatas();

    private bool changeCameFromSelf = false;

    public void ShowCalibrationToggleChanged()
    {
        AryzonSettings.Calibration.showCalibrate = showCalibrateToggle.isOn;
        AryzonSettings.Instance.Save();
    }

    private void Start()
    {
        showCalibrateToggle.isOn = AryzonSettings.Calibration.showCalibrate;
    }

    private void Awake()
    {
        headsetSelectDropdown.options.Clear();

        Dropdown.OptionData optionData = new Dropdown.OptionData(AryzonSettings.Headset.name);
        headsetSelectDropdown.options.Add(optionData);

        headsetSelectDropdown.value = 0;

        StartCoroutine(RetrieveHeadsetsRoutine());
    }

    private IEnumerator RetrieveHeadsetsRoutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://sdk.aryzon.com/GetHeadsets.php"))
        {
            yield return webRequest.SendWebRequest();

            if (!webRequest.isNetworkError)
            {
                try
                {
                    string text = webRequest.downloadHandler.text.Replace("result:", "");
                    headsets = JsonUtility.FromJson<HeadsetDatas>(text);
                } catch
                {
                    Debug.LogWarning("[Aryzon] Could not retrieve headset values.");
                }
            }
            if (headsets.headsetDatas.Length == 0)
            {
                Debug.LogWarning("[Aryzon] Could not retrieve headset values.");
            }
            else
            {
                headsetSelectDropdown.options.Clear();
                int selectionIndex = -1;
                int i = 0;
                foreach (HeadsetData headsetData in headsets.headsetDatas)
                {
                    Dropdown.OptionData optionData = new Dropdown.OptionData(headsetData.name);
                    headsetSelectDropdown.options.Add(optionData);
                    if (headsetData.name == AryzonSettings.Headset.name)
                    {
                        selectionIndex = i;
                    }
                    i++;
                }
                changeCameFromSelf = true;
                headsetSelectDropdown.value = selectionIndex;
                changeCameFromSelf = false;
            }
        }
    }

    public void OnValueChanged()
    {
        if (!changeCameFromSelf && headsets.headsetDatas.Length > 0 && headsetSelectDropdown.value > -1) {
            HeadsetData headsetData = headsets.headsetDatas[headsetSelectDropdown.value];
            statusText.text = "Loading Headset settings..";
            AryzonSettings.Instance.SettingsRetrieved += HeadsetSettingsRetrieved;
            AryzonSettings.Instance.RetrieveSettingsForHeadsetID(headsetData.id);
        }
    }

    private void HeadsetSettingsRetrieved(string status, bool success)
    {
        statusText.text = status;
        AryzonSettings.Instance.SettingsRetrieved -= HeadsetSettingsRetrieved;
    }

    private void OnEnable()
    {
        statusText.text = "";
    }

    [Serializable]
    public class HeadsetData
    {
        public int id = -1; 
        public string name = "";
    }

    [Serializable]
    public class HeadsetDatas
    {
        public HeadsetData[] headsetDatas = new HeadsetData[] { };
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

using System.Text;
using UnityEngine;

//Do not edit this class

namespace Aryzon {
    public interface IAryzonEventHandler
    {
        void OnStopStereoscopicMode(AryzonModeEventArgs e);
        void OnStartStereoscopicMode(AryzonModeEventArgs e);
        void OnStopAryzonMode(AryzonModeEventArgs e);
        void OnStartAryzonMode(AryzonModeEventArgs e);
    }

    public class AryzonModeEventArgs : EventArgs
    {
        //For later use
    }

    public delegate void SettingsRetrievedEventHandler (string status, bool success);
    public delegate void UpdateLayoutEventHandler ();

    public class AryzonSettings : MonoBehaviour {
        public event SettingsRetrievedEventHandler SettingsRetrieved;
        public event UpdateLayoutEventHandler UpdateLayout;

        public bool AryzonMode = false;
        public bool LandscapeMode = false;
        public bool PortraitMode = false;
        public bool ShowReticle = false;

        //public Camera reticleCamera;
        public AryzonManager aryzonManager;
        public GameObject aryzonInputController;

        private List<IAryzonEventHandler> aryzonEventHandlers = new List<IAryzonEventHandler>();

        public void RegisterEventHandler(IAryzonEventHandler aryzonEventHandler)
        {
            aryzonEventHandlers.Add(aryzonEventHandler);
        }

        public void UnregisterEventHandler(IAryzonEventHandler aryzonEventHandler)
        {
            while (aryzonEventHandlers.Contains(aryzonEventHandler))
            {
                aryzonEventHandlers.Remove(aryzonEventHandler);
            }
        }

        internal void OnStartStereoscopicMode(AryzonModeEventArgs e)
        {
            OnStartStereoscopicModeProtected(e);
        }

        internal void OnStopStereoscopicMode(AryzonModeEventArgs e)
        {
            OnStopStereoscopicModeProtected(e);
        }

        internal void OnStartAryzonMode(AryzonModeEventArgs e)
        {
            OnStartAryzonModeProtected(e);
        }

        internal void OnStopAryzonMode(AryzonModeEventArgs e)
        {
            OnStopAryzonModeProtected(e);
        }

        protected virtual void OnStartStereoscopicModeProtected(AryzonModeEventArgs e)
        {
            foreach (IAryzonEventHandler aryzonEventHandler in aryzonEventHandlers)
            {
                aryzonEventHandler.OnStartStereoscopicMode(e);
            }
        }

        protected virtual void OnStopStereoscopicModeProtected(AryzonModeEventArgs e)
        {
            foreach (IAryzonEventHandler aryzonEventHandler in aryzonEventHandlers)
            {
                aryzonEventHandler.OnStopStereoscopicMode(e);
            }
        }

        protected virtual void OnStartAryzonModeProtected(AryzonModeEventArgs e)
        {
            foreach (IAryzonEventHandler aryzonEventHandler in aryzonEventHandlers)
            {
                aryzonEventHandler.OnStartAryzonMode(e);
            }
        }

        protected virtual void OnStopAryzonModeProtected(AryzonModeEventArgs e)
        {
            foreach (IAryzonEventHandler aryzonEventHandler in aryzonEventHandlers)
            {
                aryzonEventHandler.OnStopAryzonMode(e);
            }
        }


        protected AryzonSettings () {}
        private static AryzonSettings _instance;
        private static object _lock = new object();
        public static AryzonSettings Instance
        {
            get
            {
                if (applicationIsQuitting) {
                    //Debug.LogWarning("[Aryzon] Instance '"+ typeof(AryzonSettings) + "' already destroyed on application quit." + " Won't create again - returning null.");
                    return null;
                }

                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (AryzonSettings) FindObjectOfType(typeof(AryzonSettings));

                        if ( FindObjectsOfType(typeof(AryzonSettings)).Length > 1 )
                        {
                            Debug.LogError("[Aryzon] there should never be more than 1 AryzonManager");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject aryzonManager = new GameObject();
                            _instance = aryzonManager.AddComponent<AryzonSettings>();
                            aryzonManager.name = "AryzonManager";

                            DontDestroyOnLoad(aryzonManager);
                        } else {
                            Debug.Log("[Aryzon] Using instance already created: " +
                                      _instance.gameObject.name);
                        }
                    }

                    return _instance;
                }
            }
        }

        private bool downKeyChanged = true;
        private bool upKeyChanged = true;

        private KeyCode _controllerDownKeyCode;
        public KeyCode controllerDownKeyCode
        {
            get
            {
                if (downKeyChanged)
                {
                    downKeyChanged = false;
                    _controllerDownKeyCode = (KeyCode)PlayerPrefs.GetInt("controllerDownKey", (int)KeyCode.None);
                }
                return _controllerDownKeyCode;
            }
            set { PlayerPrefs.SetInt("controllerDownKey", (int)value); downKeyChanged = true; }
        }

        private KeyCode _controllerUpKeyCode;
        public KeyCode controllerUpKeyCode
        {
            get
            {
                if (upKeyChanged)
                {
                    upKeyChanged = false;
                    _controllerUpKeyCode = (KeyCode)PlayerPrefs.GetInt("controllerUpKey", (int)KeyCode.None);
                }
                return _controllerUpKeyCode;
            }
            set { PlayerPrefs.SetInt("controllerUpKey", (int)value); upKeyChanged = true; }
        }

        private static bool applicationIsQuitting = false;

        public void OnDestroy () {
            applicationIsQuitting = true;
        }

        public static class Calibration {
            public static bool didCalibrate = false;
            public static bool skipCalibrate = false;
            public static bool manualxShift = false;
            public static float mxShift = 0.03f;
            public static float xShift = 0.03f;
            public static float XShift
            {
                get { if (manualxShift) { return mxShift; } return xShift; }
                set { mxShift = value; manualxShift = true; }
            }
            public static bool manualyShift = false;
            public static float myShift = -0.105f;
            public static float yShift = -0.105f;
            public static float YShift
            {
                get { if (manualyShift) { return myShift; } return yShift; }
                set { myShift = value; manualyShift = true; }
            }
            public static bool manualIPD = false;
            public static float mIPD = 0.064f;
            public static float ipd = 0.064f;
            public static float IPD
            {
                get { if (manualIPD) { return mIPD; } return ipd; }
                set { mIPD = value; manualIPD = true; }
            }
            public static bool rotatedSensor = false;
            public static bool showCalibrate = false;

            static Calibration() {
                Debug.Log("Load");
                if (SerializeStatic.Load(typeof(Calibration), Application.persistentDataPath + "/AryzonCalibrationSettings.bfd")) {
                    AryzonSettings.Instance.Apply();
                } else
                { // The file does not exist or is corrupted, create a new one and try again
                    Debug.Log("NotOKLoad");
                    AryzonSettings.Instance.SaveCalibration();
                    if (SerializeStatic.Load(typeof(Calibration), Application.persistentDataPath + "/AryzonCalibrationSettings.bfd"))
                    {
                        AryzonSettings.Instance.Apply();
                    }
                }
            }
        }

        public void Initialize () {
            if (!Calibration.didCalibrate && !Phone.aryzonCalibrated) {
                RetrieveSettingsForPhone ();
            }
        }

        public static class Phone {
            public static float xShift = 0.03f;
            public static float yShift = -0.105f;
            public static bool manualScreenWidth = false;
            public static float mScreenWidth = 0.1f;
            public static float screenWidth = 0.1f;
            public static float ScreenWidth
            {
                get { if (manualScreenWidth) { return mScreenWidth; } return screenWidth; }
                set { mScreenWidth = value; manualScreenWidth = true; }
            }
            public static bool rotatedSensor = false;
            public static bool aryzonCalibrated = false;
            public static bool predict = true;
            public static int rotationPredictSteps = 1;
            public static int positionPredictSteps = 3;

            static Phone() {
                bool ok = SerializeStatic.Load (typeof(Phone), Application.persistentDataPath + "/AryzonPhoneSettings.bfd");
                if (ok) {
                    AryzonSettings.Instance.Apply();
                }
            }
        }

        public static class Headset {
            public static string name = "Aryzon";
            public static float xShift = 0f;
            public static float yShift = 0.03f;
            public static float distortion = 0f;
            public static float redShift = 1.01f;
            public static float greenShift = 1.02f;
            public static float blueShift = 1.04f;
            public static float lensToScreen = 0.063f;
            public static float eyeToLens = 0.11f;
            public static float focalLength = 0.082f;
            public static float lensCenterDistance = 0.06f;
            public static float bottomToCenter = 0.105f;
            public static float fovFactor = 1.0f;
            public static float xRotation = 0f;
            public static bool didAcceptDisclaimer = false;
            public static bool landscapeLeft = true;

            static Headset() {
                bool ok = SerializeStatic.Load (typeof(Headset), Application.persistentDataPath + "/AryzonHeadsetSettings.bfd");
                if (ok) {
                    AryzonSettings.Instance.Apply();
                }
            }
        }

        public bool Save () {
            string savePathHeadset = Application.persistentDataPath + "/AryzonHeadsetSettings.bfd";
            string savePathPhone = Application.persistentDataPath + "/AryzonPhoneSettings.bfd";
            
            if (File.Exists (savePathHeadset)) {
                File.Delete (savePathHeadset);
            }
            if (File.Exists (savePathPhone)) {
                File.Delete (savePathPhone);
            }
            

            bool ok0 = SerializeStatic.Save(typeof(Headset), savePathHeadset);
            bool ok1 = SerializeStatic.Save(typeof(Phone), savePathPhone);
            

            return ok0 && ok1 && SaveCalibration();
        }

        public bool SaveCalibration()
        {
            string savePathCalibration = Application.persistentDataPath + "/AryzonCalibrationSettings.bfd";
            if (File.Exists(savePathCalibration))
            {
                File.Delete(savePathCalibration);
            }
            
            return SerializeStatic.Save(typeof(Calibration), savePathCalibration);
        }

        public void Apply () {
            if (UpdateLayout != null) {
                UpdateLayout ();
            }
        }

        private void RetrieveSettingsForPhone () {
            string JsonArraystring = "{\"phoneID\": \"" + SystemInfo.deviceModel + "\"}";
            byte[] body = Encoding.UTF8.GetBytes(JsonArraystring);
            StartCoroutine(RetrievedataForPhoneEnumerator("https://sdk.aryzon.com/GetPhoneValues.php", body));
        }

        public void RetrieveSettingsForCode(string id)
        {
            string JsonArraystring = "{\"userID\": \"" + id + "\"}";
            byte[] body = Encoding.UTF8.GetBytes(JsonArraystring);
            StartCoroutine(RetrievedataForCodeEnumerator("https://sdk.aryzon.com/GetValues.php", body));
        }

        public void RetrieveSettingsForHeadsetID(int id)
        {
            string JsonArraystring = "{\"headsetID\": \"" + id + "\"}";
            byte[] body = Encoding.UTF8.GetBytes(JsonArraystring);
            StartCoroutine(RetrievedataForHeadsetEnumerator("https://sdk.aryzon.com/GetHeadsetValues.php", body));
        }

        IEnumerator RetrievedataForCodeEnumerator(string url, byte[] bytes)
        {
            string returnString = "Retrieving data..";
            bool success = false;
            using (UnityWebRequest www = UnityWebRequest.Put(url, bytes))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();
                if (www.error != null)
                {
                    returnString = "Could not retrieve your settings. Please check your internet connection.";
                    success = false;
                }
                else
                {
                    string result = www.downloadHandler.text;

                    if (result.StartsWith("result:"))
                    {
                        result = result.Replace("result:", "");

                        CalibrationData data = JsonUtility.FromJson<CalibrationData>(result);
                        if (SystemInfo.deviceModel != data.MakeModel)
                        {
                            returnString = "It looks like you have a new phone, please recalibrate.";
                            success = false;
                        }
                        else
                        {
                            AryzonSettings.Headset.xShift = data.xShiftLens;
                            AryzonSettings.Headset.yShift = data.yShiftLens;
                            AryzonSettings.Headset.distortion = data.distortion;
                            AryzonSettings.Headset.redShift = data.redShift;
                            AryzonSettings.Headset.greenShift = data.greenShift;
                            AryzonSettings.Headset.blueShift = data.blueShift;
                            AryzonSettings.Headset.lensCenterDistance = data.lensCenterDistance;
                            AryzonSettings.Headset.eyeToLens = data.eyeToLens;
                            AryzonSettings.Headset.lensToScreen = data.lensToScreen;
                            AryzonSettings.Headset.focalLength = data.focalLength;
                            AryzonSettings.Headset.name = data.headsetName;
                            AryzonSettings.Headset.bottomToCenter = data.bottomToCenter;
                            AryzonSettings.Headset.fovFactor = data.fovFactor;
                            AryzonSettings.Headset.xRotation = data.xRotation;

                            if (aryzonManager && !aryzonManager.stereoscopicMode && aryzonManager.aryzonMode && AryzonSettings.Headset.landscapeLeft != data.landscapeLeft)
                            {
                                AryzonSettings.Headset.landscapeLeft = data.landscapeLeft;
                                aryzonManager.SetRotationLandscapeAndPortrait();
                            }

                            AryzonSettings.Calibration.rotatedSensor = data.rotatedSensor;
                            AryzonSettings.Calibration.xShift = data.xShift;
                            AryzonSettings.Calibration.yShift = data.yShift;
                            AryzonSettings.Calibration.ipd = data.IPD;
                            AryzonSettings.Calibration.didCalibrate = true;

                            AryzonSettings.Instance.Apply();
                            AryzonSettings.Instance.Save();
                            success = true;
                            returnString = "Successfully retrieved your settings, enjoy!";
                        }
                    }
                    else
                    {
                        returnString = "Invalid code";
                        success = false;
                    }
                }
                if (SettingsRetrieved != null)
                {
                    SettingsRetrieved(returnString, success);
                }
            }
        }

        IEnumerator RetrievedataForHeadsetEnumerator(string url, byte[] bytes)
        {
            string returnString = "Retrieving data..";
            bool success = false;
            using (UnityWebRequest www = UnityWebRequest.Put(url, bytes))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();
                if (www.error != null)
                {
                    returnString = "Could not retrieve headset settings. Please check your internet connection.";
                    success = false;
                }
                else
                {
                    string result = www.downloadHandler.text;
                    if (result.StartsWith("result:"))
                    {
                        result = result.Replace("result:", "");

                        HeadsetData data = JsonUtility.FromJson<HeadsetData>(result);

                        AryzonSettings.Headset.xShift = data.xShift;
                        AryzonSettings.Headset.yShift = data.yShift;
                        AryzonSettings.Headset.distortion = data.distortion;
                        AryzonSettings.Headset.redShift = data.redShift;
                        AryzonSettings.Headset.greenShift = data.greenShift;
                        AryzonSettings.Headset.blueShift = data.blueShift;
                        AryzonSettings.Headset.lensCenterDistance = data.lensCenterDistance;
                        AryzonSettings.Headset.eyeToLens = data.eyeToLens;
                        AryzonSettings.Headset.lensToScreen = data.lensToScreen;
                        AryzonSettings.Headset.focalLength = data.focalLength;
                        AryzonSettings.Headset.name = data.name;
                        AryzonSettings.Headset.bottomToCenter = data.bottomToCenter;
                        AryzonSettings.Headset.fovFactor = data.fovFactor;
                        AryzonSettings.Headset.xRotation = data.xRotation;
                        if (aryzonManager && !aryzonManager.stereoscopicMode && aryzonManager.aryzonMode && AryzonSettings.Headset.landscapeLeft != data.landscapeLeft)
                        {
                            AryzonSettings.Headset.landscapeLeft = data.landscapeLeft;
                            aryzonManager.SetRotationLandscapeAndPortrait();
                        }

                        AryzonSettings.Instance.Apply();
                        AryzonSettings.Instance.Save();
                        success = true;
                        returnString = "New headset settings loaded";
                    }
                    else
                    {
                        returnString = "Invalid code";
                        success = false;
                    }
                }
                if (SettingsRetrieved != null)
                {
                    SettingsRetrieved(returnString, success);
                }
            }
        }

        IEnumerator RetrievedataForPhoneEnumerator(string url, byte[] bytes)
        {
            string returnString = "Retrieving data..";
            bool success = false;

            using (UnityWebRequest www = UnityWebRequest.Put(url, bytes))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();
                if (www.error != null)
                {
                    returnString = "Could not retrieve your settings. Please check your internet connection.";
                    success = false;
                }
                else
                {
                    string result = www.downloadHandler.text;
                    if (result.StartsWith("result:"))
                    {
                        result = result.Replace("result:", "");
                        
                        CalibrationData data = JsonUtility.FromJson<CalibrationData>(result);

                        AryzonSettings.Phone.xShift = data.xShiftLens;
                        AryzonSettings.Phone.yShift = data.yShiftLens;
                        AryzonSettings.Phone.rotatedSensor = data.rotatedSensor;
                        AryzonSettings.Phone.screenWidth = data.screenWidth;
                        AryzonSettings.Phone.aryzonCalibrated = data.aryzonCalibrated;

                        AryzonSettings.Instance.Apply();
                        AryzonSettings.Instance.Save();
                        success = true;
                        returnString = "Successfully retrieved settings for your phone!";
                    }
                    else
                    {
                        returnString = "Could not find settings for your phone";
                        success = false;
                    }
                }
                if (SettingsRetrieved != null)
                {
                    SettingsRetrieved(returnString, success);
                }
            }
        }
    }

    public class SerializeStatic
    {
        public static bool Save(Type static_class, string filename)
        {
            try
            {
                FieldInfo[] fields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);
                object[,] a = new object[fields.Length,2];
                int i = 0;
                foreach (FieldInfo field in fields)
                {
                    a[i, 0] = field.Name;
                    a[i, 1] = field.GetValue(null);
                    i++;
                };
                Stream f = File.Open(filename, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();                
                formatter.Serialize(f, a);
                f.Close();
                return true;
            }
            catch (SerializationException e)
            {
                Debug.LogWarning ("[Aryzon] " + e);
                return false;
            }
        }

        public static bool Load(Type static_class, string filename)
        {
            try
            {
                if (!File.Exists(filename)) {
                    return false;
                }

                FieldInfo[] fields = static_class.GetFields(BindingFlags.Static | BindingFlags.Public);                
                object[,] a;
                Stream f = File.Open(filename, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                a = formatter.Deserialize(f) as object[,];
                f.Close();
                if (a.GetLength(0) != fields.Length) return false;
                int i = 0;
                foreach (FieldInfo field in fields)
                {
                    if (field.Name == (a[i, 0] as string))
                    {
                        field.SetValue(null, a[i,1]);
                    }
                    i++;
                };                
                return true;
            }
            catch (SerializationException e)
            {
                Debug.LogWarning ("[Aryzon] " + e);
                return false;
            }
        }
    }

    [Serializable]
    class CalibrationData {
        public float xShift = 0f;
        public float yShift = -0.105f;
        public float IPD = 0.064f;
        public string headsetName = "Aryzon";
        public float xShiftLens = 0f;
        public float yShiftLens = 0f;
        public float distortion = 0f;
        public float redShift = 0f;
        public float greenShift = 0f;
        public float blueShift = 0f;
        public float lensToScreen = 0.063f;
        public float eyeToLens = 0.11f;
        public float focalLength = 0.082f;
        public float lensCenterDistance = 0.082f;
        public float bottomToCenter = 0.105f;
        public float fovFactor = 1.0f;
        public float xRotation = 0f;
        public string MakeModel = "";
        public bool landscapeLeft = true;
        public bool rotatedSensor = false;
        public bool aryzonCalibrated = false;
        public float screenWidth = 0.1f;
    }

    [Serializable]
    class HeadsetData
    {
        public string name = "Aryzon";
        public float xShift = 0f;
        public float yShift = 0f;
        public float distortion = 0f;
        public float redShift = 0f;
        public float greenShift = 0f;
        public float blueShift = 0f;
        public float lensToScreen = 0.063f;
        public float eyeToLens = 0.11f;
        public float focalLength = 0.082f;
        public float lensCenterDistance = 0.082f;
        public float bottomToCenter = 0.105f;
        public float fovFactor = 1.0f;
        public float xRotation = 0f;
        public bool landscapeLeft = true;
    }
}

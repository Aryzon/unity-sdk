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
                    return null;
                }

                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (AryzonSettings) FindObjectOfType(typeof(AryzonSettings));

                        if ( FindObjectsOfType(typeof(AryzonSettings)).Length > 1 )
                        {
                            Debug.LogError("[Aryzon] there should never be more than 1 AryzonSettings");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject aryzonSettings = new GameObject();
                            _instance = aryzonSettings.AddComponent<AryzonSettings>();
                            aryzonSettings.name = "AryzonSettings";
                            _instance.Initialize();
                            DontDestroyOnLoad(aryzonSettings);
                        } else {
                            Debug.Log("[Aryzon] Using instance already created: " +
                                      _instance.gameObject.name);
                        }
                    }

                    return _instance;
                }
            }
        }

        [Obsolete]
        public KeyCode controllerDownKeyCode
        {
            get => Controller.Trigger.Down;
            set { Controller.Trigger = new Controller.KeyMap(value, Controller.Trigger.Up); SaveController(); }
        }

        [Obsolete]
        public KeyCode controllerUpKeyCode
        {
            get => Controller.Trigger.Up;
            set { Controller.Trigger = new Controller.KeyMap(Controller.Trigger.Down, value); SaveController(); }
        }

        private static bool applicationIsQuitting = false;

        public void OnDestroy () {
            applicationIsQuitting = true;
            Save();
        }

        public static class Calibration {
            public static bool didCalibrate = false;
            public static bool skipCalibrate = false;

            public static bool manualxShift = false;
            public static float mxShift = 0;
            public static float XShift
            {
                get {int m = 1;
                    if (!Headset.landscapeLeft) {m = -1;}
                    if (manualxShift) { return m * mxShift; } return 0;}
                set {
                    int m = 1;
                    if (!Headset.landscapeLeft) { m = -1; }
                    mxShift = m * value; manualxShift = true; }
            }
            public static bool manualyShift = false;
            public static float myShift = 0;
            public static float YShift
            {
                get {int m = 1;
                    float p = 0f; // Adjust for phone height when in landscape right
                    if (!Headset.landscapeLeft) { m = -1; p = 0.07f; }
                    if (manualyShift) { return m * (myShift + p); } return m * p; }
                set {int m = 1;
                    float p = 0f; // Adjust for phone height when in landscape right
                    if (!Headset.landscapeLeft) { m = -1; p = 0.07f; }
                    myShift = m * (value - m * p); manualyShift = true; }
            }
            public static bool manualIPD = false;
            public static float mIPD = 0.063f;
            public static float IPD
            {
                get { if (manualIPD) { return mIPD; } return 0.063f; }
                set { mIPD = value; manualIPD = true; }
            }

            public static bool manualILD = false;
            public static float mILD = Headset.lensCenterDistance;
            public static float ILD
            {
                get { if (manualILD) { return mILD; } return Headset.lensCenterDistance; }
                set { mILD = value; manualILD = true; }
            }

            public static bool manualEyeToLens = false;
            public static float mEyeToLens = Headset.lensCenterDistance;
            public static float EyeToLens
            {
                get { if (manualEyeToLens) { return mEyeToLens; } return Headset.eyeToLens; }
                set { mEyeToLens = value; manualEyeToLens = true; }
            }

            public static bool rotatedSensor = false;
            public static bool showCalibrate = false;

            static Calibration() {
                if (SerializeStatic.Load(typeof(Calibration), Application.persistentDataPath + "/AryzonCalibrationSettings.bfd"))
                {
                    AryzonSettings.Instance.Apply();
                }
            }
        }

        public void Initialize () {
            if (!Phone.aryzonCalibrated) {
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
            public static string name = "Aryzon v3";
            public static string url = "https://google.com/cardboard/cfg?p=CgZBcnl6b24SCUFyeXpvbiB2Mx3NzMw9JY_CdT0qEAAASEIAAEhCAABIQgAASEJYADWPwvU8OgiamZm-CtcjvFAAYAA";
            public static float xShift = 0f;
            public static float yShift = 0.03f;
            public static float distortion = 0f;
            public static float redShift = 1.01f;
            public static float greenShift = 1.02f;
            public static float blueShift = 1.04f;
            public static float lensToScreen = 0.063f;
            public static float eyeToLens = 0.11f;
            public static float focalLength = 0.08f;
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

        public static class Controller
        {
#if UNITY_IOS
            public static KeyMap Trigger = new KeyMap(KeyCode.H, KeyCode.G);
            public static KeyMap Up = new KeyMap(KeyCode.W, KeyCode.E);
            public static KeyMap Down = new KeyMap(KeyCode.X, KeyCode.Z);
            public static KeyMap Right = new KeyMap(KeyCode.D, KeyCode.C);
            public static KeyMap Left = new KeyMap(KeyCode.A, KeyCode.Q);
            public static KeyMap Menu = new KeyMap(KeyCode.O, KeyCode.G);
            public static KeyMap Exit = new KeyMap(KeyCode.L, KeyCode.V);
            public static KeyMap A = new KeyMap();
            public static KeyMap B = new KeyMap(KeyCode.U, KeyCode.F);
            public static KeyMap X = new KeyMap();
            public static KeyMap Y = new KeyMap(KeyCode.J, KeyCode.N);
#else
            public static KeyMap Trigger = new KeyMap(KeyCode.Return, KeyCode.Return);
            public static KeyMap Up = new KeyMap(KeyCode.UpArrow, KeyCode.UpArrow);
            public static KeyMap Down = new KeyMap(KeyCode.DownArrow, KeyCode.DownArrow);
            public static KeyMap Right = new KeyMap(KeyCode.RightArrow, KeyCode.RightArrow);
            public static KeyMap Left = new KeyMap(KeyCode.LeftArrow, KeyCode.LeftArrow);
            public static KeyMap Menu = new KeyMap();
            public static KeyMap Exit = new KeyMap(KeyCode.L, KeyCode.V);
            public static KeyMap A = new KeyMap();
            public static KeyMap B = new KeyMap(KeyCode.Joystick1Button1, KeyCode.Joystick1Button1);
            public static KeyMap X = new KeyMap();
            public static KeyMap Y = new KeyMap(KeyCode.Menu, KeyCode.Menu);
#endif
            static Controller()
            {
                bool ok = SerializeStatic.Load(typeof(Phone), Application.persistentDataPath + "/AryzonControllerSettings.bfd");
                if (ok)
                {
                    AryzonSettings.Instance.Apply();
                }
            }

            [Serializable]
            public struct KeyMap
            {
                public KeyCode Up;
                public KeyCode Down;

                public KeyMap(KeyCode Down, KeyCode Up)
                {
                    this.Down = Down;
                    this.Up = Up;
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
            

            return ok0 && ok1 && SaveCalibration() && SaveController();
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

        public bool SaveController()
        {
            string savePathController = Application.persistentDataPath + "/AryzonControllerSettings.bfd";
            if (File.Exists(savePathController))
            {
                File.Delete(savePathController);
            }

            return SerializeStatic.Save(typeof(Controller), savePathController);
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

                            AryzonSettings.Calibration.mEyeToLens = data.eyeToLens;
                            AryzonSettings.Calibration.manualEyeToLens = false;

                            AryzonSettings.Calibration.rotatedSensor = data.rotatedSensor;

                            AryzonSettings.Calibration.mIPD = data.IPD;
                            AryzonSettings.Calibration.manualIPD = false;

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
                        AryzonSettings.Headset.url = data.url;

                        if (aryzonManager && !aryzonManager.stereoscopicMode && aryzonManager.aryzonMode && AryzonSettings.Headset.landscapeLeft != data.landscapeLeft)
                        {
                            AryzonSettings.Headset.landscapeLeft = data.landscapeLeft;
                            aryzonManager.SetRotationLandscapeAndPortrait();
                        }

                        AryzonSettings.Calibration.mEyeToLens = data.eyeToLens;
                        AryzonSettings.Calibration.manualEyeToLens = false;

                        AryzonSettings.Calibration.mILD = data.lensCenterDistance;
                        AryzonSettings.Calibration.manualILD = false;

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
            catch (Exception e)
            {
                Debug.LogWarning("[Aryzon] " + e);
                return false;
            }
        }
    }

    [Serializable]
    class CalibrationData {
        public float xShift = 0f;
        public float yShift = -0.105f;
        public float IPD = 0.063f;
        public string headsetName = "Aryzon v3";
        public float xShiftLens = 0f;
        public float yShiftLens = 0f;
        public float distortion = 0f;
        public float redShift = 0f;
        public float greenShift = 0f;
        public float blueShift = 0f;
        public float lensToScreen = 0.063f;
        public float eyeToLens = 0.11f;
        public float focalLength = 0.08f;
        public float lensCenterDistance = 0.06f;
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
        public string name = "Aryzon v3";
        public string url = "https://google.com/cardboard/cfg?p=CgZBcnl6b24SCUFyeXpvbiB2Mx3NzMw9JY_CdT0qEAAASEIAAEhCAABIQgAASEJYADWPwvU8OgiamZm-CtcjvFAAYAA";
        public float xShift = 0f;
        public float yShift = 0.03f;
        public float distortion = -1f;
        public float redShift = 1.01f;
        public float greenShift = 1.02f;
        public float blueShift = 1.04f;
        public float lensToScreen = 0.0636f;
        public float eyeToLens = 0.11f;
        public float focalLength = 0.08f;
        public float lensCenterDistance = 0.06f;
        public float bottomToCenter = 0.105f;
        public float fovFactor = 1.0f;
        public float xRotation = 0f;
        public bool landscapeLeft = true;
    }
}

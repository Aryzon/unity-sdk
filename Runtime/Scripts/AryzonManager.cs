using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

using System.Runtime.InteropServices;

#if ARYZON_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
#endif

namespace Aryzon
{
	public class AryzonManager : MonoBehaviour
	{
#if UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern void setBrightnessToValue(float value);
		[DllImport("__Internal")]
		private static extern void setBrightnessToHighest();
		[DllImport("__Internal")]
		private static extern float getBrightness();
#endif

		public bool useARFoundation;
		public bool useOther;

		private bool reticleEnabled;

		public bool setAryzonModeOnStart;
		public bool aryzonMode;
		public bool stereoscopicMode;
		public bool blackBackgroundInStereoscopicMode = true;

		public TrackingEngine trackingEngine;

		[Serializable]
		public class ButtonClickedEvent : UnityEvent { }

		public ButtonClickedEvent onClick = new ButtonClickedEvent();

		public UnityEvent onARFoundationStart;
		public UnityEvent onARFoundationStop;
		public UnityEvent onOtherStart;
		public UnityEvent onOtherStop;
		public UnityEvent onOtherSet;
		public UnityEvent onStart;
		public UnityEvent onStop;
		public UnityEvent onRotationToLandscape;
		public UnityEvent onRotationToPortrait;

		public bool showARFoundationEvents;
		public bool showOtherEvents;
		public bool showAryzonModeEvents;
		public bool showRotationEvents;
		public bool startTrackingBool;
		public bool stopTrackingBool;
		public bool showReticle;
		public bool alwaysApplyPose;

		private AryzonPose _trackerPose;
		public AryzonPose trackerPose
        {
			get { return _trackerPose; }
			set
			{
				_trackerPose = value;
				if (PoseUpdate != null)
				{
					PoseUpdate(_trackerPose);
				}
			}
        }
#if UNITY_EDITOR
		public bool editorMovementsControls = true;
		EditorPoseProvider editorPoseProvider;
#endif

		public delegate void PoseUpdateEventHandler(AryzonPose pose);
		public event PoseUpdateEventHandler PoseUpdate;

		private Camera _XRCamera;
		public Camera XRCamera
		{
			get {
				if (!_XRCamera || !_XRCamera.enabled)
				{
					if (Camera.main)
					{
						XRCamera = Camera.main;
					}
					else if (Camera.allCamerasCount > 0)
					{
						foreach (Camera cam in Camera.allCameras)
						{
							if (cam.stereoTargetEye == StereoTargetEyeMask.Both)
							{
								XRCamera = cam;
								break;
							}
						}
					}
				}
				return _XRCamera;
			}
			set
			{
				_XRCamera = value;
				if (CameraUpdate != null)
				{
					CameraUpdate(_XRCamera);
				}
			}
		}

		public delegate void CameraUpdateEventHandler(Camera camera);
		public event CameraUpdateEventHandler CameraUpdate;

		private GameObject cameraObject;
		private AryzonMainPoseDriver aryzonMainPoseDriver = new AryzonMainPoseDriver();

		private float resetBrightness = 1f;
		private float timer = 0f;

		private bool objectsConnected = false;

		private bool checkingOrientation;
		private bool landscape;
		private bool landscapeMode;

		private bool autorotateToLandscapeRight;
		private bool autorotateToLandscapeLeft;
		private bool autorotateToPortrait;
		private bool autorotateToPortraitUpsideDown;
		private ScreenOrientation orientation;

		private int screenWidth = 0;
		private int sleepTimeOut;

		private AryzonCameraHandler _cameraHandler;
		private AryzonCameraHandler cameraHandler
		{
			get
			{
				if (_cameraHandler == null)
				{
					_cameraHandler = new AryzonCameraHandler();
				}
				return _cameraHandler;
			}
		}

		private void OnEnable()
		{
			screenWidth = Screen.width;
#if UNITY_EDITOR
			if (!editorPoseProvider && editorMovementsControls)
            {
				GameObject editorPoseProviderGO = new GameObject("EditorPoseProvider");
				editorPoseProvider = editorPoseProviderGO.AddComponent<EditorPoseProvider>();
				aryzonMainPoseDriver.editorPoseProviderTransform = editorPoseProvider.transform;
			}
#endif
		}

		private void ConnectObjects()
		{
			if (!objectsConnected)
			{
				reticleEnabled = false;
				objectsConnected = true;
			}
		}

		private void Awake()
		{
			AryzonSettings.Instance.aryzonManager = this;

			ConnectObjects();

			aryzonMode = false;
			landscapeMode = false;
			stereoscopicMode = false;

			showReticle = false;
		}

		private void Start()
		{
			if (setAryzonModeOnStart)
			{
				StartAryzonMode();
			}
		}

		void Update()
		{
			if (screenWidth != Screen.width)
            {
				CheckAspectRatio();
				screenWidth = Screen.width;
				AryzonCardboardSubsystemLoader.ReloadDeviceParams();

			}
			AryzonCardboardSubsystemLoader.UpdateCardboard();

#if UNITY_IOS
			if (stereoscopicMode && aryzonMode)
            {
				if (timer > 5f)
				{
					float randomFactor = UnityEngine.Random.Range(0.0f, 0.02f);
					SetScreenBrightness(0.93f + randomFactor);
					timer = 0f;
				}
				timer += Time.deltaTime;
			}
#endif

#if UNITY_EDITOR
			if (startTrackingBool)
			{
				startTrackingBool = false;
				StartAryzonMode();
			}
			if (stopTrackingBool)
			{
				stopTrackingBool = false;
				StopAryzonMode();
			}
#endif
	}

		private void LateUpdate()
		{
			if (!reticleEnabled && showReticle)
			{
				reticleEnabled = true;
			}
			else if (reticleEnabled && !showReticle)
			{
				reticleEnabled = false;
			}
			showReticle = false;
		}

		public void AddSixDoFData(Vector3 position, Quaternion rotation, long timestampNs)
        {
			aryzonMainPoseDriver.AddSixDoFData(position, rotation, timestampNs);
		}

		public void StartAryzonMode()
		{
			if (!aryzonMode)
			{
				AryzonSettings.Instance.AryzonMode = true;
				AryzonSettings.Instance.aryzonManager = this;

				if (!Application.isEditor)
				{
					SaveRotationParameters();
					SetRotationLandscapeAndPortrait();
				}
				aryzonMode = true;
				ConnectObjects();
				
				if (trackingEngine == TrackingEngine.ARFoundation)
				{
					onARFoundationStart.Invoke();
				}
				else if (trackingEngine == TrackingEngine.Other)
				{
					onOtherStart.Invoke();
				}

				CheckAspectRatio();

				onStart.Invoke();
				AryzonSettings.Instance.OnStartAryzonMode(new AryzonModeEventArgs());
			}
		}

		public void StopAryzonMode()
		{
			if (aryzonMode)
			{
				if (stereoscopicMode)
                {
					ExitStereoscopic();
                }

				AryzonSettings.Instance.AryzonMode = false;
				
				if (trackingEngine == TrackingEngine.ARFoundation)
				{
					onARFoundationStop.Invoke();
				}
				else if (trackingEngine == TrackingEngine.Other)
				{
					onOtherStop.Invoke();
				}
				if (landscape)
				{
					ResetScreenBrightness();
				}

				if (!Application.isEditor)
				{
					ResetRotationParameters();
				}
				onStop.Invoke();

				aryzonMode = false;
				AryzonSettings.Instance.OnStopAryzonMode(new AryzonModeEventArgs());
			}
		}

		private void SaveRotationParameters()
		{
			autorotateToLandscapeLeft = Screen.autorotateToLandscapeLeft;
			autorotateToLandscapeRight = Screen.autorotateToLandscapeRight;
			autorotateToPortrait = Screen.autorotateToPortrait;
			autorotateToPortraitUpsideDown = Screen.autorotateToPortraitUpsideDown;

			if (autorotateToLandscapeLeft || autorotateToLandscapeRight || autorotateToPortrait || autorotateToPortraitUpsideDown)
			{
				orientation = ScreenOrientation.AutoRotation;
			}
			else
			{
				orientation = Screen.orientation;
			}
		}

		private void ResetRotationParameters()
		{
			Screen.autorotateToLandscapeLeft = autorotateToLandscapeLeft;
			Screen.autorotateToLandscapeRight = autorotateToLandscapeRight;
			Screen.autorotateToPortrait = autorotateToPortrait;
			Screen.autorotateToPortraitUpsideDown = autorotateToPortraitUpsideDown;
			Screen.orientation = orientation;
		}

		public void SetRotationLandscapeAndPortrait()
		{
			Screen.autorotateToLandscapeRight = !AryzonSettings.Headset.landscapeLeft;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = AryzonSettings.Headset.landscapeLeft;
			Screen.autorotateToPortrait = true;
			Screen.orientation = ScreenOrientation.AutoRotation;
		}

		private void SetScreenBrightness(float value)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
#if UNITY_ANDROID
				AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");
				
				ajo.Call("runOnUiThread", new AndroidJavaRunnable(() => { 
					using (
					AndroidJavaObject windowManagerInstance = ajo.Call<AndroidJavaObject>("getWindowManager"),
					windowInstance = ajo.Call<AndroidJavaObject>("getWindow"),
					layoutParams = windowInstance.Call<AndroidJavaObject>("getAttributes")
					) {
						layoutParams.Set<float>("screenBrightness",value);
						windowInstance.Call("setAttributes", layoutParams);
					}
				}));
#endif
			}
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
#if UNITY_IPHONE
				resetBrightness = getBrightness();
				setBrightnessToValue(value);
#endif
			}
		}

		private void ResetScreenBrightness()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
#if UNITY_ANDROID
				AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
				AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");
				ajo.Call("runOnUiThread", new AndroidJavaRunnable(() => { 
					using (
					AndroidJavaObject windowManagerInstance = ajo.Call<AndroidJavaObject>("getWindowManager"),
					windowInstance = ajo.Call<AndroidJavaObject>("getWindow"),
					layoutParams = windowInstance.Call<AndroidJavaObject>("getAttributes")
					) {
						layoutParams.Set<float>("screenBrightness",-1f);
						windowInstance.Call("setAttributes", layoutParams);
					}
				}));
#endif
			}
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
#if UNITY_IPHONE
				setBrightnessToValue(resetBrightness);
#endif
			}
		}

		private void CheckAspectRatio()
		{
			if (!checkingOrientation && this.gameObject.activeInHierarchy && aryzonMode)
			{
				checkingOrientation = true;

				landscape = false;
				if (Screen.width > Screen.height)
				{
					landscape = true;
				}
				if (landscape && !landscapeMode)
				{
					EnterStereoscopic();
				}
				else if (!landscape && landscapeMode)
				{
					ExitStereoscopic();
				}
				StartCoroutine(DoNotCheckOrientationForSeconds(0.5f));
			}
		}

		private void EnterStereoscopic()
		{
			Debug.Log("[Aryzon] Entering stereoscopic mode");

			AryzonCardboardSubsystemLoader.StartCardboard();
			Application.targetFrameRate = 60;
			sleepTimeOut = Screen.sleepTimeout;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			SetScreenBrightness(0.95f); //slightly less then 1 to reduce heat
#if UNITY_IOS && !UNITY_EDITOR
			timer = 0f;
#endif

			onRotationToLandscape.Invoke();
			landscapeMode = true;

			AryzonSettings.Instance.LandscapeMode = true;
			stereoscopicMode = true;
			cameraHandler.UpdateCamerasForStereoscopicMode();
			aryzonMainPoseDriver.Enable();
			AryzonSettings.Instance.OnStartStereoscopicMode(new AryzonModeEventArgs());
		}

		private void ExitStereoscopic()
		{
			Debug.Log("[Aryzon] Exiting stereoscopic mode");
			Screen.sleepTimeout = sleepTimeOut;
			ResetScreenBrightness();

			onRotationToPortrait.Invoke();
			landscapeMode = false;

			AryzonSettings.Instance.PortraitMode = true;

			AryzonCardboardSubsystemLoader.StopCardboard();
			cameraHandler.ResetCameras();
			aryzonMainPoseDriver.Disable();

			stereoscopicMode = false;
			AryzonSettings.Instance.OnStopStereoscopicMode(new AryzonModeEventArgs());
		}

		IEnumerator DoNotCheckOrientationForSeconds(float seconds)
		{
			yield return new WaitForSeconds(seconds);
			checkingOrientation = false;
		}
	}

	public struct AryzonPose
	{
		public Vector3 position;
		public Quaternion rotation;
	}

	public enum TrackingEngine
	{
		Other,
		ARFoundation,
	}
}
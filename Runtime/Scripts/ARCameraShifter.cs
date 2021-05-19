using UnityEngine;
using UnityEngine.EventSystems;

//Do not edit this class
namespace Aryzon {

	public class ARCameraShifter {

		public bool singleMode = false;

		public GameObject cameras;
		public Camera left;
		public Camera right;

		public void Setup () {
			AryzonSettings.Instance.Initialize ();
		}

		public void UpdateLayout () {

			if (!singleMode) {
				left.transform.localPosition = new Vector3 (-(AryzonSettings.Calibration.IPD / 2), 0f,  -AryzonSettings.Headset.eyeToLens);
				right.transform.localPosition = new Vector3 ((AryzonSettings.Calibration.IPD / 2), 0f,  -AryzonSettings.Headset.eyeToLens);
			} else {
				right.transform.localPosition = new Vector3 (0f, 0f, -AryzonSettings.Headset.eyeToLens);
			}

			float xShift = AryzonSettings.Calibration.XShift;
			float yShift = AryzonSettings.Calibration.YShift;

			if (!AryzonSettings.Headset.landscapeLeft)
			{
				xShift = -xShift;
				yShift = -yShift;
			}
			yShift += AryzonSettings.Headset.bottomToCenter;

			if (AryzonSettings.Calibration.rotatedSensor)
			{
				xShift = -xShift;
				yShift = -yShift;
			}

			cameras.transform.localPosition = new Vector3(xShift, yShift, 0f);

			right.ResetProjectionMatrix ();
			left.ResetProjectionMatrix ();

			updateCam (right);
			updateCam (left);
            updateCam (right);
		}

		void updateCam(Camera cam) {

			bool isLeft = false;
			if (cam == left) {
				isLeft = true;
			}

			if (singleMode) {
				left.enabled = false;
				Rect rect = new Rect ();
				rect.width = 1f;
				rect.height = 1f;
				rect.center = new Vector2 (0.5f, 0.5f);
				right.rect = rect;
				right.fieldOfView = 32f;

				if (AryzonSettings.Calibration.rotatedSensor) {
					right.transform.localEulerAngles = new Vector3 (0f,0f,180f);
				}

				if (!isLeft) {
					cam.ResetProjectionMatrix();
				}
			} else {
				left.enabled = true;
				Rect rect = new Rect ();
				rect.width = 0.5f;
				rect.height = 1f;
				rect.center = new Vector2 (0.25f, 0.5f);
				left.rect = rect;
				rect.center = new Vector2 (0.75f, 0.5f);
				right.rect = rect;

				float b = -1f / (1f / AryzonSettings.Headset.focalLength - 1f / AryzonSettings.Headset.lensToScreen);
				float virtualPlaneDistance = b + AryzonSettings.Headset.eyeToLens;

				float m = b / AryzonSettings.Headset.lensToScreen;
				float heightFactor = Screen.height / (float)Screen.width;
				if (Screen.height > Screen.width) {
					heightFactor = 1f / heightFactor;
				}

				float mScreenWidth = mWidth ();
				float mScreenHeight = mScreenWidth * heightFactor;
				//float fov = AryzonSettings.Headset.fovFactor * Mathf.Rad2Deg * 2 * Mathf.Atan (m * (mScreenHeight / 2f) / (virtualPlaneDistance));
                float fov = Mathf.Rad2Deg * 2 * Mathf.Atan (m * (mScreenHeight / 2f) / (virtualPlaneDistance));
				float s1 = -AryzonSettings.Headset.lensCenterDistance / 2f;
				float s2 = mScreenWidth / 2f + s1;
				float s1a = m * s1;
				float s2a = m * s2;

				float center = s1a + (s2a - s1a) / 2f - s1;

				float factorX = 2 * (AryzonSettings.Calibration.IPD / 2f - center) / (s2a - s1a);
				float factorY = 0.5f - AryzonSettings.Headset.yShift / mScreenHeight;

				cam.ResetProjectionMatrix ();

				Matrix4x4 mat = cam.projectionMatrix;

				if (!isLeft) {
					factorX = -factorX;
				}

				mat[0, 2] = factorX;
				mat[1, 2] = factorY;

				cam.projectionMatrix = mat;

				left.fieldOfView = fov;
				right.fieldOfView = fov;

				ARLensCorrection lensCorrection;
				float multiplier = 1f;
				Transform camTransform;

				if (isLeft) {
					multiplier = -1f;
					lensCorrection = left.gameObject.GetComponent<ARLensCorrection> ();
					camTransform = left.transform;
				} else {
					lensCorrection = right.gameObject.GetComponent<ARLensCorrection> ();
					camTransform = right.transform;

				}
				if (AryzonSettings.Headset.distortion < 0)
				{
					lensCorrection.enabled = false;
				}
				else
				{
					lensCorrection.enabled = true;
					lensCorrection.setShift(new Vector2(-multiplier * (AryzonSettings.Headset.lensCenterDistance / mScreenWidth - 0.5f), factorY));
					lensCorrection.setDistortion(AryzonSettings.Headset.distortion);
					lensCorrection.setColorCorrection(new Vector3(AryzonSettings.Headset.redShift, AryzonSettings.Headset.greenShift, AryzonSettings.Headset.blueShift));
				}
				

				if (AryzonSettings.Calibration.rotatedSensor) {
					camTransform.localPosition = new Vector3 (multiplier * ( (AryzonSettings.Calibration.IPD) / 2), 0f,  -AryzonSettings.Headset.eyeToLens);
					camTransform.localEulerAngles = new Vector3 (AryzonSettings.Headset.xRotation, 0f, 180f);
				} else {
					camTransform.localPosition = new Vector3 (multiplier * ( (AryzonSettings.Calibration.IPD) / 2), 0f,  -AryzonSettings.Headset.eyeToLens);
					camTransform.localEulerAngles = new Vector3(AryzonSettings.Headset.xRotation, 0f, 0f);
				}
			}
		}

		private float mWidth() {

			if ((AryzonSettings.Phone.aryzonCalibrated || AryzonSettings.Phone.manualScreenWidth) && !Application.isEditor) {
				return AryzonSettings.Phone.ScreenWidth;
			}

			float pixelsPerInch = 1f;
			int pixelWidth = 1;
			int pixelHeight = 1;

			if (Application.platform == RuntimePlatform.Android) {
				pixelsPerInch = ScreenDimensionsAndroid.dpiY;
				pixelWidth = ScreenDimensionsAndroid.pixelsX;
				pixelHeight = ScreenDimensionsAndroid.pixelsY;
				if (pixelHeight > pixelWidth) {
					pixelWidth = pixelHeight;
				}

			} else {
				float dpi = Screen.dpi;
				float screenWidth = Screen.currentResolution.width;

				if (Screen.currentResolution.height > screenWidth) {
					screenWidth = Screen.currentResolution.height;
				}

				float maxResWidth = screenWidth;
				foreach (Resolution res in Screen.resolutions)
				{
					if (res.width > maxResWidth) {
						maxResWidth = res.width;
					}
				}

				float screenScale = screenWidth / maxResWidth;

				pixelsPerInch = dpi / screenScale;

				if (Application.platform == RuntimePlatform.OSXEditor && screenScale < 0.99f) {
					pixelWidth = (int)((0.5f / screenScale) * Screen.width);
				} else {
					pixelWidth = (int)(screenScale * Screen.width);
				}
			}
			float w = (pixelWidth / pixelsPerInch) * 0.0254f;
			AryzonSettings.Phone.screenWidth = w;
			return w;
		}

		public void ToggleSingleMode () {

			singleMode = !singleMode;

			updateCam (left);
			updateCam (right);
		}

		public void UpdateView () {
			updateCam (left);
			updateCam (right);
		}

		public interface ICustomMessageTarget : IEventSystemHandler
		{
			void UpdateLayout ();
		}
	}
}
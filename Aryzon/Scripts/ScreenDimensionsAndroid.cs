using UnityEngine;

public class ScreenDimensionsAndroid {

	//public static float dpiX { get; protected set; }
	public static float dpiY { get; protected set; }
	public static int pixelsX { get; protected set; }
	public static int pixelsY { get; protected set; }

	static ScreenDimensionsAndroid() {

		if (Application.platform != RuntimePlatform.Android) {
			return;
		}

		using (
			AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"),
			metricsClass = new AndroidJavaClass("android.util.DisplayMetrics")
		) {
			using (
				AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
				realMetricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
				activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
				windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
				displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
			) {
				displayInstance.Call("getMetrics", metricsInstance);
				displayInstance.Call("getRealMetrics", realMetricsInstance);

				dpiY = metricsInstance.Get<float>("ydpi");

				pixelsX = realMetricsInstance.Get<int>("widthPixels");
				pixelsY = realMetricsInstance.Get<int>("heightPixels");

				float dpi = realMetricsInstance.Get<float>("ydpi");
				int densityDPI = metricsInstance.Get<int> ("densityDpi");
				int realDensityDPI = realMetricsInstance.Get<int> ("densityDpi");


				if ((densityDPI != realDensityDPI) && ((int)dpi == realDensityDPI)) {
					dpiY = densityDPI;
				}
			}
		}
	}
}
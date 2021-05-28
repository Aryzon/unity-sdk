# Aryzon Unity SDK
## Requirements
* Unity 2019.3 or newer
* For 6DoF an [ARKit capable](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html) or [ARCore capable](https://developers.google.com/ar/devices) phone.

## Getting started
1. In the Unity package manager choose to add a package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/cardboard-xr-plugin.git`<br>*You do not have to import the samples.*
2. a. In the Unity package manager choose to add another package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/unity-sdk.git`.
b. Import the samples from this package. (Unfold **Samples -> Import**).

#### Android Specific:
Follow the official Google Cardboard SDK steps under [**Other Settings**](https://developers.google.com/cardboard/develop/unity/quickstart#other_settings) and [**Publishing Settings**](https://developers.google.com/cardboard/develop/unity/quickstart#publishing_settings). **DO NOT follow any other instructions that are there.**

### 3DoF tracking
Use 3 degrees of freedom tracking when the user position is steady and the user cannot walk around. Follow these instructions to get rotational tracking only:

3. In **Player Settings -> Other Settings** add this to the **Camera Usage Description**:<br>`Cardboard SDK requires camera permission to read the QR code (required to get the encoded device parameters).`
4. Open the Rotational Tracking scene in Assets -> Samples.
5. You can now build and run this scene.

### 6DoF Room scale tracking
Use 6 degrees of freedom tracking when the user should be able to walk around. Follow these steps to get 6DoF tracking:

3. Add **ARFoundation** through the package manager from the **Unity Registry**.
4. In the player settings head over to XR Plugin Management, select ARKit or ARCore according to your target platforms. This should automatically install the package from the package manager for you.<br>**Important: DO NOT select Cardboard**.
5. In **Player Settings -> Other Settings** add this to the **Camera Usage Description**:<br>`Camera usage is required for AR and the Cardboard SDK requires camera permission to read a QR code (required to get the encoded device parameters).`
6. Open the ARFoundation Tracking scene in Assets -> Samples.
7. You can now build and run this scene.

Note: ARSession has an option 'Match Frame Rate', by default this option is selected, however it should not be selected when building for Aryzon headsets. Most Android phones run ARCore at only 30 fps. Selecting this option causes the stereoscopic view to run at 30 fps as well. Since rotation is updated at 60 fps (or even more) we should disable this option.

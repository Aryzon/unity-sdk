# Aryzon Unity SDK
## Requirements
1. Unity 2019.3 or newer
2. 

## Getting started
1. In the Unity package manager choose to add a package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/cardboard-xr-plugin.git`<br>*You do not have to import the samples.*
2. In the Unity package manager choose to add another package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/unity-sdk.git`<br>
3. Import the samples from this package. (Unfold Samples -> Import).

#### Android Specific
Follow the official Google Cardboard SDK steps under [**Other Settings**](https://developers.google.com/cardboard/develop/unity/quickstart#other_settings) and [**Publishing Settings**](https://developers.google.com/cardboard/develop/unity/quickstart#publishing_settings). **DO NOT follow any other instructions that are there.**

### 3DoF tracking
Use 3 degrees of freedom tracking when the user position is steady and the user cannot walk around. Follow these instructions to get rotational tracking only:

4. In **Player Settings -> Other Settings** add this to the **Camera Usage Description**:<br>`Cardboard SDK requires camera permission to read the QR code (required to get the encoded device parameters).`
5. Open the Rotational Tracking scene in Assets -> Samples.
6. You can now build and run this scene.

### 6DoF Room scale tracking
Use 6 degrees of freedom tracking when the user should be able to walk around. Follow these steps to get 6DoF tracking:

4. Add ARFoundation through the package manager.
5. In the player settings head over to XR Plugin Management, select ARKit or ARCore according to your target platforms.<br>**Important: deselect Cardboard**. This will automatically install the package from the package manager for you.
6. In **Player Settings -> Other Settings** add this to the **Camera Usage Description**:<br>`Camera usage is required for AR and the Cardboard SDK requires camera permission to read a QR code (required to get the encoded device parameters).`
7. Open the ARFoundation Tracking scene in Assets -> Samples.
8. You can now build and run this scene.

Note: ARSession has an option 'Match Frame Rate', by default this option is selected, however it should not be selected when building for Aryzon headsets. Most Android phones run ARCore at only 30 fps. Selecting this option causes the stereoscopic view to run at 30 fps as well. Since rotation is updated at 60 fps (or even more) we should disable this option.

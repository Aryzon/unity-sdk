# Aryzon Unity SDK
## Requirements
* Unity 2019.3 or newer
* For 6DoF to function you will need an [ARKit capable](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html) or [ARCore capable](https://developers.google.com/ar/devices) phone.
* An MR headset like one of our [Aryzon Headsets](https://www.aryzon.com).<br>Note this SDK will also function with any VR Google Cardboard headset, however for 6DoF to work your phone's camera will need to be able to 'see' the environment. Many headsets block the camera.

## Getting started
Start by opening a new Unity project.
### Add the packages
Navigate to **Window -> Package Manager**.
1. Choose to add the **Google Cardboard XR Plugin with 6DoF support** package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/cardboard-xr-plugin.git`<br>*This is based on [our fork](https://github.com/Aryzon/cardboard) from the [Google Cardboard SDK](https://github.com/googlevr/cardboard). You do not have to import the samples from this package.*
2. Add the **Aryzon MR Headset Unity SDK** package from this Git URL:<br>`https://github.com/Aryzon/unity-sdk.git`
3. Import the samples from the **Aryzon MR Headset Unity SDK** package. (Unfold **Samples -> Import**).

#### Android only:
Navigate to **Project Settings > Player > Other Settings**.
* Choose OpenGLES2, or OpenGLES3, or both in Graphics APIs.
* Select IL2CPP in Scripting Backend.
* Select desired architectures by choosing ARMv7, ARM64, or both in Target Architectures.
* Select Require in Internet Access.

Navigate to **Project Settings > Player > Publishing Settings**.
* In the Build section, select Custom Main Gradle Template
* Add the following lines to the dependencies section of Assets/Plugins/Android/mainTemplate.gradle:
**
```
implementation 'com.android.support:appcompat-v7:28.0.0'
implementation 'com.android.support:support-v4:28.0.0'
implementation 'com.google.android.gms:play-services-vision:15.0.2'
implementation 'com.google.protobuf:protobuf-javalite:3.8.0'
```

### Choice of tracking
* Use 3 degrees of freedom tracking when the user position is steady and the user cannot walk around. Follow the 3DoF instructions to get rotational tracking only.
* Use 6 degrees of freedom tracking when the user should be able to walk around. Follow the 6DoF steps to get rotational and positional tracking.

### 3DoF tracking
4. In **Player Settings -> Other Settings** add this to the **Camera Usage Description**:<br>`Cardboard SDK requires camera permission to read the QR code (required to get the encoded device parameters).`
5. Open the Rotational Tracking scene in **Assets -> Samples**.
6. You can now build and run this scene.

### 6DoF tracking
4. Add **ARFoundation** through the package manager from the **Unity Registry**. Make sure to use ARFoundation 3.0 or newer otherwise the ARPoseDriver cannot be found.
5. In the player settings head over to XR Plugin Management, select ARKit or ARCore according to your target platforms. This should automatically install the package from the package manager for you.<br>**Important: DO NOT select Cardboard**.
6. In **Player Settings -> Other Settings** add this to the **Camera Usage Description**:<br>`Camera usage is required for AR and the Cardboard SDK requires camera permission to read a QR code (required to get the encoded device parameters).`
7. Open the ARFoundation Tracking scene in **Assets -> Samples**.
8. You can now build and run this scene.

## Adding Aryzon support to an existing scene
You can simply add Aryzon support to an existing ARFoundation scene like [one of the official samples](https://github.com/Unity-Technologies/arfoundation-samples) or any other scene. Do the following:
* Add the **Aryzon** and **AryzonInputController** prefabs to the scene from **Packages -> Aryzon MR Headset Plugin -> Runtime -> Prefabs**.
* Make sure there is an **EventSystem** in the scene.
* ARFoundation only: on the **Aryzon** GameObject set 'ARFoundation' as the tracking engine.
* ARFoundation only: on the **ARSession** GameObject deselect 'Match Frame Rate'.

Note: **ARSession** has an option 'Match Frame Rate', by default this option is selected, however it should not be selected when building for [Aryzon headsets](https://www.aryzon.com). Most Android phones run ARCore at only 30 fps. Selecting this option causes the stereoscopic view to run at 30 fps as well. Since rotation is updated at 60 fps (or even more) we should disable this option.

## Adding Aryzon support to an MRTK project
Yes that's right, you can add Aryzon support to a Microsoft Mixed Reality Toolkit project! This gives you access to all the great stuff the Microsoft Mixed Reality Toolkit has to offer and turns your phone into a HoloLens (well almost).

1. Start out with a MRTK scene like one of the Examples. For instance use the HandInteractionExample scene from the MRTK Examples package.
2. Convert the MRTK project so it runs on your phone in 2D by following [the steps in this link](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/CrossPlatform/UsingARFoundation.html).
3. Set the update type of the UnityARCameraSettingsProfile you just added to 'Before Render'.
4. Add the Aryzon prefab  from **Packages -> Aryzon MR Headset Plugin -> Runtime -> Prefabs**
5. Add an AR Session GameObject to the scene. **GameObject -> XR -> ARSession**
6. Deselect 'Match Frame Rate' on the AR Session GameObject.

### Wireless Controller
Use our [wireless controller](https://www.aryzon.com/aryzon-wireless-controller) to provide input to the MRTK input system. This gives 'Tap' functionality similar to the HoloLens 1. With this you are able click buttons, move/scale/rotate objects and more. Import [this package](https://github.com/Aryzon/unity-sdk/raw/main/Extras/AryzonMRTK.unitypackage) and add the AryzonMRTKController prefab to the scene.

## Wireless Controller Support
If you have a bluetooth controller or the [Aryzon Controller](https://www.aryzon.com/aryzon-wireless-controller) you can add this in Aryzon mode. Make sure you have the AryzonInputController or AryzonMRTKController in your scene:

1. Enter Aryzon mode in the app select Settings.
2. Tap on ‘Listen’
3. Click the button you want to use on the controller as a ‘reticle click’.

Some bluetooth controllers require to be set in a different mode in order to be recognised. Please refer to the devices manual on how to do that.

## Calibration
Mixed reality usually requires some form of interaction or alignment with the physical world. In order to get good optical results the SDK has a **calibration menu** to align the virtual and physical world. You can enable this at runtime on the phone in Aryzon Mode. Go to **Settings** and select **Show calibration menu**.

### The calibration settings
**X:** Horizontal shift<br>
**Y:** Vertical Shift<br>
**Z:** Forward Shift<br>
The settings above move the position of the virtual camera relative to the user. When changing these settings you will notice movement of virtual objects that are close by. Objects that are far away do not move as much.

**IPD:** Interpupillary distance<br>
This setting is user dependent, it is the distance between the pupils in the eyes of the user. The virtual camera renders two images from slightly different positions, depending on the IPD. Changing this setting has most effect on objects that are close by. It makes these objects appear closer by or farther away.

**ILD:** Inter lens distance<br>
This setting changes how far apart the images are rendered on screen. It is different from IPD since this effects the entire image, not just the objects that are close by. It makes all objects appear closer by or farther away.

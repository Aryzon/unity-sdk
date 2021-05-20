# Aryzon Unity SDK
## Getting started
1. In the Unity package manager choose to add a package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/cardboard-xr-plugin.git`<br>*You do not have to import the samples.*
2. For Android [follow the instructions here](https://developers.google.com/cardboard/develop/unity/quickstart#player_settings)<br>**Important: skip the Resolution and Presentation step. Choosing Autorotate or Portrait is fine.**
3. For iOS [follow the instructions here](https://developers.google.com/cardboard/develop/unity/quickstart#player_settings_2)
4. In the Unity package manager choose to add another package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/unity-sdk.git`
5. Also import the samples from this pacakage.

### 3DoF tracking
Use this type of tracking when the user position is steady and cannot walk around. This is rotational tracking only.

6. In the player settings head over to XR Plugin Management, select Cardboard for your target platforms.
7. Find the Rotational Tracking scene in Assets -> Samples.
8. You can now build and run this scene.

### 6DoF Room scale tracking
Use this type of tracking when the user should be able to walk around. This is rotational and positional tracking.

6. Add ARFoundation through the package manager.
7. For Android add ARCore and for iOS add ARKit from the package manager.
9. In the player settings head over to XR Plugin Management, select ARKit or ARCore according to your target platforms.<br>**Important: deselect Cardboard**.
10. Find the ARFoundation Tracking scene in Assets -> Samples.
11. You can now build and run this scene.

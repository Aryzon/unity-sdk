# Aryzon Unity SDK
## Getting started
1. In the Unity package manager choose to add a package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/cardboard-xr-plugin.git`<br>*You do not have to import the samples.*
2. For Android [follow the instructions here](https://developers.google.com/cardboard/develop/unity/quickstart#player_settings), for iOS [follow the instructions here](https://developers.google.com/cardboard/develop/unity/quickstart#player_settings_2)<br>**Important: skip the Resolution and Presentation step. Choosing Autorotate or Portrait is fine.**
3. In the Unity package manager choose to add another package from a Git URL. Enter this url:<br>`https://github.com/Aryzon/unity-sdk.git`
4. Also import the samples from this pacakage.

### 3DoF tracking
Use 3 degrees of freedom tracking when the user position is steady and the user cannot walk around. Follow these instructions to get rotational tracking only:

5. In the player settings head over to XR Plugin Management, select Cardboard for your target platforms.
6. Find the Rotational Tracking scene in Assets -> Samples.
7. You can now build and run this scene.

### 6DoF Room scale tracking
Use 6 degrees of freedom tracking when the user should be able to walk around. Follow these steps to get 6DoF tracking:

5. Add ARFoundation through the package manager.
6. For Android add ARCore and for iOS add ARKit from the package manager.
7. In the player settings head over to XR Plugin Management, select ARKit or ARCore according to your target platforms.<br>**Important: deselect Cardboard**.
8. Find the ARFoundation Tracking scene in Assets -> Samples.
9. You can now build and run this scene.

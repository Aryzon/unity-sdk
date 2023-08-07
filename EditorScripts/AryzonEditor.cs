using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Events;

using UnityEditor;
using UnityEditor.Events;

namespace Aryzon.EditorUI {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(AryzonManager))]
	public class AryzonEditor : Editor
	{
		static List<BuildTargetGroup> ARFoundationBuildTargetList = new List<BuildTargetGroup>() { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.WSA, BuildTargetGroup.Standalone };

		static readonly string useARFoundationSymbol = "ARYZON_ARFOUNDATION";

		AryzonManager aryzonManager;

		SerializedProperty trackingEngineProp;

        SerializedProperty addEditorMovementControlsProp;

        SerializedProperty showAryzonButtonProp;
        SerializedProperty setAryzonModeOnStartProp;
		SerializedProperty blackBackgroundProp;

		SerializedProperty onStartProp;
		SerializedProperty onStopProp;
		SerializedProperty onRotationToPortraitProp;
		SerializedProperty onRotationToLandscapeProp;

		SerializedProperty m_OnClickProperty;

		float logoHeight = 25f;
		float logoWidth = 155f;
		float fieldHeight = 16f;
		float buttonHeight = 18f;

		private int resetState = -1;

		Texture logo;

		static object gameViewSizesInstance;
		static MethodInfo getGroup;

		[InitializeOnLoadMethod]
		static void Initialize()
		{
			UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnEditorSceneManagerSceneOpened;
		}

		static void OnEditorSceneManagerSceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
		{
			AryzonManager aryzonManager = null;
			foreach (GameObject obj in scene.GetRootGameObjects())
            {
				aryzonManager = obj.GetComponentInChildren<AryzonManager>(false);
				if (aryzonManager)
                {
					break;
				}	
            }
			if (aryzonManager)
            {
				SetSymbols(aryzonManager.trackingEngine == TrackingEngine.ARFoundation);
            }
		}

		void OnEnable()
		{
			aryzonManager = (AryzonManager)target;
			logo = Resources.Load("LogoAryzonSidebar") as Texture;

			trackingEngineProp = serializedObject.FindProperty("trackingEngine");

            showStartButtonProp = serializedObject.FindProperty("showStartButton");
            setAryzonModeOnStartProp = serializedObject.FindProperty("setAryzonModeOnStart");
            addEditorMovementControlsProp = serializedObject.FindProperty("editorMovementsControls");
            blackBackgroundProp = serializedObject.FindProperty("blackBackgroundInStereoscopicMode");

			m_OnClickProperty = serializedObject.FindProperty("onClick");

			onStartProp = serializedObject.FindProperty("onStart");
			onStopProp = serializedObject.FindProperty("onStop");
			onRotationToPortraitProp = serializedObject.FindProperty("onRotationToPortrait");
			onRotationToLandscapeProp = serializedObject.FindProperty("onRotationToLandscape");

			EditorApplication.playModeStateChanged += ResetView;

			initializeGameView();
		}

		BuildTargetGroup CurrentGroup()
		{

#if UNITY_IOS
			return BuildTargetGroup.iOS;
#elif UNITY_ANDROID
        return BuildTargetGroup.Android;
#else
        return BuildTargetGroup.Standalone;
#endif
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			Rect controlRect = EditorGUILayout.GetControlRect(false, logoHeight);

			GUI.DrawTexture(new Rect(controlRect.x, controlRect.y, logoWidth, controlRect.height), logo, ScaleMode.ScaleToFit, true);

			EditorGUILayout.Space();
			GUIStyle boldWrap = EditorStyles.boldLabel;
			boldWrap.wordWrap = true;

			GUIStyle miniWrap = EditorStyles.miniLabel;
			miniWrap.wordWrap = true;
			miniWrap.fontStyle = FontStyle.Normal;

			EditorGUILayout.LabelField("Tracking engine", EditorStyles.boldLabel);
			GUI.enabled = !EditorApplication.isPlaying && !EditorApplication.isCompiling;

			TrackingEngine trackingEngine = (TrackingEngine)EditorGUILayout.EnumPopup(aryzonManager.trackingEngine, GUILayout.Height(fieldHeight));
			aryzonManager.trackingEngine = trackingEngine;
			int selected = (int)trackingEngine;

			GUI.enabled = true;

			if (selected != trackingEngineProp.enumValueIndex)
            {
				trackingEngineProp.enumValueIndex = selected;
				EditorCoroutines.StartCoroutine(setSymbolsAfter(0.2f, selected == 1), this);
			}

			if (selected == 0)
            {
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("For 6DoF to work you must call method 'AddSixDoFData(..)' of AryzonManager every time your data is updated.", miniWrap);
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(setAryzonModeOnStartProp, new GUIContent("Set Aryzon mode on Start"), GUILayout.Height(fieldHeight));
            EditorGUILayout.PropertyField(showStartButtonProp, new GUIContent("Show the Start Aryzon button"), GUILayout.Height(fieldHeight));
            EditorGUILayout.PropertyField(blackBackgroundProp, new GUIContent("Auto setup cameras for AR/MR", "This will disable the camera feed in stereoscopic mode (ARFoundation only) and set its Clear Flags to Solid Color black (any tracking engine). This is recommended for MR use, for VR you may want to render the Sky Box, in that case you will need to disable the camera feed manually."), GUILayout.Height(fieldHeight));
            EditorGUILayout.PropertyField(addEditorMovementControlsProp, new GUIContent("Add movement controls (editor only)"), GUILayout.Height(fieldHeight));
            EditorGUILayout.Space();
			EditorGUILayout.Space();

			aryzonManager.showAryzonModeEvents = EditorGUILayout.Foldout(EditorPrefs.GetBool("showAryzonModeEvents", false), "Aryzon mode events");
			EditorPrefs.SetBool("showAryzonModeEvents", aryzonManager.showAryzonModeEvents);
			if (aryzonManager.showAryzonModeEvents)
			{
				EditorGUILayout.PropertyField(onStartProp, new GUIContent("On start Aryzon mode"));
				EditorGUILayout.PropertyField(onStopProp, new GUIContent("On stop Aryzon mode"));
			}
			
			aryzonManager.showRotationEvents = EditorGUILayout.Foldout(EditorPrefs.GetBool("showRotationEvents", false), "Rotation events");
			EditorPrefs.SetBool("showRotationEvents", aryzonManager.showRotationEvents);
			if (aryzonManager.showRotationEvents)
			{
				EditorGUILayout.PropertyField(onRotationToPortraitProp, new GUIContent("On rotation to portrait"));
				EditorGUILayout.PropertyField(onRotationToLandscapeProp, new GUIContent("On rotation to landscape"));
			}

			
			EditorGUILayout.LabelField("Tip: simulate phone rotation by setting the aspect ratio of the Game screen to Tall or Wide", miniWrap);

			
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			Rect flexRect = GUILayoutUtility.GetLastRect();

			float inspectorWidth = flexRect.width;
			float spaceBetween = 4f;
			float buttonWidth = (inspectorWidth - spaceBetween) / 2f;

			EditorGUILayout.BeginHorizontal();

			if (!aryzonManager.aryzonMode && Application.isPlaying)
			{
				GUI.enabled = true;
			}
			else
			{
				GUI.enabled = false;
			}

			if (GUI.Button(new Rect(flexRect.x, flexRect.y, buttonWidth, buttonHeight), "Start"))
			{
				aryzonManager.startTrackingBool = true;
			}

			if (aryzonManager.aryzonMode && Application.isPlaying && !aryzonManager.stereoscopicMode)
			{
				GUI.enabled = true;
			}
			else
			{
				GUI.enabled = false;
			}

			if (GUI.Button(new Rect(flexRect.x + buttonWidth + spaceBetween, flexRect.y, buttonWidth, buttonHeight), "Stop"))
			{
				aryzonManager.stopTrackingBool = true;
			}

			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
			{
				EditorGUILayout.BeginHorizontal();

				GameViewSizeGroupType sizeGroupType = GameViewSizeGroupType.iOS;

				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
				{
					sizeGroupType = GameViewSizeGroupType.Android;
				}

				int mode = -1;
				int sizeIndex = GetSizeIndex();
				string inverse = "Free Aspect";
				if (sizeIndex != -1)
				{
					inverse = GetInverseWindowSizeString(sizeGroupType, sizeIndex, out mode);
				}
				if (mode == 0 || mode == -1)
				{
					GUI.enabled = true;
				}
				else
				{
					GUI.enabled = false;
				}

				if (GUI.Button(new Rect(flexRect.x, flexRect.y + 24, buttonWidth, buttonHeight), "Portrait"))
				{
					if (inverse == "Free Aspect")
					{
						SetSize(FindSize(sizeGroupType, ":16)"));
					}
					else
					{
						SetSize(FindSize(sizeGroupType, inverse));
					}
					if (EditorApplication.isPlaying && resetState == -1)
					{
						resetState = 0;
					}
				}
				if (mode == 1 || mode == -1)
				{
					GUI.enabled = true;
				}
				else
				{
					GUI.enabled = false;
				}

				if (GUI.Button(new Rect(flexRect.x + buttonWidth + spaceBetween, flexRect.y + 24, buttonWidth, buttonHeight), "Landscape"))
				{
					if (inverse == "Free Aspect")
					{
						SetSize(FindSize(sizeGroupType, "(16:"));
					}
					else
					{
						SetSize(FindSize(sizeGroupType, inverse));
					}
					if (EditorApplication.isPlaying && resetState == -1)
					{
						resetState = 1;
					}
				}

				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();

		}

		IEnumerator<UnityEngine.WaitForSeconds> setSymbolsAfter(float delay, bool _currentUseARFoundation)
		{

			yield return new WaitForSeconds(delay);

			SetSymbols(_currentUseARFoundation);

			AssetDatabase.Refresh();
		}

		static void SetSymbols(bool _currentUseARFoundation)
        {
			foreach (BuildTargetGroup buildTargetGroup in ARFoundationBuildTargetList)
			{
				string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
				List<string> allDefines = definesString.Split(';').ToList();


				if (_currentUseARFoundation)
				{
					if (!allDefines.Contains(useARFoundationSymbol))
					{
						allDefines.Add(useARFoundationSymbol);
					}
				}
				else
				{
					if (allDefines.Contains(useARFoundationSymbol))
					{
						allDefines.Remove(useARFoundationSymbol);
					}
				}

				string newSymbols = string.Join(";", allDefines.ToArray());

				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newSymbols);
			}
		}

		void ResetView(PlayModeStateChange state)
		{

			if (EditorApplication.isPlaying)
			{
				return;
			}

			if (resetState != -1)
			{

				GameViewSizeGroupType sizeGroupType = GameViewSizeGroupType.iOS;

				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
				{
					sizeGroupType = GameViewSizeGroupType.Android;
				}

				int mode = -1;
				int sizeIndex = GetSizeIndex();
				string inverse = "Free Aspect";
				if (sizeIndex != -1)
				{
					inverse = GetInverseWindowSizeString(sizeGroupType, sizeIndex, out mode);
				}
				if (((mode == 0 && resetState == 1) || (mode == 1 && resetState == 0)) && inverse != "Free Aspect")
				{
					SetSize(FindSize(sizeGroupType, inverse));
				}

				resetState = -1;
			}
		}

		public string GetInverseWindowSizeString(GameViewSizeGroupType sizeGroupType, int index, out int mode)
		{
			mode = -1;
			var group = GetGroup(sizeGroupType);
			var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
			var displayTexts = getDisplayTexts.Invoke(group, null) as string[];

			string display = displayTexts[index];
			int pren = display.IndexOf("(");
			if (pren != -1)
			{
				display = display.Substring(pren + 1, display.Length - (pren + 2));
			}
			else
			{
				return display;
			}

			string divider = "x";
			int div = display.IndexOf('x');
			if (div == -1)
			{
				div = display.IndexOf(':');
				divider = ":";
			}

			if (div == -1)
			{
				return display;
			}

			string first = "";
			string second = "";
			if (div != -1)
			{
				first = display.Substring(0, div);
				second = display.Substring(div + 1, display.Length - (div + 1));
			}
			if (int.Parse(first) > int.Parse(second))
			{
				mode = 0;
			}
			else
			{
				mode = 1;
			}
			return ("(" + second + divider + first + ")");
		}

		public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
		{
			var group = GetGroup(sizeGroupType);
			var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
			var displayTexts = getDisplayTexts.Invoke(group, null) as string[];

			for (int i = 0; i < displayTexts.Length; i++)
			{
				string display = displayTexts[i];
				if (display.Contains(text))
				{
					return i;
				}
			}
			return -1;
		}

		static object GetGroup(GameViewSizeGroupType type)
		{
			return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
		}

		void initializeGameView()
		{
			var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
			var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
			var instanceProp = singleType.GetProperty("instance");
			getGroup = sizesType.GetMethod("GetGroup");
			gameViewSizesInstance = instanceProp.GetValue(null, null);
		}

		public enum GameViewSizeType
		{
			AspectRatio, FixedResolution
		}

		public static void SetSize(int index)
		{
			var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
			var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
																BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var gvWnd = EditorWindow.GetWindow(gvWndType);
			selectedSizeIndexProp.SetValue(gvWnd, index, null);
		}

		public int GetSizeIndex()
		{
			var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
			bool isOpen = false;
			foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
			{
				if (window.titleContent.text == "Game")
				{
					isOpen = true;
					break;
				}
			}

			if (isOpen)
			{
				var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
															  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var gvWnd = EditorWindow.GetWindow(gvWndType, false, "Game", false);
				return (int)selectedSizeIndexProp.GetValue(gvWnd, null);
			}
			return -1;
		}
	}
}
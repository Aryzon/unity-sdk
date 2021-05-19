using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public class PostBuildProcessor : MonoBehaviour
{
	#if UNITY_CLOUD_BUILD
	// This method is added in the Advanced Features Settings on UCB
	// PostBuildProcessor.OnPostprocessBuildiOS
	public static void OnPostprocessBuildiOS (string exportPath)
	{
		Debug.Log("OnPostprocessBuildiOS");
		ProcessPostBuild(BuildTarget.iPhone,exportPath);
	}
	#endif

	[PostProcessBuild]
	public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
#if UNITY_IOS

		if (buildTarget == BuildTarget.iOS) {

			// Get plist
			string plistPath = pathToBuiltProject + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			// Get root
			PlistElementDict rootDict = plist.root;

			// Change value of CFBundleVersion in Xcode plist
			//var buildKey = "LSApplicationQueriesSchemes";
			//rootDict.SetString(buildKey,"<array>\n\t\t<string>aryzon</string>\n\t</array>");

			bool changeMade = false;

			PlistElementArray querieSchemeArray;
			PlistElement querieSchemeElement;
			try {
				rootDict.values.TryGetValue ("LSApplicationQueriesSchemes", out querieSchemeElement);
				querieSchemeArray = querieSchemeElement.AsArray();
			} catch {
				Debug.Log ("Could not get value");
				querieSchemeArray = rootDict.CreateArray ("LSApplicationQueriesSchemes");
				changeMade = true;
			}

			if (querieSchemeArray != null) {
				bool foundEntry = false;
				foreach (PlistElementString obj in querieSchemeArray.values) {
					if (obj.AsString () == "aryzon") {
						foundEntry = true;
						break;
					}
				}
				if (!foundEntry) {
					querieSchemeArray.AddString ("aryzon");
					changeMade = true;
				}
			}

			if (changeMade) {
				// Write to file
				File.WriteAllText(plistPath, plist.WriteToString());
			}
		}
#endif
    }
}

using System.Linq;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public class AryzonHierarchy : MonoBehaviour
{
	static AryzonHierarchy()
    {
		EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }
	private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
	{
		var obj = EditorUtility.InstanceIDToObject(instanceID);
		if (obj != null) {
			if (obj.name == "Aryzon") {
				Texture logo = Resources.Load("LogoAryzonSmall") as Texture;
				int logoSize = (int)(0.8f*selectionRect.height);
				int y = (int)(selectionRect.y+(selectionRect.height - logoSize)/2f);
				GUI.DrawTexture(new Rect(selectionRect.x + selectionRect.width - logoSize - 5, y, logoSize, logoSize), logo, ScaleMode.ScaleToFit, true);
            }
        }
    }
}
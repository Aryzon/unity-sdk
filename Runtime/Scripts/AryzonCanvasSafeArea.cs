using UnityEngine;

public class AryzonCanvasSafeArea : MonoBehaviour
{
    public bool simulateInEditor = false;
#if !UNITY_WSA
    RectTransform Panel;
    Rect LastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        Panel = GetComponent<RectTransform>();
        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        Rect safeArea = GetSafeArea();

        if (safeArea != LastSafeArea)
            ApplySafeArea(safeArea);
    }

    Rect GetSafeArea()
    {
#if UNITY_EDITOR
        if (simulateInEditor)
        {
            Rect r = Screen.safeArea;
            float notch = 0.075f;
            float bottom = 0.025f;
            float sizeY = r.size.y;
            r.size = new Vector2(r.size.x, sizeY*(1f - (notch+bottom)));
            r.position = new Vector2(r.position.x, r.position.y + sizeY * bottom);
            return r;
        }
#endif
        return Screen.safeArea;
    }

    void ApplySafeArea(Rect r)
    {
        LastSafeArea = r;

        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        Panel.anchorMin = anchorMin;
        Panel.anchorMax = anchorMax;

        Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
    }
#endif
}
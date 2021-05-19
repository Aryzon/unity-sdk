using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aryzon.UI {
    [RequireComponent(typeof(Canvas))]
    public class AryzonCanvasUtility : MonoBehaviour, IAryzonEventHandler
    {
		[EnumFlag(2)] public PresentCanvasMode presentCanvasMode;
        private Canvas thisCanvas;
        private Camera beforeCamera;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            beforeCamera = thisCanvas.worldCamera;
            AryzonSettings.Instance.RegisterEventHandler(this);

            #if !UNITY_WSA
            
            if ((AryzonSettings.Instance.aryzonManager == null || !AryzonSettings.Instance.aryzonManager.aryzonMode) && !presentCanvasMode.HasFlag(PresentCanvasMode.ShowIn2DMode)) {
                gameObject.SetActive(false);
            } else if (AryzonSettings.Instance.aryzonManager != null && AryzonSettings.Instance.aryzonManager.aryzonMode && !AryzonSettings.Instance.aryzonManager.stereoscopicMode && !presentCanvasMode.HasFlag(PresentCanvasMode.ShowIn2DMode)) {
                
                gameObject.SetActive(false);
            } else if ((AryzonSettings.Instance.aryzonManager == null && !presentCanvasMode.HasFlag(PresentCanvasMode.ShowInStereoscopicMode) && !presentCanvasMode.HasFlag(PresentCanvasMode.HideShowWithAryzonMode))) {
                
                gameObject.SetActive(false);
            } else if (AryzonSettings.Instance.aryzonManager != null && (AryzonSettings.Instance.aryzonManager.aryzonMode && AryzonSettings.Instance.aryzonManager.stereoscopicMode && !presentCanvasMode.HasFlag(PresentCanvasMode.ShowInStereoscopicMode))) {
                gameObject.SetActive(false);
            }

            #endif
			
        }

        public void OnStartStereoscopicMode (AryzonModeEventArgs args) {
            
			if (presentCanvasMode.HasFlag(PresentCanvasMode.ShowInStereoscopicMode)) {
                gameObject.SetActive(true);
            } else if (!presentCanvasMode.HasFlag(PresentCanvasMode.ShowInStereoscopicMode)) {
                gameObject.SetActive(false);
            }
            if (presentCanvasMode.HasFlag(PresentCanvasMode.SetWorldSpaceCamera)) {
                beforeCamera = thisCanvas.worldCamera;
                if (!AryzonSettings.Instance.aryzonManager.XRCamera) {
                    Debug.LogWarning("[Aryzon] Please set the XRCamera in the AryzonManager.");
                } 
                thisCanvas.worldCamera = AryzonSettings.Instance.aryzonManager.XRCamera;
            }
        }
        public void OnStopStereoscopicMode (AryzonModeEventArgs args) {
            
            if (presentCanvasMode.HasFlag(PresentCanvasMode.ShowIn2DMode) && !presentCanvasMode.HasFlag(PresentCanvasMode.HideShowWithAryzonMode)) {
                gameObject.SetActive(true);
            } else if (!presentCanvasMode.HasFlag(PresentCanvasMode.ShowIn2DMode)) {
                gameObject.SetActive(false);
            }
            if (presentCanvasMode.HasFlag(PresentCanvasMode.SetWorldSpaceCamera)) {
                thisCanvas.worldCamera = beforeCamera;
            }
        }

		public void OnStartAryzonMode (AryzonModeEventArgs args) {
            beforeCamera = thisCanvas.worldCamera;
            if (presentCanvasMode.HasFlag(PresentCanvasMode.HideShowWithAryzonMode)) {
                gameObject.SetActive(false);
            }
        }
		public void OnStopAryzonMode (AryzonModeEventArgs args) {
            thisCanvas.worldCamera = beforeCamera;
            if (presentCanvasMode.HasFlag(PresentCanvasMode.HideShowWithAryzonMode)) {
                gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (AryzonSettings.Instance != null) {
                AryzonSettings.Instance.RegisterEventHandler(this);
            }
        }
    }

    public enum PresentCanvasMode {
        ShowIn2DMode = 1,
        ShowInStereoscopicMode = 1 << 1,
        HideShowWithAryzonMode = 1 << 2,
        SetWorldSpaceCamera = 1 << 4
    }
}
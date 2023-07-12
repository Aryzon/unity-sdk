using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Aryzon;
using Aryzon.UI;

public class AryzonReticle : MonoBehaviour, IAryzonEventHandler
{
    [HeaderAttribute("Reticle")]
    public GameObject prefab;

    public bool timedClick = true;
    public bool showDebugRay = true;
    public float debugRayLength = 5f;
    public float maxRaycastLength = 25f;
    public float reticleDistance = 1f;
    public float timeToClick = 0.8f;
    public LayerMask excludeLayers;

    //[SerializeField] (for future use)
    private bool _alwaysShowReticle = false;
    public bool alwaysShowReticle
    {
        get { return _alwaysShowReticle; }
        set { _alwaysShowReticle = value; reticleModeSet = true; reticleMode = _alwaysShowReticle; }
    }

    [HeaderAttribute("Canvas")]
    public bool disableScreenOverlaysInStereoMode = true;
    public bool setWorldSpaceCameras = true;

    private Camera reticleCam;
    /*{
        get { return AryzonSettings.Instance.aryzonManager.XRCamera; }
    }*/

    private List<Canvas> screenOverlayCanvasses = new List<Canvas>();
    private List<Canvas> worldSpaceCanvasses = new List<Canvas>();
    private List<Camera> worldSpaceCameras = new List<Camera>();

    private bool reticleModeSet = false;
    private bool reticleMode = false;

    private AryzonInputModule aryzonInputModule;
    private BaseInputModule baseInputModule;
    private AryzonReticleAnimator reticleAnimator;

    private GameObject eventSystemGO;
    private GameObject reticle;

    private Collider previousCollider;
    private AryzonRaycastObject previousRaycastObject;

    private void OnEnable()
    {
        AryzonSettings.Instance.RegisterEventHandler(this);
        StartCoroutine(SetInputControllerWhenReady());
        if (alwaysShowReticle)
        {
            ShowReticle();
        }
    }

    IEnumerator SetInputControllerWhenReady()
    {
        while (!AryzonSettings.Instance || !AryzonSettings.Instance.aryzonManager)
        {
            yield return null;
        }
        AryzonSettings.Instance.aryzonInputController = gameObject;
    }

    public void OnStartAryzonMode(AryzonModeEventArgs e)
    {

    }

    public void OnStartStereoscopicMode(AryzonModeEventArgs e)
    {
        ShowReticle();
    }

    public void HideReticle()
    {
        reticleModeSet = true;
        reticleMode = false;
    }

    public void ShowReticle()
    {
        reticleModeSet = true;
        reticleMode = true;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (alwaysShowReticle && !reticleMode)
        {
            ShowReticle();
        }
#endif

        if (reticleModeSet)
        {
            reticleModeSet = false;
            if (reticleMode)
            {
                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem)
                {
                    baseInputModule = EventSystem.current.currentInputModule;
                }
                else
                {
                    eventSystemGO = new GameObject("EventSystem");
                    currentEventSystem = eventSystemGO.AddComponent<EventSystem>();
                }
                if (baseInputModule)
                {
                    eventSystemGO = baseInputModule.gameObject;
                }
                aryzonInputModule = eventSystemGO.AddComponent<AryzonInputModule>();
                baseInputModule.enabled = false;
                aryzonInputModule.forceModuleActive = true;
                aryzonInputModule.SetClickTime(timeToClick);

                if (!reticleCam)
                {
                    GameObject monoReticleCameraObj = new GameObject("Mono Reticle Camera");
                    monoReticleCameraObj.transform.parent = AryzonSettings.Instance.aryzonManager.XRCamera.transform;
                    monoReticleCameraObj.transform.localPosition = Vector3.zero;
                    monoReticleCameraObj.transform.localRotation = Quaternion.identity;

                    reticleCam = monoReticleCameraObj.AddComponent<Camera>();
                    reticleCam.clearFlags = CameraClearFlags.Nothing;
                    reticleCam.enabled = false;
                    reticleCam.stereoTargetEye = StereoTargetEyeMask.None;

                    reticleCam.depth = 99;
                }

                if (!reticle)
                {
                    reticle = Instantiate(prefab, reticleCam.transform);
                }
                aryzonInputModule.reticleTransform = reticle.transform;
                aryzonInputModule.reticleDistance = reticleDistance;
                aryzonInputModule.timedClick = timedClick;
                reticleAnimator = reticle.GetComponent<AryzonReticleAnimator>();
                aryzonInputModule.reticleAnimator = reticleAnimator;

                screenOverlayCanvasses.Clear();
                worldSpaceCanvasses.Clear();
                worldSpaceCameras.Clear();

                if (disableScreenOverlaysInStereoMode || setWorldSpaceCameras)
                {
                    foreach (Canvas c in FindObjectsOfType<Canvas>())
                    {
                        if (c.isRootCanvas && c.isActiveAndEnabled && c.gameObject.tag != "Aryzon" && !c.GetComponent<AryzonCanvasUtility>())
                        {
                            if (disableScreenOverlaysInStereoMode && c.renderMode == RenderMode.ScreenSpaceOverlay)
                            {
                                screenOverlayCanvasses.Add(c);
                                c.enabled = false;
                            }
                            else if (setWorldSpaceCameras && c.renderMode == RenderMode.WorldSpace)
                            {
                                worldSpaceCanvasses.Add(c);
                                worldSpaceCameras.Add(c.worldCamera);
                                c.worldCamera = reticleCam;
                            }
                        }
                    }
                }
            }
            else
            {
                if (aryzonInputModule)
                {
                    aryzonInputModule.enabled = false;
                }
                if (baseInputModule)
                {
                    baseInputModule.enabled = true;
                }
                if (reticle)
                {
                    Destroy(reticle);
                }
                if (reticleCam)
                {
                    Destroy(reticleCam.gameObject);
                }
                foreach (Canvas c in screenOverlayCanvasses)
                {
                    if (c)
                    {
                        c.enabled = true;
                    }
                }
                screenOverlayCanvasses.Clear();

                int i = 0;
                foreach (Canvas c in worldSpaceCanvasses)
                {
                    c.worldCamera = worldSpaceCameras[i];
                    i++;
                }
                worldSpaceCanvasses.Clear();
                worldSpaceCameras.Clear();
            }
        }

        if (reticleMode)
        {
            Raycast();
        }
    }

    private virtual void Raycast()
    {
        if (showDebugRay)
        {
            Debug.DrawRay(reticleCam.transform.position, reticleCam.transform.forward * debugRayLength, Color.magenta);
        }
        
        Ray ray = new Ray(new Vector3(reticleCam.transform.position.x+0.06f, reticleCam.transform.position.y, reticleCam.transform.position.z), reticleCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastLength, ~excludeLayers))
        {
            if (previousCollider != hit.collider)
            {
                AryzonRaycastObject raycastObject = hit.collider.GetComponent<AryzonRaycastObject>();
                aryzonInputModule.SetExternalObject(raycastObject);

                if (previousRaycastObject)
                {
                    previousRaycastObject.PointerOff();
                }
                previousRaycastObject = raycastObject;
            }
            aryzonInputModule.SetHitDistance(Vector3.Distance(reticleCam.transform.position, hit.point));
            previousCollider = hit.collider;
        }
        else
        {
            aryzonInputModule.SetExternalObject(null);
            if (previousRaycastObject)
            {
                previousRaycastObject.PointerOff();
                previousRaycastObject = null;
            }
            previousCollider = null;
        }
    }

    public void OnStopAryzonMode(AryzonModeEventArgs e)
    {

    }

    public void OnStopStereoscopicMode(AryzonModeEventArgs e)
    {
        if (!alwaysShowReticle)
        {
            HideReticle();
        }
    }

    private void OnDisable()
    {
        if (AryzonSettings.Instance)
        {
            AryzonSettings.Instance.UnregisterEventHandler(this);
        }
    }
}

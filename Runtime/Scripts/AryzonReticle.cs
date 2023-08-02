using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Aryzon;
using Aryzon.UI;


public interface IAryzonReticleRaycast
{
    void Raycast(Vector3 origin, Vector3 direction, float length, LayerMask layerMask, AryzonReticleAnimator reticleAnimator);
}

public class AryzonReticle : MonoBehaviour, IAryzonEventHandler
{
    [HeaderAttribute("Reticle")]
    public GameObject prefab;

    public MonoBehaviour interactionDelegate;

    [SerializeField] private MonoBehaviour externalReticleRaycast;
    public IAryzonReticleRaycast ExternalReticleRaycast
    {
        get => externalReticleRaycastInterface;
        set
        {
            externalReticleRaycast = null;
            externalReticleRaycastInterface = value;
        }
    }
    private IAryzonReticleRaycast externalReticleRaycastInterface;

    public bool useAryzonInputModule = true;
    public bool timedClick = true;
    public bool showDebugRay = true;
    public float debugRayLength = 5f;
    public float maxRaycastLength = 25f;
    public float reticleDistance = 1f;
    public float timeToClick = 0.8f;
    public LayerMask layers;

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

    private List<Canvas> screenOverlayCanvasses = new List<Canvas>();
    private List<Canvas> worldSpaceCanvasses = new List<Canvas>();
    private List<Camera> worldSpaceCameras = new List<Camera>();

    private bool reticleModeSet = false;
    private bool reticleMode = false;

    private AryzonInputModule aryzonInputModule;
    private BaseInputModule baseInputModule;
    public AryzonReticleAnimator reticleAnimator;

    private GameObject eventSystemGO;
    private GameObject reticle;

    private Collider previousCollider;

    private void Awake()
    {
        if (externalReticleRaycast != null)
        {
            externalReticleRaycastInterface = externalReticleRaycast as IAryzonReticleRaycast;
            if (externalReticleRaycastInterface == null)
            {
                externalReticleRaycast = null;
                throw new InvalidCastException();
            }
        }
    }

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
                if (useAryzonInputModule)
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
                    aryzonInputModule.timedClick = timedClick;
                }

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

                reticleAnimator = reticle.GetComponent<AryzonReticleAnimator>();
                reticleAnimator.defaultDistance = reticleDistance;

                if (useAryzonInputModule)
                {
                    aryzonInputModule.reticleAnimator = reticleAnimator;
                    if (interactionDelegate && interactionDelegate.GetType() == typeof(IAryzonReticleInteractionDelegate))
                    {
                        aryzonInputModule.interactionDelegate = (IAryzonReticleInteractionDelegate)interactionDelegate;
                    }
                }

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

        if (reticleMode && useAryzonInputModule)
        {
            Raycast();
        }
    }

    private void Raycast()
    {
        Vector3 origin = new Vector3(reticleCam.transform.position.x, reticleCam.transform.position.y, reticleCam.transform.position.z);
        Vector3 direction = reticleCam.transform.forward;

        if (externalReticleRaycastInterface != null)
        {
            externalReticleRaycastInterface.Raycast(origin, direction, maxRaycastLength, layers, reticleAnimator);
        }

        if (!useAryzonInputModule) return;

        if (showDebugRay)
        {
            Debug.DrawRay(reticleCam.transform.position, reticleCam.transform.forward * debugRayLength, Color.magenta);
        }
        
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastLength, layers))
        {
            if (previousCollider != hit.collider)
            {
                aryzonInputModule.SetExternalObject(hit.collider.gameObject);
            }
            float distance = Mathf.Min(maxRaycastLength, Vector3.Distance(reticleCam.transform.position, hit.point));

            aryzonInputModule.SetHitDistance(distance);
            previousCollider = hit.collider;
        }
        else
        {
            aryzonInputModule.SetExternalObject(null);
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

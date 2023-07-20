using System.Collections;
using System.Text;
using Aryzon;

namespace UnityEngine.EventSystems
{
    public class AryzonInputModule : PointerInputModule
    {
        private readonly MouseState m_MouseState = new MouseState();

        public bool timedClick = true;

        public StandaloneInputModule standaloneInputModule;

        public AryzonReticleAnimator reticleAnimator;

        public IAryzonReticleInteractionDelegate interactionDelegate;

        private AryzonRaycastObject externalRaycastObject;
        private GameObject externalGO;
        private GameObject lastGO;

        private float timer;
        private float clickTime = 0.8f;
        private float offStateTimer = 0f;
        private float hitDistance;

        private bool didClick = false;
        private bool releaseManually = false;
        private bool pressing = false;
        private bool didSetOffState = false;

        private ReticleState reticleState = ReticleState.OffState;

        protected AryzonInputModule()
        { }

        [SerializeField]
        private bool m_ForceModuleActive;

        public void SetExternalObject(GameObject obj)
        {
            if (externalGO != obj)
            {
                if (externalRaycastObject) externalRaycastObject.PointerOff();
                if (interactionDelegate != null && interactionDelegate.IsInteractable(externalGO))
                {
                    interactionDelegate.PointerOff(externalGO);
                }
            }

            externalRaycastObject = obj?.GetComponent<AryzonRaycastObject>();
            externalGO = obj;
        }

        public void SetHitDistance(float distance)
        {
            hitDistance = distance;
        }

        public void SetClickTime(float time)
        {
            clickTime = time;
        }

        public bool forceModuleActive
        {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        public override bool IsModuleSupported()
        {
            return forceModuleActive;
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            if (m_ForceModuleActive)
                return true;

            return false;
        }


        public override void Process()
        {
            GazeControl();
        }

        public GameObject GameObjectUnderPointer(int pointerId)
        {
            var lastPointer = GetLastPointerEventData(pointerId);
            if (lastPointer != null)
                return lastPointer.pointerCurrentRaycast.gameObject;
            return null;
        }

        public GameObject GameObjectUnderPointer()
        {
            return GameObjectUnderPointer(PointerInputModule.kMouseLeftId);
        }

        protected static PointerEventData.FramePressState StateForButton(string buttonCode)
        {
            var pressed = Input.GetButtonDown(buttonCode);
            var released = Input.GetButtonUp(buttonCode);

            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        protected MouseState CreateGazePointerEvent(int id)
        {
            PointerEventData leftData;
            var created = GetPointerData(kMouseLeftId, out leftData, true);

            Vector2 pos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            leftData.Reset();
            leftData.delta = Vector2.zero;
            leftData.position = pos;
            leftData.scrollDelta = Vector2.zero;
            leftData.button = PointerEventData.InputButton.Left;

            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            RaycastResult raycast = FindFirstRaycast(m_RaycastResultCache);

            leftData.pointerCurrentRaycast = raycast;

            m_RaycastResultCache.Clear();

            PointerEventData.FramePressState state = StateForButton("Fire1");

            m_MouseState.SetButtonState(
                PointerEventData.InputButton.Left,
                state,
                leftData);

            return m_MouseState;
        }


        private void GazeControl()
        {
            var pointerData = CreateGazePointerEvent(0);

            var leftPressData = pointerData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            bool pressed = leftPressData.PressedThisFrame();
            bool released = leftPressData.ReleasedThisFrame();

            bool controllerPressed = Input.GetKeyDown(AryzonSettings.Instance.controllerDownKeyCode);
            bool controllerReleased = Input.GetKeyUp(AryzonSettings.Instance.controllerUpKeyCode);

            if (timedClick && (controllerPressed || controllerReleased))
            {
                Debug.Log("[Aryzon] Deactivating timed gaze");
                timedClick = false;
            }

            pressed = pressed || controllerPressed;
            released = released || controllerReleased;

            ProcessPress(leftPressData.buttonData, pressed, released);
            ProcessMove(leftPressData.buttonData);
        }

        



        private void SetOffState()
        {
            if (reticleState != ReticleState.OffState)
            {
                reticleState = ReticleState.OffState;
                offStateTimer = 0f;
                StartCoroutine(SetOffStateAfterDelay(0.5f));
            }
            else
            {
                offStateTimer = 0f;
            }
        }

        private void SetOverState()
        {
            if (reticleState != ReticleState.OverState)
            {
                reticleAnimator.SetOverState();
                reticleState = ReticleState.OverState;
            }
            if (externalRaycastObject)
            {
                externalRaycastObject.PointerOver();
            }
        }

        private void SetClickedState()
        {
            if (reticleState != ReticleState.ClickedState)
            {
                reticleAnimator.SetClickedState();
                reticleState = ReticleState.ClickedState;
            }
        }

        private IEnumerator SetOffStateAfterDelay (float delay)
        {
            didSetOffState = false;
            while (offStateTimer <= delay && reticleState == ReticleState.OffState)
            {
                offStateTimer += Time.deltaTime;
                yield return null;
            }
            if (reticleState == ReticleState.OffState)
            {
                reticleAnimator.SetOffState();
            }
            didSetOffState = true;
        }

        private IEnumerator ReleaseTimedClickAfterDelay(float delay)
        {
            float t = 0f;
            while (t < delay && pressing)
            {
                t += Time.deltaTime;
                yield return null;
            }
            if (pressing)
            {
                releaseManually = true;
            }
        }

        
        private void ProcessPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            GameObject currentGO;
            if (externalGO)
            {
                currentGO = externalGO;

                if (!pressing)
                {
                    reticleAnimator.SetDistance(hitDistance);
                }
            }
            else
            {
                currentGO = pointerEvent.pointerCurrentRaycast.gameObject;

                if (pointerEvent.pointerCurrentRaycast.isValid || pressing)
                {
                    if (!pressing)
                    {
                        reticleAnimator.SetDistance(pointerEvent.pointerCurrentRaycast.distance);
                    }
                }
                else if (didSetOffState && reticleState == ReticleState.OffState)
                {
                    reticleAnimator.SetDefaultPose();
                }
            }

            bool wentOver = false;
            bool stayedOver = false;
            bool wentOff = false;

            bool isInteractable = false;
            if (currentGO && interactionDelegate != null)
            {
                isInteractable = interactionDelegate.IsInteractable(currentGO);
            }

            if (isInteractable)
            {
                if (currentGO != lastGO)
                {
                    wentOver = true;
                    Debug.Log("Went over: " + currentGO.name);
                }
                else
                {
                    stayedOver = true;
                    Debug.Log("Stayed over: " + currentGO.name);
                }
            }
            else
            {
                if (currentGO != lastGO)
                {
                    wentOff = true;
                }
            }

            if (wentOver || wentOff)
            {
                timer = 0f;
                didClick = false;
            }

            if (pressing)
            {
                if (released)
                {
                    HandleReleased(pointerEvent, currentGO);

                    pressing = false;

                    if (wentOver || stayedOver)
                    {
                        Debug.Log("SetOverState 1");
                        SetOverState();
                    }
                    else
                    {
                        Debug.Log("SetOffState 1");
                        SetOffState();
                    }
                }
                if (releaseManually)
                {
                    releaseManually = false;

                    pressing = false;

                    if (wentOver || stayedOver)
                    {
                        Debug.Log("SetOverState 2");
                        SetOverState();
                    }
                    else
                    {
                        Debug.Log("SetOffState 2");
                        SetOffState();
                    }
                }
            }
            else
            {
                if (timedClick && stayedOver && !didClick)
                {
                    if (timer > clickTime)
                    {
                        pressing = true;
                        didClick = true;
                        StartCoroutine(PerformClick(pointerEvent, currentGO));
                        StartCoroutine(ReleaseTimedClickAfterDelay(0.3f));
                        timer = 0f;
                        SetClickedState();
                    }

                    timer += Time.deltaTime;
                }
                if (wentOver)
                {
                    Debug.Log("Went over");
                    SetOverState();
                }
                else if (stayedOver)
                {
                    Debug.Log("Stayed Over");
                    if (pressed)
                    {
                        Debug.Log("Pressed");
                        pressing = true;
                        HandlePressed(pointerEvent, currentGO);
                        SetClickedState();
                    }
                } else if (wentOff)
                {
                    Debug.Log("Went off");
                    SetOffState();
                }
            }

            lastGO = currentGO;
        }

        private void HandlePressed(PointerEventData pointerEvent, GameObject currentGO)
        {
            if (currentGO == externalGO)
            {
                if (externalRaycastObject)
                {
                    externalRaycastObject.PointerDown();
                }
            }
            else
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentGO, pointerEvent);

                if (pointerEvent.pointerEnter != currentGO)
                {
                    HandlePointerExitAndEnter(pointerEvent, currentGO);
                    pointerEvent.pointerEnter = currentGO;
                }

                var newPressed = ExecuteEvents.ExecuteHierarchy(currentGO, pointerEvent, ExecuteEvents.pointerDownHandler);

                if (newPressed == null)
                {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentGO);
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentGO;
            }
        }

        private void HandleReleased(PointerEventData pointerEvent, GameObject currentGO)
        {
            if (externalRaycastObject)
            {
                externalRaycastObject.PointerUp();
            }
            else if (currentGO)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentGO);

                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;



                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
            }
        }

        private IEnumerator PerformClick(PointerEventData pointerEvent, GameObject currentOverGo)
        {
            HandlePressed(pointerEvent, currentOverGo);
            yield return null;
            HandleReleased(pointerEvent, currentOverGo);
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Input: GazeBasicInputModule");
            var pointerData = GetLastPointerEventData(kMouseLeftId);
            if (pointerData != null)
                sb.AppendLine(pointerData.ToString());

            return sb.ToString();
        }

        protected override void OnEnable()
        {
            if (standaloneInputModule)
            {
                standaloneInputModule.DeactivateModule();
                standaloneInputModule.enabled = false;
            }
            this.forceModuleActive = true;
            this.ActivateModule();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (externalRaycastObject)
            {
                externalRaycastObject.PointerOff();
            }

            this.forceModuleActive = false;
            this.DeactivateModule();
            if (standaloneInputModule)
            {
                standaloneInputModule.enabled = true;
                standaloneInputModule.ActivateModule();
            }
            base.OnDisable();
            Destroy(this);
        }
    }
}

namespace Aryzon
{
    public enum ReticleState
    {
        OffState,
        OverState,
        ClickedState
    }
}
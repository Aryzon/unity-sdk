using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Aryzon;
using System.Runtime.InteropServices;
using System.Linq;

namespace Aryzon
{
    public class AryzonControllerManager : MonoBehaviour
    {
        public enum Connection
        {
            Disconnected,
            Connected,
            Unknown
        }

        public static AryzonControllerManager Instance;
        public bool activeOutsideAryzonMode = false;

        [Header("Trigger")]
        public UnityEvent OnTriggerDown;
        public UnityEvent OnTriggerUp;

        [Header("Menu")]
        public UnityEvent OnMenuDown;
        public UnityEvent OnMenuUp;

        [Header("Exit")]
        public UnityEvent OnExitDown;
        public UnityEvent OnExitUp;

        [Header("Thumbstick")]
        public UnityEvent<Vector2> OnThumbDown;
        public UnityEvent OnThumbUp;

        [Header("Other")]
        public UnityEvent OnADown;
        public UnityEvent OnAUp;

        public UnityEvent OnBDown;
        public UnityEvent OnBUp;

        public UnityEvent OnXDown;
        public UnityEvent OnXUp;

        public UnityEvent OnYDown;
        public UnityEvent OnYUp;

        [Header("Controller")]
        public UnityEvent<Connection> OnControllerConnected;
        public UnityEvent<Connection> OnControllerDisconnected;

        public Connection ConnectionStatus
        {
            get => (Connection)getControllerConnectionStatus();
        }

        public bool TriggerDown { get => triggerDown; }
        public bool ThumbDown { get => thumbDown; }
        public bool MenuDown { get => menuDown; }
        public bool ExitDown { get => exitDown; }
        public bool ADown { get => aDown; }
        public bool BDown { get => bDown; }
        public bool XDown { get => xDown; }
        public bool YDown { get => yDown; }

        public bool IsUsed
        {
            get => isUsed;
        }
        private bool isUsed;

        private bool triggerDown;
        private bool menuDown;
        private bool exitDown;
        private bool aDown;
        private bool bDown;
        private bool xDown;
        private bool yDown;

        private bool upDown;
        private bool downDown;
        private bool leftDown;
        private bool rightDown;

        private bool thumbWasDown;
        private bool thumbDown
        {
            get => upDown | downDown | leftDown | rightDown;
        }

        public Vector2 Thumbstick
        {
            get => thumb;
        }
        private Vector2 thumb;

        private Coroutine checkConnection;

#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern uint getControllerConnectionStatus();
#else
        private uint getControllerConnectionStatus()
        {
            return Input.GetJoystickNames().Any((t) => t.ToLower().Contains("aryzon")) ? (uint)1 : 0;
        }
#endif

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                Debug.LogError("[Aryzon] Only one AryzonControllerManager can exist at a time.");

            }
            else Instance = this;
        }

        private void OnEnable()
        {
            isUsed = false;
            checkConnection = StartCoroutine(CheckConnection());
        }

        private void OnDisable()
        {
            StopCoroutine(checkConnection);
            checkConnection = null;
            if (ConnectionStatus == Connection.Connected) OnControllerConnected?.Invoke(Connection.Connected);
        }

        private IEnumerator CheckConnection()
        {
            bool wasConnected = false;
            while (true)
            {
                Connection cS = ConnectionStatus;
                if (!wasConnected && cS == Connection.Connected) { OnControllerConnected?.Invoke(cS); wasConnected = true; isUsed = true; }
                else if (wasConnected && cS != Connection.Connected) { OnControllerDisconnected?.Invoke(cS); wasConnected = false; isUsed = false; }
                yield return new WaitForSeconds(1f);
            }
        }

        private void Update()
        {
            if (!activeOutsideAryzonMode && (!AryzonSettings.Instance.AryzonMode || !AryzonSettings.Instance.LandscapeMode)) return;

            if (AryzonSettings.Controller.Trigger.Down == AryzonSettings.Controller.Trigger.Up)
            {
                if (!triggerDown && Input.GetKeyDown(AryzonSettings.Controller.Trigger.Down)) DoTriggerDown();
                if (triggerDown && !Input.GetKeyDown(AryzonSettings.Controller.Trigger.Down)) DoTriggerReleased();
            }
            else
            {
                if (Input.GetKeyDown(AryzonSettings.Controller.Trigger.Down)) DoTriggerDown();
                if (triggerDown && Input.GetKeyDown(AryzonSettings.Controller.Trigger.Up)) DoTriggerReleased();
            }

            if (menuDown && Input.GetKeyDown(AryzonSettings.Controller.Menu.Up)) DoMenuReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.Menu.Down)) DoMenuDown();

            if (exitDown && Input.GetKeyDown(AryzonSettings.Controller.Exit.Up)) DoExitReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.Exit.Down)) DoExitDown();

            if (aDown && Input.GetKeyDown(AryzonSettings.Controller.A.Up)) DoAReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.A.Down)) DoADown();

            if (bDown && Input.GetKeyDown(AryzonSettings.Controller.B.Up)) DoBReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.B.Down)) DoBDown();

            if (xDown && Input.GetKeyDown(AryzonSettings.Controller.X.Up)) DoXReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.X.Down)) DoXDown();

            if (yDown && Input.GetKeyDown(AryzonSettings.Controller.Y.Up)) DoYReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.Y.Down)) DoYDown();

            if (upDown && Input.GetKeyDown(AryzonSettings.Controller.Up.Up)) DoUpReleased();
            if (downDown && Input.GetKeyDown(AryzonSettings.Controller.Down.Up)) DoDownReleased();
            if (leftDown && Input.GetKeyDown(AryzonSettings.Controller.Left.Up)) DoLeftReleased();
            if (rightDown && Input.GetKeyDown(AryzonSettings.Controller.Right.Up)) DoRightReleased();

            if (Input.GetKeyDown(AryzonSettings.Controller.Up.Down)) DoUpDown();
            if (Input.GetKeyDown(AryzonSettings.Controller.Down.Down)) DoDownDown();
            if (Input.GetKeyDown(AryzonSettings.Controller.Left.Down)) DoLeftDown();
            if (Input.GetKeyDown(AryzonSettings.Controller.Right.Down)) DoRightDown();

            if (upDown && downDown)
            {
                DoUpReleased();
                DoDownReleased();
            }
            if (leftDown && rightDown)
            {
                DoLeftReleased();
                DoRightReleased();
            }

            float x = (upDown ? 1f : 0f) - (downDown ? 1f : 0f);
            float y = (rightDown ? 1f : 0f) - (leftDown ? 1f : 0f);

            if (!thumbWasDown && thumbDown)
            {
                thumb = new Vector2(x, y).normalized;
                DoThumbDown();
            }
            else if (thumbWasDown && !thumbDown)
            {
                thumb = Vector2.zero;
                DoThumbReleased();
            }

            thumbWasDown = thumbDown;
        }

        private void DoTriggerDown()
        {
            isUsed = true;
            triggerDown = true;
            OnTriggerDown?.Invoke();
        }

        private void DoMenuDown()
        {
            isUsed = true;
            menuDown = true;
            OnMenuDown?.Invoke();
        }

        private void DoExitDown()
        {
            isUsed = true;
            exitDown = true;
            OnExitDown?.Invoke();
        }

        private void DoADown()
        {
            isUsed = true;
            aDown = true;
            OnADown?.Invoke();
        }

        private void DoBDown()
        {
            isUsed = true;
            bDown = true;
            OnBDown?.Invoke();
        }

        private void DoXDown()
        {
            isUsed = true;
            xDown = true;
            OnXDown?.Invoke();
        }

        private void DoYDown()
        {
            isUsed = true;
            yDown = true;
            OnYDown?.Invoke();
        }

        private void DoUpDown()
        {
            isUsed = true;
            upDown = true;
        }

        private void DoDownDown()
        {
            isUsed = true;
            downDown = true;
        }

        private void DoLeftDown()
        {
            isUsed = true;
            leftDown = true;
        }

        private void DoRightDown()
        {
            isUsed = true;
            rightDown = true;
        }

        private void DoThumbDown()
        {
            isUsed = true;
            OnThumbDown?.Invoke(thumb);
        }



        private void DoTriggerReleased()
        {
            triggerDown = false;
            OnTriggerUp?.Invoke();
        }

        private void DoMenuReleased()
        {
            menuDown = false;
            OnMenuUp?.Invoke();
        }

        private void DoExitReleased()
        {
            exitDown = false;
            OnExitUp?.Invoke();
        }

        private void DoAReleased()
        {
            aDown = false;
            OnAUp?.Invoke();
        }

        private void DoBReleased()
        {
            bDown = false;
            OnBUp?.Invoke();
        }

        private void DoXReleased()
        {
            xDown = false;
            OnXUp?.Invoke();
        }

        private void DoYReleased()
        {
            yDown = false;
            OnYUp?.Invoke();
        }

        private void DoUpReleased()
        {
            upDown = false;
        }

        private void DoDownReleased()
        {
            downDown = false;
        }

        private void DoLeftReleased()
        {
            leftDown = false;
        }

        private void DoRightReleased()
        {
            rightDown = false;
        }

        private void DoThumbReleased()
        {
            OnThumbUp?.Invoke();
        }
    }
}

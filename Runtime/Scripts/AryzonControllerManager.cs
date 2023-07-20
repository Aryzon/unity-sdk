using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Aryzon;
using System.Runtime.InteropServices;
using OpenUp.Interpreter.EventSystem.Controllers;
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
                if (!wasConnected && cS == Connection.Connected) { OnControllerConnected?.Invoke(cS); wasConnected = true; }
                else if (wasConnected && cS != Connection.Connected) { OnControllerDisconnected?.Invoke(cS); wasConnected = false; }
                yield return new WaitForSeconds(1f);
            }
        }

        private void Update()
        {
            if (!activeOutsideAryzonMode && (!AryzonSettings.Instance.AryzonMode || !AryzonSettings.Instance.LandscapeMode)) return;

            if (triggerDown && Input.GetKeyDown(AryzonSettings.Controller.Trigger.Up)) TriggerReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.Trigger.Down)) TriggerDown();

            if (menuDown && Input.GetKeyDown(AryzonSettings.Controller.Menu.Up)) MenuReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.Menu.Down)) MenuDown();

            if (exitDown && Input.GetKeyDown(AryzonSettings.Controller.Exit.Up)) ExitReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.Exit.Down)) ExitDown();

            if (aDown && Input.GetKeyDown(AryzonSettings.Controller.A.Up)) AReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.A.Down)) ADown();

            if (bDown && Input.GetKeyDown(AryzonSettings.Controller.B.Up)) BReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.B.Down)) BDown();

            if (xDown && Input.GetKeyDown(AryzonSettings.Controller.X.Up)) XReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.X.Down)) XDown();

            if (yDown && Input.GetKeyDown(AryzonSettings.Controller.Y.Up)) YReleased();
            if (Input.GetKeyDown(AryzonSettings.Controller.Y.Down)) YDown();

            if (upDown && Input.GetKeyDown(AryzonSettings.Controller.Up.Up)) UpReleased();
            if (downDown && Input.GetKeyDown(AryzonSettings.Controller.Down.Up)) DownReleased();
            if (leftDown && Input.GetKeyDown(AryzonSettings.Controller.Left.Up)) LeftReleased();
            if (rightDown && Input.GetKeyDown(AryzonSettings.Controller.Right.Up)) RightReleased();

            if (Input.GetKeyDown(AryzonSettings.Controller.Up.Down)) UpDown();
            if (Input.GetKeyDown(AryzonSettings.Controller.Down.Down)) DownDown();
            if (Input.GetKeyDown(AryzonSettings.Controller.Left.Down)) LeftDown();
            if (Input.GetKeyDown(AryzonSettings.Controller.Right.Down)) RightDown();

            if (upDown && downDown)
            {
                UpReleased();
                DownReleased();
            }
            if (leftDown && rightDown)
            {
                LeftReleased();
                RightReleased();
            }

            float x = (upDown ? 1f : 0f) - (downDown ? 1f : 0f);
            float y = (rightDown ? 1f : 0f) - (leftDown ? 1f : 0f);

            if (!thumbWasDown && thumbDown)
            {
                thumb = new Vector2(x, y).normalized;
                ThumbDown();
            }
            else if (thumbWasDown && !thumbDown)
            {
                thumb = Vector2.zero;
                ThumbReleased();
            }

            thumbWasDown = thumbDown;
        }

        private void TriggerDown()
        {
            triggerDown = true;
            OnTriggerDown?.Invoke();
        }

        private void MenuDown()
        {
            menuDown = true;
            OnMenuDown?.Invoke();
        }

        private void ExitDown()
        {
            exitDown = true;
            OnExitDown?.Invoke();
        }

        private void ADown()
        {
            aDown = true;
            OnADown?.Invoke();
        }

        private void BDown()
        {
            bDown = true;
            OnBDown?.Invoke();
        }

        private void XDown()
        {
            xDown = true;
            OnXDown?.Invoke();
        }

        private void YDown()
        {
            yDown = true;
            OnYDown?.Invoke();
        }

        private void UpDown()
        {
            upDown = true;
        }

        private void DownDown()
        {
            downDown = true;
        }

        private void LeftDown()
        {
            leftDown = true;
        }

        private void RightDown()
        {
            rightDown = true;
        }

        private void ThumbDown()
        {
            OnThumbDown?.Invoke(thumb);
        }



        private void TriggerReleased()
        {
            triggerDown = false;
            OnTriggerUp?.Invoke();
        }

        private void MenuReleased()
        {
            menuDown = false;
            OnMenuUp?.Invoke();
        }

        private void ExitReleased()
        {
            exitDown = false;
            OnExitUp?.Invoke();
        }

        private void AReleased()
        {
            aDown = false;
            OnAUp?.Invoke();
        }

        private void BReleased()
        {
            bDown = false;
            OnBUp?.Invoke();
        }

        private void XReleased()
        {
            xDown = false;
            OnXUp?.Invoke();
        }

        private void YReleased()
        {
            yDown = false;
            OnYUp?.Invoke();
        }

        private void UpReleased()
        {
            upDown = false;
        }

        private void DownReleased()
        {
            downDown = false;
        }

        private void LeftReleased()
        {
            leftDown = false;
        }

        private void RightReleased()
        {
            rightDown = false;
        }

        private void ThumbReleased()
        {
            OnThumbUp?.Invoke();
        }
    }
}
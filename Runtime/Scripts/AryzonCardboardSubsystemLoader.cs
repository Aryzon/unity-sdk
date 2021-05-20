using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.XR;

using Google.XR.Cardboard;

namespace Aryzon
{
    public class AryzonCardboardSubsystemLoader
    {
        private static IntPtr _inputPointer;
        public static IntPtr inputPointer
        {
            get { if (isStarted) { return _inputPointer; } else { return IntPtr.Zero; } }
            set { _inputPointer = value; }
        }

        private static IntPtr _displayPointer;
        public static IntPtr displayPointer
        {
            get { if (isStarted) { return _displayPointer; } else { return IntPtr.Zero; } }
            set { _displayPointer = value; }
        }

        private XRLoader loader;

        public static bool isInitialized = false;
        public static bool isStarted = false;

        private string inputMatch = "Input";

        public void StartCardboard()
        {
            if (!loader)
            {
                loader = ScriptableObject.CreateInstance<XRLoader>();
                XRLoader.renderWidgets = false;
            }
#if !UNITY_EDITOR
            loader.Initialize();
#endif
            loader.Start();
            ConnectCardboardInputSystem();

            isStarted = true;

            ReloadDeviceParams();

            if (!Api.HasDeviceParams())
            {
                Api.ScanDeviceParams();
            }
        }

        public void StopCardboard()
        {
            if (loader)
            {
                loader.Stop();
                loader.Deinitialize();
            }
            isStarted = false;
        }

        public void ReloadDeviceParams()
        {
            if (!isStarted)
            {
                return;
            }
            Api.ReloadDeviceParams();
        }

        public void UpdateCardboard()
        {
            if (!isStarted)
            {
                return;
            }
            if (XRLoader.renderWidgets)
            {
                if (Api.IsGearButtonPressed)
                {
                    Api.ScanDeviceParams();
                }

                if (Api.IsCloseButtonPressed)
                {
                    Application.Quit();
                }
            }
            if (Api.HasNewDeviceParams())
            {
                Api.ReloadDeviceParams();
            }

            Api.UpdateScreenParams();
        }

        private void ConnectCardboardInputSystem()
        {
            List<XRInputSubsystemDescriptor> inputs = new List<XRInputSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(inputs);

            foreach (var d in inputs)
            {
                if (d.id.Equals(inputMatch))
                {
                    XRInputSubsystem inputInst = d.Create();

                    if (inputInst != null)
                    {
                        GCHandle handle = GCHandle.Alloc(inputInst);
                        inputPointer = GCHandle.ToIntPtr(handle);
                    }
                }
            }
        }
    }
}
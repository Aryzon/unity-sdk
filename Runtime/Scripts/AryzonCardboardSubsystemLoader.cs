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

        private static XRLoader loader;

        public static bool isInitialized = false;
        public static bool isStarted = false;

        private static string inputMatch = "Input";

        public static void StartCardboard()
        {
            if (!loader)
            {
                loader = ScriptableObject.CreateInstance<XRLoader>();
                //XRLoader.renderWidgets = false;
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

        public static void StopCardboard()
        {
            if (loader)
            {
                loader.Stop();
                loader.Deinitialize();
            }
            isStarted = false;
        }

        public static void ReloadDeviceParams()
        {
            if (!isStarted)
            {
                return;
            }
            if (!String.IsNullOrWhiteSpace(AryzonSettings.Headset.url))
            {
                Api.SaveDeviceParams(AryzonSettings.Headset.url);
            }
            if (AryzonSettings.Calibration.manualILD)
            {
                Api.SetInterLensDistance(AryzonSettings.Calibration.ILD);
            }
            if (AryzonSettings.Calibration.manualIPD)
            {
                Api.SetInterpupillaryDistance(AryzonSettings.Calibration.IPD);
            }
            Api.ReloadDeviceParams();
        }

        public static void UpdateCardboard()
        {
            if (!isStarted)
            {
                return;
            }
            
            if (Api.IsGearButtonPressed)
            {
                Api.ScanDeviceParams();
            }

            if (Api.IsCloseButtonPressed)
            {
                AryzonSettings.Instance.aryzonManager.StopAryzonMode();
            }

            if (Api.HasNewDeviceParams())
            {
                Api.ReloadDeviceParams();
            }

            Api.UpdateScreenParams();
        }

        private static void ConnectCardboardInputSystem()
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

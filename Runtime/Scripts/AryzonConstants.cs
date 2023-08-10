using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Aryzon.AryzonSettings.Controller;

namespace Aryzon
{
    public static class Constants
    {
#if UNITY_ANDROID
        public const string CardboardApi = "GfxPluginCardboard";
#elif UNITY_IOS
        public const string CardboardApi = "__Internal";
#else
        public const string CardboardApi = "NOT_AVAILABLE";
#endif

        public static class ControllerModeA
        {
#if UNITY_IOS
            public static KeyMap Trigger = new KeyMap(KeyCode.H, KeyCode.G);
            public static KeyMap Up = new KeyMap(KeyCode.W, KeyCode.E);
            public static KeyMap Down = new KeyMap(KeyCode.X, KeyCode.Z);
            public static KeyMap Right = new KeyMap(KeyCode.D, KeyCode.C);
            public static KeyMap Left = new KeyMap(KeyCode.A, KeyCode.Q);
            public static KeyMap Menu = new KeyMap(KeyCode.O, KeyCode.G);
            public static KeyMap Exit = new KeyMap(KeyCode.L, KeyCode.V);
            public static KeyMap A = new KeyMap();
            public static KeyMap B = new KeyMap(KeyCode.U, KeyCode.F);
            public static KeyMap X = new KeyMap();
            public static KeyMap Y = new KeyMap(KeyCode.J, KeyCode.N);
#else
            public static KeyMap Trigger = new KeyMap(KeyCode.Return, KeyCode.Return);
            public static KeyMap Up = new KeyMap(KeyCode.UpArrow, KeyCode.UpArrow);
            public static KeyMap Down = new KeyMap(KeyCode.DownArrow, KeyCode.DownArrow);
            public static KeyMap Right = new KeyMap(KeyCode.RightArrow, KeyCode.RightArrow);
            public static KeyMap Left = new KeyMap(KeyCode.LeftArrow, KeyCode.LeftArrow);
            public static KeyMap Menu = new KeyMap();
            public static KeyMap Exit = new KeyMap(KeyCode.L, KeyCode.V);
            public static KeyMap A = new KeyMap();
            public static KeyMap B = new KeyMap(KeyCode.JoystickButton1, KeyCode.JoystickButton1);
            public static KeyMap X = new KeyMap();
            public static KeyMap Y = new KeyMap(KeyCode.Menu, KeyCode.Menu);
#endif
        }

        public static class ControllerModeY
        {
#if UNITY_IOS
            public static KeyMap Trigger = new KeyMap(KeyCode.O, KeyCode.G);
            public static KeyMap Up = new KeyMap(KeyCode.A, KeyCode.Q);
            public static KeyMap Down = new KeyMap(KeyCode.D, KeyCode.C);
            public static KeyMap Right = new KeyMap(KeyCode.W, KeyCode.E);
            public static KeyMap Left = new KeyMap(KeyCode.X, KeyCode.Z);
            public static KeyMap Menu = new KeyMap();
            public static KeyMap Exit = new KeyMap(KeyCode.L, KeyCode.V);
            public static KeyMap A = new KeyMap(KeyCode.H, KeyCode.R);
            public static KeyMap B = new KeyMap(KeyCode.U, KeyCode.F);
            public static KeyMap X = new KeyMap(KeyCode.Y, KeyCode.T);
            public static KeyMap Y = new KeyMap(KeyCode.J, KeyCode.N);
#else
            public static KeyMap Trigger = new KeyMap(KeyCode.Return, KeyCode.Return);
            public static KeyMap Up = new KeyMap(KeyCode.UpArrow, KeyCode.UpArrow);
            public static KeyMap Down = new KeyMap(KeyCode.DownArrow, KeyCode.DownArrow);
            public static KeyMap Right = new KeyMap(KeyCode.RightArrow, KeyCode.RightArrow);
            public static KeyMap Left = new KeyMap(KeyCode.LeftArrow, KeyCode.LeftArrow);
            public static KeyMap Menu = new KeyMap();
            public static KeyMap Exit = new KeyMap(KeyCode.L, KeyCode.V);
            public static KeyMap A = new KeyMap();
            public static KeyMap B = new KeyMap(KeyCode.JoystickButton1, KeyCode.JoystickButton1);
            public static KeyMap X = new KeyMap();
            public static KeyMap Y = new KeyMap(KeyCode.Menu, KeyCode.Menu);
#endif
        }
    }
}

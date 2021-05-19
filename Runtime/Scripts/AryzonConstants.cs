using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aryzon
{
    public static class Constants
    {
#if UNITY_ANDROID
        public const string CardboardApi = "cardboard_api";
#elif UNITY_IOS
        public const string CardboardApi = "__Internal";
#else
        public const string CardboardApi = "NOT_AVAILABLE";
#endif
    }
}
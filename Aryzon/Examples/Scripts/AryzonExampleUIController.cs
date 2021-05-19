using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Aryzon;
public class AryzonExampleUIController : MonoBehaviour
{
    public GameObject UI;

    private UnityAction onStartAryzonMode;
    private UnityAction onStopAryzonMode;

    private void Awake()
    {
        onStartAryzonMode = OnStartAryzonMode;
        onStopAryzonMode = OnStopAryzonMode;
    }

    private void Start()
    {
        if (AryzonSettings.Instance.aryzonManager)
        {
            AryzonSettings.Instance.aryzonManager.onStart.AddListener(onStartAryzonMode);
            AryzonSettings.Instance.aryzonManager.onStop.AddListener(OnStopAryzonMode);
        }
    }

    private void OnStartAryzonMode()
    {
        UI.SetActive(false);
    }

    private void OnStopAryzonMode()
    {
        UI.SetActive(true);
    }

    private void OnDestroy()
    {
        if (AryzonSettings.Instance && AryzonSettings.Instance.aryzonManager)
        {
            AryzonSettings.Instance.aryzonManager.onStart.RemoveListener(onStartAryzonMode);
            AryzonSettings.Instance.aryzonManager.onStop.RemoveListener(OnStopAryzonMode);
        }
    }

    public void StartAryzonMode()
    {
        AryzonSettings.Instance.aryzonManager.StartAryzonMode();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aryzon
{
    public class AryzonControllerButtonUI : MonoBehaviour
    {
        public List<GameObject> ConnectedObjects;
        public List<GameObject> SetModeObjects;
        public List<GameObject> DisconnectedObjects;
        public List<GameObject> UnkownObjects;

        private void OnEnable()
        {
            if (AryzonControllerManager.Instance == null) { gameObject.SetActive(false); return; }
            AryzonControllerManager.Instance.OnControllerConnected.AddListener(ConnectionChanged);
            AryzonControllerManager.Instance.OnControllerDisconnected.AddListener(ConnectionChanged);
            AryzonControllerManager.Instance.OnModeChanged.AddListener(ModeChanged);
            ConnectionChanged(AryzonControllerManager.Instance.ConnectionStatus);
        }

        private void OnDisable()
        {
            if (AryzonControllerManager.Instance == null) { gameObject.SetActive(false); return; }
            AryzonControllerManager.Instance.OnControllerConnected.RemoveListener(ConnectionChanged);
            AryzonControllerManager.Instance.OnControllerDisconnected.RemoveListener(ConnectionChanged);
            AryzonControllerManager.Instance.OnModeChanged.RemoveListener(ModeChanged);
        }

        public void ModeChanged(int mode)
        {
            ConnectionChanged(AryzonControllerManager.Instance.ConnectionStatus);
        }

        public void ConnectionChanged(AryzonControllerManager.Connection connectionStatus)
        {
            if (connectionStatus == AryzonControllerManager.Connection.Connected)
            {
                if (AryzonControllerManager.Instance.CorrectMode == 1)
                {
                    foreach (GameObject obj in ConnectedObjects) obj.SetActive(true);
                    foreach (GameObject obj in SetModeObjects) obj.SetActive(false);
                    foreach (GameObject obj in DisconnectedObjects) obj.SetActive(false);
                    foreach (GameObject obj in UnkownObjects) obj.SetActive(false);
                }
                else
                {
                    foreach (GameObject obj in ConnectedObjects) obj.SetActive(false);
                    foreach (GameObject obj in SetModeObjects) obj.SetActive(true);
                    foreach (GameObject obj in DisconnectedObjects) obj.SetActive(false);
                    foreach (GameObject obj in UnkownObjects) obj.SetActive(false);
                }
            }
            else if (connectionStatus == AryzonControllerManager.Connection.Disconnected)
            {
                foreach (GameObject obj in ConnectedObjects) obj.SetActive(false);
                foreach (GameObject obj in SetModeObjects) obj.SetActive(false);
                foreach (GameObject obj in DisconnectedObjects) obj.SetActive(true);
                foreach (GameObject obj in UnkownObjects) obj.SetActive(false);
            }
            else
            {
                foreach (GameObject obj in ConnectedObjects) obj.SetActive(false);
                foreach (GameObject obj in SetModeObjects) obj.SetActive(false);
                foreach (GameObject obj in DisconnectedObjects) obj.SetActive(false);
                foreach (GameObject obj in UnkownObjects) obj.SetActive(true);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aryzon
{
    public class AryzonControllerUI : MonoBehaviour
    {
        public List<GameObject> ConnectedObjects;
        public List<GameObject> DisconnectedObjects;
        public List<GameObject> UnkownObjects;

        public GameObject explanationOverlay;

        private List<GameObject> cycleList = new List<GameObject>();

        private void OnEnable()
        {
            AryzonControllerManager.Instance.OnControllerConnected.AddListener(ConnectionChanged);
            AryzonControllerManager.Instance.OnControllerDisconnected.AddListener(ConnectionChanged);
            ConnectionChanged(AryzonControllerManager.Instance.ConnectionStatus);
        }

        private void OnDisable()
        {
            AryzonControllerManager.Instance.OnControllerConnected.RemoveListener(ConnectionChanged);
            AryzonControllerManager.Instance.OnControllerDisconnected.RemoveListener(ConnectionChanged);
        }

        public void ConnectionChanged(AryzonControllerManager.Connection connectionStatus)
        {
            if (connectionStatus == AryzonControllerManager.Connection.Connected)
            {
                foreach (GameObject obj in ConnectedObjects) obj.SetActive(true);
                foreach (GameObject obj in DisconnectedObjects) obj.SetActive(false);
                foreach (GameObject obj in UnkownObjects) obj.SetActive(false);

                explanationOverlay.SetActive(false);
            }
            else if (connectionStatus == AryzonControllerManager.Connection.Disconnected)
            {
                foreach (GameObject obj in ConnectedObjects) obj.SetActive(false);
                foreach (GameObject obj in DisconnectedObjects) obj.SetActive(true);
                foreach (GameObject obj in UnkownObjects) obj.SetActive(false);

                explanationOverlay.SetActive(true);
            }
            else
            {
                foreach (GameObject obj in ConnectedObjects) obj.SetActive(false);
                foreach (GameObject obj in DisconnectedObjects) obj.SetActive(false);
                foreach (GameObject obj in UnkownObjects) obj.SetActive(true);

                explanationOverlay.SetActive(true);
            }
        }
    }
}
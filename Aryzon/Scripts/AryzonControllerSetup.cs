using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Aryzon
{
    public class AryzonControllerSetup : MonoBehaviour
    {
        private float timer = 0f;
        private float maxTime = 60f;
        private bool listening = false;

        private bool downInUpdate = false;

        private KeyCode keyDown = KeyCode.None;
        private KeyCode keyUp = KeyCode.None;

        public Text status;
        public GameObject button;
        public GameObject cancelButton;
        public GameObject clearButton;

        private void OnEnable()
        {
            keyUp = AryzonSettings.Instance.controllerUpKeyCode;
            keyDown = AryzonSettings.Instance.controllerDownKeyCode;

            if (keyUp == KeyCode.None)
            {
                status.text = "";
                button.SetActive(true);
                cancelButton.SetActive(false);
                clearButton.SetActive(false);
            }
            else
            {
                if (keyDown == keyUp)
                {
                    status.text = "Using key \'" + keyUp + "\'";
                }
                else
                {
                    status.text = "Using keydown \'" + keyDown + "\', keyup \'" + keyUp + "\'";
                }
                button.SetActive(false);
                cancelButton.SetActive(false);
                clearButton.SetActive(true);
            }
        }

        public void Listen()
        {
            timer = 0f;
            status.text = "Listening...";
            button.SetActive(false);
            clearButton.SetActive(false);
            cancelButton.SetActive(true);

            StartCoroutine(StartListeningAfter(0.5f));
        }

        public void CancelListening()
        {
            listening = false;
            status.text = "";
            button.SetActive(true);
            cancelButton.SetActive(false);
            if (keyUp != KeyCode.None)
            {
                clearButton.SetActive(true);
            }
            else
            {
                clearButton.SetActive(false);
            }
        }


        public void ClearKey()
        {
            status.text = "";
            button.SetActive(true);
            clearButton.SetActive(false);
            keyDown = KeyCode.None;
            keyUp = KeyCode.None;
        }

        IEnumerator StartListeningAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            listening = true;
            downInUpdate = false;
        }




        public void Update()
        {
            if (listening)
            {
                foreach (KeyCode keyCode in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
                {
                    if (!downInUpdate && Input.GetKeyDown(keyCode))
                    {
                        downInUpdate = true;

                        AryzonSettings.Instance.controllerDownKeyCode = keyCode;
                        Debug.Log("Detected keyDown code: " + keyCode);
                        keyDown = keyCode;
                    }
                    if (downInUpdate && Input.GetKeyUp(keyCode))
                    {
                        AryzonSettings.Instance.controllerUpKeyCode = keyCode;
                        keyUp = keyCode;
                        Debug.Log("Detected keyUp code: " + keyCode);
                        if (keyDown == keyUp)
                        {
                            status.text = "Using key \'" + keyUp + "\'";
                        }
                        else
                        {
                            status.text = "Using keydown \'" + keyDown + "\', keyup \'" + keyUp + "\'";
                        }
                        listening = false;
                        cancelButton.SetActive(false);
                        clearButton.SetActive(true);
                    }
                }

                if (timer > maxTime)
                {
                    CancelListening();
                }
                timer += Time.deltaTime;
            }
        }

        private void OnDisable()
        {
            listening = false;
        }
    }
}
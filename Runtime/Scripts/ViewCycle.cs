using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aryzon
{
    public class ViewCycle : MonoBehaviour
    {
        public List<GameObject> cycleObjects;
        public float cycleTime = 2f;

        private GameObject currentObject;
        private float timer = 0f;

        void Update()
        {
            if (cycleObjects.Count == 0) return;
            if (timer > cycleTime)
            {
                timer = 0f;
                int index = cycleObjects.IndexOf(currentObject) + 1;
                if (index == cycleObjects.Count) index = 0;

                for (int i=0;i<cycleObjects.Count;i++)
                {
                    if (i != index) cycleObjects[i].SetActive(false);
                }
                currentObject = cycleObjects[index];
                currentObject.SetActive(true);
            }
            timer += Time.deltaTime;
        }

        private void OnEnable()
        {
            if (cycleObjects.Count > 0)
            {
                foreach (GameObject obj in cycleObjects)
                {
                    obj.SetActive(false);
                }

                currentObject = cycleObjects[0];
                currentObject.SetActive(true);
                timer = 0f;
            }
        }
    }
}
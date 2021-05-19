using UnityEngine;
using UnityEngine.Events;

namespace Aryzon
{
    // This class shows how to receive and handle reticle  events in your code.
    // Place this component on an object with a renderer, like a cube and its
    // material will change color when the reticle hovers over it.
    // It is recommended to duplicate and edit this code in your own class.

    [RequireComponent(typeof(AryzonRaycastObject))]
    public class AryzonExampleRaycastHandler : MonoBehaviour
    {
        public Color defaultColor = Color.white;
        public Color overColor = Color.cyan;
        public Color downColor = Color.yellow;
        public Color clickedColor = Color.magenta;

        private AryzonRaycastObject raycastObject;
        private Renderer objectRenderer;

        private void Awake()
        {
            raycastObject = gameObject.GetComponent<AryzonRaycastObject>();
            objectRenderer = gameObject.GetComponent<Renderer>();
            objectRenderer.material.color = defaultColor;
        }

        private void OnEnable()
        {
            if (raycastObject)
            {
                raycastObject.OnPointerOff.AddListener(Off);
                raycastObject.OnPointerOver.AddListener(Over);
                raycastObject.OnPointerUp.AddListener(Clicked);
                raycastObject.OnPointerDown.AddListener(Down);
            }
        }

        private void OnDisable()
        {
            if (raycastObject)
            {
                raycastObject.OnPointerOff.RemoveListener(Off);
                raycastObject.OnPointerOver.RemoveListener(Over);
                raycastObject.OnPointerUp.RemoveListener(Clicked);
                raycastObject.OnPointerDown.RemoveListener(Down);
            }
        }

        private void Off()
        {
            objectRenderer.material.color = defaultColor;
        }

        private void Over()
        {
            objectRenderer.material.color = overColor;
        }

        private void Down()
        {
            objectRenderer.material.color = downColor;
        }

        private void Clicked()
        {
            objectRenderer.material.color = clickedColor;
        }
    }
}
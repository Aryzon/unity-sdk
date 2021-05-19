using UnityEngine;
using UnityEngine.Events;

namespace Aryzon
{
    [RequireComponent(typeof(Collider))]
    public class AryzonRaycastObject : MonoBehaviour
    {
        public UnityEvent OnPointerDown;
        public UnityEvent OnPointerUp;
        public UnityEvent OnPointerOver;
        public UnityEvent OnPointerOff;

        public void PointerDown()
        {
            OnPointerDown.Invoke();
        }

        public void PointerUp()
        {
            OnPointerUp.Invoke();
        }

        public void PointerOver()
        {
            OnPointerOver.Invoke();
        }

        public void PointerOff()
        {
            OnPointerOff.Invoke();
        }
    }
}
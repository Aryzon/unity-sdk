using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aryzon
{
    public interface IAryzonReticleInteractionDelegate
    {
        public bool IsInteractable(GameObject raycastObject);

        public void PointerUp(GameObject raycastObject);
        public void PointerDown(GameObject raycastObject);
        public void PointerOver(GameObject raycastObject);
        public void PointerOff(GameObject raycastObject);
    }
}
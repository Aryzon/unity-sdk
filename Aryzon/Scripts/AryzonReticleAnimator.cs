using UnityEngine;

public class AryzonReticleAnimator : MonoBehaviour
{
    public Animator reticleAnimator;
    private int previousState = 0;
    public int state = 0;

    private void Start()
    {
        SetOffState();
    }

    public void SetOffState()
    {
        state = 0;
    }

    public void SetOverState()
    {
        state = 1;
    }

    public void SetClickedState()
    {
        state = 2;
    }

    private void Update()
    {
        if (previousState != state)
        {
            reticleAnimator.SetInteger("state", state);
            previousState = state;
        }
    }
}
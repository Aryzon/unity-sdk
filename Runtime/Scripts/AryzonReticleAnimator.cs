using UnityEngine;

public class AryzonReticleAnimator : MonoBehaviour
{
    public Animator reticleAnimator;
    public float defaultDistance = 0.7f;

    private int state = 0;
    public int State
    {
        get => state;
    }

    private int previousState = 0;

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

    public void SetPose(Pose p)
    {
        transform.position = p.position;
        float z = Mathf.Max(defaultDistance, transform.localPosition.z);
        Vector3 zScale = Vector3.one * (z / defaultDistance);

        transform.localScale = zScale;
    }

    public void SetDefaultPose()
    {
        SetDistance(defaultDistance);
    }

    public void SetDistance(float distance)
    {
        float z = Mathf.Max(defaultDistance, distance);

        Vector3 zScale = Vector3.one * (z / defaultDistance);

        transform.localPosition = new Vector3(0f, 0f, z);
        transform.localScale = zScale;
    }
}
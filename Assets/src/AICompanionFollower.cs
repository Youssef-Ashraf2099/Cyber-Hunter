using UnityEngine;

public class AICompanionFollower : MonoBehaviour
{
    public Transform arCamera;
    public float distanceFromCamera = 2f;
    public float heightOffset = -0.5f;

    void Update()
    {
        Vector3 forward = arCamera.forward;
        forward.y = 0; // keep on horizontal plane
        forward.Normalize();

        transform.position = arCamera.position + forward * distanceFromCamera + Vector3.up * heightOffset;
        transform.LookAt(arCamera); // face the camera
    }
}

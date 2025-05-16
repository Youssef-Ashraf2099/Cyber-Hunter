using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneDetectorManger : MonoBehaviour
{
    public ARPlaneManager planeManager;

    void Update()
    {
        foreach (var plane in planeManager.trackables)
        {
            Debug.Log($"Plane found at {plane.transform.position}, alignment: {plane.alignment}");
        }
    }
}

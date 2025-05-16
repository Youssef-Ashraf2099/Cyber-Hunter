using UnityEngine;

public class EasterEggSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EasterEgg
    {
        public GameObject prefab;
        public float spawnProbability = 0.1f;
        public float lifetime = 5f;
    }

    public EasterEgg[] easterEggs;

    [Header("Screen Spawn Bounds")]
    [Range(0f, 1f)] public float minX = 0.2f;
    [Range(0f, 1f)] public float maxX = 0.8f;
    [Range(0f, 1f)] public float minY = 0.2f;
    [Range(0f, 1f)] public float maxY = 0.8f;

    public float raycastDistance = 5f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        TrySpawnEasterEgg();
    }

    void TrySpawnEasterEgg()
    {
        foreach (var egg in easterEggs)
        {
            if (Random.value < egg.spawnProbability)
            {
                Vector3 screenPos = new Vector3(
                    Random.Range(minX, maxX) * Screen.width,
                    Random.Range(minY, maxY) * Screen.height,
                    0f
                );

                Ray ray = mainCamera.ScreenPointToRay(screenPos);

                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
                {
                    // Spawn at hit point
                    GameObject spawned = Instantiate(egg.prefab, hit.point, Quaternion.LookRotation(-mainCamera.transform.forward));
                    AddBehavior(spawned, egg);
                }
                else
                {
                    // Fallback: in front of camera
                    Vector3 fallbackPos = mainCamera.transform.position + mainCamera.transform.forward * 2f;
                    GameObject spawned = Instantiate(egg.prefab, fallbackPos, Quaternion.LookRotation(-mainCamera.transform.forward));
                    AddBehavior(spawned, egg);
                }

                break; // Spawn only one
            }
        }
    }

    void AddBehavior(GameObject obj, EasterEgg egg)
    {
        EasterEggBehavior behavior = obj.AddComponent<EasterEggBehavior>();
        behavior.SetParameters(egg.lifetime);
    }
}

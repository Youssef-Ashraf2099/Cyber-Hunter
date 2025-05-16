using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System
#if UNITY_2018_1_OR_NEWER
using Vuforia;
#endif

public class EasterEggManager : MonoBehaviour
{
    [System.Serializable]
    public class EasterEggItem
    {
        public GameObject prefab;
        public AudioClip soundEffect;
        [Range(0f, 100f)]
        public float spawnProbability = 5f; // Default 5% chance
        [Range(0f, 300f)]
        public float lifetimeSeconds = 60f; // How long the easter egg stays if not tapped
    }

    [Header("Easter Egg Settings")]
    public List<EasterEggItem> easterEggs = new List<EasterEggItem>();

    [Header("Spawn Settings")]
    public float checkInterval = 10f; // How often to check for spawning an easter egg (seconds)
    public float minSpawnDistance = 1.0f; // Minimum distance from camera
    public float maxSpawnDistance = 3.0f; // Maximum distance from camera
    public int maxConcurrentEasterEggs = 3; // Maximum number of easter eggs at once
    public LayerMask groundLayerMask; // Layer mask for ground detection

    [Header("Display Settings")]
    [Tooltip("Apply a scale multiplier to make prefabs more visible in AR")]
    public float prefabScaleMultiplier = 1.0f;
    [Tooltip("Set to true to make objects face the camera")]
    public bool faceCamera = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Header("Camera Settings")]
    public Camera vuforiaCamera; // Reference to your Vuforia camera

    // Input System reference
    private PlayerInput playerInput;
    private InputAction tapAction;

    private float nextCheckTime;
    private List<GameObject> activeEasterEggs = new List<GameObject>();
    private bool isSpawning = false;

    // Track currently tapped objects to prevent double-tapping
    private HashSet<GameObject> currentlyTappedObjects = new HashSet<GameObject>();

    // Debug variables
    private bool lastClickHit = false;
    private int spawnAttempts = 0;
    private int successfulSpawns = 0;

    private void Awake()
    {
        // Set up Input System
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            // Add PlayerInput component if it doesn't exist
            playerInput = gameObject.AddComponent<PlayerInput>();

            // Create a new input action asset at runtime
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = asset.AddActionMap("EasterEgg");
            tapAction = actionMap.AddAction("Tap", InputActionType.Button, "<Pointer>/press");
            tapAction.AddBinding("<Mouse>/leftButton");
            tapAction.AddBinding("<Touchscreen>/touch*/press");

            playerInput.actions = asset;
            playerInput.defaultActionMap = "EasterEgg";
        }
        else
        {
            // Get existing tap action if component already exists
            tapAction = playerInput.actions.FindAction("Tap");
        }

        // Enable the action and add callback
        tapAction.Enable();
        tapAction.performed += ctx => OnTap(ctx);
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        // Get current pointer position
        Vector2 screenPosition = Pointer.current.position.ReadValue();
        Debug.Log($"Tap detected at {screenPosition}");

        // Check if we hit an easter egg
        CheckForEasterEggHit(screenPosition);
    }

    private void Start()
    {
        // If no camera is assigned, try to find the main camera
        if (vuforiaCamera == null)
        {
            vuforiaCamera = Camera.main;
            Debug.Log("Using main camera: " + (vuforiaCamera != null ? vuforiaCamera.name : "None found"));
        }

        // If no AudioSource is assigned, create one
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("Created AudioSource component");
            }
        }

        // Initialize layer mask if not set
        if (groundLayerMask.value == 0)
        {
            groundLayerMask = Physics.DefaultRaycastLayers;
            Debug.Log("Using default raycast layers for ground detection");
        }


       
        // Set first check time
        nextCheckTime = Time.time + 10f; // First check after 1 second for testing



        Debug.Log("EasterEggManager started successfully");
    }

    private void Update()
    {
        // Check if it's time to potentially spawn an easter egg
        if (Time.time >= nextCheckTime && !isSpawning)
        {
            CheckForEasterEggSpawn();
            nextCheckTime = Time.time + checkInterval;
        }

        // Update facing for easter eggs if needed
        if (faceCamera && activeEasterEggs.Count > 0)
        {
            foreach (var egg in activeEasterEggs)
            {
                if (egg != null)
                {
                    // Make the egg face the camera
                    Vector3 dirToCamera = vuforiaCamera.transform.position - egg.transform.position;
                    dirToCamera.y = 0; // Keep the egg upright (no tilt)
                    if (dirToCamera != Vector3.zero)
                    {
                        egg.transform.rotation = Quaternion.LookRotation(dirToCamera);
                    }
                }
            }
        }
    }

    private void CheckForEasterEggSpawn()
    {
        // Don't spawn more than the max concurrent easter eggs
        if (activeEasterEggs.Count >= maxConcurrentEasterEggs)
        {
            Debug.Log($"Not spawning: already have {activeEasterEggs.Count} easter eggs active");
            return;
        }

        spawnAttempts++;
        Debug.Log($"Checking for easter egg spawn, attempt #{spawnAttempts}");

        foreach (EasterEggItem item in easterEggs)
        {
            // Roll for chance to spawn this easter egg
            float roll = Random.Range(0f, 100f);
            if (roll <= item.spawnProbability)
            {
                Debug.Log($"Easter egg will spawn! Roll: {roll}, Probability: {item.spawnProbability}");
                StartCoroutine(SpawnEasterEggRoutine(item));
                return; // Only try to spawn one easter egg per check
            }
        }

        Debug.Log("No easter eggs spawned this check");
    }

    private IEnumerator SpawnEasterEggRoutine(EasterEggItem item)
    {
        isSpawning = true;

        // Try to find a valid surface to place the easter egg
        bool placedSuccessfully = false;
        int maxAttempts = 5;
        int attempts = 0;

        while (!placedSuccessfully && attempts < maxAttempts)
        {
            attempts++;

            // Try to find a surface using raycasting
            Vector3 spawnPosition;
            if (TryFindSurfacePosition(out spawnPosition))
            {
                // Place on the detected surface
                GameObject easterEgg = Instantiate(item.prefab, spawnPosition, Quaternion.identity);

                // Apply scale multiplier to make it more visible in AR
                easterEgg.transform.localScale *= prefabScaleMultiplier;

                // Make sure the easter egg is facing upright with random rotation on Y axis
                if (!faceCamera)
                {
                    easterEgg.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
                else
                {
                    // Face the camera initially
                    Vector3 dirToCamera = vuforiaCamera.transform.position - easterEgg.transform.position;
                    dirToCamera.y = 0; // Keep upright
                    if (dirToCamera != Vector3.zero)
                    {
                        easterEgg.transform.rotation = Quaternion.LookRotation(dirToCamera);
                    }
                }

                // Make sure the collider is enabled and correct
                EnsureCollider(easterEgg);

                // Add EasterEggData component to store reference to the sound
                EasterEggData eggData = easterEgg.AddComponent<EasterEggData>();
                eggData.soundEffect = item.soundEffect;

                // Add to active list
                activeEasterEggs.Add(easterEgg);

                // Set up auto-destruction after lifetime
                StartCoroutine(DestroyAfterLifetime(easterEgg, item.lifetimeSeconds));

                placedSuccessfully = true;
                successfulSpawns++;
                Debug.Log($"Easter egg placed successfully on surface! Total successful spawns: {successfulSpawns}");
            }
            else
            {
                Debug.Log($"No surface found on attempt {attempts}, trying again");
                yield return new WaitForSeconds(0.2f);
            }
        }

        // If we couldn't find a surface, fall back to placing in front of the camera
        if (!placedSuccessfully)
        {
            Debug.Log("Falling back to camera-relative placement");

            // Place in front of camera
            Vector3 randomPosition = GetRandomPositionInView();

            // Instantiate the easter egg
            GameObject easterEgg = Instantiate(item.prefab, randomPosition, Quaternion.identity);

            // Apply scale multiplier
            easterEgg.transform.localScale *= prefabScaleMultiplier;

            // Random rotation on y-axis or face camera
            if (!faceCamera)
            {
                easterEgg.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
            else
            {
                // Face the camera
                easterEgg.transform.LookAt(vuforiaCamera.transform);
                // Keep upright
                Vector3 eulerAngles = easterEgg.transform.eulerAngles;
                easterEgg.transform.eulerAngles = new Vector3(0, eulerAngles.y, 0);
            }

            // Make sure the collider is enabled and correct
            EnsureCollider(easterEgg);

            // Add EasterEggData component to store reference to the sound
            EasterEggData eggData = easterEgg.AddComponent<EasterEggData>();
            eggData.soundEffect = item.soundEffect;

            // Add to active list
            activeEasterEggs.Add(easterEgg);

            // Set up auto-destruction after lifetime
            StartCoroutine(DestroyAfterLifetime(easterEgg, item.lifetimeSeconds));

            successfulSpawns++;
            Debug.Log($"Easter egg placed using fallback method! Total successful spawns: {successfulSpawns}");
        }

        isSpawning = false;
    }

    private bool TryFindSurfacePosition(out Vector3 position)
    {
        position = Vector3.zero;

        if (vuforiaCamera == null)
        {
            Debug.LogError("No camera assigned for surface detection");
            return false;
        }

        // Try a few random positions in the camera's view
        for (int i = 0; i < 5; i++)
        {
            // Create a ray from a random point on screen
            Vector2 screenPos = new Vector2(
                Random.Range(Screen.width * 0.3f, Screen.width * 0.7f),
                Random.Range(Screen.height * 0.3f, Screen.height * 0.7f)
            );

            Ray ray = vuforiaCamera.ScreenPointToRay(screenPos);
            RaycastHit hit;

            // Cast ray to find surfaces
            if (Physics.Raycast(ray, out hit, 10f, groundLayerMask))
            {
                // We found a surface
                position = hit.point + Vector3.up * 0.05f; // Small offset to avoid z-fighting
                return true;
            }
        }

        // Try directly in front of the camera
        Ray centerRay = vuforiaCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit centerHit;

        if (Physics.Raycast(centerRay, out centerHit, 10f, groundLayerMask))
        {
            position = centerHit.point + Vector3.up * 0.05f;
            return true;
        }

        return false;
    }

    private void EnsureCollider(GameObject obj)
    {
        // Check if the object or any of its children have a collider
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
        {
            // No collider found, add a box collider
            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();

            // Automatically size the box collider based on children
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                // Start with the first renderer's bounds
                Bounds bounds = renderers[0].bounds;

                // Expand to include all renderers
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                // Convert to local space
                bounds.center = obj.transform.InverseTransformPoint(bounds.center);
                boxCollider.center = bounds.center;
                boxCollider.size = bounds.size;
            }

            Debug.Log("Added BoxCollider to easter egg");
        }
        else
        {
            // Make sure all colliders are enabled
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
            Debug.Log($"Easter egg already has {colliders.Length} colliders");
        }
    }

    private Vector3 GetRandomPositionInView()
    {
        if (vuforiaCamera == null)
        {
            Debug.LogError("No camera assigned for view position");
            return Vector3.forward * 2f;
        }

        // Get random position within camera's view at a fixed distance
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Get a random point within the camera's view frustum
        // Adjusted to be more centered in the view
        float horizontalFov = Random.Range(-0.3f, 0.3f); // Reduced horizontal range
        float verticalFov = Random.Range(-0.2f, 0.2f); // Reduced vertical range

        // Create position in front of camera
        Vector3 directionVector = vuforiaCamera.transform.forward;
        directionVector += vuforiaCamera.transform.right * horizontalFov;
        directionVector += vuforiaCamera.transform.up * verticalFov;
        directionVector.Normalize();

        return vuforiaCamera.transform.position + directionVector * distance;
    }

    private void CheckForEasterEggHit(Vector2 screenPosition)
    {
        if (vuforiaCamera == null)
        {
            Debug.LogError("No camera assigned for hit detection");
            return;
        }

        Ray ray = vuforiaCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        Debug.Log($"Casting ray from screen position: {screenPosition}");

        // Use a larger max distance for the raycast to ensure we catch all objects
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log($"Hit something: {hit.collider.gameObject.name}");

            // Check direct hit
            GameObject hitObject = hit.collider.gameObject;
            EasterEggData eggData = hitObject.GetComponent<EasterEggData>();

            // If not found on the direct hit, check in parents
            if (eggData == null)
            {
                eggData = hitObject.GetComponentInParent<EasterEggData>();
                if (eggData != null)
                {
                    hitObject = eggData.gameObject;
                }
            }

            if (eggData != null)
            {
                // Check if we're already processing this object
                if (currentlyTappedObjects.Contains(hitObject))
                {
                    Debug.Log("Already processing this easter egg, ignoring additional tap");
                    return;
                }

                Debug.Log("Hit an easter egg!");
                lastClickHit = true;

                // Add to currently tapped objects
                currentlyTappedObjects.Add(hitObject);

                // Play sound effect
                if (eggData.soundEffect != null && audioSource != null)
                {
                    Debug.Log($"Playing sound: {eggData.soundEffect.name}");
                    audioSource.PlayOneShot(eggData.soundEffect);
                }
                else
                {
                    Debug.LogWarning("Sound effect or audio source is missing");
                }

                // Remove from active list if it's in there
                if (activeEasterEggs.Contains(hitObject))
                {
                    activeEasterEggs.Remove(hitObject);
                }

                // Destroy after sound finishes playing (if there's a sound)
                float destroyDelay = eggData.soundEffect != null ? eggData.soundEffect.length : 0f;
                destroyDelay = Mathf.Max(destroyDelay, 0.5f); // At least 0.5 seconds
                StartCoroutine(DestroyAfterDelay(hitObject, destroyDelay));
            }
            else
            {
                Debug.Log("Hit object is not an easter egg");
                lastClickHit = false;
            }
        }
        else
        {
            Debug.Log("Ray didn't hit anything");
            lastClickHit = false;
        }
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        Debug.Log($"Will destroy easter egg after {delay} seconds");

        // Optional: Add some effect to show it's being collected
        if (obj != null)
        {
            // Simple scale down animation
            float duration = delay;
            float startTime = Time.time;
            Vector3 startScale = obj.transform.localScale;

            while (Time.time < startTime + duration && obj != null)
            {
                float t = (Time.time - startTime) / duration;
                obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            // Destroy the object
            if (obj != null)
            {
                Debug.Log("Destroying easter egg now");
                currentlyTappedObjects.Remove(obj);
                Destroy(obj);
            }
        }
    }

    private IEnumerator DestroyAfterLifetime(GameObject obj, float lifetime)
    {
        Debug.Log($"Easter egg will auto-destroy after {lifetime} seconds if not collected");
        yield return new WaitForSeconds(lifetime);

        // Remove from active list if it's still there
        if (obj != null && activeEasterEggs.Contains(obj))
        {
            Debug.Log("Auto-destroying uncollected easter egg");
            activeEasterEggs.Remove(obj);
            currentlyTappedObjects.Remove(obj);
            Destroy(obj);
        }
    }
}

// Helper class to store data for each easter egg instance
public class EasterEggData : MonoBehaviour
{
    public AudioClip soundEffect;
}
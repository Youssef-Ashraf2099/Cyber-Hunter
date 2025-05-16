using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HiddenMSGManager : MonoBehaviour
{
    [System.Serializable]
    public class HiddenEasterEgg
    {
        public string name;
        public GameObject eggObject; // Contains both Image and Button
        public AudioClip sound;
        [Range(0f, 1f)]
        public float probability = 1f;
        [Tooltip("Delay before this egg can appear again (seconds)")]
        public float cooldown = 10f;
    }

    [Header("Configuration")]
    public float minSpawnInterval = 15f;
    public float maxSpawnInterval = 45f;
    public float eggLifetime = 5f;
    [Range(0f, 1f)]
    public float spawnChance = 0.7f;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Easter Eggs")]
    public List<HiddenEasterEgg> easterEggs;

    private HiddenEasterEgg currentEgg;
    private bool isActive = false;
    private Dictionary<HiddenEasterEgg, float> eggCooldowns = new Dictionary<HiddenEasterEgg, float>();

    void Awake()
    {
        // Validate all egg objects and components
        foreach (var egg in easterEggs)
        {
            if (egg.eggObject == null)
            {
                Debug.LogError($"Easter Egg '{egg.name}' has no GameObject assigned!", this);
                continue;
            }

            // Ensure components exist
            if (egg.eggObject.GetComponent<Image>() == null)
            {
                Debug.LogError($"Easter Egg '{egg.name}' is missing Image component!", egg.eggObject);
            }

            if (egg.eggObject.GetComponent<Button>() == null)
            {
                Debug.LogError($"Easter Egg '{egg.name}' is missing Button component!", egg.eggObject);
            }

            // Initialize as inactive
            egg.eggObject.SetActive(false);
        }
    }

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        // Update cooldowns
        if (eggCooldowns.Count > 0)
        {
            List<HiddenEasterEgg> keys = new List<HiddenEasterEgg>(eggCooldowns.Keys);
            foreach (var egg in keys)
            {
                eggCooldowns[egg] -= Time.deltaTime;
                if (eggCooldowns[egg] <= 0)
                {
                    eggCooldowns.Remove(egg);
                }
            }
        }
    }

    private void ScheduleNextSpawn()
    {
        if (!isActive && this != null && gameObject.activeInHierarchy)
        {
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
            Invoke(nameof(TrySpawnEasterEgg), delay);
        }
    }

    private void TrySpawnEasterEgg()
    {
        if (Random.value <= spawnChance)
        {
            ShowRandomEasterEgg();
        }
        else
        {
            ScheduleNextSpawn();
        }
    }

    private void ShowRandomEasterEgg()
    {
        currentEgg = GetAvailableEasterEgg();
        if (currentEgg == null)
        {
            ScheduleNextSpawn();
            return;
        }

        // Activate the GameObject
        currentEgg.eggObject.SetActive(true);
        isActive = true;

        // Set up button click
        Button btn = currentEgg.eggObject.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnEggClicked);

        // Set up automatic hide
        Invoke(nameof(HideCurrentEgg), eggLifetime);

        // Put egg on cooldown
        eggCooldowns[currentEgg] = currentEgg.cooldown;
    }

    private HiddenEasterEgg GetAvailableEasterEgg()
    {
        if (easterEggs == null || easterEggs.Count == 0)
        {
            Debug.LogWarning("No easter eggs configured!");
            return null;
        }

        float total = 0f;
        foreach (var egg in easterEggs)
        {
            if (!eggCooldowns.ContainsKey(egg))
            {
                total += egg.probability;
            }
        }

        if (total <= 0) return null;

        float rand = Random.value * total;
        float cumulative = 0f;
        foreach (var egg in easterEggs)
        {
            if (!eggCooldowns.ContainsKey(egg))
            {
                cumulative += egg.probability;
                if (rand <= cumulative)
                {
                    return egg;
                }
            }
        }

        return null;
    }

    private void OnEggClicked()
    {
        if (!isActive || currentEgg == null) return;

        // Play sound if available
        if (currentEgg.sound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(currentEgg.sound);
            }
            else
            {
                Debug.LogWarning("AudioSource is not assigned!", this);
            }
        }

        HideCurrentEgg();
    }

    private void HideCurrentEgg()
    {
        if (!isActive || currentEgg == null) return;

        // Cancel any pending hide invokes
        CancelInvoke(nameof(HideCurrentEgg));

        // Hide the egg
        currentEgg.eggObject.SetActive(false);
        isActive = false;

        // Schedule next spawn
        ScheduleNextSpawn();
    }

    // For debugging
    public void ForceSpawnEasterEgg()
    {
        CancelInvoke(nameof(TrySpawnEasterEgg));
        ShowRandomEasterEgg();
    }
}
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private GameObject targetPrefab; // Assign the prefab in the Inspector
    [SerializeField] private string animationName;    // Name of the animation to play

    private Animator animator;

    void Start()
    {
        if (targetPrefab != null)
        {
            animator = targetPrefab.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on the target prefab.");
            }
        }
        else
        {
            Debug.LogError("Target prefab is not assigned.");
        }
    }

    void OnBecameVisible()
    {
        if (animator != null)
        {
            animator.Play(animationName, 0, 0f); // Start the animation from the beginning
            animator.speed = 1f; // Ensure the animation loops
        }
    }

    void OnBecameInvisible()
    {
        if (animator != null)
        {
            animator.speed = 0f; // Pause the animation when the prefab is not visible
        }
    }
}

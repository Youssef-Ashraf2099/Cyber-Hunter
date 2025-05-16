using UnityEngine;

public class EasterEggBehavior : MonoBehaviour
{
    [SerializeField] private AnimationClip animationClip; // Assign in Inspector
    [SerializeField] private AudioClip soundClip;         // Assign in Inspector
    [SerializeField] private float lifetime = 5f;

    private bool clicked = false;
    private AudioSource audioSource;
    private Animation animationComponent;

    void Start()
    {
        // Add or use existing Animation component
        animationComponent = gameObject.GetComponent<Animation>();
        if (animationComponent == null)
            animationComponent = gameObject.AddComponent<Animation>();

        if (animationClip != null)
        {
            animationComponent.clip = animationClip;
            animationComponent.playAutomatically = false;
            animationComponent.AddClip(animationClip, animationClip.name);
        }

        // Add or use existing AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = soundClip;
        audioSource.playOnAwake = false;

        Invoke(nameof(DestroyIfNotClicked), lifetime);
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;

        if (animationClip != null)
            animationComponent.Play(animationClip.name);

        if (soundClip != null)
            audioSource.Play();

        float destroyTime = Mathf.Max(
            animationClip != null ? animationClip.length : 0f,
            soundClip != null ? soundClip.length : 0f,
            1f
        );

        Destroy(gameObject, destroyTime);
    }

    void DestroyIfNotClicked()
    {
        if (!clicked)
            Destroy(gameObject);
    }
    public void SetParameters(float lifetime)
    {
        this.lifetime = lifetime;
    }

}

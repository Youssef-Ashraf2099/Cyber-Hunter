using UnityEngine;

public class AICompanionBehavior : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    [Header("Voice Clips")]
    public AudioClip[] idleComments;
    public AudioClip cheerClip;
    public AudioClip helpClip;
    public AudioClip greetingClip;

    [Header("Timing")]
    public float idleTalkInterval = 10f;
    private float idleTimer = 0f;

    private bool isTalking = false;

    void Update()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTalkInterval && !isTalking)
        {
            PlayRandomIdleLine();
        }
    }

    public void OnGameStart()
    {
        Speak(greetingClip, "Wave");
    }

    public void OnQuestionnaireCompleted()
    {
        Speak(cheerClip, "Cheer");
    }

    public void OnPlayerLooksLost()
    {
        Speak(helpClip, "Talk");
    }

    private void PlayRandomIdleLine()
    {
        if (idleComments.Length == 0) return;
        int index = Random.Range(0, idleComments.Length);
        Speak(idleComments[index], "Talk");
    }

    private void Speak(AudioClip clip, string animationTrigger)
    {
        if (clip == null) return;
        isTalking = true;
        animator.SetTrigger(animationTrigger);
        audioSource.PlayOneShot(clip);
        idleTimer = 0f;

        Invoke(nameof(EndTalking), clip.length);
    }

    private void EndTalking()
    {
        isTalking = false;
    }
}

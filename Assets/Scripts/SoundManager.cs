using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioClip flipClip;
    public AudioClip matchClip;
    public AudioClip mismatchClip;
    public AudioClip gameOverClip;
    private AudioSource source;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public void PlayFlip() => Play(flipClip);
    public void PlayMatch() => Play(matchClip);
    public void PlayMismatch() => Play(mismatchClip);
    public void PlayGameOver() => Play(gameOverClip);

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        source.PlayOneShot(clip);
    }
}

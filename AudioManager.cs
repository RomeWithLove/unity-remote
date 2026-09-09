using System.Collections;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip ambientCrowdLoop;
    [SerializeField] private AudioClip matchStartRoarClip;
    [SerializeField] private AudioClip tackleFumbleRoarClip;
    [SerializeField] private AudioClip victoryRoarClip;

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float ambientBaseVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float roarVolume = 1.0f;
    [SerializeField] private float fadeDuration = 1.5f;

    protected override void Awake()
    {
        base.Awake();

        AudioSource[] sources = GetComponents<AudioSource>();
        if (ambientSource == null && sources.Length > 0)
        {
            ambientSource = sources[0];
        }

        if (sfxSource == null && sources.Length > 1)
        {
            sfxSource = sources[1];
        }
    }

    private void Start()
    {
        PlayAmbientCrowd();
    }

    public void PlayAmbientCrowd()
    {
        if (ambientSource != null && ambientCrowdLoop != null)
        {
            ambientSource.clip = ambientCrowdLoop;
            ambientSource.loop = true;
            ambientSource.volume = ambientBaseVolume;
            ambientSource.Play();
        }
    }

    public void PlayMatchStartCheer() => PlayCrowdRoar(matchStartRoarClip);
    public void PlayTackleRoar() => PlayCrowdRoar(tackleFumbleRoarClip);

    public void PlayVictoryCheer()
    {
        PlayCrowdRoar(victoryRoarClip);
        StartCoroutine(SwellAmbientVolume(1.0f, fadeDuration));
    }

    private void PlayCrowdRoar(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, roarVolume);
        }
    }

    private IEnumerator SwellAmbientVolume(float targetVolume, float duration)
    {
        if (ambientSource == null) yield break;

        float startVolume = ambientSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            ambientSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ambientSource.volume = targetVolume;
    }
}
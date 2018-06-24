using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioLibrary))]
public class AudioManager : MonoBehaviour {

    public static AudioManager instance;
    private AudioLibrary library;
    [SerializeField] bool persistentAudioSources = false;

    List<AudioSource> availableAudio = new List<AudioSource>();
    List<AudioSource> playingAudio = new List<AudioSource>();

    [Header("Footstep Settings")]
    [SerializeField] string physicsMaterialPrefix = "PM_";
    [SerializeField] string footstepPrefix = "Footstep ";

    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            library = GetComponent<AudioLibrary>();
            StartCoroutine(CheckPlayingAudio());
        }
    }

    public void PlaySound(AudioClip clip)
    {
        SetUpAudioSource(clip, 0f, Vector3.zero, 1f, 1f);
    }

    public void PlaySound(AudioClip clip, float pitch)
    {
        SetUpAudioSource(clip, 0f, Vector3.zero, 1f, pitch);
    }

    public void PlaySound(AudioClip clip, float volume, float pitch)
    {
        SetUpAudioSource(clip, 0f, Vector3.zero, volume, pitch);
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        SetUpAudioSource(clip, 1f, position, 1f, 1f);
    }

    public void PlaySound(AudioClip clip, Vector3 position, float pitch)
    {
        SetUpAudioSource(clip, 1f, position, 1f, pitch);
    }

    public void PlaySound(AudioClip clip, Vector3 position, float volume, float pitch)
    {
        SetUpAudioSource(clip, 1f, position, volume, pitch);
    }

    public void PlaySound(string clip)
    {
        SetUpAudioSource(library.GetClipFromName(clip), 0f, Vector3.zero, 1f, 1f);
    }

    public void PlaySound(string clip, float pitch)
    {
        SetUpAudioSource(library.GetClipFromName(clip), 0f, Vector3.zero, 1f, pitch);
    }

    public void PlaySound(string clip, float volume, float pitch)
    {
        SetUpAudioSource(library.GetClipFromName(clip), 0f, Vector3.zero, volume, pitch);
    }

    public void PlaySound(string clip, Vector3 position)
    {
        SetUpAudioSource(library.GetClipFromName(clip), 1f, position, 1f, 1f);
    }

    public void PlaySound(string clip, Vector3 position, float pitch)
    {
        SetUpAudioSource(library.GetClipFromName(clip), 1f, position, 1f, pitch);
    }

    public void PlaySound(string clip, Vector3 position, float volume, float pitch)
    {
        SetUpAudioSource(library.GetClipFromName(clip), 1f, position, volume, pitch);
    }

    public void PlayFootstep(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit))
        {
            PhysicMaterial mat = hit.collider.material;
            string matName = mat.name;
            string clip = matName.Substring(physicsMaterialPrefix.Length, matName.Length - physicsMaterialPrefix.Length - 11);
            clip = footstepPrefix + clip;
            PlaySound(library.GetClipFromName(clip), hit.point, Random.Range(0.8f, 1.2f));
        }
    }

    void SetUpAudioSource(AudioClip clip, float spatial, Vector3 position, float volume, float pitch)
    {
        if (clip == null)
            return;
        AudioSource source = GetAudioSource();
        source.clip = clip;
        source.spatialBlend = spatial;
        source.transform.position = position;
        source.volume = volume;
        source.pitch = pitch;

        source.Play();
        if (persistentAudioSources)
            playingAudio.Add(source);
        else
            Destroy(source.gameObject, clip.length);

        source.transform.SetParent(transform);
    }

    AudioSource GetAudioSource()
    {
        if (persistentAudioSources)
        {
            if (availableAudio.Count == 0)
            {
                GameObject g = new GameObject("AudioSource");
                AudioSource source = g.AddComponent<AudioSource>();
                return source;
            }
            else
            {
                AudioSource source = availableAudio[0];
                source.enabled = true;
                availableAudio.Remove(source);
                return source;
            }
        }
        else
        {
            GameObject g = new GameObject("AudioSource");
            AudioSource source = g.AddComponent<AudioSource>();
            return source;
        }
    }

    IEnumerator CheckPlayingAudio()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);
            if (playingAudio.Count > 0)
            {
                List<AudioSource> finishedAudio = new List<AudioSource>();
                foreach (AudioSource source in playingAudio)
                {
                    if (!source.isPlaying)
                    {
                        finishedAudio.Add(source);
                        source.enabled = false;
                    }
                }
                foreach(AudioSource source in finishedAudio)
                {
                    playingAudio.Remove(source);
                    availableAudio.Add(source);
                }
            }
        }
    }

}

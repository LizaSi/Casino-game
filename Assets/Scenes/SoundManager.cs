using System.Collections.Generic;
using UnityEngine;

public enum SoundName
{
    Win,
    Lose
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public SoundName soundName;
        public AudioClip clip;      
        [HideInInspector]
        public AudioSource source;
    }

    public List<Sound> sounds;   

    public void PlaySound(SoundName soundName)
    {
        var sound = sounds.Find(s => s.soundName == soundName);
        if (sound != null)
        {
            sound.source.PlayOneShot(sound.clip);
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
        }
    }
}
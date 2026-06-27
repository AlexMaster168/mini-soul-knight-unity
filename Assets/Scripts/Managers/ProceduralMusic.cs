using UnityEngine;

public class ProceduralMusic : MonoBehaviour
{
    public static ProceduralMusic Instance;

    private AudioSource musicSource;
    private AudioClip dungeonClip;
    private AudioClip bossClip;
    private AudioClip victoryClip;
    private bool isBossTheme;
    private bool isVictory;
    private float masterVolume = 0.4f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = masterVolume;
        musicSource.playOnAwake = false;

        dungeonClip = Resources.Load<AudioClip>("Music/DungeonTheme");
        bossClip = Resources.Load<AudioClip>("Music/BossTheme");
        victoryClip = Resources.Load<AudioClip>("Music/VictoryTheme");

        if (dungeonClip == null)
        {
            dungeonClip = Resources.Load<AudioClip>("Music/DungeonTheme.mp3");
            bossClip = Resources.Load<AudioClip>("Music/BossTheme.mp3");
        }

        StartMusic();
    }

    public void StartMusic()
    {
        if (dungeonClip == null) return;
        isBossTheme = false;
        isVictory = false;
        musicSource.clip = dungeonClip;
        musicSource.volume = masterVolume;
        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StartBossMusic()
    {
        if (bossClip == null) return;
        isBossTheme = true;
        isVictory = false;
        musicSource.clip = bossClip;
        musicSource.volume = masterVolume;
        musicSource.Play();
    }

    public void StartVictoryMusic()
    {
        isVictory = true;
        isBossTheme = false;
        musicSource.volume = masterVolume;

        if (victoryClip != null)
        {
            musicSource.clip = victoryClip;
            musicSource.loop = false;
            musicSource.Play();
            Invoke(nameof(ResumeDungeonMusic), victoryClip.length);
        }
        else
        {
            PlayVictoryJingle();
        }
    }

    void ResumeDungeonMusic()
    {
        musicSource.loop = true;
        if (dungeonClip != null)
        {
            musicSource.clip = dungeonClip;
            musicSource.Play();
        }
        isVictory = false;
    }

    void PlayVictoryJingle()
    {
        int[] notes = { 72, 72, 72, 79, 84, 79, 84, 88, 84, 79, 76, 79, 84, 88, 91, 88, 84, 79, 76, 72 };
        float bpm = 140f;
        float beatLen = 60f / bpm / 2f;

        for (int i = 0; i < notes.Length; i++)
        {
            float freq = 440f * Mathf.Pow(2f, (notes[i] - 69) / 12f);
            PlayJingleNote(freq, 0.15f, beatLen * 0.9f, i * beatLen);
        }

        Invoke(nameof(ResumeDungeonMusic), notes.Length * beatLen + 0.5f);
    }

    void PlayJingleNote(float freq, float volume, float duration, float delay)
    {
        int sampleRate = 44100;
        int sampleLen = (int)(sampleRate * duration);
        float[] samples = new float[sampleLen];

        for (int i = 0; i < sampleLen; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 6f) * (1f - Mathf.Exp(-t * 100f));
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.15f;
            samples[i] = wave * env * volume;
        }

        AudioClip clip = AudioClip.Create("jingle", sampleLen, 1, sampleRate, false);
        clip.SetData(samples, 0);

        GameObject noteObj = new GameObject("JingleNote");
        AudioSource src = noteObj.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = masterVolume;
        src.Play();
        Destroy(noteObj, duration + delay + 0.1f);
    }

    public void SetBossMode(bool boss)
    {
        if (boss && !isBossTheme)
        {
            StartBossMusic();
        }
        else if (!boss && isBossTheme && !isVictory)
        {
            StartMusic();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(string type)
    {
        switch (type)
        {
            case "shoot": PlaySFXNote(1400f, 0.1f, 0.03f); break;
            case "hit": PlaySFXNote(250f, 0.15f, 0.05f); break;
            case "death":
                PlaySFXNote(200f, 0.2f, 0.1f);
                PlaySFXNote(150f, 0.15f, 0.12f);
                break;
            case "pickup":
                PlaySFXNote(880f, 0.1f, 0.05f);
                PlaySFXNote(1100f, 0.1f, 0.06f);
                break;
            case "bossDeath":
                PlaySFXNote(800f, 0.25f, 0.1f);
                PlaySFXNote(600f, 0.2f, 0.12f);
                PlaySFXNote(400f, 0.2f, 0.15f);
                PlaySFXNote(200f, 0.25f, 0.2f);
                break;
            case "shopOpen":
                PlaySFXNote(660f, 0.08f, 0.05f);
                PlaySFXNote(880f, 0.08f, 0.05f);
                PlaySFXNote(1100f, 0.08f, 0.06f);
                break;
        }
    }

    void PlaySFXNote(float freq, float volume, float duration)
    {
        int sampleRate = 44100;
        int sampleLen = (int)(sampleRate * duration);
        float[] samples = new float[sampleLen];

        for (int i = 0; i < sampleLen; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 10f);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.15f;
            samples[i] = wave * env * volume;
        }

        AudioClip clip = AudioClip.Create("sfx", sampleLen, 1, sampleRate, false);
        clip.SetData(samples, 0);

        GameObject noteObj = new GameObject("SFX");
        noteObj.transform.position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource src = noteObj.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = 1f;
        src.spatialBlend = 0f;
        src.Play();
        Destroy(noteObj, duration + 0.1f);
    }
}

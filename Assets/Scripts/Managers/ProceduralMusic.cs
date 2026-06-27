using UnityEngine;

public class ProceduralMusic : MonoBehaviour
{
    public static ProceduralMusic Instance;
    private float noteTimer;
    private float bpm = 140f;
    private int noteIndex;
    private bool playing;
    private int sampleRate = 44100;

    private int[] melody = {
        72, 75, 79, 84, 79, 75, 72, 67,
        72, 76, 79, 84, 81, 79, 76, 72,
        74, 77, 81, 86, 81, 77, 74, 69,
        72, 75, 79, 84, 87, 84, 79, 75
    };

    private int[] bass = {
        48, 48, 55, 55, 48, 48, 53, 53,
        48, 48, 55, 55, 50, 50, 53, 53,
        50, 50, 57, 57, 50, 50, 55, 55,
        48, 48, 55, 55, 48, 48, 55, 55
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartMusic();
    }

    void Update()
    {
        if (!playing) return;

        noteTimer -= Time.deltaTime;
        if (noteTimer <= 0)
        {
            float beatLen = 60f / bpm / 2f;
            noteTimer = beatLen;

            PlayNote(MidiToFreq(melody[noteIndex % melody.Length]), 0.2f, 0.12f);
            PlayNote(MidiToFreq(bass[noteIndex % bass.Length]) * 0.5f, 0.15f, 0.2f);

            if (noteIndex % 4 == 0)
                PlayNote(MidiToFreq(36), 0.25f, 0.06f);

            noteIndex++;
        }
    }

    public void StartMusic()
    {
        playing = true;
        noteTimer = 0;
        noteIndex = 0;
    }

    public void StopMusic() { playing = false; }

    public void PlaySFX(string type)
    {
        switch (type)
        {
            case "shoot": PlayNote(1200f, 0.15f, 0.04f); break;
            case "hit": PlayNote(300f, 0.2f, 0.05f); break;
            case "death": PlayNote(150f, 0.3f, 0.15f); break;
            case "pickup":
                PlayNote(880f, 0.15f, 0.06f);
                PlayNote(1100f, 0.15f, 0.08f);
                break;
        }
    }

    void PlayNote(float freq, float volume, float duration)
    {
        int sampleLen = (int)(sampleRate * duration);
        float[] samples = new float[sampleLen];

        for (int i = 0; i < sampleLen; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 10f);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.2f
                       + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.1f;
            samples[i] = wave * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("sfx", sampleLen, 1, sampleRate, false);
        clip.SetData(samples, 0);

        GameObject noteObj = new GameObject("SFX");
        noteObj.transform.position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource src = noteObj.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = 0.5f;
        src.spatialBlend = 0f;
        src.Play();
        Destroy(noteObj, duration + 0.1f);
    }

    float MidiToFreq(int midi) { return 440f * Mathf.Pow(2f, (midi - 69) / 12f); }
}

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.StartMusic();
    }

    public void PlayShootSound()
    {
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.PlaySFX("shoot");
    }

    public void PlayHitSound()
    {
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.PlaySFX("hit");
    }

    public void PlayDeathSound()
    {
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.PlaySFX("death");
    }

    public void PlayPickupSound()
    {
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.PlaySFX("pickup");
    }
}

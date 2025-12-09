using UnityEngine;

public class AudioController : MonoBehaviour
{
    [HideInInspector]
    public AudioSource EffectsSource;
    [HideInInspector]
    public AudioSource MusicSource;

    [SerializeField] private bool AutoPlayMusic = false;

    [Header("UI SOUNDS")]
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip LevelWin, LevelFail, SelectShape, DeselectShape;

    [Header("Canon Sounds")]
    public AudioClip _spawn;
    public AudioClip _shoot;
    public AudioClip _enemyHit;
    public AudioClip _fullSlot;

    public static AudioController Instance = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        EffectsSource = gameObject.AddComponent<AudioSource>();
        MusicSource = gameObject.AddComponent<AudioSource>();

        SetMusicVolume(true);
        SetSoundVolume(true);


    }


    public void Play(AudioClip clip, AudioSource _source = null)
    {
        if (_source == null)
        {
            // Sử dụng PlayOneShot để phát nhiều sound đồng thời
            EffectsSource.PlayOneShot(clip);
        }
        else
        {
            _source.PlayOneShot(clip);
        }

    }


    public void StopSound()
    {
        EffectsSource.Stop();
    }

    public void StopMusic()
    {
        MusicSource.Stop();
    }

    // Play a single clip through the music source.
    public void PlayMusic(AudioClip clip)
    {
        // Debug.Log("Play Music: " + clip.name);
        MusicSource.clip = clip;
        MusicSource.loop = true;
        // MusicSource.volume = PlayerMusic;
        MusicSource.Play();
    }

    public void SetMusicVolume(bool isOn)
    {
        MusicSource.volume = isOn ? 1 : 0;
    }


    public void SetSoundVolume(bool isOn)
    {
        EffectsSource.volume = isOn ? 1 : 0;
    }

    public void PauseAllSound(bool bg = false)
    {
        EffectsSource.volume = 0f;
        EffectsSource.Pause();
        if (bg)
        {
            MusicSource.volume = 0f;
            MusicSource.Pause();
        }

    }



    #region external call function
    //ui
    public void ButtonClick() { this.Play(click); }
    public void LevelWinSound() { this.Play(LevelWin); }
    public void LevelFailSound() { this.Play(LevelFail); }
    public void SelectShapeSound() { this.Play(SelectShape); }
    public void DeselectShapeSound() { this.Play(DeselectShape); }

    // canon
    public void Spawn() { this.Play(_spawn); }
    public void Shoot() { this.Play(_shoot); }
    public void EnemyHit() { this.Play(_enemyHit); }
    public void FullSlot() { this.Play(_fullSlot); }
    #endregion
}
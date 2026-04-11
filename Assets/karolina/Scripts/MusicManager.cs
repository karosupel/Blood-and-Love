using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;
    private AudioSource audioSource;
    public AudioClip backgroudMusic;
    [SerializeField] private AudioClip hellMusic;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private bool autoFindMusicSliderOnSceneLoad = true;

    private PlayerHealth trackedPlayerHealth;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance.ApplyConfigurationFrom(this);
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {



        PlayBackgroundMusic(false, backgroudMusic);


        HookMusicSlider();

        SceneManager.sceneLoaded += HandleSceneLoaded;
        HookPlayerHealth();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnhookPlayerHealth();
            UnhookMusicSlider();
            Instance = null;
        }
    }

    public static void SetVolume(float volume)
    {
        if (Instance == null || Instance.audioSource == null)
        {
            return;
        }

        Instance.audioSource.volume = volume;
    }

    public void PlayBackgroundMusic(bool reset, AudioClip audioClip = null)
    {
        if(audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if (audioSource.clip != null)
        {
            if(reset)
            {
                audioSource.Stop();
            }
            audioSource.Play();
        }
    }

    public void PauseBackgroudMusic()
    {
        audioSource.Pause();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HookPlayerHealth();
        HookMusicSlider();
    }

    private void HookMusicSlider()
    {
        UnhookMusicSlider();

        if (musicSlider == null && autoFindMusicSliderOnSceneLoad)
        {
            musicSlider = FindObjectOfType<Slider>();
        }

        if (musicSlider == null)
        {
            return;
        }

        musicSlider.onValueChanged.AddListener(HandleMusicSliderValueChanged);
        HandleMusicSliderValueChanged(musicSlider.value);
    }

    private void UnhookMusicSlider()
    {
        if (musicSlider == null)
        {
            return;
        }

        musicSlider.onValueChanged.RemoveListener(HandleMusicSliderValueChanged);
    }

    private void HandleMusicSliderValueChanged(float value)
    {
        SetVolume(value);
    }

    private void HookPlayerHealth()
    {
        UnhookPlayerHealth();

        trackedPlayerHealth = FindObjectOfType<PlayerHealth>();
        if (trackedPlayerHealth == null)
        {
            return;
        }

        trackedPlayerHealth.OnAfterlifeStateChanged += HandleAfterlifeStateChanged;
        HandleAfterlifeStateChanged(trackedPlayerHealth.IsInAfterlife);
    }

    private void UnhookPlayerHealth()
    {
        if (trackedPlayerHealth == null)
        {
            return;
        }

        trackedPlayerHealth.OnAfterlifeStateChanged -= HandleAfterlifeStateChanged;
        trackedPlayerHealth = null;
    }

    private void HandleAfterlifeStateChanged(bool isInAfterlife)
    {
        if (audioSource == null)
        {
            return;
        }

        AudioClip targetClip = isInAfterlife ? hellMusic : backgroudMusic;
        if (targetClip == null)
        {
            return;
        }

        if (audioSource.clip == targetClip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = targetClip;
        audioSource.Play();
    }

    private void ApplyConfigurationFrom(MusicManager sceneMusicManager)
    {
        if (sceneMusicManager == null)
        {
            return;
        }

        if (sceneMusicManager.backgroudMusic != null)
        {
            backgroudMusic = sceneMusicManager.backgroudMusic;
        }

        if (sceneMusicManager.hellMusic != null)
        {
            hellMusic = sceneMusicManager.hellMusic;
        }

        if (sceneMusicManager.musicSlider != null)
        {
            musicSlider = sceneMusicManager.musicSlider;
            HookMusicSlider();
        }

        if (audioSource == null)
        {
            return;
        }

        if (trackedPlayerHealth != null)
        {
            HandleAfterlifeStateChanged(trackedPlayerHealth.IsInAfterlife);
            return;
        }

        if (backgroudMusic == null)
        {
            return;
        }

        if (audioSource.clip == backgroudMusic && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = backgroudMusic;
        audioSource.Play();
    }
}

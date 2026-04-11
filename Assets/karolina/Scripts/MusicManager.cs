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

    private PlayerHealth trackedPlayerHealth;

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }



        PlayBackgroundMusic(false, backgroudMusic);


        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(delegate { SetVolume(musicSlider.value); });
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
        HookPlayerHealth();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnhookPlayerHealth();
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
}

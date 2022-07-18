using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] GameObject settingsPanel;
    [SerializeField] Slider masterVol, musicVol, ambienceVol, sfxVol;
    [SerializeField] Button backBtn, pauseBtn;
    [SerializeField] Sprite pauseSprite, playSprite;

    private bool paused;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        settingsPanel.SetActive(false);
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        masterVol.value = audioManager.GetMixerVolume(AudioManager.MixerType.MASTER);
        musicVol.value = audioManager.GetMixerVolume(AudioManager.MixerType.MUSIC);
        ambienceVol.value = audioManager.GetMixerVolume(AudioManager.MixerType.AMBIENCE);
        sfxVol.value = audioManager.GetMixerVolume(AudioManager.MixerType.SFX);

        masterVol.onValueChanged.AddListener(MasterChanged);
        musicVol.onValueChanged.AddListener(MusicChanged);
        ambienceVol.onValueChanged.AddListener(AmbienceChanged);
        sfxVol.onValueChanged.AddListener(SFXChanged);

        backBtn.onClick.AddListener(Resume);
        pauseBtn.onClick.AddListener(Pause);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P");
            if (!paused)
                Pause();
            else
                Resume();
        }
    }

    private void MasterChanged(float val)
    {
        audioManager.SetMixerVolume(AudioManager.MixerType.MASTER, val);
    }

    private void MusicChanged(float val)
    {
        audioManager.SetMixerVolume(AudioManager.MixerType.MUSIC, val);
    }

    private void AmbienceChanged(float val)
    {
        audioManager.SetMixerVolume(AudioManager.MixerType.AMBIENCE, val);
    }

    private void SFXChanged(float val)
    {
        audioManager.SetMixerVolume(AudioManager.MixerType.SFX, val);
    }

    private void Pause()
    {
        settingsPanel.SetActive(true);
        pauseBtn.GetComponent<Image>().sprite = playSprite;
        Time.timeScale = 0;
    }

    private void Resume()
    {
        pauseBtn.GetComponent<Image>().sprite = pauseSprite;
        Time.timeScale = 1f;
        settingsPanel.SetActive(false);
    }
}

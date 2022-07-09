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

    private bool paused;
    private AudioManager audioManger;

    // Start is called before the first frame update
    void Start()
    {
        settingsPanel.SetActive(false);
        audioManger = GameObject.FindObjectOfType<AudioManager>();
        masterVol.value = audioManger.GetMixerVolume(AudioManager.MixerType.MASTER);
        musicVol.value = audioManger.GetMixerVolume(AudioManager.MixerType.MUSIC);
        ambienceVol.value = audioManger.GetMixerVolume(AudioManager.MixerType.AMBIENCE);
        sfxVol.value = audioManger.GetMixerVolume(AudioManager.MixerType.SFX);

        backBtn.onClick.AddListener(Resume);
        pauseBtn.onClick.AddListener(Pause);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!paused)
                Pause();
            else
                Resume();
        }
    }

    private void Pause()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 0;
    }

    private void Resume()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}

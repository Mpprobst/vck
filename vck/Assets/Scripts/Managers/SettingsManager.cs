using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get { return _instance; } }
    private static SettingsManager _instance;

    [SerializeField] GameObject settingsPanel;
    public Slider masterVol, musicVol, ambienceVol, sfxVol, brightnessSlider;
    [SerializeField] Button backBtn, pauseBtn;
    [SerializeField] Sprite pauseSprite, playSprite;

    private bool paused;
    private AudioManager audioManager;
    private LightingController lighting;

    // Start is called before the first frame update
    public void Initialize()
    {
        if (_instance == null)
            _instance = this;

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

        lighting = GameObject.FindObjectOfType<LightingController>();
        brightnessSlider.value = 1;
        brightnessSlider.onValueChanged.AddListener(lighting.ChangeLightIntensity);

        backBtn.onClick.AddListener(ToggleTime);
        pauseBtn.onClick.AddListener(ToggleTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleTime();
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

    private void ToggleTime()
    {
        paused = !paused;
        settingsPanel.SetActive(paused);
        pauseBtn.GetComponent<Image>().sprite = paused ? playSprite : pauseSprite;
        Time.timeScale = paused ? 0 : 1;
        var system = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        system.SetSelectedGameObject(null);
    }
}

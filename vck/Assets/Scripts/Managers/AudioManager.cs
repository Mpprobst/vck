using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public enum MixerType { MASTER, MUSIC, AMBIENCE, SFX }
    [SerializeField] AudioMixer mixer;


    // Start is called before the first frame update
    public void Initialize()
    {
        if (LeaderboardManager.Instance.CurrentUser.authenticated)
        {
            // set volumes based on user profile
            SetMixerVolume(MixerType.MASTER, LeaderboardManager.Instance.CurrentUser.settings.master);
            SetMixerVolume(MixerType.MUSIC, LeaderboardManager.Instance.CurrentUser.settings.music);
            SetMixerVolume(MixerType.AMBIENCE, LeaderboardManager.Instance.CurrentUser.settings.ambience);
            SetMixerVolume(MixerType.SFX, LeaderboardManager.Instance.CurrentUser.settings.sfx);
        }
    }

    public float GetMixerVolume(MixerType mix)
    {
        float vol = 0f;
        mixer.GetFloat($"{mix.ToString().ToLower()}Volume", out vol);
        vol = Mathf.Pow(10f, (vol / 20f));
        return vol;
    }

    public void SetMixerVolume(MixerType mix, float volume)
    {
        volume = Mathf.Log10(volume) * 20f;
        mixer.SetFloat($"{mix.ToString().ToLower()}Volume", volume);
    }
}

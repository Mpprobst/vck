using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public enum MixerType { MASTER, MUSIC, AMBIENCE, SFX }
    [SerializeField] AudioMixer mixer;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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

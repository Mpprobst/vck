using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public enum MixerType { MASTER, MUSIC, AMBIENCE, SFX }
    [SerializeField] AudioMixer master, music, ambience, sfx;

    private AudioMixer[] mixers;

    // Start is called before the first frame update
    void Start()
    {
        mixers = new AudioMixer[4];
        mixers[0] = master;
        mixers[1] = music;
        mixers[2] = ambience;
        mixers[3] = sfx;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float GetMixerVolume(MixerType mix)
    {
        float vol = 0f;
        mixers[(int)mix].GetFloat("volume", out vol);
        return vol;
    }

    public void SetMixerVolume(MixerType mix, float volume)
    {
        mixers[(int)mix].SetFloat("volume", volume);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightingController : MonoBehaviour
{
    public static LightingController Instance { get { return _instance; } }
    private static LightingController _instance;

    [SerializeField] Light2D globalLight, backgroundLight;
    private float globalBaseIntensity, backgroundBaseIntensity, intensityMultiplier;

    public void Initialize()
    {
        if (_instance == null)
            _instance = this;

        globalBaseIntensity = globalLight.intensity;
        backgroundBaseIntensity = backgroundLight.intensity;
        ChangeLightIntensity(1);

        if (LeaderboardManager.Instance.CurrentUser.HasEntries())
        {
            ChangeLightIntensity(LeaderboardManager.Instance.CurrentUser.settings.brightness);
        }
    }

    public void ChangeLightIntensity(float val)
    {
        globalLight.intensity = globalBaseIntensity * val * (val > 1f ? 5f : 1f);
        backgroundLight.intensity = backgroundBaseIntensity * val;
        intensityMultiplier = val;
        foreach (var lamp in GameObject.FindObjectsOfType<LampPost>())
        {
            lamp.UpdateLightIntensity(intensityMultiplier);
        }
    }

    public float GetLightIntensity()
    {
        return intensityMultiplier;
    }

}

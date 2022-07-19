using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LampPost : MonoBehaviour
{
    private Light2D lampLight;
    private float baseIntensity;

    // Start is called before the first frame update
    void Start()
    {
        lampLight = GetComponentInChildren<Light2D>();
        baseIntensity = lampLight.intensity;

        UpdateLightIntensity(LightingController.Instance.GetLightIntensity());
    }

    public void UpdateLightIntensity(float multiplier)
    {
        lampLight.intensity = baseIntensity * multiplier;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBrakeLight : MonoBehaviour
{
    private Renderer brakeRenderer;
    private Material brakeMat;

    [SerializeField] private float emissionIntensity = 2.0f;
    [SerializeField] private float offDelay = 0.1f;

    private float lastBrakeTime = -999f;
    private bool isOn = false;

    private void Start()
    {
        brakeRenderer = GetComponent<Renderer>();

        Material[] materials = brakeRenderer.materials;
        materials[1] = new Material(materials[1]);
        brakeRenderer.materials = materials;

        brakeMat = materials[1];
    }

    public void SetBrakeLight(float brakeInput)
    {
        if (brakeMat.Equals(null)) return;

        if (brakeInput > 0.05f)
        {
            lastBrakeTime = Time.time;

            if (!isOn)
            {
                brakeMat.SetColor("_EmissionColor", Color.red * emissionIntensity);
                brakeMat.EnableKeyword("_EMISSION");
                isOn = true;
            }
        }
        else
        {
            if (isOn && Time.time > lastBrakeTime + offDelay)
            {
                brakeMat.SetColor("_EmissionColor", Color.black);
                brakeMat.DisableKeyword("_EMISSION");
                isOn = false;
            }
        }
    }
}

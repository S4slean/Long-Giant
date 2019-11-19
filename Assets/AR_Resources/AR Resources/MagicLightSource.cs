using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MagicLightSource : MonoBehaviour
{

    public Material[] revealMats;
    public Light m_light;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (Material reveal in revealMats)
        {
            reveal.SetVector("_LightPosition", m_light.transform.position);
            reveal.SetVector("_LightDirection", -m_light.transform.forward);
            reveal.SetFloat("_LightAngle", m_light.spotAngle);
        }
        
    }
}
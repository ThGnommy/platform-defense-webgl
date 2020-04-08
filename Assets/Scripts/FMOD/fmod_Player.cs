using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fmod_Player : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string footsteps;
    FMOD.Studio.EventInstance footstepsEv;

    [FMODUnity.ParamRef]
    public string materialParameter;
    FMOD.Studio.PARAMETER_ID Material;

    float m_Material;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

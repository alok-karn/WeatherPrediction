using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeSkybox : MonoBehaviour
{
    // Start is called before the first frame update
    public Material Sky;
    void Start()
    {
        RenderSettings.skybox = Sky;
        // print which skybox is being used
        print("Skybox: " + Sky.name);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

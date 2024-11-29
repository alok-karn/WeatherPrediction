using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SkyboxChange : MonoBehaviour
{
    private string apiUrl = "http://localhost:5000/api/skybox";

    // Class to represent the API response
    [System.Serializable]
    private class SkyboxResponse
    {
        public string skybox;
    }

    void Start()
    {
        StartCoroutine(GetSkyboxFromAPI());
    }

    IEnumerator GetSkyboxFromAPI()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Request Error: " + webRequest.error);
            }
            else
            {
                // Parse JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("API Response: " + jsonResponse);

                SkyboxResponse response = JsonUtility.FromJson<SkyboxResponse>(jsonResponse);

                if (response != null && !string.IsNullOrEmpty(response.skybox))
                {
                    Debug.Log("Skybox name from API: " + response.skybox);
                    SetSkybox(response.skybox.Trim());
                }
                else
                {
                    Debug.LogError("Invalid or empty skybox name in response.");
                }
            }
        }
    }

    void SetSkybox(string skyboxName)
    {
        Debug.Log("Setting skybox to " + skyboxName);

        Material skyboxMaterial = Resources.Load<Material>(skyboxName);
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox successfully changed to " + skyboxName);
        }
        else
        {
            Debug.LogError("Skybox material not found: " + skyboxName);
        }
    }
}

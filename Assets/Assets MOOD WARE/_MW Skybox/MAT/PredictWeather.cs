using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PredictWeather : MonoBehaviour
{
    public InputField tempMinField;
    public InputField tempMaxField;
    public InputField precipitationField;
    public InputField windField;
    public Button predictButton;
    public Text responseText;

    public ParticleSystem rainParticles;
    public ParticleSystem rainParticles1;
    public ParticleSystem rainParticles2;

    public ParticleSystem snowParticles;
    public ParticleSystem snowParticles1;
    public ParticleSystem snowParticles2;

    public PostProcessVolume postProcessVolume;
    private AutoExposure autoExposure;


    private string apiUrl = "http://localhost:5000/predict";

    void Start()
    {
        rainParticles.Stop();
        rainParticles1.Stop();
        rainParticles2.Stop();
        snowParticles.Stop();
        snowParticles1.Stop();
        snowParticles2.Stop();
        predictButton.onClick.AddListener(OnPredictButtonClick);
        if (postProcessVolume.profile.TryGetSettings(out autoExposure))
        {
            Debug.Log("Auto Exposure found and ready!");
        }
        else
        {
            Debug.LogError("Auto Exposure not found in Post Process Volume!");
        }
    }


    void OnPredictButtonClick()
    {
        float tempMin, tempMax, precipitation, wind;


        // Validate and parse input fields
        if (float.TryParse(tempMinField.text, out tempMin) &&
            float.TryParse(tempMaxField.text, out tempMax) &&
            float.TryParse(precipitationField.text, out precipitation) &&
            float.TryParse(windField.text, out wind))
        {
            // Debug.Log("Temp Min: " + tempMin + ", Temp Max: " + tempMax + ", Precipitation: " + precipitation + ", Wind: " + wind);
            StartCoroutine(CallPredictApi(tempMin, tempMax, precipitation, wind));
        }
        else
        {
            responseText.text = "Please enter valid numeric values!";
            Debug.LogError("Invalid input fields");
        }
    }

    IEnumerator CallPredictApi(float tempMin, float tempMax, float precipitation, float wind)
    {
        // Create JSON payload
        Debug.Log("Temp Min: " + tempMin + ", Temp Max: " + tempMax + ", Precipitation: " + precipitation + ", Wind: " + wind);

        // string jsonPayload = JsonUtility.ToJson(new
        // {
        //     temp_min = tempMin,
        //     temp_max = tempMax,
        //     precipitation = precipitation,
        //     wind = wind
        // });
        WeatherPayload payload = new WeatherPayload(tempMin, tempMax, precipitation, wind);
        string jsonPayload = JsonUtility.ToJson(payload);

        Debug.Log("JSON Payload: " + jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                responseText.text = "Error: " + webRequest.error;
            }
            else
            {
                string response = webRequest.downloadHandler.text;
                PredictResponseData responseData = JsonUtility.FromJson<PredictResponseData>(response);
                if (responseData != null)
                {
                    responseText.text = $"Prediction: {responseData.prediction}\nSkyBox: {responseData.skybox}";
                    Debug.Log("Response: " + responseData.prediction + ", Skybox: " + responseData.skybox);

                    if (!string.IsNullOrEmpty(responseData.prediction))
                    {
                        if (responseData.prediction.ToLower().Equals("rain"))
                        {
                            rainParticles.Play();
                            rainParticles1.Play();
                            rainParticles2.Play();
                            // StartCoroutine(SimulateLightning());
                            EnableAutoExposure(true);
                        }
                        else
                        {
                            rainParticles.Stop();
                            rainParticles1.Stop();
                            rainParticles2.Stop();
                            EnableAutoExposure(false);
                        }
                    }

                    if (!string.IsNullOrEmpty(responseData.prediction))
                    {
                        if (responseData.prediction.ToLower().Equals("snow"))
                        {
                            snowParticles.Play();
                            snowParticles1.Play();
                            snowParticles2.Play();
                        }
                        else
                        {
                            snowParticles.Stop();
                            snowParticles1.Stop();
                            snowParticles2.Stop();
                        }
                    }

                    if (!string.IsNullOrEmpty(responseData.skybox))
                    {
                        SetSkyBox(responseData.skybox.Trim());
                    }
                }
                else
                {
                    responseText.text = "Invalid response from server!";
                    Debug.LogError("Invalid response from server!");
                }
            }
        }
    }

    // IEnumerator SimulateLightning()
    // {
    //     if (autoExposure != null)
    //     {
    //         float originalMin = autoExposure.minLuminance.value;
    //         float originalMax = autoExposure.maxLuminance.value;

    //         autoExposure.minLuminance.value = 1.0f;
    //         autoExposure.maxLuminance.value = 5.0f;

    //         yield return new WaitForSeconds(0.1f);

    //         autoExposure.minLuminance.value = originalMin;
    //         autoExposure.maxLuminance.value = originalMax;
    //     }
    // }

    void EnableAutoExposure(bool enable)
    {
        if (autoExposure != null)
        {
            if (enable)
            {
                autoExposure.enabled.value = true;
                autoExposure.keyValue.value = 3.0f;
                Debug.Log("Auto Exposure enabled!");
            }
            else
            {
                autoExposure.enabled.value = false;
                autoExposure.keyValue.value = 0.0f;
                Debug.Log("Auto Exposure disabled!");
            }
        }
        else
        {
            Debug.LogError("Auto Exposure not found in Post Process Volume!");
        }
    }

    void SetSkyBox(string skyboxName)
    {
        Debug.Log("Setting skybox to: " + skyboxName);
        Material skyboxMaterial = Resources.Load<Material>(skyboxName);
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox set successfully! to: " + skyboxName);
        }
        else
        {
            Debug.LogError("Skybox material not found! " + skyboxName);
        }
    }

    [System.Serializable]
    public class PredictResponseData
    {
        public string prediction;
        public string skybox;
    }

    [System.Serializable]
    public class WeatherPayload
    {
        public float temp_min;
        public float temp_max;
        public float precipitation;
        public float wind;

        public WeatherPayload(float tempMin, float tempMax, float precipitation, float wind)
        {
            this.temp_max = tempMax;
            this.temp_min = tempMin;
            this.precipitation = precipitation;
            this.wind = wind;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TrainModelButton : MonoBehaviour
{
    private string trainApiUrl = "http://localhost:5000/train";
    public Text responseText; // UI Text element to display the response

    // Method to be triggered when the button is clicked
    public void OnTrainModelButtonClick()
    {
        StartCoroutine(CallTrainApi());
    }

    IEnumerator CallTrainApi()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(trainApiUrl, ""))
        {
            // Send the request
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                if (responseText != null)
                {
                    responseText.text = "Error: " + webRequest.error;
                }
            }
            else
            {
                // Get the response and display it
                string jsonResponse = webRequest.downloadHandler.text;
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);
                Debug.Log("API Response: " + responseData.message);

                if (responseText != null)
                {
                    responseText.text = responseData.message;
                }
            }
        }
    }
    [System.Serializable]
    private class ResponseData
    {
        public string message;
    }
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class LMStudioClient : MonoBehaviour
{
    private string apiURL = "http://192.168.0.102:1234/v1/chat/completions";
    // Store the conversation here
    private List<Message> chatHistory = new List<Message>();

    [System.Serializable]
    public class Message { public string role; public string content; }

    [System.Serializable]
    public class ChatRequest
    {
        public string model = "local-model";
        public Message[] messages;
        public float temperature = 0.7f;
    }

    void Start()
    {
        //// 1. Define your Campus Knowledge Base
        //string campusKnowledge = @"
        //CAMPUS NAME: PNB (Politeknik Negeri Bali).
        //LOCATIONS: 
        //- The Library: Located near the south gate, open 8am-8pm. It has 3 section.
        //- Engineering Building: The blue building. It houses the VR lab.
        //- Cafeteria: Famous for 'Nasi Jinggo'. Located behind the Auditorium.
        //TOUR GUIDE PERSONA: You are 'Putu', a friendly student guide. 
        //RULES: 
        //- Only talk about PNB campus. 
        //- If you don't know an answer, say 'I'm not sure about that, let's ask the admin office!'
        //";
        ////- Keep answers under 3 sentences for VR readability."

        //// Initialize with the System Prompt
        //chatHistory.Add(new Message
        //{
        //    role = "system",
        //    content = "You are Putu, a guide for PNB Bali. Use the following facts: " + campusKnowledge
        //});
    }

    public void AskAI(string userPrompt)
    {
        StartCoroutine(PostRequest(userPrompt));
    }

    IEnumerator PostRequest(string prompt)
    {
        // 1. Add user message to history
        chatHistory.Add(new Message { role = "user", content = prompt });

        ChatRequest requestData = new ChatRequest();
        requestData.messages = chatHistory.ToArray(); // Send the WHOLE history

        //// Setup the message structure
        //ChatRequest requestData = new ChatRequest();
        //requestData.messages = new Message[] {
        //    new Message { role = "system", content = "You are a helpful campus tour guide for PNB VR." },
        //    new Message { role = "user", content = prompt }
        //};

        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(apiURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 🔥 START TIMER
            float startTime = Time.realtimeSinceStartup;

            yield return request.SendWebRequest();

            // 🔥 END TIMER
            float endTime = Time.realtimeSinceStartup;
            float inferenceTimeMs = (endTime - startTime) * 1000f;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                string rawResponse = request.downloadHandler.text;
                Debug.Log("AI Response: " + rawResponse);

                // Simple way to extract the 'content' field from the OpenAI-style JSON response
                // without using a 3rd party JSON library
                string key = "\"content\": \"";
                int start = rawResponse.IndexOf(key) + key.Length;
                int end = rawResponse.IndexOf("\"", start);
                string aiMessage = rawResponse.Substring(start, end - start);

                // Clean up newline characters from JSON
                aiMessage = aiMessage.Replace("\\n", "\n");

                // Display it in VR (Assuming you have a reference to the same TMP_Text)
                GameObject.FindObjectOfType<QuestSpeechHandler>().text.text = 
                    "AI: " + aiMessage +
                    $"\n\n<color=yellow>Latency: {inferenceTimeMs:F2} ms</color>";

                // 🔥 LOG to console
                Debug.Log($"Inference Time: {inferenceTimeMs:F2} ms");

                // 2. Add AI's response to history so it remembers what it said
                chatHistory.Add(new Message { role = "assistant", content = aiMessage });
            }
            //else
            //{
            //    Debug.Log("AI Response: " + request.downloadHandler.text);
            //    // Tip: Use a JSON parser like Newtonsoft.Json to extract the specific text response
            //}
        }
    }
}
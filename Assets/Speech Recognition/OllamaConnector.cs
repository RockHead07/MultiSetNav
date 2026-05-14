using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaConnector : MonoBehaviour
{
    public static OllamaConnector instance;

    [Header("Ollama Settings")]
    [Tooltip("IP laptop di jaringan WiFi lokal")]
    public string ollamaHost = "192.168.18.150";
    public int ollamaPort = 11434;
    public string modelName = "llama3.2:latest";

    private string OllamaURL => $"https://{ollamaHost}:{ollamaPort}/api/generate";

    // System prompt khusus ekstrak POI — singkat dan terarah
    private const string SYSTEM_PROMPT = @"
Kamu adalah sistem ekstraksi tujuan navigasi indoor.
Tugasmu HANYA mengekstrak nama lokasi tujuan dari kalimat pengguna.
Jawab HANYA dengan JSON berikut, tanpa teks lain:
{""poi"": ""nama lokasi""}

Contoh:
Input: ""saya mau ke MMB Studio""
Output: {""poi"": ""MMB Studio""}

Input: ""di mana toilet lantai 2""  
Output: {""poi"": ""Toilet""}

Input: ""mau ketemu dokter mata""
Output: {""poi"": ""Poli Mata""}

Jika tidak ada tujuan yang jelas, jawab: {""poi"": """"}
";

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator ExtractPOI(string spokenText, Action<string> onResult)
    {
        string prompt = $"{SYSTEM_PROMPT}\nInput: \"{spokenText}\"\nOutput:";

        // Buat request body
        string requestBody = JsonUtility.ToJson(new OllamaRequest
        {
            model = modelName,
            prompt = prompt,
            stream = false
        });

        Debug.Log($"[Ollama] Mengirim: {spokenText}");

        using (UnityWebRequest request = new UnityWebRequest(OllamaURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15; // timeout 15 detik

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Ollama] Error: {request.error}");
                onResult?.Invoke(null);
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[Ollama] Response raw: {responseText}");

            // Parse response Ollama
            OllamaResponse ollamaResponse = JsonUtility.FromJson<OllamaResponse>(responseText);
            string generatedText = ollamaResponse.response.Trim();
            Debug.Log($"[Ollama] Generated: {generatedText}");

            // Parse JSON poi dari generated text
            string poiName = ParsePOIFromJson(generatedText);
            Debug.Log($"[Ollama] POI extracted: {poiName}");

            onResult?.Invoke(poiName);
        }
    }

    string ParsePOIFromJson(string jsonText)
    {
        try
        {
            // Cari pattern {"poi": "..."}
            int start = jsonText.IndexOf("{");
            int end = jsonText.LastIndexOf("}");
            if (start < 0 || end < 0) return null;

            string cleanJson = jsonText.Substring(start, end - start + 1);
            POIResult result = JsonUtility.FromJson<POIResult>(cleanJson);
            return string.IsNullOrEmpty(result.poi) ? null : result.poi;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Ollama] Parse error: {e.Message}");
            return null;
        }
    }

    // ── Data classes untuk serialisasi JSON ──

    [Serializable]
    class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    [Serializable]
    class OllamaResponse
    {
        public string response;
        public bool done;
    }

    [Serializable]
    class POIResult
    {
        public string poi;
    }
}
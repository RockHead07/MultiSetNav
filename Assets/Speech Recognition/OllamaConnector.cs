using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events; // Ditambahkan: untuk UnityEvent agar bisa kirim event ke UI
using UnityEngine.Networking;

public class OllamaConnector : MonoBehaviour
{
    public static OllamaConnector instance;

    [Header("Ollama Settings")]
    [Tooltip("IP laptop di jaringan WiFi lokal")]
    public string ollamaHost = "192.168.18.150";
    public int ollamaPort = 11434;
    public string modelName = "llama3.2:latest";
    public bool useHttps = false;

    // Event ketika koneksi gagal setelah retry habis, UI bisa tampilkan pesan error
    [Header("Events")]
    [Tooltip("Event dipanggil ketika koneksi Ollama gagal setelah retry habis.")]
    public UnityEvent onConnectionFailed;

    // Property read-only untuk cek apakah sedang memproses request
    public bool IsProcessing { get; private set; }

    // Jumlah maksimal percobaan (1 awal + 1 retry = 2)
    private const int MAX_ATTEMPTS = 2;
    // Jeda sebelum retry, memberi waktu server pulih
    private const float RETRY_DELAY_SECONDS = 2f;

    private string OllamaURL => $"{(useHttps ? "https" : "http")}://{ollamaHost}:{ollamaPort}/api/generate";

    // System prompt khusus ekstrak POI — singkat dan terarah
    private const string SYSTEM_PROMPT = @"
Kamu adalah sistem ekstraksi tujuan navigasi indoor.
Tugasmu HANYA mengekstrak nama lokasi tujuan dari kalimat pengguna.
Jawab HANYA dengan JSON berikut, tanpa teks lain:
{""poi"": ""nama lokasi""}

Contoh:
Input: ""saya mau ke BAAK""
Output: {""poi"": ""BAAK""}

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
        // Tandai sedang proses agar UI bisa tampilkan loading
        IsProcessing = true;

        string prompt = $"{SYSTEM_PROMPT}\nInput: \"{spokenText}\"\nOutput:";

        // Buat request body
        string requestBody = JsonUtility.ToJson(new OllamaRequest
        {
            model = modelName,
            prompt = prompt,
            stream = false
        });

        Debug.Log($"[Ollama] Mengirim: {spokenText}");

        string responseText = null; // Simpan response jika berhasil
        bool requestSucceeded = false; // Flag keberhasilan

        // Loop retry: coba MAX_ATTEMPTS kali (percobaan awal + 1 retry)
        for (int attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
        {
            Debug.Log($"[Ollama] Percobaan ke-{attempt} dari {MAX_ATTEMPTS}");

            using (UnityWebRequest request = new UnityWebRequest(OllamaURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                // Timeout diubah 15 -> 30 detik, LLM lokal butuh waktu lebih lama
                request.timeout = 30;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Berhasil — simpan response, keluar loop
                    responseText = request.downloadHandler.text;
                    requestSucceeded = true;
                    Debug.Log($"[Ollama] Berhasil di percobaan ke-{attempt}");
                    break;
                }
                else
                {
                    Debug.LogWarning($"[Ollama] Gagal percobaan ke-{attempt}: {request.error}");
                    // Jika belum percobaan terakhir, tunggu sebelum retry
                    if (attempt < MAX_ATTEMPTS)
                    {
                        Debug.Log($"[Ollama] Menunggu {RETRY_DELAY_SECONDS}s sebelum retry...");
                        yield return new WaitForSeconds(RETRY_DELAY_SECONDS);
                    }
                }
            }
        }

        // Semua percobaan gagal
        if (!requestSucceeded)
        {
            Debug.LogError($"[Ollama] Semua {MAX_ATTEMPTS} percobaan gagal. Server tidak tersedia.");
            // Panggil event agar UI bisa tampilkan "Server tidak tersedia"
            onConnectionFailed?.Invoke();
            onResult?.Invoke(null);
            IsProcessing = false; // Selesai proses
            yield break;
        }

        // Berhasil — parse response
        Debug.Log($"[Ollama] Response raw: {responseText}");

        // Parse response Ollama
        OllamaResponse ollamaResponse = JsonUtility.FromJson<OllamaResponse>(responseText);
        string generatedText = ollamaResponse.response.Trim();
        Debug.Log($"[Ollama] Generated: {generatedText}");

        // Parse JSON poi dari generated text
        string poiName = ParsePOIFromJson(generatedText);
        Debug.Log($"[Ollama] POI extracted: {poiName}");

        onResult?.Invoke(poiName);
        IsProcessing = false; // Selesai proses
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
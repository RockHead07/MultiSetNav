using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using TMPro;

public class VoiceInputHandler : MonoBehaviour
{
    [Header("UI")]
    public Button btnVoice;
    public TMP_Text txtStatus;
    public TMP_Text txtResult;

    private AndroidJavaObject speechRecognizer;
    private AndroidJavaObject currentActivity;
    private bool isListening = false;

    void Start()
    {
        if (btnVoice == null)
        {
            Debug.LogError("[VoiceInputHandler] btnVoice belum di-assign.");
            enabled = false;
            return;
        }

        if (txtStatus == null)
        {
            Debug.LogWarning("[VoiceInputHandler] txtStatus belum di-assign.");
        }

        if (txtResult == null)
        {
            Debug.LogWarning("[VoiceInputHandler] txtResult belum di-assign.");
        }

        // Minta permission mic saat start
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        btnVoice.onClick.AddListener(OnVoiceButtonClicked);
        if (txtStatus != null)
        {
            txtStatus.text = "Siap mendengarkan...";
        }
    }

    void OnVoiceButtonClicked()
    {
        if (!isListening)
            StartListening();
    }

    void StartListening()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            isListening = true;
            if (txtStatus != null)
            {
                txtStatus.text = "Mendengarkan...";
            }
            btnVoice.interactable = false;

            // Ambil current Android activity
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // Buat intent untuk Speech Recognition
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");

            string ACTION_RECOGNIZE_SPEECH = intentClass.GetStatic<string>("ACTION_RECOGNIZE_SPEECH");
            intent.Call<AndroidJavaObject>("setAction", ACTION_RECOGNIZE_SPEECH);

            // Set bahasa Indonesia
            intent.Call<AndroidJavaObject>("putExtra",
                "android.speech.extra.LANGUAGE", "id-ID");
            intent.Call<AndroidJavaObject>("putExtra",
                "android.speech.extra.LANGUAGE_MODEL", "free_form");
            intent.Call<AndroidJavaObject>("putExtra",
                "android.speech.extra.PROMPT", "Sebutkan tujuan Anda...");
            intent.Call<AndroidJavaObject>("putExtra",
                "android.speech.extra.MAX_RESULTS", 1);

            // Jalankan speech recognizer via activity
            currentActivity.Call("startActivityForResult", intent, 100);
        }
        catch (System.Exception e)
        {
            if (txtStatus != null)
            {
                txtStatus.text = "Error: " + e.Message;
            }
            ResetButton();
        }
        #else
        // Mode editor — simulasi input teks untuk testing di PC
        if (txtStatus != null)
        {
            txtStatus.text = "[EDITOR] Simulasi: 'saya mau ke MMB Studio'";
        }
        if (txtResult != null)
        {
            txtResult.text = "saya mau ke MMB Studio";
        }
        StartCoroutine(SendToOllama("saya mau ke MMB Studio"));
        #endif
    }

    // Dipanggil otomatis oleh Android saat speech selesai
    // Tambahkan ini di AndroidManifest atau via UnityPlayerActivity override
    public void OnSpeechResult(string result)
    {
        if (string.IsNullOrEmpty(result))
        {
            if (txtStatus != null)
            {
                txtStatus.text = "Tidak terdeteksi, coba lagi.";
            }
            ResetButton();
            return;
        }

        if (txtStatus != null)
        {
            txtStatus.text = "Teks diterima!";
        }
        if (txtResult != null)
        {
            txtResult.text = result;
        }
        StartCoroutine(SendToOllama(result));
    }

    public void OnSpeechError(string error)
    {
        if (txtStatus != null)
        {
            txtStatus.text = "Error speech: " + error;
        }
        ResetButton();
    }

    IEnumerator SendToOllama(string spokenText)
    {
        if (txtStatus != null)
        {
            txtStatus.text = "Memproses ke Ollama...";
        }
        yield return OllamaConnector.instance.ExtractPOI(spokenText, OnPOIReceived);
    }

    void OnPOIReceived(string poiName)
    {
        if (string.IsNullOrEmpty(poiName))
        {
            if (txtStatus != null)
            {
                txtStatus.text = "POI tidak ditemukan, coba lagi.";
            }
        }
        else
        {
            if (txtStatus != null)
            {
                txtStatus.text = $"Navigasi ke: {poiName}";
            }
            // TODO: sambungkan ke NavigationController MultisetNav
            // POI target = POIManager.CariPOI(poiName);
            // NavigationController.instance.SetDestination(target);
        }
        ResetButton();
    }

    void ResetButton()
    {
        isListening = false;
        btnVoice.interactable = true;
    }
}
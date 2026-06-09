using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class YoloManager : MonoBehaviour
{
    [Header("Pengaturan API")]
    public string apiUrl = "http://localhost:5000/api/human"; 
    public float jedaPolling = 5f;

    [Header("Pengaturan NavMesh Lorong 1")]
    public NavMeshObstacle rintanganLorong1;
    [Tooltip("Opsional: Masukkan objek TembokLorong1 ke sini agar wujud kubusnya ikut terlihat/hilang")]
    public MeshRenderer visualTembok1;

    [Header("Pengaturan NavMesh Lorong 2")]
    public NavMeshObstacle rintanganLorong2;
    [Tooltip("Opsional: Masukkan objek TembokLorong2 ke sini agar wujud kubusnya ikut terlihat/hilang")]
    public MeshRenderer visualTembok2;

    [Header("UI Peringatan")]
    public GameObject uiPutarBalik;
    public float durasiNotif = 4f;

    private bool statusTembok1_Sebelumnya = false; 
    private bool statusTembok2_Sebelumnya = false;

    [System.Serializable]
    public class DataKeramaian
    {
        public string status;
        public int rute_1_human;
        public bool crowded_1;
        public int rute_2_human;
        public bool crowded_2;
        public string timestamp;
    }

    void Start()
    {
        if (uiPutarBalik != null) uiPutarBalik.SetActive(false);
        StartCoroutine(TarikDataBerkala());
    }

    IEnumerator TarikDataBerkala()
    {
        while (true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[YOLO API] Gagal konek: " + webRequest.error);
                }
                else
                {
                    string jsonText = webRequest.downloadHandler.text;
                    DataKeramaian data = JsonUtility.FromJson<DataKeramaian>(jsonText);

                    Debug.Log($"[YOLO] Rute 1: {data.rute_1_human} org ({data.crowded_1}) | Rute 2: {data.rute_2_human} org ({data.crowded_2})");

                    bool targetTembok1_Nyala = false;
                    bool targetTembok2_Nyala = false;

                    if (data.crowded_1 == true && data.crowded_2 == true)
                    {
                        if (data.rute_1_human < data.rute_2_human)
                        {
                            targetTembok1_Nyala = false; 
                            targetTembok2_Nyala = true;
                        }
                        else
                        {
                            targetTembok1_Nyala = true;
                            targetTembok2_Nyala = false;
                        }
                    }
                    else
                    {
                        targetTembok1_Nyala = data.crowded_1;
                        targetTembok2_Nyala = data.crowded_2;
                    }

                    // Notif putar balik
                    if (statusTembok1_Sebelumnya == false && targetTembok1_Nyala == true)
                    {
                        Debug.Log("Peringatan: Tembok 1 baru saja ditutup!");
                        StartCoroutine(TampilkanNotifikasi());
                    }
                    else if (statusTembok2_Sebelumnya == false && targetTembok2_Nyala == true)
                    {
                        Debug.Log("Peringatan: Tembok 2 baru saja ditutup!");
                        StartCoroutine(TampilkanNotifikasi());
                    }

                    AturTembok(1, targetTembok1_Nyala);
                    AturTembok(2, targetTembok2_Nyala);

                    // Memori 5 detik
                    statusTembok1_Sebelumnya = targetTembok1_Nyala;
                    statusTembok2_Sebelumnya = targetTembok2_Nyala;
                }
            }
            yield return new WaitForSeconds(jedaPolling);
        }
    }

    IEnumerator TampilkanNotifikasi()
    {
        if (uiPutarBalik != null)
        {
            uiPutarBalik.SetActive(true); 
            yield return new WaitForSeconds(durasiNotif); 
            uiPutarBalik.SetActive(false); 
        }
    }

    private void AturTembok(int nomorRute, bool nyalakanTembok)
    {
        if (nomorRute == 1)
        {
            if (rintanganLorong1 != null) rintanganLorong1.enabled = nyalakanTembok;
            if (visualTembok1 != null) visualTembok1.enabled = nyalakanTembok;
        }
        else if (nomorRute == 2)
        {
            if (rintanganLorong2 != null) rintanganLorong2.enabled = nyalakanTembok;
            if (visualTembok2 != null) visualTembok2.enabled = nyalakanTembok;
        }
    }
}
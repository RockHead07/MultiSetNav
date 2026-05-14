using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Manager untuk scanning dan pencarian POI di scene.
/// Assign poiRoot ke Transform parent yang berisi semua POI.
/// </summary>
public class POIManager : MonoBehaviour
{
    [Header("POI Root")]
    [Tooltip("Transform parent yang berisi semua POI. Semua children (termasuk inactive) akan di-scan.")]
    [SerializeField] private Transform poiRoot;

    private readonly List<POIData> poiList = new List<POIData>();
    private readonly Dictionary<string, POIData> lookupDict = new Dictionary<string, POIData>();

    void Awake()
    {
        ScanPOIs();
    }

    /// <summary>
    /// Scan ulang semua POI di bawah poiRoot.
    /// Dipanggil otomatis di Awake, bisa dipanggil manual jika POI berubah runtime.
    /// </summary>
    public void ScanPOIs()
    {
        poiList.Clear();
        lookupDict.Clear();

        if (poiRoot == null)
        {
            Debug.LogWarning("[POIManager] poiRoot belum di-assign! Tidak bisa scan POI.");
            return;
        }

        // GetComponentsInChildren dengan includeInactive = true
        POIData[] allPOIs = poiRoot.GetComponentsInChildren<POIData>(true);

        foreach (POIData poi in allPOIs)
        {
            poiList.Add(poi);

            // Register EffectiveName
            string normalizedName = Normalize(poi.EffectiveName);
            if (!string.IsNullOrEmpty(normalizedName) && !lookupDict.ContainsKey(normalizedName))
            {
                lookupDict[normalizedName] = poi;
            }

            // Register sinonim
            if (poi.sinonim != null)
            {
                foreach (string syn in poi.sinonim)
                {
                    if (string.IsNullOrWhiteSpace(syn)) continue;
                    string normalizedSyn = Normalize(syn);
                    if (!lookupDict.ContainsKey(normalizedSyn))
                    {
                        lookupDict[normalizedSyn] = poi;
                    }
                }
            }
        }

        Debug.Log($"[POIManager] Scanned {poiList.Count} POI(s) dari '{poiRoot.name}', {lookupDict.Count} entri lookup.");
    }

    /// <summary>
    /// Cari POI terbaik berdasarkan query string.
    /// Mencoba exact match dulu, lalu contains match, lalu partial scoring.
    /// </summary>
    /// <param name="query">Teks query dari Ollama atau user input.</param>
    /// <returns>POIData yang paling cocok, atau null jika tidak ditemukan.</returns>
    public POIData FindBestMatch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        string normalizedQuery = Normalize(query);

        // 1) Exact match di dictionary
        if (lookupDict.TryGetValue(normalizedQuery, out POIData exactMatch))
        {
            Debug.Log($"[POIManager] Exact match: '{query}' -> '{exactMatch.EffectiveName}'");
            return exactMatch;
        }

        // 2) Contains match — query ada di dalam key, atau key ada di dalam query
        POIData bestContains = null;
        int bestContainsLen = 0;

        foreach (var kvp in lookupDict)
        {
            // Key yang terkandung dalam query (misal query="pergi ke mmb studio", key="mmb studio")
            if (normalizedQuery.Contains(kvp.Key) && kvp.Key.Length > bestContainsLen)
            {
                bestContains = kvp.Value;
                bestContainsLen = kvp.Key.Length;
            }
            // Query yang terkandung dalam key (misal query="mmb", key="mmb studio")
            else if (kvp.Key.Contains(normalizedQuery) && normalizedQuery.Length > bestContainsLen)
            {
                bestContains = kvp.Value;
                bestContainsLen = normalizedQuery.Length;
            }
        }

        if (bestContains != null)
        {
            Debug.Log($"[POIManager] Contains match: '{query}' -> '{bestContains.EffectiveName}'");
            return bestContains;
        }

        // 3) Word overlap scoring
        string[] queryWords = normalizedQuery.Split(' ');
        POIData bestScored = null;
        int bestScore = 0;

        foreach (var kvp in lookupDict)
        {
            int score = 0;
            foreach (string word in queryWords)
            {
                if (word.Length < 2) continue; // Skip single chars
                if (kvp.Key.Contains(word))
                {
                    score += word.Length; // Kata lebih panjang = skor lebih tinggi
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestScored = kvp.Value;
            }
        }

        if (bestScored != null && bestScore >= 3) // Minimal 3 karakter match
        {
            Debug.Log($"[POIManager] Scored match: '{query}' -> '{bestScored.EffectiveName}' (score: {bestScore})");
            return bestScored;
        }

        Debug.LogWarning($"[POIManager] Tidak ditemukan match untuk: '{query}'");
        return null;
    }

    /// <summary>
    /// Normalisasi string: lowercase, hapus punctuation, collapse whitespace.
    /// </summary>
    private static string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Lowercase
        string result = input.ToLowerInvariant();

        // Hapus semua karakter non-alphanumeric kecuali spasi
        result = Regex.Replace(result, @"[^\w\s]", "", RegexOptions.None);

        // Collapse whitespace
        result = Regex.Replace(result, @"\s+", " ").Trim();

        return result;
    }

    /// <summary>
    /// Mendapatkan semua POI yang sudah di-scan. Readonly.
    /// </summary>
    public IReadOnlyList<POIData> GetAllPOIs()
    {
        return poiList.AsReadOnly();
    }
}

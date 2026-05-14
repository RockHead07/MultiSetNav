using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Adapter navigasi sederhana yang menerima POIData dan meneruskan ke sistem navigasi.
/// Gunakan UnityEvent di Inspector untuk wiring ke NavigationController atau sistem nav lain.
/// </summary>
public class NavigationAdapter : MonoBehaviour
{
    [Header("Navigation Controller")]
    [Tooltip("Komponen NavigationController dari MultiSet SDK. Jika diisi, adapter akan mencoba memanggil SetPOIforNavigation(POI) via SendMessage.")]
    [SerializeField] private Component navigationController;

    [Tooltip("Nama method pada NavigationController yang menerima parameter POI.")]
    [SerializeField] private string setPoiMethodName = "SetPOIForNavigation";

    [Tooltip("Jika true, gunakan SendMessage untuk memanggil method navigasi.")]
    [SerializeField] private bool useSendMessage = true;

    [Header("Events")]
    [Tooltip("Event dipanggil saat navigasi ke POI diminta. Parameter: Transform tujuan.")]
    [SerializeField] private UnityEvent<Transform> onNavigateToTransform;

    [Tooltip("Event dipanggil saat navigasi ke POI diminta. Parameter: Vector3 posisi tujuan.")]
    [SerializeField] private UnityEvent<Vector3> onNavigateToPosition;

    [Tooltip("Event dipanggil saat navigasi ke POI diminta. Parameter: nama POI.")]
    [SerializeField] private UnityEvent<string> onNavigateToName;

    [Header("Debug")]
    [SerializeField] private bool logNavigation = true;

    /// <summary>
    /// Dipanggil oleh VoiceInputHandler.onPoiMatched via Inspector.
    /// Menerima POIData dan meneruskan informasi navigasi ke event yang terhubung.
    /// </summary>
    public void NavigateToPOI(POIData poi)
    {
        if (poi == null)
        {
            Debug.LogWarning("[NavigationAdapter] POIData null, navigasi dibatalkan.");
            return;
        }

        if (logNavigation)
        {
            Debug.Log($"[NavigationAdapter] Navigasi ke: {poi.EffectiveName} " +
                      $"(pos: {poi.transform.position})");
        }

        // Invoke semua event — sistem navigasi bisa mendengarkan salah satu
        onNavigateToTransform?.Invoke(poi.transform);
        onNavigateToPosition?.Invoke(poi.transform.position);
        onNavigateToName?.Invoke(poi.EffectiveName);

        // Panggil NavigationController jika diset (tanpa dependency langsung ke tipe POI)
        if (navigationController != null)
        {
            Component poiComponent = poi.GetComponent("POI");
            if (poiComponent == null)
            {
                Debug.LogWarning("[NavigationAdapter] Komponen POI tidak ditemukan pada target. Pastikan POI SDK terpasang di GameObject.");
                return;
            }

            if (useSendMessage)
            {
                navigationController.SendMessage(setPoiMethodName, poiComponent, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}

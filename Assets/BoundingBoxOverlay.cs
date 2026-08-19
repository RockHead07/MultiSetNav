using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class BoundingBoxOverlay : MonoBehaviour
{
    [Header("Settings")]
    public string targetTag = "Human"; 
    public Color boxColor = Color.red; 
    public float lineWidth = 3f; 
    public string labelText = "person 0.96"; 

    private Camera myCam; 
    private Canvas myCanvas;
    private RectTransform canvasRect;
    
    // Sistem Pool untuk menyimpan kotak agar tidak berat/lag
    private List<RectTransform> activeBoxUIs = new List<RectTransform>();
    private GameObject boxPrefab;

    void Start()
    {
        myCam = GetComponent<Camera>();

        // 1. GENERATE CANVAS OTOMATIS
        GameObject canvasObj = new GameObject("CCTV_Canvas_Auto");
        canvasObj.transform.SetParent(transform); // Jadikan child dari CCTV Camera
        
        myCanvas = canvasObj.AddComponent<Canvas>();
        myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // MENGUNCI 100% KE LAYAR CCTV (Bypass bug OnGUI Unity)
        myCanvas.targetDisplay = myCam.targetDisplay; 
        
        canvasRect = canvasObj.GetComponent<RectTransform>();

        // 2. BIKIN CETAKAN KOTAK UI
        CreateBoxPrefab();
    }

    void CreateBoxPrefab()
    {
        boxPrefab = new GameObject("BoxTemplate");
        boxPrefab.SetActive(false);
        boxPrefab.transform.SetParent(myCanvas.transform, false);
        
        RectTransform boxRt = boxPrefab.AddComponent<RectTransform>();
        boxRt.anchorMin = Vector2.zero;
        boxRt.anchorMax = Vector2.zero;
        boxRt.pivot = Vector2.zero;

        // Bikin 4 Sisi Garis Kotak
        CreateLine(boxRt, "Top", new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f, 1), new Vector2(0, lineWidth));
        CreateLine(boxRt, "Bottom", new Vector2(0,0), new Vector2(1,0), new Vector2(0.5f, 0), new Vector2(0, lineWidth));
        CreateLine(boxRt, "Left", new Vector2(0,0), new Vector2(0,1), new Vector2(0, 0.5f), new Vector2(lineWidth, 0));
        CreateLine(boxRt, "Right", new Vector2(1,0), new Vector2(1,1), new Vector2(1, 0.5f), new Vector2(lineWidth, 0));

        // Bikin Teks
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(boxRt, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = labelText;
        txt.color = boxColor;
        
        // Ambil font bawaan Unity agar tidak error
        Font defFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (defFont == null) defFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = defFont;
        
        txt.fontSize = 24;
        txt.fontStyle = FontStyle.Bold;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        
        // Tambahkan bayangan hitam biar teksnya tebal dan jelas
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);

        // Atur posisi teks di atas kotak
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 1);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.pivot = new Vector2(0, 0);
        textRt.anchoredPosition = new Vector2(0, 5); 
        textRt.sizeDelta = new Vector2(0, 30);
    }

    // Fungsi perakit garis
    void CreateLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(parent, false);
        Image img = line.AddComponent<Image>();
        img.color = boxColor;
        
        RectTransform rt = line.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
    }

    void LateUpdate()
    {
        // Bersihkan layar dari kotak sebelumnya
        foreach (RectTransform box in activeBoxUIs) box.gameObject.SetActive(false);

        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        int boxIndex = 0;

        foreach (GameObject obj in targets)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) continue;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            // Jika manusianya terlihat oleh CCTV, gambar kotaknya!
            if (CalculateBox(bounds, out Vector2 pos, out Vector2 size))
            {
                RectTransform activeBox;
                if (boxIndex < activeBoxUIs.Count)
                {
                    activeBox = activeBoxUIs[boxIndex];
                }
                else
                {
                    GameObject newBox = Instantiate(boxPrefab, myCanvas.transform);
                    activeBox = newBox.GetComponent<RectTransform>();
                    activeBoxUIs.Add(activeBox);
                }

                activeBox.gameObject.SetActive(true);
                
                // Tempel kotaknya pas di badan
                activeBox.anchoredPosition = pos;
                activeBox.sizeDelta = size;

                boxIndex++;
            }
        }
    }

    bool CalculateBox(Bounds bounds, out Vector2 pos, out Vector2 size)
    {
        pos = Vector2.zero;
        size = Vector2.zero;

        Vector3 cen = bounds.center;
        Vector3 ext = bounds.extents;
        Vector3[] corners = new Vector3[8]
        {
            new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
            new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),
            new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
            new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
            new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
            new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),
            new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
            new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
        };

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        bool isVisible = false;

        foreach (Vector3 corner in corners)
        {
            Vector3 vp = myCam.WorldToViewportPoint(corner);
            if (vp.z > 0)
            {
                isVisible = true; 
                min.x = Mathf.Min(min.x, vp.x);
                min.y = Mathf.Min(min.y, vp.y);
                max.x = Mathf.Max(max.x, vp.x);
                max.y = Mathf.Max(max.y, vp.y);
            }
        }

        if (!isVisible || min.x == float.MaxValue) return false;

        // Hitung pixel rasio layar yang murni bebas bug
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float x = min.x * canvasWidth;
        float y = min.y * canvasHeight;
        float width = (max.x - min.x) * canvasWidth;
        float height = (max.y - min.y) * canvasHeight;

        if (width <= 0 || height <= 0) return false;

        pos = new Vector2(x, y);
        size = new Vector2(width, height);
        
        return true;
    }
}
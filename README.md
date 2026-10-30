# MultiSetNav

Proyek Unity ini berisi eksplorasi MultiSet SDK dengan sample scene (multiplayer dan on-device localization). Repo ini cocok sebagai basis belajar, percobaan, dan kolaborasi.

## Ringkas
<div align="center">
	<code><img width="50" src="https://raw.githubusercontent.com/marwin1991/profile-technology-icons/refs/heads/main/icons/c%23.png" alt="C#" title="C#"/></code>
	<code><img width="50" src="https://raw.githubusercontent.com/marwin1991/profile-technology-icons/refs/heads/main/icons/unity.png" alt="Unity" title="Unity"/></code>
</div>

- Engine: Unity 6000.3.14f1
- SDK utama: MultiSet Unity SDK
- Fitur yang tampak: multiplayer sample, on-device localization, AR (ARCore)

## Struktur Penting
- `Assets/Scenes/` scene proyek (default: SampleScene)
- `Assets/Samples/MultiSet-SDK/` sample resmi dari MultiSet SDK
- `Assets/MapPSDKUwithCrowdDetection.unity` scene demo crowd detection + navigasi
- `Assets/MapRSI.unity` scene demo routing berbasis kepadatan
- `Backend/` server YOLO (Flask + Ultralytics) untuk deteksi kerumunan
- `Packages/manifest.json` daftar paket Unity
- `ProjectSettings/` pengaturan proyek Unity

## Cara Menjalankan
1. Buka Unity Hub.
2. Add proyek ini dari folder repo.
3. Gunakan Unity 6000.3.14f1.
4. Buka scene di `Assets/Scenes/SampleScene.unity`.
5. Tekan Play.

## Catatan Kolaborasi
- Hindari meng-commit folder `Library/` dan `Temp/` (sudah diabaikan oleh .gitignore).
- Jika menambah package, commit perubahan di `Packages/manifest.json` dan `Packages/packages-lock.json`.
- Jika menambah scene atau asset baru, pastikan `.meta` ikut ter-commit.

## Konfigurasi Sebelum Demo

Bagian ini menjelaskan langkah-langkah yang **harus dilakukan sebelum menjalankan demo**, terutama jika environment berubah (misalnya berpindah jaringan WiFi).

### 1. Mengubah IP Ollama

File: `Assets/Speech Recognition/OllamaConnector.cs`  
Field: `ollamaHost` (bisa diubah via Inspector atau langsung di script)

Ollama berjalan di laptop/PC sebagai server lokal. HP Android berkomunikasi ke Ollama melalui WiFi. Karena kebanyakan jaringan menggunakan **DHCP** (IP berubah-ubah), IP laptop bisa berubah setiap kali konek ulang ke WiFi.

**Langkah:**
1. Cek IP laptop saat ini: buka terminal, ketik `ipconfig` (Windows) atau `ifconfig` (Mac/Linux)
2. Cari alamat IPv4 di adapter WiFi (contoh: `192.168.18.150`)
3. Di Unity Inspector, pilih GameObject yang memiliki komponen `OllamaConnector`
4. Ubah field **Ollama Host** ke IP terbaru
5. Pastikan HP dan laptop berada di **jaringan WiFi yang sama**

### 2. Menjalankan Server Ollama

Sebelum demo, pastikan Ollama sudah berjalan di laptop:

```bash
# Jalankan model llama3.2 (akan otomatis download jika belum ada)
ollama run llama3.2

# Atau untuk menjalankan sebagai server background:
ollama serve
```

Pastikan port default `11434` tidak terblokir oleh firewall. Untuk testing cepat:
```bash
curl http://localhost:11434/api/generate -d '{"model":"llama3.2:latest","prompt":"test","stream":false}'
```

### 3. Menggunakan Tool Auto Attach POIData

Unity Editor menyediakan tool otomatis untuk menempelkan komponen `POIData` ke semua child GameObject di bawah root POI.

**Langkah:**
1. Buka Unity Editor
2. Klik menu **Tools > POI > Auto Attach POIData**
3. Tool akan scan semua children di bawah root POI dan menambahkan komponen `POIData` jika belum ada
4. Setelah selesai, isi field `poiName`, `kategori`, dan `sinonim` di setiap POIData via Inspector

### 4. Menambahkan POI Baru ke Scene

**Langkah step-by-step:**
1. Di Hierarchy, cari atau buat GameObject parent bernama **"POIs"** (atau nama lain yang sudah di-assign ke `poiRoot` di POIManager)
2. Klik kanan pada "POIs" > **Create Empty** untuk membuat child GameObject baru
3. Beri nama sesuai lokasi (contoh: "BAAK", "Toilet Lt2", "Lab Teori 201")
4. Posisikan GameObject ke lokasi yang benar di scene (sesuai peta indoor)
5. Pilih GameObject tersebut, klik **Add Component** > cari **POIData**
6. Isi field berikut di Inspector:
   - **Poi Name**: Nama resmi POI (contoh: "BAAK"). Jika kosong, akan pakai nama GameObject
   - **Kategori**: Kategori POI (contoh: "layanan", "toilet", "ruangan", "kantin")
   - **Sinonim**: Klik **+** untuk menambah alias/nama lain. Contoh untuk BAAK: "biro akademik", "administrasi akademik"
7. Pastikan komponen **POI** dari MultiSet SDK juga terpasang di GameObject yang sama (agar navigasi SDK berfungsi)

### 5. Wiring NavigationAdapter di Inspector

Pilih GameObject yang memiliki komponen `NavigationAdapter`, lalu isi field berikut:

| Field | Yang Harus Di-Assign | Keterangan |
|-------|---------------------|------------|
| **Navigation Controller** | Drag komponen `NavigationController` dari MultiSet SDK | Untuk memanggil navigasi via SendMessage |
| **Set Poi Method Name** | `SetPOIForNavigation` (default) | Nama method di NavigationController |
| **Navigation UI Controller** | Drag komponen `NavigationUIController` | Untuk menampilkan progress slider |
| **Start Navigation UI Method Name** | `ClickedStartNavigation` (default) | Nama method di NavigationUIController |
| **Destination Select UI** | Drag panel UI daftar destinasi (opsional) | Akan disembunyikan setelah navigasi mulai |
| **On Navigate To Transform/Position/Name** | Wire ke handler yang sesuai | Event alternatif jika tidak pakai SendMessage |
| **On Navigation Failed** | Wire ke UI error handler | Ditampilkan saat navigasi gagal |

>[!TIP]
>**Tips:** Klik kanan pada komponen NavigationAdapter di Inspector > **Validate Wiring** untuk mengecek apakah semua referensi sudah benar.

### 6. NavMeshObstacleHelper (Obstacle Dinamis)

File `Assets/VoiceInput/NavMeshObstacleHelper.cs` adalah **pola/pattern** untuk obstacle dinamis yang terintegrasi dengan deteksi kerumunan dari backend YOLO (`/api/human`).

Cara menggunakan:
1. Buat GameObject baru di scene untuk merepresentasikan area kerumunan
2. Tambahkan komponen **NavMeshObstacleHelper** (NavMeshObstacle akan otomatis ditambahkan)
3. Script akan otomatis mengkonfigurasi carving pada NavMeshObstacle
4. Data dari YOLO backend memanggil `SetObstacleSize()` dan `SetObstacleActive()` untuk mengupdate obstacle secara real-time

>[!NOTE]
> **Catatan**: Pendekatan NavMeshObstacle carving dipilih karena lebih ringan dibanding full NavMesh rebaking, dan cukup akurat untuk bounding box kerumunan.

### 7. Deteksi Keramaian (YOLO + NavMesh Dinamis)

Pipeline deteksi kerumunan sudah diimplementasikan penuh. Alur data:

```
CCTV / IP Webcam (Backend/yolo_api.py, YOLOv8n)
    ↓
GET /api/human  (setiap 5 detik)
    ↓
YoloManager         → toggle NavMeshObstacle per lorong + notifikasi "Putar Balik"
BoundingBoxOverlay  → gambar bounding box tag "Human" di layar CCTV
```

**Menjalankan backend YOLO:**
```bash
cd Backend
pip install -r requirements.txt
python yolo_api.py
# masukkan URL kamera saat diminta (contoh: http://192.168.1.10:8080/video, atau 0 untuk webcam laptop)
```
Server berjalan di `http://localhost:5000/api/human`.

**Setup di Unity:**
1. Tambahkan komponen `YoloManager` ke GameObject di scene
2. Assign `rintanganLorong1` / `rintanganLorong2` (NavMeshObstacle per lorong)
3. Assign `visualTembok1` / `visualTembok2` (MeshRenderer tembok, opsional)
4. Assign `uiPutarBalik` (prefab `Panel_Notif_PutarBalik`)
5. Sesuaikan `apiUrl` ke IP laptop server YOLO

### 8. Routing Berbasis Kepadatan (Density Routing)

Untuk rute yang menyesuaikan tingkat keramaian secara halus (dengan hysteresis agar tidak berkedip):

```
Backend /density  →  NavigationBridge (polling 1 detik, hysteresis K=2 / M=3)
                          ↓
                    NavMeshController (recompute NavMeshPath)
                          ↓
                    ARRouteVisualizer (render garis AR + panah arah)
                          ↓
                    LatencyLogger (CSV: t4→t5→t6)
```

| Komponen | Fungsi |
|---|---|
| `NavigationBridge` | Polling `/density`, terapkan hysteresis, panggil controller |
| `NavMeshController` | Mapping `zone_id` → `NavMeshObstacle`, hitung ulang path |
| `ARRouteVisualizer` | Render `LineRenderer` neon + panah arah di sepanjang path |
| `LatencyLogger` | Catat latensi ke `latency_unity.csv` di `persistentDataPath` |

Scene contoh: `Assets/MapPSDKUwithCrowdDetection.unity` dan `Assets/MapRSI.unity`.


## Troubleshooting
- Jika scene kosong atau error, reimport project (Right click folder proyek > Reimport All).
- Jika package MultiSet tidak terdownload, cek koneksi dan re-open Unity.

## Kontak
Jika ada pertanyaan, buat issue atau diskusikan via chat tim.

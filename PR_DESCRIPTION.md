# feat: YOLO crowd detection + dynamic NavMesh density routing

> **Jenis PR:** Feature + Breaking Change
> **Branch:** `backend` → `main`
> **⚠️ Perlu merge manual:** PR ini **TIDAK** fast-forward. `main` sudah maju 6 commit dan ada **13 file yang konflik nyata**. Baca [Bagian 5](#5--panduan-resolusi-konflik-untuk-tim-merger) sebelum merge.

---

## 1. 📌 Ringkasan

PR ini mengintegrasikan **deteksi keramaian berbasis YOLO** dengan **routing NavMesh dinamis** pada proyek AR navigation MultiSetNav, serta menambahkan dua scene demo, map data MultiSet baru, dan navbar UI.

Ini menuntaskan roadmap yang sudah tertulis di `README.md` ("di masa depan, data dari YOLO backend akan memanggil `SetObstacleSize()` dan `SetObstacleActive()`") dan section *"Integrating YOLO Crowd Detection"* di `CLAUDE.md`.

**Statistik PR:** 192 file berubah, +15.449 / −18.539 baris, 14 commit.

### ⚠️ Status Divergence (WAJIB DIBACA)

`main` sudah bergerak **6 commit / 844 file** sejak branch `backend` dibuat:

```
main:    f02f063 → d2c0d83 → 1a4506b → 6a811bf → fdb3ac3 → e127147 (HEAD)
backend: ... → 4febe79 (HEAD)          ← 8 commit di belakang titik cabang
```

| Komponen | `main` | `backend` | Status |
|---|---|---|---|
| Commit ahead | 6 | 8 | **Diverged** — bukan fast-forward |
| File berubah | 844 | 192 | — |
| **File overlap** | — | — | **44 file** |
| → Isi identik | — | — | **31 file** (aman, auto-merge) |
| → **Konflik nyata** | — | — | **🔴 13 file** |

**Commit yang sudah masuk `main` tapi belum ada di `backend`:**
- `e127147` UI ToolKit implemented on Register Page
- `fdb3ac3` feat: Add UI Toolkit onboarding flow (Login, Register, Home dashboard)
- `6a811bf` feat: Add UI Toolkit hybrid layer with Splash & Auth Gate screens
- `1a4506b` Clean up Meta Quest residue & redesign multiplayer friend list
- `d2c0d83` Adding video Meta, and icons
- `f02f063` Feat: New different ui changes and backend changes

> **Konsekuensi:** `main` **sudah menghapus Meta Quest** (`1a4506b`) — ini sejalan dengan penghapusan `com.multiset.sdk.quest` di PR ini. Namun `main` juga menambahkan **UI Toolkit layer lengkap**, Coplay/MCP packages, dan mengubah `manifest.json` + `ProjectSettings.asset` — semua ini harus digabung hati-hati.

---

## 2. 🎯 Fitur Baru

### 2.1 Crowd Detection (sudah ada di commit `f8c1e49`)

| Komponen | Fungsi |
|---|---|
| `Assets/YoloManager.cs` | Polling `GET /api/human` setiap 5 detik. Menyalakan/mematikan `NavMeshObstacle` per lorong (carving) + memicu notifikasi "Putar Balik". Logika pemilihan lorong: jika kedua lorong ramai, pilih yang lebih sepi. |
| `Assets/BoundingBoxOverlay.cs` | Menggambar bounding box untuk semua GameObject bertag `Human` di CCTV camera, dengan object pooling agar tidak membebani frame. |
| `Assets/Resources/Panel_Notif_PutarBalik.prefab` | Panel notifikasi "putar balik". |
| `Assets/Resources/TembokLorong1.prefab`, `TembokLorong2.prefab` | Visual tembok/hambatan per lorong. |

### 2.2 Density Routing Pipeline (baru — commit `3ff57c9`)

```
Backend /density  →  NavigationBridge (polling 1 detik, hysteresis K=2 / M=3)
                          ↓
                    NavMeshController (recompute NavMeshPath)
                          ↓
                    ARRouteVisualizer (render garis AR + panah arah)
                          ↓
                    LatencyLogger (CSV: t4 → t5 → t6)
```

| Komponen | Fungsi |
|---|---|
| `Assets/NavigationBridge.cs` | Polling `GET /density` setiap 1 detik. Menerapkan **hysteresis** (`K=2` siklus untuk mengaktifkan obstacle, `M=3` siklus untuk menonaktifkan) supaya obstacle tidak berkedip saat data noisy. |
| `Assets/NavMeshController.cs` | Mapping `zone_id` → `NavMeshObstacle` (carving), lalu recompute `NavMeshPath` menuju emergency exit terdekat. |
| `Assets/ARRouteVisualizer.cs` | Merender `LineRenderer` neon cyan sepanjang path + instantiate panah arah tiap 1 meter (`arrowSpacing`). |
| `Assets/LatencyLogger.cs` | Menulis CSV ke `Application.persistentDataPath/latency_unity.csv` berisi `t4_api_received`, `t5_navmesh_done`, `t6_ar_updated`, dan delta latensi. |

### 2.3 Scene & Aset (commit `f6b634b`)

- `Assets/MapPSDKUwithCrowdDetection.unity` — scene demo crowd detection
- `Assets/MapRSI.unity` — scene demo density routing
- `Assets/MultiSet/MapData/MAP_9UMXWR0PI7K5/` — map baru (prefab + GLB)
- `Assets/MultiSet/MapData/MAP_PHVXJ15C2CF0/` — map baru (prefab + GLB)
- NavMesh ter-bake untuk sample Navigation & Quest Localization

### 2.4 Navbar UI (commit `4571ef9`)

- `Assets/UI/Scriprt_Navbar/Button_navManager.cs` — `BottomNavigationManager`: tab bar bawah dengan styling aktif (kuning) / nonaktif (transparan), auto-wiring listener dengan closure-safe index.
- Set ikon UI: back, mic, search, info, user-group, map kampus.
- Aset Human Character Dummy (Kevin Iglesias, 57 file) + `FinalBaseMesh*.obj` sebagai visual manusia/nyamuk.

### 2.5 Perbaikan Repo (commit `c9ae0d0`)

- **Menghapus gitlink submodule palsu `MultiSetNav`** — folder project sebelumnya ter-commit sebagai submodule (160000) tanpa `.gitmodules`, yang menyebabkan error saat clone.
- Untrack 13 artefak `*.lscache` dan `MultiSetNav.slnx`.
- `*.webm` ditambahkan ke Git LFS.

### 2.6 Dokumentasi (commit `4febe79`)

- `README.md`: menambahkan section **7 (Deteksi Keramaian)** dan **8 (Routing Berbasis Kepadatan)** dengan instruksi setup lengkap.

---

## 3. ⚠️ BREAKING CHANGES

| # | Perubahan | Dampak | Aksi yang diperlukan |
|---|---|---|---|
| 1 | `Packages/manifest.json` **menghapus `com.multiset.sdk.quest`** | Sample Quest SDK tidak akan resolve | Jika Quest masih dipakai, re-add dependency ini |
| 2 | `WholePSDKU_V2.unity` **dihapus** (−7.535 baris) | Scene ini sudah tidak ada | Migrasi ke `MapPSDKUwithCrowdDetection.unity` |
| 3 | `ProjectSettings/NavMeshAreas.asset`: `agentSlope 43.42 → 60`, `agentClimb 0.75 → 0.5` | Perilaku pathfinding berubah; NavMesh lama tidak valid | **Re-bake NavMesh** di semua scene |
| 4 | Tag baru `Human` di `TagManager.asset` | `BoundingBoxOverlay` dan prefab bergantung pada tag ini | Tag sudah ikut di-commit; pastikan tidak bentrok saat merge |
| 5 | `MultiSetNav` gitlink dihapus | Clone lama yang menganggapnya submodule perlu `git rm --cached MultiSetNav` | Sudah ditangani di PR ini |

---

## 4. 🧪 Testing yang Sudah Dilakukan

- ✅ Scene `MapPSDKUwithCrowdDetection` berjalan di Unity Editor 6000.3.14f1 tanpa missing script
- ✅ `YoloManager` menerima response `/api/human` dan toggle `NavMeshObstacle` sesuai `crowded_1`/`crowded_2`
- ✅ Notifikasi "Putar Balik" muncul saat status lorong berubah dari ramai → sepi
- ✅ `NavigationBridge` polling `/density` dan menerapkan hysteresis tanpa flapping
- ✅ `ARRouteVisualizer` merender ulang path saat zone berubah status
- ✅ `LatencyLogger` menghasilkan `latency_unity.csv` di `persistentDataPath`
- ✅ `BottomNavigationManager` menampilkan state aktif/nonaktif dengan benar

> **Belum diuji:** build APK di perangkat Android fisik (perlu WiFi yang sama dengan server Ollama + YOLO).

---

## 5. 🔀 Panduan Resolusi Konflik (untuk Tim Merger)

### 5.1 ⚠️ Prasyarat: Aktifkan UnityYAMLMerge

`.gitattributes` mendeklarasikan `*.unity`, `*.prefab`, `*.asset`, `*.meta` dengan `merge=unityyamlmerge`. **Tanpa driver ini terdaftar, semua file tersebut akan konflik teks mentah** dan berisiko merusak scene.

Verifikasi di mesin merger:

```bash
git config --get merge.unityyamlmerge.driver
```

Jika kosong, daftarkan (Windows, sesuaikan versi Unity):

```bash
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.3.14f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %A %B %L %P'
git config --global merge.unityyamlmerge.recursive binary
```

Atau lebih aman: set `mergespecfile.txt` Unity ke `%l` → `useOurs` untuk fallback otomatis.

### 5.2 🔴 13 File Konflik Nyata (hasil analisis otomatis)

File-file berikut **isi di `main` dan `backend` berbeda** dan akan memicu konflik saat merge:

| # | File | Tipe | Rekomendasi |
|---|---|---|---|
| 1 | `Assets/_Recovery/0 (3).unity` | 🔴 Biner | Ambil **`backend`** (lebih baru, 423 KB vs 410 KB) |
| 2 | `Assets/video/video.webm` | 🔴 Biner (LFS) | Ambil **`backend`** (sudah LFS pointer, 131 byte) |
| 3 | `Assets/Samples/.../Navigation/WholePSDKU/NavMesh-NavMesh.asset` | 🔴 Biner | Ambil **`backend`** — atau re-bake |
| 4 | `Assets/Icon.meta` | 🟠 YAML | Ambil **`backend`** |
| 5 | `Assets/video.meta` | 🟠 YAML | Ambil **`backend`** |
| 6 | `Assets/Samples/.../Localization/Localization.unity` | 🟠 YAML | Ambil **`backend`**, verifikasi di Unity |
| 7 | `Assets/Samples/.../Resources/Materials/NavPathMaterial.mat` | 🟠 YAML | Ambil **`backend`** |
| 8 | `Assets/Samples/MultiSet Quest SDK/.../NavPathMaterial.mat` | 🟠 YAML | Ambil **`backend`** |
| 9 | `Assets/XR/Settings/OpenXR Package Settings.asset` | 🟠 YAML | **Gabungkan** — lihat catatan di bawah |
| 10 | `Packages/manifest.json` | 🟢 Teks | **Gabungkan manual** — lihat catatan di bawah |
| 11 | `Packages/packages-lock.json` | 🟢 Teks | **Regenerate** — jangan merge manual |
| 12 | `ProjectSettings/ProjectSettings.asset` | 🟠 YAML | **Gabungkan** — lihat catatan di bawah |
| 13 | `MultiSetNav.slnx` | 🟢 Teks | Ambil **`backend`** (file ini sudah di-untrack di PR ini) |

#### Catatan khusus per file

**#10 `Packages/manifest.json` — PERLU KEPUTUSAN**

| Package | `main` | `backend` (PR ini) | Rekomendasi |
|---|---|---|---|
| `com.coplaydev.coplay` | ✅ ada | ❌ tidak ada | **Pertahankan dari `main`** |
| `com.coplaydev.unity-mcp` | ✅ ada | ❌ tidak ada | **Pertahankan dari `main`** |
| `com.multiset.sdk.quest` | ✅ dihapus | ✅ dihapus | Sama — tidak konflik |
| `com.unity.probuilder` | ❌ tidak ada | ✅ `6.0.9` | **Ambil dari `backend`** |
| `com.veriorpies.parrelsync` | `file:com.veriorpies.parrelsync` (vendored) | git URL | **Ambil dari `main`** (vendored lebih stabil) |

Hasil merge yang diharapkan: **gabungan keduanya** — Coplay/MCP dari `main` + ProBuilder dari `backend`, ParrelSync versi vendored dari `main`.

**#11 `Packages/packages-lock.json`**
Jangan merge manual. Hapus file lalu biarkan Unity regenerate:
```bash
git checkout --theirs -- Packages/packages-lock.json   # atau --ours
rm Packages/packages-lock.json
# buka Unity → Package Manager akan regenerate → commit
```

**#9 `Assets/XR/Settings/OpenXR Package Settings.asset`**
Konflik pada setting Meta Quest yang sudah di-cleanup di `main` (`1a4506b`):
- `m_latencyOptimization: 0` → `1`
- `m_optimizeBufferDiscards: 0` → `1`
- `MetaQuestTouchPlusControllerProfile m_enabled: 1` → `0`

**Ambil versi `main`** — `main` sudah membersihkan residu Quest, sejalan dengan arah PR ini.

**#12 `ProjectSettings/ProjectSettings.asset`**
Perbedaan:
- Urutan `preloadedAssets` (kosmetik — ambil `main`)
- `AndroidMinSdkVersion: 32` (backend) vs `29` (main) → **konfirmasi kebutuhan minimum**, `32` lebih ketat
- `main` menambahkan konfigurasi build target icons

**Gabungkan**: ambil urutan `preloadedAssets` dan icons dari `main`, pertahankan `AndroidMinSdkVersion: 32` dari `backend` (kecuali tim memutuskan lain).

### 5.3 ✅ 31 File Overlap yang Aman (isi identik)

File-file ini berubah di kedua branch tapi **isinya byte-identical** — git akan auto-merge tanpa konflik:

```
Assets/DefaultNetworkPrefabs.asset
Assets/Icon/back.png (+.meta), ic--outline-info.png (+.meta), info.png (+.meta)
Assets/Icon/Map Kampus PSDKU LAMONGAN.png (+.meta)
Assets/Icon/material-symbols--search (1).png (+.meta), (2).png (+.meta)
Assets/Icon/mic.png (+.meta), New Render Texture.renderTexture (+.meta)
Assets/Icon/rivet-icons--user-group-solid.png (+.meta), (1).png (+.meta)
Assets/Icon/weui--back-filled.png (+.meta)
Assets/Samples/MultiSet Quest SDK/.../Localization.unity
Assets/Samples/MultiSet-SDK/.../NavPathMaterial.mat (Navigation)
Assets/UI/Scriprt_Navbar.meta
Assets/UI/Scriprt_Navbar/Button_navManager.cs (+.meta)
Assets/video/Desain tanpa judul.mp4 (+.meta), download.mp4 (+.meta)
Assets/video/video.webm.meta
```

> ✅ Ini menandakan kedua branch mengambil aset dari sumber yang sama — tidak perlu tindakan.

### 5.4 🔴 File Biner Tambahan — JANGAN merge teks

Selain #1–#3 di atas, file biner berikut **hanya ada di `backend`** (tidak konflik, tapi tetap tidak bisa di-merge teks):

| File | Rekomendasi |
|---|---|
| `Assets/_Recovery/0 (4).unity` | Ambil **versi `backend`** |
| `Assets/NavMesh-MSET_ILCVRIPL5RMZ.asset` | Ambil **`backend`** |
| `Assets/Samples/.../Navigation/Navigation/NavMesh-NavMesh.asset` | Ambil **`backend`** |
| `Assets/Samples/.../NavMeshExport/NavMeshExport/NavMesh-Map Space.asset` | Ambil **`backend`** |
| `Assets/Samples/.../NavMeshExport/NavMeshExport/NavMesh-NavMesh.asset` | Ambil **`backend`** |

### 5.5 🟢 File Baru di PR ini — tidak konflik

File berikut **hanya ditambahkan di `backend`**, sehingga masuk bersih tanpa konflik:

```
Assets/NavigationBridge.cs (+.meta)
Assets/NavMeshController.cs (+.meta)
Assets/ARRouteVisualizer.cs (+.meta)
Assets/LatencyLogger.cs (+.meta)
Assets/MapRSI.unity (+.meta)
Assets/MapPSDKUwithCrowdDetection.unity (+.meta)
Assets/MultiSet/MapData/MAP_9UMXWR0PI7K5/ (+.meta)
Assets/MultiSet/MapData/MAP_PHVXJ15C2CF0/ (+.meta)
Assets/Kevin Iglesias/ (57 files)
Assets/FinalBaseMesh.obj, Assets/FinalBaseMesh 1.obj (LFS)
Assets/UI/Images/Jalur ramai!.png
ProjectSettings/ShaderGraphSettings.asset
ProjectSettings/Packages/com.unity.probuilder/
```

### 5.6 ⚠️ Rename palsu — ABAIKAN

Git mendeteksi 5 "rename" yang sebenarnya add/delete tidak berhubungan. **Jangan ikuti saran rename git**, terima hasil apa adanya:

```
R077  Navigation/WholePSDKU.meta           → Assets/Icon.meta
R067  Navigation/WholePSDKU.unity.meta     → Localization.meta
R067  Navigation/WholePSDKU_V2.unity.meta  → NavMeshExport.meta
R079  Navigation/WholePSDKU/NavMesh-*.meta → NavMeshExport/NavMesh-*.meta
R085  Navigation/WholePSDKU.unity          → _Recovery/0 (4).unity
```

### 5.7 🔁 Urutan Merge yang Disarankan

```bash
git checkout main
git pull origin main

# (opsional) merge backend DULU ke branch lokal untuk melihat konflik
git merge --no-commit --no-ff backend
git status          # lihat 13 file konflik

# 1. Selesaikan file TEKS/TEKS-KONFIG dulu:
#    Packages/manifest.json  -> gabungkan (Coplay+MCP dari main, ProBuilder dari backend)
#    ProjectSettings/ProjectSettings.asset -> gabungkan
#    Assets/XR/Settings/OpenXR Package Settings.asset -> ambil main
#    Assets/Icon.meta, Assets/video.meta, MultiSetNav.slnx -> ambil backend

# 2. File BINER (pilih versi utuh, JANGAN merge teks):
git checkout --theirs -- "Assets/_Recovery/0 (3).unity"
git checkout --theirs -- "Assets/Samples/.../NavMesh-NavMesh.asset"
git checkout --theirs -- "Assets/video/video.webm"

# 3. YAML scene/material -> ambil backend, verifikasi di Unity
git checkout --theirs -- "Assets/Samples/.../Localization.unity"
git checkout --theirs -- "Assets/Samples/.../NavPathMaterial.mat"

# 4. packages-lock.json -> regenerate via Unity
rm Packages/packages-lock.json

git add -A
git commit
```

> **Penting:** `--theirs`/`--ours` bergantung arah merge. Jika tim merger melakukan `git checkout main && git merge backend`, maka `--theirs` = `backend`. **Selalu verifikasi** sebelum commit:
> ```bash
> git log --oneline -1 -- "<file>"     # commit mana yang menang
> git diff --stat HEAD -- "<file>"     # pastikan tidak ada perubahan tak terduga
> ```

**Setelah merge, WAJIB:**
1. Buka project di Unity 6000.3.14f1, tunggu package resolve
2. **Karena `main` menambahkan Coplay + unity-mcp**, pastikan package tersebut tetap resolve (network ke GitHub)
3. Re-import All jika ada missing reference
4. **Re-bake NavMesh** dengan `agentSlope=60`, `agentClimb=0.5`
5. Cek setiap scene tidak ada "Missing (Mono Script)"
6. **Cek bentrok UI**: `main` punya UI Toolkit layer (Splash/AuthGate/Login/Register/Home) + `backend` punya navbar UGUI — pastikan keduanya tidak saling menimpa di scene yang sama
7. Jalankan scene `MapPSDKUwithCrowdDetection` sebagai smoke test
8. Jalankan scene UI Toolkit baru sebagai smoke test

---

## 6. 📋 Checklist Reviewer

**Keputusan yang diperlukan:**
- [ ] Konfirmasi `com.multiset.sdk.quest` boleh dihapus dari `manifest.json` — ⚠️ **catatan:** `main` sudah lebih dulu menghapusnya di `1a4506b` ("Clean up Meta Quest residue"), jadi ini sejalan
- [ ] Konfirmasi `WholePSDKU_V2.unity` sudah tidak dipakai siapa pun
- [ ] Setujui perubahan `agentSlope 43.42→60` / `agentClimb 0.75→0.5` (perlu re-bake)
- [ ] **`manifest.json` merge:** setuju gabungan = Coplay + unity-mcp (dari `main`) + ProBuilder (dari PR ini) + ParrelSync vendored (dari `main`)
- [ ] **`ProjectSettings.asset`:** `AndroidMinSdkVersion` 32 (PR ini) vs 29 (`main`) — pilih mana?
- [ ] Setujui penambahan aset 3D: `Kevin Iglesias/` (2.75 MB), `FinalBaseMesh*.obj` (2.46 MB ×2)

**Verifikasi:**
- [ ] `packages-lock.json` ikut ter-commit (sesuai aturan README baris 30)
- [ ] Semua asset baru punya `.meta` (sudah diaudit — ✅ lolos)
- [ ] Tag `Human` tidak bentrok dengan tag di `main`
- [ ] `YoloManager.apiUrl` dan `NavigationBridge.serverUrl` masih `localhost` — perlu disesuaikan per environment demo
- [ ] Konfirmasi `main` tidak punya file dengan nama sama (`NavigationBridge.cs`, `NavMeshController.cs`, dll.) — ✅ sudah dicek, tidak ada

---

## 7. ⚠️ Catatan Teknis

### 7.1 ⚠️ File melebihi batas GitHub
`Assets/MultiSet/MapData/MAP_PHVXJ15C2CF0/MAP_PHVXJ15C2CF0.glb` berukuran **71.22 MB**, melebihi rekomendasi GitHub 50 MB. GitHub menerima push (hanya warning), tetapi:
- ⚠️ **Tidak masuk Git LFS** — `.gitattributes` belum punya `*.glb`. Sebaiknya ditambahkan di follow-up PR.
- Jika ingin mengurangi ukuran repo, pertimbangkan Git LFS atau Git LFS migrate.

### 7.2 `.glb` belum di LFS
Pola `*.glb` belum terdaftar di `.gitattributes`. Disarankan follow-up:
```gitattributes
*.glb    lfs
```

### 7.3 Konfigurasi IP masih hardcoded localhost
- `YoloManager.apiUrl` = `http://localhost:5000/api/human`
- `NavigationBridge.serverUrl` = `http://localhost:5000/density`

Untuk demo di perangkat Android, ganti ke IP laptop (DHCP, berubah tiap ganti WiFi) — sama seperti pola `OllamaConnector` yang sudah didokumentasikan di README.

### 7.4 Backend sudah ada di main
`Backend/yolo_api.py` dan `Backend/requirements.txt` **sudah identik** di `main` dan `backend`, sehingga tidak muncul di diff PR ini.

### 7.5 Perbaikan repo yang ikut PR ini
- Gitlink submodule palsu `MultiSetNav` dihapus (menyebabkan error clone)
- 13 file `*.lscache` + `MultiSetNav.slnx` di-untrack dan masuk `.gitignore`
- `*.webm` dipindah ke Git LFS (sebelumnya binary 683 KB masuk raw)

### 7.6 Dampak perubahan main terhadap isi PR
Karena `main` bergerak setelah `backend` dibuat:
- Beberapa file di PR ini **sudah ada di `main`** dengan isi identik (video, icons, navbar) → tidak perlu di-review ulang
- `main` menghapus Meta Quest secara paralel → **PR ini dan `main` sejalan** di isu Quest

---

## 8. 🗺️ Langkah Setelah Merge

1. Buka project di Unity 6000.3.14f1
2. Tunggu package resolve (ProBuilder ditambahkan, Quest SDK dihapus, Coplay/MCP tetap dari `main`)
3. Re-import All jika ada aset missing
4. **Re-bake NavMesh** semua scene dengan setting agent baru (`slope 60`, `climb 0.5`)
5. Jalankan `pip install -r Backend/requirements.txt` lalu `python Backend/yolo_api.py`
6. Smoke test `MapPSDKUwithCrowdDetection` dan `MapRSI`
7. Smoke test UI Toolkit layer (Splash → AuthGate → Login/Register → Home) agar tidak bentrok dengan navbar UGUI
8. Sesuaikan `apiUrl` / `serverUrl` ke IP server di environment demo

---

## 9. 📚 Referensi Dokumentasi

- `README.md` section 7 & 8 (ditambahkan di PR ini)
- `CLAUDE.md` → *"Integrating YOLO Crowd Detection"* dan *"Dynamic Obstacles"*
- `.gitattributes` → konfigurasi Git LFS dan `unityyamlmerge`
- Repo ini tidak memiliki `CONTRIBUTING.md` / PR template, sehingga panduan merge di atas disusun khusus untuk PR ini

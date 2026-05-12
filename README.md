# MultiSetNav

Proyek Unity ini berisi eksplorasi MultiSet SDK dengan sample scene (multiplayer dan on-device localization). Repo ini cocok sebagai basis belajar, percobaan, dan kolaborasi.

## Ringkas
- Engine: Unity 6000.3.14f1
- SDK utama: MultiSet Unity SDK
- Fitur yang tampak: multiplayer sample, on-device localization, AR (ARCore)

## Struktur Penting
- `Assets/Scenes/` scene proyek (default: SampleScene)
- `Assets/Samples/MultiSet-SDK/` sample resmi dari MultiSet SDK
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

## Troubleshooting
- Jika scene kosong atau error, reimport project (Right click folder proyek > Reimport All).
- Jika package MultiSet tidak terdownload, cek koneksi dan re-open Unity.

## Kontak
Jika ada pertanyaan, buat issue atau diskusikan via chat tim.

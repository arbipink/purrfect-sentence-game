# Purrfect Sentence - Audio And Effects

## Role

Peran saya di kelompok adalah Audio and Effects Engineer. Bagian ini hanya menambahkan dan menghubungkan audio, backsound, sound effect, dan visual effect ke sistem yang sudah ada.

UI final dikerjakan oleh bagian UI/UX dan tidak diubah.

## Script Yang Dibuat

- `Assets/Scripts/AudioManager.cs`
- `Assets/Scripts/SceneAudioController.cs`
- `Assets/Scripts/SimpleVFXManager.cs`
- `Assets/Scripts/ButtonClickSFX.cs`
- `Assets/Editor/PurrfectAudioEffectsSetup.cs`

## BGM Per Scene

- `MainMenu`: `mainMenuBGM`
- `Credit`: `mainMenuBGM`
- `Scene_Easy` atau `LevelEasy`: `easyBGM`
- `Scene_Medium` atau `LevelMedium`: `mediumBGM`
- `Scene_Hard` atau `LevelHard`: `hardBGM`

## SFX Per Event

- Button Play, Credit, Exit, Easy, Medium, Hard, Resume, Restart, Back to Menu: `buttonClickSFX`
- Pause toggle: `buttonClickSFX` dan `pauseSFX`
- Pause slider BGM: mengatur `AudioManager.SetBGMVolume`
- Pause slider SFX: mengatur `AudioManager.SetSFXVolume`
- Pause slider VFX jika tersedia: mengatur `SimpleVFXManager.SetVFXIntensity`
- Player slash atau tap kata: `slashSFX`
- Jawaban SPOK benar: `correctAnswerSFX`
- Jawaban SPOK salah: `wrongAnswerSFX`
- Enemy kalah atau hilang: `enemyDefeatedSFX`
- HP berkurang: `damageSFX`
- Game Over: `gameOverSFX`
- Level Complete: `levelCompleteSFX`

## VFX Per Event

- Slash trail: prefab slash jika tersedia, fallback memakai `LineRenderer`
- Correct answer: sparkle prefab jika tersedia, fallback memakai particle sparkle sederhana
- Enemy defeated: pop atau explosion prefab jika tersedia, fallback memakai particle burst
- Damage: prefab damage jika tersedia dan red flash overlay singkat
- Wrong answer: prefab wrong feedback jika tersedia dan shake ringan
- Level complete: sparkle prefab jika tersedia, fallback memakai particle sparkle

Semua audio dan VFX dibuat null-safe. Jika AudioClip atau prefab belum diisi, game tetap berjalan tanpa error.

## Cara Menjalankan Setup Tool

1. Buka Unity.
2. Pilih menu `Tools > Purrfect Sentence > Setup Audio And Effects`.
3. Tunggu log `[Purrfect Audio/Effects]` muncul di Console.
4. Tool akan membuat `AudioManager`, `SimpleVFXManager`, dan `SceneAudioController` jika belum ada.
5. Tool akan mencoba auto-assign AudioClip dan prefab VFX berdasarkan nama file.
6. Tool akan menambahkan komponen `ButtonClickSFX` ke Button yang sudah ada tanpa menghapus atau mengubah desain UI.

## Cara Test

### Test BGM

1. Jalankan scene `MainMenu`; pastikan BGM menu terdengar.
2. Masuk ke `Scene_Easy`; pastikan BGM easy terdengar.
3. Masuk ke `Scene_Medium`; pastikan BGM medium terdengar.
4. Masuk ke `Scene_Hard`; pastikan BGM hard terdengar.

### Test Button SFX

Klik Play, Credit, Exit, Easy, Medium, Hard, Resume, Restart, dan Back to Menu. Setiap klik harus memutar `buttonClickSFX`.

### Test Slash SFX

Klik atau drag pada kata/enemy. Saat kata dipilih atau disambungkan, `slashSFX` dan slash trail harus muncul.

### Test Correct Dan Wrong Answer

Susun urutan SPOK yang benar untuk memutar `correctAnswerSFX` dan sparkle. Susun urutan salah untuk memutar `wrongAnswerSFX` dan shake ringan.

### Test Damage SFX

Biarkan enemy menyentuh player sampai HP berkurang. `damageSFX` dan red flash harus muncul.

### Test Game Over SFX

Biarkan HP habis. `gameOverSFX` harus diputar satu kali sebelum scene restart.

### Test Level Complete SFX

Selesaikan semua kalimat pada level. `levelCompleteSFX` dan sparkle harus muncul satu kali.

## Catatan

- UI final tidak dihapus, tidak diganti, dan tidak didesain ulang.
- Gameplay SPOK utama tidak dirombak.
- Folder audio yang ditemukan di project saat ini adalah `Assets/Audio/BGM` dan `Assets/Audio/SFX`.
- Kandidat VFX ditemukan di folder prefab effect seperti `Assets/Hovl Studio/Magic effects pack`, `Assets/JMO Assets/Cartoon FX Remaster`, `Assets/Andtech/Star Pack`, dan `Assets/Prefabs`.

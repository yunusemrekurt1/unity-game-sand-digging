# Kum Kazma Oyunu — Unity 6 (2D) — v2.0

> 🎮 **Tek yapman gereken:** Unity 6'da bu klasörü aç, derlenmesini bekle, Ana Menü otomatik açılır. Bölüm seç, oyna.

## ✨ v2.0 Yenilikler
- **⭐ Yıldız Sistemi:** Her bölümde 9 top. 3/6/9 ulaştırırsan 1/2/3 yıldız. PlayerPrefs ile kalıcı kaydedilir.
- **🏠 Ana Menü:** 10 bölüm butonu, her birinde en yüksek yıldız skorun görünür.
- **🕳️ 5× Daha Derin:** Haritalar 30–75 unit uzunluğunda. Kamera aşağı kayarken sürekli yeni tehlikeler.
- **🌀 Vortex Kasa:** Top kasaya girerken dönerek küçülerek yok olur. Artık boşluğa düşme görüntüsü yok.
- **💣 Krater Bomba:** Patladığında etrafındaki **kumu da temizler** ve yarıçaptaki tüm topları yok eder.
- **🌊 Su Alanı:** Toplar suda yavaşlar, yer çekimi azalır.
- **🔥 Lav:** Dokunan top anında yok olur.
- **🐍 Yılan:** Devriye gezen kinematik düşman, değdiği topu yok eder.

---

## 🚀 Hızlı Başlangıç

1. ZIP'i çıkar.
2. Unity Hub → **Add** → klasörü seç → Unity 6 ile aç.
3. Proje yüklenirken Editor otomatik olarak:
   - Tüm tag'leri ekler
   - 10 prefabı üretir (Top, Kaya, Diken, Bomba, Kasa, Cogaltici, HareketliKaya, Su, Lav, Yilan)
   - 1 Ana Menü + 10 bölüm sahnesi oluşturur
   - Build Settings'e ekler
4. **▶ Play** → Ana Menü açılır → **OYNA** → Bölüm seç → Oyna!

Otomatik kurulum olmazsa: `Tools → Bolumleri Uret → Hepsini Uret`.

---

## 🎯 Yıldız Sistemi

| Ulaşan Top | Yıldız |
|-----------|--------|
| 3-5 | ⭐ |
| 6-8 | ⭐⭐ |
| 9 | ⭐⭐⭐ |
| 0-2 | Kaybettin |

Yıldızlar `PlayerPrefs` altında `Stars_Level_01 … Stars_Level_10` anahtarlarıyla saklanır. Sıfırlamak için: `Tools → Bolumleri Uret → Yildizlari Sifirla`.

---

## 🕹️ Oynanış

| Giriş | Etki |
|-------|------|
| Ekrana basılı tut + sürükle | Kum kazar |
| Ekran dışına gitme | Yan duvarlar engel |
| Kamera | Otomatik olarak en aşağıdaki topu takip eder |

Her bölüm başında 9 top üstten düşmeye başlar. Kumu kazıp yönlendir, tehlikelerden uzak tut, kasaya ulaştır.

---

## 🏗️ 10 Bölüm İçeriği

| # | Ana Tuzak | Derinlik |
|---|-----------|----------|
| 1 | Sadece kaya | 30 |
| 2 | Kayalar (derin) | 35 |
| 3 | 🏔️ Kaya Labirenti | 40 |
| 4 | 🪨 + 🌊 Su Havuzları | 45 |
| 5 | ⚡ Dikenler + 🌊 Su | 50 |
| 6 | 💣 Krater Bombalar | 55 |
| 7 | ✨ x3 Çoğaltıcı + Dikenler | 60 |
| 8 | 🔥 Lav + Diken + Bomba | 65 |
| 9 | 🔴🟢 Renkli + 🐍 Yılan | 70 |
| 10 | 🏁 FINAL — hepsi birden | 75 |

---

## 🛠️ Script Listesi

| Script | Görev |
|--------|-------|
| `KumKazici.cs` | Piksel silme + elips önleme + public krater API |
| `KameraTakip.cs` | Y-only smooth follow + baslangic clamp |
| `Top.cs` | Renk + ölüm event |
| `BitisKutusu.cs` | **Vortex emme + küçülme animasyonu** |
| `Diken.cs` | Diken + **Krater Bomba** (kumu da temizler) |
| `Su.cs` | **YENİ** — gravityScale + linearDamping azaltır |
| `Lav.cs` | **YENİ** — tek değişte topu yok eder |
| `Yilan.cs` | **YENİ** — Ping-pong patrol |
| `Cogaltici.cs` | x2/x3 top klonlama |
| `HareketliKaya.cs` | Hareketli engel |
| `LevelManager.cs` | **Yıldız hesap + PlayerPrefs kayıt** |
| `YildizSistemi.cs` | **YENİ** — static yıldız helper |
| `AnaMenu.cs` | **YENİ** — Menu + Bölüm Seçme |
| `UIYonetici.cs` | HUD + **yıldız animasyonlu** Kazan paneli |
| `TopSpawner.cs` | Opsiyonel periyodik top üretici |

---

## 🎨 Kendi Görsellerinle Değiştirme

`Assets/Sprites/` klasöründe PNG'leri üzerine yaz:
- `kum.png`, `top.png`, `kaya.png`, `diken.png`, `bomba.png`, `kasa.png`, `cogaltici.png`, `su.png`, `lav.png`, `yilan.png`, `yildiz.png`

Unity otomatik olarak doğru import ayarlarını (Read/Write + Full Rect + Uncompressed) uygular.

---

## ❓ Sık Sorun

| Problem | Çözüm |
|---------|-------|
| Sahneler oluşmadı | `Tools → Bolumleri Uret → Hepsini Uret` |
| Compile error | Scripts kurulu olmalı: `2D Sprite`, `UGUI` (Package Manager) |
| Yıldızlar kayıp | Uygulama içi - `Tools → Yildizlari Sifirla` |
| Top kumdan geçiyor | Top prefab: Rigidbody2D → **Continuous** collision |
| Bomba kumu patlatmıyor | KumKazici sahnede olmalı — Level_XX'te KumAna var mı? |

---

🏖️⛏️ **İyi eğlenceler, kral!**

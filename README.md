# 🌍 LAST RESOURCE

> **COP31 temalı, karbon ayak izi odaklı post-apokaliptik hayatta kalma oyunu — V1.1 Prototip**

![Unity](https://img.shields.io/badge/Unity-6%20(6000.6.6f1)-black?logo=unity)
![Dil](https://img.shields.io/badge/C%23-Tek%20Script-239120?logo=csharp)
![Durum](https://img.shields.io/badge/Durum-Oynanabilir%20Prototip-brightgreen)
![Asset](https://img.shields.io/badge/Harici%20Asset-Yok-blue)


**Game Up – COP31 (HP Garage)** etkinliği için geliştirilmiştir.

---

## 📖 İçindekiler

1. [Oyun Hakkında](#-oyun-hakkında)
2. [Hikaye](#-hikaye)
3. [Oynanış](#-oynanış)
4. [Temel Mekanikler](#-temel-mekanikler)
5. [Üretim ve Geri Dönüşüm Tarifleri](#-üretim-ve-geri-dönüşüm-tarifleri)
6. [Kontroller](#-kontroller)
7. [Kurulum](#-kurulum)
8. [Teknik Bilgiler](#-teknik-bilgiler)
9. [Jüri İçin Kısa Bilgilendirme](#-jüri-için-kısa-bilgilendirme)
10. [Gelecek Sürümler (Yol Haritası)](#-gelecek-sürümler-yol-haritası)
11. [Ekip](#-ekip)

---

## 🎮 Oyun Hakkında

**Last Resource**, COP31'in çevresel ilkelerini oyun mekaniğinin tam ortasına koyan, birinci / üçüncü kişi bakış açılı bir hayatta kalma oyunudur.

Oyuncu, iklim krizinin ihmal edilmesiyle çökmüş bir dünyada, çöplerin arasından buldukları ile hayatta kalmaya çalışır. Oyunun ana fikri basittir:

> **Her şey üretmek mümkündür, ama hiçbir şey bedava değildir.**

Oyuncunun elindeki en kritik kaynak su ya da yiyecek değil, **Karbon Bütçesi**'dir. Yaptığın her üretim bütçeni harcar; geri dönüşüm, fidan dikmek, güneş paneli kurmak ve kirleten fabrikaları durdurmak ise bütçeni geri kazandırır. Bütçe sıfırın altına düşerse atmosfer zehirli sise döner ve oyuncuyu öldürür.

Amaç, oyuncuya **"tükettiğim her şeyin bir karbon bedeli var"** düşüncesini oynayarak hissettirmektir.

### ✨ Öne Çıkan Özellikler

- 🌫️ Karbon bütçesi sıfırın altına düşünce ölümcül **zehirli sis**
- ♻️ **Geri dönüşüm** kazandırır, üretim harcar: gerçek bir karbon dengesi hissi
- 💧 **Su döngüsü:** kirli göl suyunu filtreyle arıtma, fabrikaları kapatınca gölün kendini temizlemesi
- ☢️ Radyasyon bölgeleri: yüksek risk, yüksek ödül (nadir pil ve cam)
- 🌱 Fidan dikme ve ⚡ güneş paneli ile **pasif karbon kazancı**
- 🏭 Atık vanalarını kapatarak kirleten fabrikaları durdurma
- 🏅 Eko puan ve rütbe sistemi (Çöpçü → Yeşil Muhafız)
- 🧍 Görünür karakter, **V tuşuyla 1. / 3. kişi kamera** geçişi
- 🗺️ Mini harita, görev paneli, çanta paneli
- ⌨️ Menüden **yeniden atanabilir tuşlar**
- 🔊 Tamamen kod ile üretilmiş ses efektleri ve rüzgâr ambiyansı

---

## 📜 Hikaye

2000'li yıllarda sayısız zirve, sempozyum ve COP31 gibi büyük buluşmalar yapıldı. İnsanoğlu dinlemedi: tüketim, israf ve emisyon sürdü. Sonunda çöküş geldi.

Şimdi çok az insan kaldı. Kimyasal sis, radyasyon ve zehirli sular her yerde. Eskiden değerini bilmediğin her şey bugün **1000 kat değerli**.

Bir gün çöpleri karıştırırken bir el kitabı buluyorsun: COP31 tarafından yazılmış **"Yeniden Üretim Rehberi"**. Dünyayı onarmanın sırları bu kitabın içinde, ama bir uyarıyla:

> *Her üretim KARBON BÜTÇENİ tüketir. Bütçe sıfırın altına inerse atmosfer seni zehirler.*

Geriye tek bir soru kalıyor: **Elindeki son kaynakla dünyayı onarabilecek misin?**

---

## 🕹️ Oynanış

Oyun iki fazdan oluşur.

### 🏕️ Faz 1: Sığınak (Öğretici)

Oyuncu, çitlerle çevrili küçük bir sığınakta başlar. Sığınakta turuncu bir **tezgâh** bulunur (tezgâhta üretim karbon maliyetini **%40 düşürür**). Görevler oyunu kilitlemeden öğretir:

- [ ] Yerden 5 parça eşya topla
- [ ] El kitabıyla bir **Su Filtresi** üret
- [ ] Tezgâhta 1 üretim yap
- [ ] Su iç

### 🌋 Faz 2: Açık Dünya

Görevler bitince sığınak çiti açılır. Oyuncu, yaklaşık 100 birim yarıçaplı harabe dünyayı keşfeder: terk edilmiş arabalar, yıkık binalar, market kalıntıları, kirli bir göl, 2 fabrika ve 3 radyasyon bölgesi. Hedefler:

- [ ] Her iki fabrikanın **atık vanasını kapat** (E tuşunu basılı tut)
- [ ] **3 fidan** dik
- [ ] **1 güneş paneli** kur
- [ ] Gölü **%70 temizle**

Hepsi tamamlandığında oyun kazanılır. 🎉

### 💀 Kaybetme Koşulları

| Durum | Sonuç |
|---|---|
| Can 0'a düşerse | Ölüm |
| Karbon bütçesi < 0 | Zehirli sis can götürür |
| Zehir 60'ın üstüne çıkarsa | Sürekli can kaybı |
| Kirli su içmek | Zehirlenme |
| Radyasyon bölgesinde maskesiz kalmak | Radyasyon hasarı |

---

## ⚙️ Temel Mekanikler

| Mekanik | Açıklama |
|---|---|
| ❤️ **Can** | 0 olursa ölürsün. Su yüksek, zehir ve radyasyon düşükken yavaşça yenilenir. |
| 💧 **Su** | Zamanla azalır, koşmak daha hızlı tüketir. Temiz su iç; kirli su zehirler. |
| ☠️ **Zehir** | Aktif fabrika yakını, kirli su ve zehirli sisten artar. 60'ın üstü can götürür. |
| ☢️ **Radyasyon** | Kırmızı bölgelerde artar. Maske filtresi etkiyi azaltır. Bölgelerde değerli pil ve cam bulunur. |
| 🌍 **Karbon Bütçesi** | 60 kg ile başlar. Üretim ve hazır su harcar. Geri dönüşüm, fidan, güneş paneli ve vana kapatma kazandırır. |
| 🔄 **Su Döngüsü** | Boş PET şişeyle gölden kirli su al → Su Filtresiyle arıt. Fabrikaların vanalarını kapatırsan göl kendini temizler. |
| 🎒 **Çanta** | 40 eşya sınırı. Çöpü geri dönüştürerek yer aç. |
| ⭐ **Eko Puan** | Toplama, geri dönüşüm, fidan, panel ve vana puan verir. Rütbe atlayınca çantana ödül gelir. |

### 🌡️ Karbon Dengesi (Oyun İçi Değerler)

| Kaynak | Etki |
|---|---|
| Aktif fabrika | Bütçeden saniyede **-0.08 kg** (fabrika başına) |
| Dikilen fidan | Saniyede **+0.05 kg** |
| Kurulan güneş paneli | Saniyede **+0.10 kg** |
| Tezgâhta üretim | Maliyet **×0.6** |
| Hazır PET su içmek | **-0.8 kg** bütçe |

> ℹ️ Karbon değerleri oyun dengesi için **kurgusaldır**; gerçek emisyon hesabı değildir.

### 🏅 Rütbeler

| Rütbe | Gereken Puan | Ödül |
|---|---|---|
| Çöpçü | 0 | — |
| Toplayıcı | 40 | Bandaj + Maske Filtresi |
| Onarıcı | 120 | Su Filtresi + Kompost + Bandaj |
| Yeşil Muhafız | 250 | 2× Maske Filtresi + Fidan Seti |

---

## 🔧 Üretim ve Geri Dönüşüm Tarifleri

Oyunda **17 farklı eşya** ve **9 tarif** bulunur. Negatif karbon değeri, o işlemin bütçene **kredi kazandırdığı** anlamına gelir.

| Tarif | Girdi | Çıktı | Karbon |
|---|---|---|---|
| ♻️ Geri dönüşüm | 3× Teneke Kutu | 1× Hurda Metal | **-0.30** (kredi) |
| ♻️ Geri dönüşüm | 2× PET Şişe | 2× Plastik Parça | **-0.20** (kredi) |
| ♻️ Kompost yap | 3× Organik Atık | 1× Kompost | **-0.20** (kredi) |
| 🔧 Su Filtresi | Plastik + Kumaş + Cam | 1× Su Filtresi | +0.20 |
| 🔧 Suyu Arıt | Kirli Su + Su Filtresi | 1× Arıtılmış Su | +0.10 |
| 🌱 Fidan Seti | Kompost + Plastik | 1× Fidan Seti | +0.10 |
| 😷 Maske Filtresi | 2× Kumaş + Plastik | 1× Maske Filtresi (120 sn koruma) | +0.20 |
| 🩹 Bandaj | 2× Kumaş | 1× Bandaj (+35 can) | +0.05 |
| ⚡ Güneş Paneli | 2× Hurda + 2× Cam + Pil | 1× Güneş Paneli | +2.50 |

---

## ⌨️ Kontroller

Tüm tuşlar (hareket ve fare hariç) menüdeki **Tuş Atamaları** ekranından değiştirilebilir.

| Eylem | Varsayılan Tuş |
|---|---|
| Hareket | `W` `A` `S` `D` |
| Etrafa bakma | Fare |
| Etkileşim / Topla | `E` (vanalar için basılı tut) |
| Üretim / El kitabı | `C` |
| Koşu | `Left Shift` |
| Su iç | `Q` |
| Bandaj kullan | `H` |
| Maske filtresi tak | `F` |
| Fidan dik | `T` |
| Güneş paneli kur | `G` |
| Görev paneli aç/kapa | `J` |
| Çanta paneli aç/kapa | `Tab` |
| Harita aç/kapa | `M` |
| Yardım | `F1` |
| Kamera (1. / 3. kişi) | `V` |
| Duraklat menüsü | `Esc` |
| Hikayeyi ilerlet | `Enter` |
| Yeniden başla | `R` |

---

## 🚀 Kurulum

### Gereksinimler

- **Unity 6** (geliştirme sürümü: `6000.6.6f1`)
- Ek paket veya harici asset **gerekmez**
- Yeni **Input System** ile eski **Input Manager** ikisiyle de çalışır

### Adımlar

1. Unity Hub'dan yeni bir **3D projesi** oluştur (Unity 6).
2. Bu depodaki `LastResourcePrototype.cs` dosyasını `Assets/` klasörüne at.
3. Boş bir sahne aç → **Create Empty** ile yeni bir GameObject oluştur → script'i üzerine sürükle.
4. **Play** tuşuna bas. 🎮

> 💡 Script sahnede yoksa Play'e basınca **kendini otomatik olarak oluşturur**. Yani boş sahnede doğrudan Play'e basmak da yeterlidir.

Oyun menüsü: **Oyna / Nasıl Oynanır / Tuş Atamaları / Çıkış**

---

## 🛠️ Teknik Bilgiler

| Konu | Detay |
|---|---|
| Motor | Unity 6 (6000.6.6f1) |
| Dil | C# |
| Yapı | **Tek dosya** (`LastResourcePrototype.cs`), sahneye otomatik kurulur |
| Arayüz | Kod ile çizilen `OnGUI` arayüzü |
| Girdi | Input System ve eski Input Manager desteği (`#if ENABLE_INPUT_SYSTEM`) |
| Dünya | Unity ilkel şekilleri (Cube, Cylinder, Sphere, Plane) ile kodla oluşturulur |
| Zemin dokusu | Perlin gürültüsüyle **kodla üretilen** dikişsiz doku |
| Ses | Tüm ses efektleri ve rüzgâr ambiyansı **kodla (prosedürel) üretilir** |
| Kontrol | `CharacterController`, 1. / 3. kişi kamera, yürüme animasyonlu karakter |
| Atmosfer | Duman/kül parçacıkları, dinamik sis ve gökyüzü (zehir seviyesine göre değişir) |
| Denge | Kodun başındaki `BALANCE` bloğundan tek yerden ayarlanabilir |

### 🎨 Asset Kullanımı

Bu projede **hazır (harici) asset kullanılmamıştır.** Modeller, dokular, sesler ve arayüz tamamen kodla üretilmiştir. Bu, projeyi tek script ile çalıştırılabilir ve hafif tutar.

İlerleyen sürümlerde görsel kaliteyi artırmak için **hazır asset kullanılabilir** (örneğin 3D çevre modelleri, karakter ve animasyon paketleri, ses efektleri). Böyle bir durumda kullanılan her asset, lisansıyla birlikte bu belgeye eklenecektir.

---

## 🧑‍⚖️ Jüri İçin Kısa Bilgilendirme

| Başlık | Bilgi |
|---|---|
| **Etkinlik** | Game Up – COP31 (HP Garage) |
| **Proje durumu** | Baştan sona oynanabilir **V1.1 prototip** |
| **COP31 bağlantısı** | Karbon ayak izi, geri dönüşüm, sürdürülebilir üretim, temiz su, yeniden ağaçlandırma ve temiz enerji oyunun çekirdek mekaniklerine dönüştürülmüştür. |
| **Fark yaratan fikir** | Oyuncuyu bir "satın alma" sistemiyle değil, **karbon bütçesinin ölümcül olmasıyla** sınırlıyoruz. Üretim bedelini oyuncu bizzat hisseder. |
| **Öğretici yan** | Oyuncu oynarken geri dönüşümün karbon kazandırdığını, üretimin ise bedeli olduğunu keşfeder. |
| **Hızlı deneme** | Boş sahne + 1 script + Play |
| **Kapsam notu** | Bu bir prototiptir. Faz 3 (fabrika içi, canavarlar) bilinçli olarak V2'ye bırakılmıştır. |

---

## 🔮 Gelecek Sürümler (Yol Haritası)

### 🧟 V2: Fabrika İçi ve Canavarlar

Faz 3'te oyuncu, vanayı kapatmak için fabrikanın içine girer. Her vananın başında kirliliğin canlandırdığı bir **bekçi canavar** vardır; yakalanırsan zehir +40.

| Canavar | Davranış |
|---|---|
| 🟤 **Çamur Sürüngeni** | Kirli su birikintilerinden çıkar, yavaştır, temas zehirler. |
| 🌫️ **Duman Hayaleti** | Sisle birlikte görünür hale gelir, maske filtresini tüketir. |
| 🤖 **Hurda Golem** | Atık yığınlarından oluşur, sese tepki verir. |
| 🪲 **Radyasyon Böceği** | Sürü halinde hareket eder, radyasyon bölgelerinde çoğalır. |

### 🧩 Diğer Planlanan Geliştirmeler

- [ ] Fabrika içi **vana bulmacası** (Faz 3)
- [ ] Daha fazla fabrika, bölge ve harita çeşitliliği
- [ ] Gün/gece döngüsü ve hava olayları (asit yağmuru, toz fırtınası)
- [ ] Gelişmiş eşya ve tarif ağacı (örneğin rüzgâr türbini, yağmur suyu toplayıcı)
- [ ] Kayıt sistemi (save/load)
- [ ] **Hazır asset ile** görsel yenileme (modeller, karakter animasyonları, ses efektleri)
- [ ] Müzik ve gelişmiş ses tasarımı
- [ ] Gamepad desteği
- [ ] Çoklu dil desteği (TR / EN)
- [ ] Karbon ayak izi bilgi kartları (eşyaların gerçek karbon etkisini gösteren eğitici notlar)
- [ ] Başarımlar ve oyun sonu istatistik ekranı
- [ ] Menüler için `OnGUI` yerine Unity UI Toolkit'e geçiş

---

## 👥 Ekip

| **Mehmet Kerem Usta** |
| **Hamideddin Hulusi General** |

📧 İletişim: crownell184luis@gmail.com
🔗 GitHub: `BURAYA_GITHUB_LİNKİ`

---

<p align="center"><i>"Eskiden değerini bilmediğin her şey bugün 1000 kat değerli."</i></p>

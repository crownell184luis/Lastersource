// =====================================================================
//  LAST RESOURCE  |  COP31 temalı hayatta kalma  |  V1.1 (karakter + çevre güncellemesi)
//  Unity 6 (6000.x)  |  TEK DOSYA  |  Asset gerekmez
//
//  KURULUM: Boş sahne -> Create Empty -> bu script'i ekle -> Play.
//  (Script sahnede yoksa Play'e basınca kendini otomatik oluşturur.)
//  Menü: Oyna / Nasıl oynanır / Tuş atamaları / Çıkış.  Oyunda Esc = duraklat menüsü.
//  YENİ: Görünür karakter (3. kişi kamera). V tuşu = 1. / 3. kişi geçişi.
//  Denge ayarları: aşağıdaki "BALANCE" bloğu.
//  V2-HOOK ile aranırsa canavarlı faz için yer tutucular bulunur.
// =====================================================================
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ----------------------------- VERİ -----------------------------------
public enum Item { PetBottle, Can, Scrap, Plastic, Cloth, Battery, Glass, Organic, DirtyWater, Filter, CleanWater, BottledWater, Compost, Sapling, SolarPanel, MaskFilter, Bandage }

public class ItemDef { public string name, hint; public Color color; public PrimitiveType shape; public Vector3 size; }
public class Recipe { public string name; public float carbon; public Item outId; public int outN; public (Item id, int n)[] inp; }

public static class DB
{
    public static Dictionary<Item, ItemDef> I = new Dictionary<Item, ItemDef>();
    public static List<Recipe> R = new List<Recipe>();

    static void A(Item id, string n, Color c, PrimitiveType s, Vector3 sz, string h)
    { I[id] = new ItemDef { name = n, color = c, shape = s, size = sz, hint = h }; }

    // carbon: kg CO2 (oyun dengesi için kurgusal). Negatif = geri dönüşümle kazanılan kredi.
    static void Rc(string n, float carbon, Item o, int on, params (Item, int)[] inp)
    { R.Add(new Recipe { name = n, carbon = carbon, outId = o, outN = on, inp = inp }); }

    static DB()
    {
        var cy = PrimitiveType.Cylinder; var cu = PrimitiveType.Cube; var sp = PrimitiveType.Sphere;
        A(Item.PetBottle, "PET Şişe", new Color(.4f, .7f, 1f), cy, new Vector3(.18f, .45f, .18f), "geri dönüşür / su kabı");
        A(Item.Can, "Teneke Kutu", new Color(.75f, .75f, .8f), cy, new Vector3(.2f, .25f, .2f), "3 adet = 1 hurda");
        A(Item.Scrap, "Hurda Metal", new Color(.8f, .4f, .1f), cu, new Vector3(.4f, .2f, .3f), "güneş paneli için");
        A(Item.Plastic, "Plastik Parça", new Color(1f, .3f, .6f), cu, new Vector3(.35f, .08f, .3f), "filtre, maske, fidan seti");
        A(Item.Cloth, "Kumaş", new Color(.9f, .85f, .6f), cu, new Vector3(.4f, .04f, .4f), "filtre, maske, bandaj");
        A(Item.Battery, "Eski Pil", new Color(.7f, 1f, .2f), cy, new Vector3(.15f, .3f, .15f), "güneş paneli için");
        A(Item.Glass, "Cam Kırığı", new Color(.6f, 1f, 1f), sp, new Vector3(.25f, .25f, .25f), "filtre, panel");
        A(Item.Organic, "Organik Atık", new Color(.45f, .3f, .15f), sp, new Vector3(.3f, .25f, .3f), "3 adet = kompost");
        A(Item.DirtyWater, "Kirli Su", new Color(.5f, .4f, .1f), cy, new Vector3(.18f, .45f, .18f), "ARITMADAN İÇME");
        A(Item.Filter, "Su Filtresi", new Color(.3f, .9f, .5f), cy, new Vector3(.2f, .3f, .2f), "suyu arıtır");
        A(Item.CleanWater, "Arıtılmış Su", new Color(.3f, .8f, 1f), cy, new Vector3(.18f, .45f, .18f), "güvenli, düşük karbon");
        A(Item.BottledWater, "Hazır PET Su", new Color(.7f, .9f, 1f), cy, new Vector3(.18f, .45f, .18f), "içince -0.8kg bütçe");
        A(Item.Compost, "Kompost", new Color(.3f, .2f, .1f), sp, new Vector3(.3f, .25f, .3f), "fidan seti için");
        A(Item.Sapling, "Fidan Seti", new Color(.2f, .8f, .2f), cy, new Vector3(.2f, .3f, .2f), "dikilince bütçe +3");
        A(Item.SolarPanel, "Güneş Paneli", new Color(.1f, .2f, .7f), cu, new Vector3(.6f, .05f, .4f), "kurulunca pasif kredi");
        A(Item.MaskFilter, "Maske Filtresi", new Color(.9f, .9f, .9f), cy, new Vector3(.2f, .1f, .2f), "120 sn koruma");
        A(Item.Bandage, "Bandaj", new Color(1f, .5f, .5f), cu, new Vector3(.3f, .1f, .2f), "+35 can");

        Rc("Geri dön.: 3 Teneke -> Hurda", -0.3f, Item.Scrap, 1, (Item.Can, 3));
        Rc("Geri dön.: 2 PET -> 2 Plastik", -0.2f, Item.Plastic, 2, (Item.PetBottle, 2));
        Rc("Kompost yap", -0.2f, Item.Compost, 1, (Item.Organic, 3));
        Rc("Su Filtresi", 0.2f, Item.Filter, 1, (Item.Plastic, 1), (Item.Cloth, 1), (Item.Glass, 1));
        Rc("Suyu Arıt", 0.1f, Item.CleanWater, 1, (Item.DirtyWater, 1), (Item.Filter, 1));
        Rc("Fidan Seti", 0.1f, Item.Sapling, 1, (Item.Compost, 1), (Item.Plastic, 1));
        Rc("Maske Filtresi", 0.2f, Item.MaskFilter, 1, (Item.Cloth, 2), (Item.Plastic, 1));
        Rc("Bandaj", 0.05f, Item.Bandage, 1, (Item.Cloth, 2));
        Rc("Güneş Paneli", 2.5f, Item.SolarPanel, 1, (Item.Scrap, 2), (Item.Glass, 2), (Item.Battery, 1));
    }
}

// ------------------------------ GİRDİ ---------------------------------
// İlk NB eylem menüden yeniden atanabilir. Hem yeni Input System hem eski Input Manager ile çalışır.
public enum K { Interact, Craft, Sprint, Drink, Heal, Mask, Plant, Solar, Quest, Inv, Map, Help, Cam, Menu, Confirm, Restart }

public static class Inp
{
    public const int NB = 13;
    public static readonly string[] Names = { "Etkileşim", "Üretim / el kitabı", "Koşu", "Su iç", "Bandaj kullan", "Maske filtresi tak", "Fidan dik", "Güneş paneli kur", "Görev paneli aç/kapa", "Çanta paneli aç/kapa", "Harita aç/kapa", "Yardım", "Kamera (1. / 3. kişi)" };
    public static KeyCode[] bind = Defaults();
    public static KeyCode[] Defaults()
    { return new[] { KeyCode.E, KeyCode.C, KeyCode.LeftShift, KeyCode.Q, KeyCode.H, KeyCode.F, KeyCode.T, KeyCode.G, KeyCode.J, KeyCode.Tab, KeyCode.M, KeyCode.F1, KeyCode.V, KeyCode.Escape, KeyCode.Return, KeyCode.R }; }
    public static void Reset() { bind = Defaults(); }

    public static string Nice(K k)
    {
        string s = bind[(int)k].ToString();
        return s.Replace("LeftShift", "Shift").Replace("Return", "Enter").Replace("Escape", "Esc").Replace("Alpha", "");
    }

    public static Vector2 Move()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current; if (k == null) return Vector2.zero;
        return new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }
    public static Vector2 Look()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current == null ? Vector2.zero : Mouse.current.delta.ReadValue() * 0.08f;
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 2f;
#endif
    }
#if ENABLE_INPUT_SYSTEM
    static Dictionary<KeyCode, Key> cache = new Dictionary<KeyCode, Key>();
    static Key Conv(KeyCode c)
    {
        Key k; if (cache.TryGetValue(c, out k)) return k;
        string n = c.ToString();
        if (c == KeyCode.Return || c == KeyCode.KeypadEnter) n = "Enter";
        else
        {
            int code = (int)c;
            int firstDigit = (int)KeyCode.Alpha0;
            int lastDigit = (int)KeyCode.Alpha9;
            if (code >= firstDigit && code <= lastDigit)
                n = "Digit" + (code - firstDigit);
        }
        if (!System.Enum.TryParse(n, out k)) k = Key.None;
        cache[c] = k; return k;
    }
    public static bool Down(K a) { var kb = Keyboard.current; if (kb == null) return false; var k = Conv(bind[(int)a]); return k != Key.None && kb[k].wasPressedThisFrame; }
    public static bool Held(K a) { var kb = Keyboard.current; if (kb == null) return false; var k = Conv(bind[(int)a]); return k != Key.None && kb[k].isPressed; }
#else
    public static bool Down(K a) { return Input.GetKeyDown(bind[(int)a]); }
    public static bool Held(K a) { return Input.GetKey(bind[(int)a]); }
#endif
}

// ------------------------- DÜNYA NESNELERİ -----------------------------
public enum Kind { Pickup, Valve, Lake, Bench, Crate }
public class Node
{
    public Kind kind; public Transform t; public Item item; public string label = ""; public float reach = 2.8f, radius, progress, baseY, phase;
    public bool done; public Factory fac; public GameObject extra;
}
public class Factory { public bool active = true; public Transform root; public ParticleSystem ps; public Renderer pipe, lamp; }

// ============================ ANA OYUN ================================
public class LastResourcePrototype : MonoBehaviour
{
    // ------------------------- BALANCE (dengeyi buradan ayarla) -------------------------
    const float THIRST = .3f, SPRINT_THIRST = .5f;       // su azalması (birim/sn)
    const float WALK = 4.5f, RUN = 7.5f;                 // hız
    const float FAC_DRAIN = .08f;                        // aktif fabrika başına karbon bütçesi kaybı (kg/sn)
    const float TREE_GAIN = .05f, SOLAR_GAIN = .1f;      // fidan / panel pasif kazanç (kg/sn)
    const float EXPO_NEAR = 1.6f, EXPO_FAR = .1f, SMOG_EXPO = 1.0f, TOX_DECAY = .35f; // zehir
    const float RAD_RATE = 5f, RAD_DECAY = .6f;          // radyasyon
    const float BENCH_MULT = .6f;                        // tezgâhta karbon çarpanı
    const float START_BUDGET = 60f;
    const int Cap = 40;
    const float ROAD_Z = -3f;                            // ana yolun z konumu
    const float CAM_DIST = 3.4f;                         // 3. kişi kamera uzaklığı

    enum St { Menu, Story, Play, Pause, Dead, Win }
    enum Scr { Main, Help, Keys }
    St st = St.Menu; Scr scr = Scr.Main; int page, waitKey = -1; int phase = 1;

    float health = 100, thirst = 100, tox, rad, maskT, budget = START_BUDGET, eco, purity = 20;
    int rankIdx, collected, benchCrafts, trees, solar; bool madeFilter, drank, inZone;
    bool showQuests = true, showInv = true, showMap = true, craftOpen;

    Dictionary<Item, int> inv = new Dictionary<Item, int>();
    List<Node> nodes = new List<Node>(); List<Factory> facs = new List<Factory>();
    List<Transform> radZones = new List<Transform>(); List<Transform> treeObjs = new List<Transform>();
    List<(Vector3 c, float r)> blocked = new List<(Vector3, float)>();
    List<GameObject> fence = new List<GameObject>();
    Node tgt; Renderer lakeR; Light benchLight, fireLight; Transform benchT;

    // karakter modeli + kamera
    Transform pl, camT, model, legL, legR, armL, armR; GameObject modelGo;
    bool thirdPerson = true; float walkPh, walkAmp, camUse = CAM_DIST;
    Material grassMat;

    Camera cm; CharacterController cc; float pitch, velY, bobT, stepT, valveSnd;
    GameObject world; Texture2D px, soft, vig, groundTex; GUIStyle S; Material pm, skyMat;
    ParticleSystem ash; AudioSource au;
    AudioClip cPick, cCraft, cHurt, cGeiger, cDrink, cValve, cDone, cStep;
    string msg = "", cause = "", banner = ""; float msgT, bannerT, flash, lastHealth = 100, nextHurt;

    static readonly string[] Story = {
        "2000'lerde sayısız zirve, sempozyum ve COP31 gibi buluşmalar yapıldı.\nİnsanoğlu dinlemedi: tüketim, israf ve emisyon sürdü.\nSonunda çöküş geldi.",
        "Şimdi çok az insan kaldı. Kimyasal sis, radyasyon ve zehirli sular her yerde.\nEskiden değerini bilmediğin her şey bugün 1000 kat değerli.",
        "Çöpleri karıştırırken bir el kitabı buldun: COP31 tarafından yazılmış 'Yeniden Üretim Rehberi'.\nDünyayı onarmanın sırları burada.\n\nDİKKAT: Her üretim KARBON BÜTÇENİ tüketir.\nBütçe sıfırın altına inerse atmosfer seni zehirler."
    };
    static readonly (int pts, string name, (Item, int)[] rw)[] Ranks = {
        (0, "Çöpçü", new (Item, int)[0]),
        (40, "Toplayıcı", new[] { (Item.Bandage, 1), (Item.MaskFilter, 1) }),
        (120, "Onarıcı", new[] { (Item.Filter, 1), (Item.Compost, 1), (Item.Bandage, 1) }),
        (250, "Yeşil Muhafız", new[] { (Item.MaskFilter, 2), (Item.Sapling, 1) })
    };
    static readonly (string h, string t)[] HelpRows = {
        ("CAN", "0 olursa ölürsün. Su yüksek, zehir ve radyasyon düşükken yavaşça yenilenir."),
        ("SU", "Zamanla azalır, koşmak daha hızlı tüketir. Temiz su iç; kirli su zehirler."),
        ("ZEHİR", "Aktif fabrika yakını, kirli su ve zehirli sisten artar. 60'ın üstü can götürür."),
        ("RADYASYON", "Kırmızı bölgelerde artar. Maske filtresi etkiyi azaltır. Bölgelerde değerli pil vardır."),
        ("KARBON BÜTÇESİ", "Üretim ve hazır su harcar. Geri dönüşüm, fidan, güneş paneli ve vana kapatma kazandırır. Sıfırın altı = ZEHİRLİ SİS."),
        ("SU DÖNGÜSÜ", "Boş PET şişeyle gölden kirli su al -> Su Filtresiyle arıt. Fabrikaların vanalarını kapatırsan göl kendini temizler."),
        ("ÇANTA", "40 eşya sınırı. Çöpü geri dönüştürerek yer aç."),
        ("EKO PUAN", "Toplama, geri dönüşüm, fidan, panel ve vana puan verir. Rütbe atlayınca çantana ödül gelir."),
        ("KAMERA", "Tuş atamalarındaki 'Kamera' tuşu (varsayılan V) 1. ve 3. kişi görünümü arasında geçiş yapar."),
        ("HEDEF", "Faz 1: öğretici. Faz 2: vanaları kapat, 3 fidan dik, panel kur, gölü %70 temizle.")
    };

    // =====================================================================
    //  KURULUM
    // =====================================================================
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindFirstObjectByType<LastResourcePrototype>() == null)
            new GameObject("LastResource").AddComponent<LastResourcePrototype>();
    }

    void Start()
    {
        px = new Texture2D(1, 1); px.SetPixel(0, 0, Color.white); px.Apply();
        NewGame(true);
    }

    static Texture2D MakeSoft(int n)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            { float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f, n / 2f)) / (n / 2f); float a = Mathf.SmoothStep(0, 1, 1 - Mathf.Clamp01(d)); t.SetPixel(x, y, new Color(1, 1, 1, a)); }
        t.Apply(); t.wrapMode = TextureWrapMode.Clamp; return t;
    }
    static Texture2D MakeVig(int n)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            { float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f, n / 2f)) / (n / 2f); t.SetPixel(x, y, new Color(1, 1, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.4f, 1.15f, d)))); }
        t.Apply(); t.wrapMode = TextureWrapMode.Clamp; return t;
    }
    static Texture2D MakeGround()
    {
        int n = 256; var t = new Texture2D(n, n, TextureFormat.RGB24, true); t.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            int u = x < n / 2 ? x : n - 1 - x, v = y < n / 2 ? y : n - 1 - y; // ayna: dikişsiz döşeme
            float a = Mathf.PerlinNoise(u * .03f, v * .03f), b = Mathf.PerlinNoise(u * .15f + 20, v * .15f + 20), c = Mathf.PerlinNoise(u * .6f + 5, v * .6f + 5);
            var col = Color.Lerp(new Color(.16f, .13f, .1f), new Color(.46f, .38f, .28f), a); col *= .8f + b * .3f; col *= .9f + c * .15f;
            if (Random.value < .004f) col = new Color(.55f, .55f, .5f);
            t.SetPixel(x, y, col);
        }
        t.Apply(); return t;
    }

    AudioClip Tone(float f0, float f1, float dur, float vol, float noise)
    {
        int sr = 44100, n = (int)(sr * dur); var d = new float[n]; float ph = 0;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n; ph += 2 * Mathf.PI * Mathf.Lerp(f0, f1, t) / sr; float env = (1 - t) * Mathf.Min(1, i / 150f);
            d[i] = (Mathf.Sin(ph) * (1 - noise) + (Random.value * 2 - 1) * noise) * env * vol;
        }
        var c = AudioClip.Create("t", n, 1, sr, false); c.SetData(d, 0); return c;
    }
    AudioClip WindClip()
    {
        int sr = 22050, n = sr * 4; var d = new float[n]; float y = 0;
        for (int i = 0; i < n; i++) { y = y * .985f + (Random.value * 2 - 1) * .015f; d[i] = y * 2.2f * Mathf.Min(1, Mathf.Min(i, n - i) / 3000f); }
        var c = AudioClip.Create("wind", n, 1, sr, false); c.SetData(d, 0); return c;
    }
    void Snd(AudioClip c, float v = 1f) { if (au && c) au.PlayOneShot(c, v); }

    GameObject P(PrimitiveType t, Vector3 pos, Vector3 sc, Color c, bool col = false, Transform par = null)
    {
        var g = GameObject.CreatePrimitive(t);
        if (!col) Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(par ? par : world.transform, false);
        g.transform.position = pos; g.transform.localScale = sc;
        var rr = g.GetComponent<Renderer>(); rr.material.color = c; Gloss(rr, .12f);
        rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; rr.receiveShadows = true; return g;
    }
    // karakter parçası: yerel (local) konum kullanır
    GameObject Part(PrimitiveType t, Transform par, Vector3 lp, Vector3 ls, Color c, float gloss = .1f)
    {
        var g = GameObject.CreatePrimitive(t);
        var col = g.GetComponent<Collider>(); if (col) Destroy(col);
        g.transform.SetParent(par, false); g.transform.localPosition = lp; g.transform.localScale = ls;
        var r = g.GetComponent<Renderer>(); r.material.color = c; Gloss(r, gloss); return g;
    }
    static void Glow(Renderer r, Color c)
    { var m = r.material; m.EnableKeyword("_EMISSION"); if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c); }
    static void Gloss(Renderer r, float v)
    { var m = r.material; if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", v); if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", v); }

    ParticleSystem PS(Transform par, Vector3 wpos, Color c0, Color c1, float rate, float life, float speed, float s0, float s1, float grav, bool box, Vector3 boxSize, bool up)
    {
        if (pm == null) return null;
        var go = new GameObject("PS"); go.transform.SetParent(par, false); go.transform.position = wpos;
        if (up) go.transform.rotation = Quaternion.Euler(-90, 0, 0);
        var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main; m.loop = true; m.startLifetime = life; m.startSpeed = speed; m.startSize = new ParticleSystem.MinMaxCurve(s0, s1);
        m.startColor = c0; m.simulationSpace = ParticleSystemSimulationSpace.World; m.maxParticles = 500; m.gravityModifier = grav;
        var e = ps.emission; e.rateOverTime = rate;
        var sh = ps.shape;
        if (box) { sh.shapeType = ParticleSystemShapeType.Box; sh.scale = boxSize; } else { sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 10; sh.radius = .5f; }
        var col = ps.colorOverLifetime; col.enabled = true; var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(c0, 0), new GradientColorKey(c1, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(c0.a, .15f), new GradientAlphaKey(0, 1) });
        col.color = g;
        var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, .6f, 1, 1.6f));
        go.GetComponent<ParticleSystemRenderer>().material = pm;
        ps.Play(); return ps;
    }

    Vector3 Rnd(float r0, float r1) { float a = Random.value * 6.283f, r = Random.Range(r0, r1); return new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r); }
    bool Free(Vector3 p, float r) { foreach (var b in blocked) if (Vector2.Distance(new Vector2(p.x, p.z), new Vector2(b.c.x, b.c.z)) < b.r + r) return false; return true; }
    Vector3 RndFree(float r0, float r1, float r = 1f) { var p = Rnd(r0, r1); for (int i = 0; i < 14 && !Free(p, r); i++) p = Rnd(r0, r1); return p; }
    bool OffRoad(Vector3 p, float m = 0) { return Mathf.Abs(p.z - ROAD_Z) > 7f + m; }

    // =====================================================================
    //  KARAKTER MODELİ (primitiflerden)
    // =====================================================================
    void BuildModel()
    {
        modelGo = new GameObject("Model"); model = modelGo.transform; model.SetParent(pl, false);
        model.localPosition = Vector3.zero; model.localRotation = Quaternion.identity;
        var cu = PrimitiveType.Cube; var cy = PrimitiveType.Cylinder; var sp = PrimitiveType.Sphere;
        var jacket = new Color(.34f, .42f, .22f); var pants = new Color(.2f, .2f, .26f); var skin = new Color(.85f, .66f, .5f);
        var scarf = new Color(.92f, .45f, .1f); var pack = new Color(.46f, .32f, .18f); var boot = new Color(.1f, .09f, .09f);

        Part(cu, model, new Vector3(0, 1.12f, 0), new Vector3(.56f, .66f, .32f), jacket);               // gövde
        Part(cu, model, new Vector3(0, .84f, 0), new Vector3(.54f, .12f, .31f), new Color(.15f, .12f, .1f)); // kemer
        Part(sp, model, new Vector3(0, 1.7f, 0), new Vector3(.3f, .32f, .3f), skin);                     // kafa
        Part(cu, model, new Vector3(0, 1.86f, -.02f), new Vector3(.33f, .12f, .33f), new Color(.15f, .15f, .17f)); // bere
        var gog = Part(cu, model, new Vector3(0, 1.74f, .14f), new Vector3(.3f, .08f, .08f), new Color(.05f, .08f, .1f));
        Glow(gog.GetComponent<Renderer>(), new Color(.1f, .6f, .7f) * .8f);                              // gözlük
        var mk = Part(cy, model, new Vector3(0, 1.63f, .17f), new Vector3(.12f, .07f, .12f), new Color(.45f, .47f, .5f));
        mk.transform.localRotation = Quaternion.Euler(90, 0, 0);                                         // maske
        Part(cu, model, new Vector3(0, 1.48f, 0), new Vector3(.38f, .12f, .38f), scarf);                 // atkı
        Part(cu, model, new Vector3(0, 1.15f, -.27f), new Vector3(.42f, .52f, .22f), pack);              // sırt çantası
        Part(cu, model, new Vector3(0, 1.45f, -.28f), new Vector3(.36f, .08f, .2f), pack * .8f);
        var bt = Part(cy, model, new Vector3(.25f, 1.0f, -.3f), new Vector3(.1f, .16f, .1f), new Color(.4f, .7f, 1f)); bt.name = "BagBottle";

        legL = new GameObject("LegL").transform; legL.SetParent(model, false); legL.localPosition = new Vector3(-.14f, .8f, 0);
        legR = new GameObject("LegR").transform; legR.SetParent(model, false); legR.localPosition = new Vector3(.14f, .8f, 0);
        foreach (var lg in new[] { legL, legR })
        {
            Part(cu, lg, new Vector3(0, -.38f, 0), new Vector3(.21f, .76f, .23f), pants);
            Part(cu, lg, new Vector3(0, -.74f, .05f), new Vector3(.23f, .12f, .32f), boot);
        }
        armL = new GameObject("ArmL").transform; armL.SetParent(model, false); armL.localPosition = new Vector3(-.37f, 1.4f, 0);
        armR = new GameObject("ArmR").transform; armR.SetParent(model, false); armR.localPosition = new Vector3(.37f, 1.4f, 0);
        foreach (var ar in new[] { armL, armR })
        {
            Part(cu, ar, new Vector3(0, -.29f, 0), new Vector3(.15f, .58f, .16f), jacket * .92f);
            Part(sp, ar, new Vector3(0, -.62f, 0), new Vector3(.14f, .14f, .14f), new Color(.2f, .2f, .2f)); // eldiven
        }
    }

    void AnimateModel(float dt, bool moving, bool run, Vector2 m)
    {
        if (model == null) return;
        walkAmp = Mathf.Lerp(walkAmp, moving ? (run ? 55f : 35f) : 0f, dt * 10f);
        if (moving) walkPh += dt * (run ? 12f : 8f);
        float sw = Mathf.Sin(walkPh) * walkAmp;
        legL.localRotation = Quaternion.Euler(sw, 0, 0); legR.localRotation = Quaternion.Euler(-sw, 0, 0);
        armL.localRotation = Quaternion.Euler(-sw, 0, 0); armR.localRotation = Quaternion.Euler(sw, 0, 0);
        if (thirdPerson && moving) model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.LookRotation(new Vector3(m.x, 0, m.y)), dt * 12f);
        else if (!thirdPerson) model.localRotation = Quaternion.identity;
    }

    // kamera: oyuncu hareketinden sonra çalışır (LateUpdate)
    void CamUpdate(float dt)
    {
        if (pl == null || camT == null || model == null) return;
        if (st == St.Menu)
        {
            modelGo.SetActive(true); model.localRotation = Quaternion.identity;
            camT.localPosition = new Vector3(0, 1.45f, 3.4f); camT.localRotation = Quaternion.Euler(6, 180, 0);
            cm.fieldOfView = Mathf.Lerp(cm.fieldOfView, 55f, dt * 4f); return;
        }
        bool tp = thirdPerson || st == St.Dead;
        modelGo.SetActive(tp);
        if (st == St.Dead) model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(-88, 0, 0), dt * 4f);
        if (!tp)
        {
            camT.localRotation = Quaternion.Euler(pitch, 0, 0);
            camT.localPosition = new Vector3(0, 1.6f + Mathf.Sin(bobT) * .035f, 0);
            camUse = CAM_DIST; return;
        }
        var rot = Quaternion.Euler(pitch, 0, 0);
        var pivotL = new Vector3(0, 1.55f, 0);
        var desiredL = pivotL + rot * new Vector3(.55f, .25f, -CAM_DIST);
        Vector3 pivotW = pl.TransformPoint(pivotL), desiredW = pl.TransformPoint(desiredL);
        Vector3 dir = desiredW - pivotW; float len = dir.magnitude; dir /= len;
        float use = len; RaycastHit hit;
        if (Physics.SphereCast(pivotW, .2f, dir, out hit, len, ~0, QueryTriggerInteraction.Ignore)) use = Mathf.Max(.5f, hit.distance - .05f);
        camUse = use < camUse ? use : Mathf.Lerp(camUse, use, dt * 8f);
        camT.position = pivotW + dir * camUse; camT.rotation = pl.rotation * rot;
    }
    void LateUpdate() { if (px == null || cm == null) return; CamUpdate(Mathf.Min(Time.deltaTime, .05f)); }

    // =====================================================================
    //  OYUN KURULUMU
    // =====================================================================
    void NewGame(bool toMenu)
    {
        if (world) Destroy(world);
        world = new GameObject("World");
        nodes.Clear(); facs.Clear(); radZones.Clear(); treeObjs.Clear(); blocked.Clear(); inv.Clear(); fence.Clear(); lakeR = null; benchLight = null; fireLight = null; tgt = null;
        health = lastHealth = 100; thirst = 100; tox = 0; rad = 0; maskT = 0; budget = START_BUDGET; eco = 0; rankIdx = 0; purity = 20; flash = 0;
        collected = benchCrafts = trees = solar = 0; madeFilter = drank = false; phase = 1; craftOpen = false; pitch = 0; velY = 0; cause = ""; waitKey = -1; camUse = CAM_DIST;
        st = toMenu ? St.Menu : St.Play; scr = Scr.Main; page = 0;
        if (model) model.localRotation = Quaternion.identity;

        if (cc == null)
        {
            soft = MakeSoft(64); vig = MakeVig(128); groundTex = MakeGround();
            var psh = Shader.Find("Sprites/Default"); if (psh) pm = new Material(psh) { mainTexture = soft };
            var sky = Shader.Find("Skybox/Procedural"); if (sky) { skyMat = new Material(sky); RenderSettings.skybox = skyMat; }

            var pg = new GameObject("Player"); pl = pg.transform;
            cc = pg.AddComponent<CharacterController>(); cc.height = 1.8f; cc.center = Vector3.up * .9f; cc.radius = .4f;
            cm = Camera.main; if (cm == null) { var cg = new GameObject("Cam"); cm = cg.AddComponent<Camera>(); cg.tag = "MainCamera"; cg.AddComponent<AudioListener>(); }
            camT = cm.transform; camT.SetParent(pl, false); camT.localPosition = new Vector3(0, 1.6f, 0); camT.localRotation = Quaternion.identity;
            cm.clearFlags = skyMat ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor; cm.farClipPlane = 260; cm.fieldOfView = 70; cm.nearClipPlane = .1f;
            BuildModel();

            var li = FindFirstObjectByType<Light>();
            if (li == null)
            {
                var lightObject = new GameObject("Directional Light");
                li = lightObject.AddComponent<Light>();
            }
            li.type = LightType.Directional;
            li.color = new Color(1f, .68f, .42f);
            li.intensity = 1.5f; li.shadowStrength = .85f;
            li.shadows = LightShadows.Soft;
            li.transform.rotation = Quaternion.Euler(24, 40, 0);
            QualitySettings.shadowDistance = 90; QualitySettings.antiAliasing = 4;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.55f, .42f, .33f); RenderSettings.ambientEquatorColor = new Color(.34f, .27f, .22f); RenderSettings.ambientGroundColor = new Color(.14f, .1f, .08f);
            RenderSettings.fogDensity = .012f; RenderSettings.fogColor = new Color(.62f, .5f, .38f);

            au = pg.AddComponent<AudioSource>(); au.spatialBlend = 0; au.volume = .8f;
            var w = pg.AddComponent<AudioSource>(); w.clip = WindClip(); w.loop = true; w.volume = .22f; w.spatialBlend = 0; w.Play();
            cPick = Tone(700, 1200, .1f, .35f, 0); cCraft = Tone(400, 800, .22f, .35f, 0); cHurt = Tone(160, 70, .25f, .5f, .4f);
            cGeiger = Tone(3000, 3000, .02f, .5f, .9f); cDrink = Tone(500, 220, .3f, .35f, .15f); cValve = Tone(95, 65, .14f, .4f, .5f);
            cDone = Tone(500, 1000, .5f, .4f, 0); cStep = Tone(110, 80, .06f, .25f, .7f);

            ash = PS(pl, pl.position + Vector3.up * 9, new Color(.8f, .8f, .72f, .55f), new Color(.6f, .6f, .55f, 0), 55, 7, 0, .05f, .13f, .03f, true, new Vector3(36, 1, 36), false);
        }
        cc.enabled = false; pl.position = new Vector3(0, .1f, 0); pl.rotation = Quaternion.identity; cc.enabled = true;

        // zemin (dokulu, mat) - 500x500
        var gr = P(PrimitiveType.Plane, Vector3.zero, new Vector3(50, 1, 50), Color.white, true);
        var grm = gr.GetComponent<Renderer>().material; grm.mainTexture = groundTex; grm.mainTextureScale = new Vector2(117, 117); Gloss(gr.GetComponent<Renderer>(), .03f);

        // sabit bölgeler (dekor/eşya çakışmasın)
        blocked.Add((Vector3.zero, 4)); blocked.Add((new Vector3(7, 0, 4), 6)); blocked.Add((new Vector3(-6, 0, 6), 3)); blocked.Add((new Vector3(10, 0, -12), 3));
        blocked.Add((new Vector3(62, 0, 38), 17)); blocked.Add((new Vector3(-50, 0, 62), 17)); blocked.Add((new Vector3(38, 0, -28), 14));
        foreach (var z in ZonePos) blocked.Add((z, 12));

        // ---- ÇEVRE ----
        Scorch(); BuildRoad(); Mountains(); Skyline();
        for (int i = 0; i < 11; i++) { var p = Rnd(16, 105); if (Free(p, 9) && OffRoad(p, 5)) Ruin(p); }
        for (int i = 0; i < 70; i++)
        {
            var p = Rnd(12, 115); if (!Free(p, 2) || !OffRoad(p)) continue;
            float h = Random.Range(3f, 7f); var tc = new Color(.17f, .13f, .1f) * Random.Range(.8f, 1.3f);
            P(PrimitiveType.Cylinder, p + Vector3.up * h * .5f, new Vector3(.3f, h * .5f, .3f), tc);
            for (int b = 0; b < 4; b++)
            {
                var dir = Quaternion.Euler(0, Random.Range(0, 360), 0) * Vector3.forward;
                var br = P(PrimitiveType.Cylinder, p + Vector3.up * (h * Random.Range(.5f, .85f)) + dir * .5f, new Vector3(.1f, .9f, .1f), tc);
                br.transform.rotation = Quaternion.AngleAxis(60, Vector3.Cross(Vector3.up, dir));
            }
        }
        for (int i = 0; i < 45; i++)
        {
            var p = Rnd(8, 110); if (!Free(p, 2) || !OffRoad(p, -2)) continue; float s = Random.Range(.6f, 2f);
            var rb = P(PrimitiveType.Cube, p + Vector3.up * s * .3f, new Vector3(s, s * .7f, s * 1.2f), new Color(.25f, .25f, .27f) * Random.Range(.8f, 1.2f));
            rb.transform.rotation = Quaternion.Euler(Random.Range(-8, 8), Random.Range(0, 360), Random.Range(-8, 8));
        }

        // sığınak: tezgâh + çatı + ateş ışığı
        var bp = new Vector3(7, 0, 4);
        var bench = P(PrimitiveType.Cube, bp + Vector3.up * .5f, new Vector3(2.2f, 1, 1.2f), new Color(1f, .5f, .1f)); benchT = bench.transform;
        nodes.Add(new Node { kind = Kind.Bench, t = bench.transform, label = "TEZGÂH (üretim -%40 karbon)", reach = 3.5f });
        P(PrimitiveType.Cylinder, bp + new Vector3(-2, 1.4f, -1.5f), new Vector3(.15f, 1.4f, .15f), new Color(.3f, .25f, .2f));
        P(PrimitiveType.Cylinder, bp + new Vector3(2, 1.4f, -1.5f), new Vector3(.15f, 1.4f, .15f), new Color(.3f, .25f, .2f));
        P(PrimitiveType.Cube, bp + new Vector3(0, 2.85f, -.5f), new Vector3(5, .12f, 3.5f), new Color(.4f, .38f, .35f)).transform.rotation = Quaternion.Euler(8, 0, 0);
        var lg = new GameObject("BenchLight"); lg.transform.SetParent(world.transform); lg.transform.position = bp + Vector3.up * 2.4f;
        benchLight = lg.AddComponent<Light>(); benchLight.type = LightType.Point; benchLight.color = new Color(1f, .6f, .25f); benchLight.range = 16; benchLight.intensity = 3;

        Camp(bp);
        Tent(bp + new Vector3(5.5f, 0, 3f), 20, new Color(.55f, .5f, .35f)); Tent(bp + new Vector3(3.5f, 0, 8.5f), -35, new Color(.3f, .4f, .3f));
        Barrel(bp + new Vector3(2.8f, 0, -1.4f)); Barrel(bp + new Vector3(3.5f, 0, -.9f));
        Car(new Vector3(-47, 0, ROAD_Z + 1f), 8); Car(new Vector3(-24, 0, ROAD_Z - 1.2f), 172); Car(new Vector3(37, 0, ROAD_Z + .8f), -6);
        Car(new Vector3(63, 0, ROAD_Z - 1f), 190); Car(new Vector3(88, 0, ROAD_Z + 1.2f), 4);
        for (int i = 0; i < 3; i++) { var cp = RndFree(30, 90, 4f); if (OffRoad(cp, 2)) Car(cp, Random.Range(0, 360)); }
        for (int i = -3; i <= 3; i++) Lamp(new Vector3(i * 28 + 17, 0, ROAD_Z + 4.6f), Random.Range(-8f, 8f));
        for (int i = 0; i < 14; i++) { var bpos = RndFree(10, 100, 1.5f); Barrel(bpos); if (Random.value < .5f) Barrel(bpos + new Vector3(Random.Range(-1.4f, 1.4f), 0, Random.Range(-1.4f, 1.4f))); }
        Puddles(14); Tufts(220);
        if (!toMenu || true) BuildFence();

        Crate(new Vector3(-6, 0, 6)); Crate(new Vector3(10, 0, -12));
        Scatter(Item.PetBottle, 5, 5, 25); Scatter(Item.Can, 6, 5, 25); Scatter(Item.Scrap, 3, 6, 25); Scatter(Item.Plastic, 5, 5, 25);
        Scatter(Item.Cloth, 5, 5, 25); Scatter(Item.Battery, 2, 8, 25); Scatter(Item.Glass, 4, 5, 25); Scatter(Item.Organic, 5, 5, 25);
        Add(Item.BottledWater, 1);
        if (!toMenu) { banner = "FAZ 1: SIĞINAK"; bannerT = 4; Toast("Eşyaların üstünde adı yazar. Yaklaş ve " + Inp.Nice(K.Interact) + " ile topla."); }
    }
    static readonly Vector3[] ZonePos = { new Vector3(30, 0, 60), new Vector3(-40, 0, -45), new Vector3(55, 0, -60) };

    // ---------------------------- ÇEVRE YARDIMCILARI ----------------------------
    void Scorch()
    {
        for (int i = 0; i < 12; i++)
        {
            var p = Rnd(8, 120); float r = Random.Range(6f, 18f);
            P(PrimitiveType.Cylinder, p + Vector3.up * .005f, new Vector3(r, .004f, r), new Color(.09f, .075f, .065f));
        }
        for (int i = 0; i < 8; i++)
        {
            var p = Rnd(8, 120); float r = Random.Range(4f, 12f);
            P(PrimitiveType.Cylinder, p + Vector3.up * .007f, new Vector3(r, .004f, r), new Color(.5f, .47f, .4f));
        }
    }

    void BuildRoad()
    {
        P(PrimitiveType.Cube, new Vector3(0, .03f, ROAD_Z), new Vector3(240, .05f, 7), new Color(.12f, .12f, .13f));
        P(PrimitiveType.Cube, new Vector3(0, .035f, ROAD_Z + 3.7f), new Vector3(240, .04f, .5f), new Color(.2f, .18f, .15f)); // kenar toprak
        P(PrimitiveType.Cube, new Vector3(0, .035f, ROAD_Z - 3.7f), new Vector3(240, .04f, .5f), new Color(.2f, .18f, .15f));
        for (int i = -23; i <= 23; i++)
        {
            if (Random.value < .15f) continue; // aşınmış çizgiler
            P(PrimitiveType.Cube, new Vector3(i * 5f, .07f, ROAD_Z), new Vector3(2.2f, .02f, .18f), new Color(.7f, .6f, .2f));
        }
    }

    void Mountains()
    {
        int n = 36;
        for (int i = 0; i < n; i++)
        {
            float a = i / (float)n * 6.2832f + Random.Range(-.05f, .05f), r = Random.Range(150f, 175f);
            float w = Random.Range(70f, 110f), h = Random.Range(50f, 110f);
            var m = P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r), new Vector3(w, h, w * Random.Range(.7f, 1.1f)), new Color(.22f, .18f, .15f) * Random.Range(.8f, 1.2f));
            m.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            m.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    void Skyline()
    {
        int n = 24;
        for (int i = 0; i < n; i++)
        {
            float a = i / (float)n * 6.2832f + Random.Range(-.1f, .1f), r = Random.Range(112f, 142f);
            float w = Random.Range(14f, 24f), h = Random.Range(35f, 95f), d = Random.Range(14f, 24f);
            var b = P(PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * r, h / 2f - 3f, Mathf.Sin(a) * r), new Vector3(w, h, d), new Color(.2f, .2f, .22f) * Random.Range(.8f, 1.2f));
            b.transform.rotation = Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(0, 360), Random.Range(-3f, 3f));
            b.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    void BuildFence()
    {
        int n = 44; float R = 28.6f;
        for (int i = 0; i < n; i++)
        {
            float a = i / (float)n * 6.2832f; var pos = new Vector3(Mathf.Cos(a) * R, 0, Mathf.Sin(a) * R); var tan = new Vector3(-Mathf.Sin(a), 0, Mathf.Cos(a));
            var seg = P(PrimitiveType.Cube, pos + Vector3.up * .6f, new Vector3(.2f, 1.1f, 4.1f), new Color(.38f, .2f, .1f) * Random.Range(.8f, 1.2f));
            seg.transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(Random.Range(-3f, 3f), 0, Random.Range(-4f, 4f)); fence.Add(seg);
            fence.Add(P(PrimitiveType.Cylinder, pos + Vector3.up * .9f, new Vector3(.2f, .9f, .2f), new Color(.25f, .22f, .2f)));
            if (i % 2 == 0)
            {
                var st2 = P(PrimitiveType.Cube, pos + Vector3.up * 1.05f, new Vector3(.22f, .16f, 4.1f), new Color(.85f, .7f, .1f));
                st2.transform.rotation = seg.transform.rotation; fence.Add(st2);
            }
            if (i % 4 == 0)
            {
                var l = P(PrimitiveType.Sphere, pos + Vector3.up * 1.95f, Vector3.one * .3f, new Color(1f, .55f, .1f));
                Glow(l.GetComponent<Renderer>(), new Color(1f, .5f, .05f)); fence.Add(l);
            }
        }
    }

    void Camp(Vector3 bp)
    {
        var fp = bp + new Vector3(-3.6f, 0, 2.8f);
        for (int i = 0; i < 8; i++)
        {
            float a = i / 8f * 6.2832f;
            P(PrimitiveType.Sphere, fp + new Vector3(Mathf.Cos(a) * .75f, .12f, Mathf.Sin(a) * .75f), new Vector3(.38f, .26f, .38f), new Color(.3f, .3f, .32f) * Random.Range(.8f, 1.2f));
        }
        for (int i = 0; i < 3; i++)
        {
            var lgo = P(PrimitiveType.Cylinder, fp + Vector3.up * .2f, new Vector3(.12f, .6f, .12f), new Color(.2f, .12f, .07f));
            lgo.transform.rotation = Quaternion.Euler(80, i * 60 + 10, 0);
        }
        PS(world.transform, fp + Vector3.up * .3f, new Color(1f, .55f, .1f, .9f), new Color(1f, .15f, 0f, 0), 35, .9f, 1.3f, .35f, .6f, -.05f, false, Vector3.one, true);
        var fl = new GameObject("FireLight"); fl.transform.SetParent(world.transform); fl.transform.position = fp + Vector3.up * .8f;
        fireLight = fl.AddComponent<Light>(); fireLight.type = LightType.Point; fireLight.color = new Color(1f, .5f, .2f); fireLight.range = 10; fireLight.intensity = 2.5f;
        blocked.Add((fp, 2f));
    }

    void Tent(Vector3 pos, float yaw, Color c)
    {
        var t = P(PrimitiveType.Cube, pos + Vector3.up * .75f, new Vector3(1.5f, 1.5f, 2.4f), c);
        t.transform.rotation = Quaternion.Euler(0, yaw, 45);
        blocked.Add((pos, 2.2f));
    }

    void Barrel(Vector3 p)
    {
        var cs = new[] { new Color(.5f, .22f, .1f), new Color(.15f, .3f, .5f) };
        bool toxic = Random.value < .35f;
        var b = P(PrimitiveType.Cylinder, p + Vector3.up * .5f, new Vector3(.7f, .5f, .7f), toxic ? new Color(.3f, .6f, .1f) : cs[Random.Range(0, 2)]);
        if (toxic) Glow(b.GetComponent<Renderer>(), new Color(.1f, .3f, 0f));
        if (Random.value < .3f) { b.transform.position = p + Vector3.up * .35f; b.transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0); }
    }

    void Car(Vector3 pos, float yaw)
    {
        var g = new GameObject("Car"); g.transform.SetParent(world.transform); g.transform.position = pos;
        var c = Color.HSVToRGB(Random.value, .3f, .32f);
        P(PrimitiveType.Cube, pos + Vector3.up * .75f, new Vector3(4.2f, .8f, 1.9f), c, true, g.transform);
        P(PrimitiveType.Cube, pos + new Vector3(-.3f, 1.4f, 0), new Vector3(2.2f, .75f, 1.65f), c, false, g.transform);
        P(PrimitiveType.Cube, pos + new Vector3(-.3f, 1.42f, 0), new Vector3(2.25f, .45f, 1.7f), new Color(.05f, .07f, .09f), false, g.transform);
        foreach (var sx in new[] { -1.35f, 1.35f })
            foreach (var sz in new[] { -.95f, .95f })
            {
                var w = P(PrimitiveType.Cylinder, pos + new Vector3(sx, .4f, sz), new Vector3(.8f, .12f, .8f), new Color(.05f, .05f, .05f), false, g.transform);
                w.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        g.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), yaw, Random.Range(-3f, 3f));
        blocked.Add((pos, 3.2f));
    }

    void Lamp(Vector3 b, float lean)
    {
        var g = new GameObject("Lamp"); g.transform.SetParent(world.transform); g.transform.position = b;
        var dark = new Color(.14f, .14f, .15f);
        P(PrimitiveType.Cylinder, b + Vector3.up * 2.4f, new Vector3(.12f, 2.4f, .12f), dark, false, g.transform);
        P(PrimitiveType.Cube, b + new Vector3(0, 4.75f, -.55f), new Vector3(.1f, .1f, 1.3f), dark, false, g.transform);
        bool on = Random.value < .4f;
        var bulb = P(PrimitiveType.Sphere, b + new Vector3(0, 4.65f, -1.2f), Vector3.one * .3f, on ? new Color(1f, .85f, .5f) : new Color(.25f, .25f, .25f), false, g.transform);
        if (on) Glow(bulb.GetComponent<Renderer>(), new Color(1f, .8f, .4f));
        g.transform.rotation = Quaternion.Euler(lean, 0, Random.Range(-10f, 10f));
    }

    void Puddles(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var p = Rnd(5, 100); if (!Free(p, 1)) continue; float r = Random.Range(1.5f, 4f);
            var pd = P(PrimitiveType.Cylinder, p + Vector3.up * .025f, new Vector3(r, .01f, r * Random.Range(.6f, 1f)), new Color(.1f, .12f, .14f));
            Gloss(pd.GetComponent<Renderer>(), .95f); pd.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    void Tufts(int n)
    {
        if (grassMat == null)
        {
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grassMat = new Material(tmp.GetComponent<Renderer>().sharedMaterial); Destroy(tmp);
            grassMat.color = new Color(.4f, .34f, .15f);
        }
        for (int i = 0; i < n; i++)
        {
            var p = Rnd(3, 120); if (!Free(p, .5f)) continue;
            var tuft = new GameObject("tuft"); tuft.transform.SetParent(world.transform); tuft.transform.position = p;
            for (int b = 0; b < 3; b++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                var col = g.GetComponent<Collider>(); if (col) Destroy(col);
                g.transform.SetParent(tuft.transform, false);
                g.transform.localPosition = new Vector3(Random.Range(-.15f, .15f), .22f, Random.Range(-.15f, .15f));
                g.transform.localScale = new Vector3(.03f, .22f, .03f);
                g.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
                var r = g.GetComponent<Renderer>(); r.sharedMaterial = grassMat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }

    void Ruin(Vector3 c)
    {
        var par = new GameObject("Ruin"); par.transform.SetParent(world.transform); par.transform.position = c;
        float w = Random.Range(7f, 10f), d = Random.Range(6f, 9f); var col = new Color(.32f, .3f, .28f) * Random.Range(.8f, 1.1f); col.a = 1;
        Wall(par.transform, c + new Vector3(0, 0, d / 2), true, w, col); Wall(par.transform, c - new Vector3(0, 0, d / 2), true, w, col);
        Wall(par.transform, c + new Vector3(w / 2, 0, 0), false, d, col); Wall(par.transform, c - new Vector3(w / 2, 0, 0), false, d, col);
        par.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        blocked.Add((c, 9f));
    }
    void Wall(Transform par, Vector3 mid, bool alongX, float len, Color col)
    {
        float h = Random.Range(1.5f, 4f), gap = 2.4f, gp = Random.Range(-len / 2 + 2f, len / 2 - 2f);
        Seg(par, mid, alongX, -len / 2, gp - gap / 2, h, col); Seg(par, mid, alongX, gp + gap / 2, len / 2, h * Random.Range(.4f, 1f), col);
    }
    void Seg(Transform par, Vector3 mid, bool alongX, float a, float b, float h, Color col)
    {
        float l = b - a; if (l < .3f) return; float cp = (a + b) / 2;
        var pos = mid + (alongX ? new Vector3(cp, h / 2, 0) : new Vector3(0, h / 2, cp));
        P(PrimitiveType.Cube, pos, alongX ? new Vector3(l, h, .5f) : new Vector3(.5f, h, l), col, true, par);
    }

    void Crate(Vector3 pos)
    {
        var g = P(PrimitiveType.Cube, pos + Vector3.up * .4f, new Vector3(1, .8f, 1), new Color(.1f, .6f, .6f));
        nodes.Add(new Node { kind = Kind.Crate, t = g.transform, label = "Market Kalıntısı", reach = 2.5f });
    }
    void Scatter(Item it, int n, float r0, float r1) { for (int i = 0; i < n; i++) SpawnPickup(it, RndFree(r0, r1)); }
    void SpawnPickup(Item it, Vector3 pos)
    {
        var d = DB.I[it];
        var sc = d.shape == PrimitiveType.Cylinder ? new Vector3(d.size.x, d.size.y * .5f, d.size.z) : d.size;
        float by = d.size.y * .5f + .18f;
        var g = P(d.shape, pos + Vector3.up * by, sc, d.color); Glow(g.GetComponent<Renderer>(), d.color * .25f);
        g.transform.rotation = Quaternion.Euler(Random.Range(0, 20), Random.Range(0, 360), 0);
        float dm = Mathf.Max(d.size.x, d.size.z) * 2.4f + .2f;
        var disc = P(PrimitiveType.Cylinder, pos + Vector3.up * .02f, new Vector3(dm, .01f, dm), d.color * .6f); Glow(disc.GetComponent<Renderer>(), d.color * .3f);
        nodes.Add(new Node { kind = Kind.Pickup, t = g.transform, item = it, reach = 2.6f, baseY = by, phase = Random.value * 6f, extra = disc });
    }

    void StartPhase2()
    {
        phase = 2;
        foreach (var f in fence) if (f) Destroy(f);
        fence.Clear();
        // dünya sınırı: büyük kaya halkası
        for (int i = 0; i < 60; i++)
        {
            float a = i / 60f * 6.2832f + Random.Range(-.03f, .03f), r = Random.Range(98f, 103f);
            var bd = P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * r, .5f, Mathf.Sin(a) * r), new Vector3(Random.Range(5f, 9f), Random.Range(3f, 6f), Random.Range(5f, 9f)), new Color(.2f, .19f, .19f) * Random.Range(.8f, 1.2f));
            bd.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
        Scatter(Item.PetBottle, 8, 30, 92); Scatter(Item.Can, 8, 30, 92); Scatter(Item.Scrap, 8, 30, 92); Scatter(Item.Plastic, 8, 30, 92);
        Scatter(Item.Cloth, 8, 30, 92); Scatter(Item.Battery, 4, 30, 92); Scatter(Item.Glass, 8, 30, 92); Scatter(Item.Organic, 8, 30, 92);
        foreach (var z in ZonePos) { SpawnPickup(Item.Battery, z + Rnd(0, 5)); SpawnPickup(Item.Glass, z + Rnd(0, 5)); } // risk/ödül
        Crate(new Vector3(-30, 0, -40)); Crate(new Vector3(50, 0, 10));

        P(PrimitiveType.Cylinder, new Vector3(38, .015f, -28), new Vector3(23, .02f, 23), new Color(.18f, .15f, .1f)); // kıyı
        var lk = P(PrimitiveType.Cylinder, new Vector3(38, .03f, -28), new Vector3(20, .03f, 20), Color.gray);
        lakeR = lk.GetComponent<Renderer>(); Gloss(lakeR, .95f);
        nodes.Add(new Node { kind = Kind.Lake, t = lk.transform, label = "GÖL", radius = 10, reach = 2.5f });

        MakeFactory(new Vector3(62, 0, 38)); MakeFactory(new Vector3(-50, 0, 62));

        foreach (var rp in ZonePos)
        {
            var z = P(PrimitiveType.Cylinder, rp + Vector3.up * .03f, new Vector3(18, .03f, 18), new Color(.8f, .1f, .1f)); Glow(z.GetComponent<Renderer>(), new Color(.5f, .05f, .05f));
            var pole = P(PrimitiveType.Cylinder, rp + Vector3.up * 1.5f, new Vector3(.2f, 1.5f, .2f), new Color(1f, .9f, 0f)); Glow(pole.GetComponent<Renderer>(), new Color(.8f, .7f, 0f));
            radZones.Add(z.transform);
            PS(world.transform, rp + Vector3.up * .3f, new Color(1f, .9f, .2f, .7f), new Color(.6f, 1f, .2f, 0), 22, 3.5f, 0, .15f, .35f, -.04f, true, new Vector3(15, .1f, 15), false);
        }
        banner = "FAZ 2: AÇIK DÜNYA"; bannerT = 4; Snd(cDone);
        Toast("Sığınak çiti açıldı. Fabrikalar, kirli göl ve radyasyon bölgeleri artık erişilebilir. Haritaya (" + Inp.Nice(K.Map) + ") bak.");
    }

    void MakeFactory(Vector3 pos)
    {
        var f = new Factory();
        var root = new GameObject("Factory"); root.transform.SetParent(world.transform); root.transform.position = pos; f.root = root.transform;
        P(PrimitiveType.Cube, pos + Vector3.up * 4, new Vector3(14, 8, 10), new Color(.33f, .33f, .36f), true, root.transform);
        P(PrimitiveType.Cube, pos + new Vector3(-3, 9, 0), new Vector3(6, 2, 8), new Color(.28f, .28f, .3f), false, root.transform);
        P(PrimitiveType.Cylinder, pos + new Vector3(4, 7, 0), new Vector3(2.2f, 7, 2.2f), new Color(.25f, .2f, .2f), true, root.transform);
        for (int i = 0; i < 5; i++) Glow(P(PrimitiveType.Cube, pos + new Vector3(-5.5f + i * 2.6f, 5, -5.05f), new Vector3(1.2f, 1.2f, .1f), new Color(1f, .8f, .3f), false, root.transform).GetComponent<Renderer>(), new Color(1f, .7f, .2f) * .9f);
        for (int i = 0; i < 4; i++) P(PrimitiveType.Cylinder, pos + new Vector3(-6 + i * 1.1f, .6f, -10), new Vector3(.9f, .6f, .9f), i % 2 == 0 ? new Color(.2f, .6f, .1f) : new Color(.6f, .15f, .1f), true, root.transform);
        f.lamp = P(PrimitiveType.Sphere, pos + new Vector3(4, 14.4f, 0), Vector3.one * .9f, Color.red, false, root.transform).GetComponent<Renderer>();
        f.ps = PS(root.transform, pos + new Vector3(4, 14.6f, 0), new Color(.1f, .1f, .09f, .9f), new Color(.35f, .33f, .3f, 0), 24, 8, 2.2f, 2.5f, 4.5f, -.02f, false, Vector3.one, true);
        f.pipe = P(PrimitiveType.Cube, pos + new Vector3(0, .3f, -8), new Vector3(1.2f, .6f, 6), new Color(.5f, .6f, .1f), false, root.transform).GetComponent<Renderer>(); Glow(f.pipe, new Color(.2f, .4f, 0f));
        var vp = pos + new Vector3(5, 0, -8);
        P(PrimitiveType.Cylinder, vp + Vector3.up * .6f, new Vector3(.2f, .6f, .2f), Color.gray, false, root.transform);
        var wheel = P(PrimitiveType.Cylinder, vp + Vector3.up * 1.3f, new Vector3(1.3f, .08f, 1.3f), new Color(.9f, .1f, .1f), false, root.transform); Glow(wheel.GetComponent<Renderer>(), new Color(.6f, 0, 0));
        nodes.Add(new Node { kind = Kind.Valve, t = wheel.transform, label = "ATIK VANASI", reach = 2.5f, fac = f }); facs.Add(f);
    }

    // =====================================================================
    //  ENVANTER / YARDIMCILAR
    // =====================================================================
    int Total() { int t = 0; foreach (var v in inv.Values) t += v; return t; }
    bool Has(Item i, int n = 1) { int c; return inv.TryGetValue(i, out c) && c >= n; }
    int Cnt(Item i) { int c; return inv.TryGetValue(i, out c) ? c : 0; }
    void Add(Item i, int n) { inv[i] = Cnt(i) + n; }
    void Take(Item i, int n) { inv[i] = Cnt(i) - n; }
    void Toast(string s) { msg = s; msgT = 5f; }
    void Hurt(float v, string c) { health -= v; cause = c; }
    string Kn(K k) { return Inp.Nice(k); }

    void AddEco(float v)
    {
        eco += v;
        while (rankIdx + 1 < Ranks.Length && eco >= Ranks[rankIdx + 1].pts)
        {
            rankIdx++; foreach (var r in Ranks[rankIdx].rw) Add(r.Item1, r.Item2);
            banner = "RÜTBE: " + Ranks[rankIdx].name.ToUpper(); bannerT = 3; Toast("Ödül çantana eklendi!"); Snd(cDone, .6f);
        }
    }

    // =====================================================================
    //  OYUN DÖNGÜSÜ
    // =====================================================================
    void Cur(bool free) { Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = free; }

    void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, .05f);
        if (msgT > 0) msgT -= dt; if (bannerT > 0) bannerT -= dt; if (flash > 0) flash = Mathf.Max(0, flash - dt * 1.5f);
        if (waitKey >= 0) return;
        Visuals(dt);

        switch (st)
        {
            case St.Menu: pl.Rotate(0, 6 * dt, 0); Cur(true); return;
            case St.Story: Cur(true); if (Inp.Down(K.Confirm)) NextPage(); return;
            case St.Pause:
                Cur(true);
                if (Inp.Down(K.Menu)) { if (scr != Scr.Main) scr = Scr.Main; else st = St.Play; }
                return;
            case St.Dead: case St.Win: Cur(true); if (Inp.Down(K.Restart)) NewGame(false); return;
        }

        // --- OYNANIŞ ---
        if (Inp.Down(K.Menu)) { if (craftOpen) craftOpen = false; else { st = St.Pause; scr = Scr.Main; } return; }
        if (Inp.Down(K.Help)) { st = St.Pause; scr = Scr.Help; craftOpen = false; return; }
        if (Inp.Down(K.Craft)) craftOpen = !craftOpen;
        if (Inp.Down(K.Quest)) showQuests = !showQuests;
        if (Inp.Down(K.Inv)) showInv = !showInv;
        if (Inp.Down(K.Map)) showMap = !showMap;
        if (Inp.Down(K.Cam)) { thirdPerson = !thirdPerson; Toast(thirdPerson ? "3. kişi kamera" : "1. kişi kamera"); }
        Cur(craftOpen);
        MoveP(dt); Interact(dt); UseKeys(); Vitals(dt); Sim(dt); Progress();

        if (health < lastHealth - .001f) { flash = Mathf.Min(1f, flash + (lastHealth - health) * .12f + .03f); if (Time.time > nextHurt) { Snd(cHurt, .5f); nextHurt = Time.time + .9f; } }
        lastHealth = health;
    }

    void NextPage() { page++; if (page >= Story.Length) { st = St.Play; Cur(false); banner = "FAZ 1: SIĞINAK"; bannerT = 4; Toast("Eşyaların üstünde adı yazar. Yaklaş ve " + Kn(K.Interact) + " ile topla."); } }

    void MoveP(float dt)
    {
        bool tp = thirdPerson;
        if (!craftOpen) { var l = Inp.Look(); pl.Rotate(0, l.x, 0); pitch = Mathf.Clamp(pitch - l.y, tp ? -35f : -80f, tp ? 60f : 80f); }
        var m = craftOpen ? Vector2.zero : Inp.Move();
        bool moving = m.sqrMagnitude > .1f; bool run = moving && Inp.Held(K.Sprint);
        velY = cc.isGrounded ? -1f : velY - 20f * dt;
        var mv = pl.right * m.x + pl.forward * m.y; if (mv.sqrMagnitude > 1) mv.Normalize();
        mv *= run ? RUN : WALK; mv.y = velY; cc.Move(mv * dt);

        // adım sesi, kamera sallanması (1. kişi), FOV, karakter animasyonu
        if (moving && cc.isGrounded) { bobT += dt * (run ? 12f : 8f); stepT += dt * (run ? 12f : 8f); if (stepT > Mathf.PI) { stepT -= Mathf.PI; Snd(cStep, .5f); } }
        cm.fieldOfView = Mathf.Lerp(cm.fieldOfView, run ? 80f : 70f, dt * 6f);
        AnimateModel(dt, moving, run, m);

        float R = phase == 1 ? 28f : 95f; var p = pl.position; var f = new Vector2(p.x, p.z);
        if (f.magnitude > R)
        {
            f = f.normalized * R; cc.enabled = false; pl.position = new Vector3(f.x, p.y, f.y); cc.enabled = true;
            if (msgT <= 0) Toast(phase == 1 ? "Henüz uzaklaşma: önce el kitabıyla temel becerileri öğren." : "Dünyanın sınırı.");
        }
    }

    void Interact(float dt)
    {
        tgt = null; float best = 99; var pp = pl.position; var fw = new Vector2(pl.forward.x, pl.forward.z);
        foreach (var n in nodes)
        {
            if (n.done || !n.t) continue;
            var d2 = new Vector2(n.t.position.x - pp.x, n.t.position.z - pp.z); float d = d2.magnitude - n.radius;
            if (d > n.reach) continue;
            if (n.kind != Kind.Lake && d > 1f && Vector2.Dot(fw, d2.normalized) < .25f) continue;
            if (d < best) { best = d; tgt = n; }
        }
        foreach (var n in nodes) if (n.kind == Kind.Valve && n != tgt) n.progress = 0;
        if (tgt == null || craftOpen) return;
        if (tgt.kind == Kind.Valve)
        {
            if (Inp.Held(K.Interact))
            {
                tgt.progress += dt / 2.5f; tgt.t.Rotate(0, 220 * dt, 0);
                valveSnd -= dt; if (valveSnd <= 0) { Snd(cValve, .6f); valveSnd = .35f; }
                if (tgt.progress >= 1) CloseValve(tgt);
            }
            else tgt.progress = 0;
        }
        else if (Inp.Down(K.Interact)) UseNode(tgt);
    }

    void UseNode(Node n)
    {
        switch (n.kind)
        {
            case Kind.Pickup:
                if (Total() >= Cap) { Toast("Çanta dolu! Geri dönüştür veya kullan."); return; }
                Add(n.item, 1); n.done = true; Destroy(n.t.gameObject); if (n.extra) Destroy(n.extra); collected++; AddEco(1); Snd(cPick, .6f);
                Toast("+ " + DB.I[n.item].name); break;
            case Kind.Crate:
                n.done = true; n.t.GetComponent<Renderer>().material.color = Color.gray; Add(Item.BottledWater, 1); Snd(cPick);
                Toast("Hazır PET su buldun. İçersen karbon bütçen azalır, boş şişeyi geri dönüştür!"); break;
            case Kind.Bench:
                craftOpen = true; Toast("Tezgâhta üretim karbon maliyeti %40 daha düşük."); break;
            case Kind.Lake:
                if (purity >= 70) { thirst = Mathf.Min(100, thirst + 35); drank = true; Snd(cDrink); Toast("Temizlenen gölden içtin."); }
                else if (Has(Item.PetBottle)) { Take(Item.PetBottle, 1); Add(Item.DirtyWater, 1); Snd(cPick); Toast("Kirli su doldurdun. Arıtmadan İÇME!"); }
                else Toast("Su almak için boş PET şişe gerek."); break;
        }
    }

    void CloseValve(Node n)
    {
        n.done = true; n.fac.active = false;
        if (n.fac.ps) { n.fac.ps.Stop(); Destroy(n.fac.ps.gameObject, 8f); }
        n.fac.pipe.material.color = Color.gray; Glow(n.fac.pipe, Color.black); budget += 4; AddEco(50); Snd(cDone);
        banner = "VANA KAPANDI"; bannerT = 2.5f; Toast("Fabrika zehir salmayı kesti (+4kg bütçe).");
    }

    void UseKeys()
    {
        if (Inp.Down(K.Drink)) Drink();
        if (Inp.Down(K.Heal)) { if (Has(Item.Bandage)) { Take(Item.Bandage, 1); health = Mathf.Min(100, health + 35); Snd(cPick); } else Toast("Bandajın yok."); }
        if (Inp.Down(K.Mask)) { if (Has(Item.MaskFilter)) { Take(Item.MaskFilter, 1); maskT = 120; Snd(cPick); Toast("Maske takıldı: zehir ve radyasyon azaldı."); } else Toast("Maske filtren yok."); }
        if (Inp.Down(K.Plant)) Plant();
        if (Inp.Down(K.Solar)) PlaceSolar();
    }

    void Drink()
    {
        if (Has(Item.CleanWater)) { Take(Item.CleanWater, 1); thirst = Mathf.Min(100, thirst + 45); tox = Mathf.Max(0, tox - 3); drank = true; AddEco(3); Snd(cDrink); Toast("Arıtılmış su: güvenli ve düşük karbon."); }
        else if (Has(Item.BottledWater)) { Take(Item.BottledWater, 1); thirst = Mathf.Min(100, thirst + 40); budget -= .8f; Add(Item.PetBottle, 1); drank = true; Snd(cDrink); Toast("Hazır su: -0.8kg bütçe. Boş şişe çantada, geri dönüştür!"); }
        else if (Has(Item.DirtyWater)) { Take(Item.DirtyWater, 1); thirst = Mathf.Min(100, thirst + 25); tox = Mathf.Min(100, tox + 30); Add(Item.PetBottle, 1); cause = "Kirli su içmek"; Snd(cHurt); Toast("KİRLİ SU! Zehirlendin."); }
        else Toast("Suyun yok.");
    }

    void Plant()
    {
        if (!Has(Item.Sapling)) { Toast("Fidan setin yok."); return; }
        Take(Item.Sapling, 1); var pos = pl.position + pl.forward * 2f; pos.y = 0;
        var root = new GameObject("Tree"); root.transform.SetParent(world.transform); root.transform.position = pos;
        P(PrimitiveType.Cylinder, pos + Vector3.up, new Vector3(.4f, 1, .4f), new Color(.4f, .25f, .1f), false, root.transform);
        P(PrimitiveType.Sphere, pos + Vector3.up * 2.6f, Vector3.one * 2.2f, new Color(.15f, .6f, .15f), false, root.transform);
        P(PrimitiveType.Sphere, pos + new Vector3(.6f, 2.2f, .3f), Vector3.one * 1.4f, new Color(.2f, .7f, .2f), false, root.transform);
        root.transform.localScale = Vector3.one * .3f; treeObjs.Add(root.transform);
        trees++; budget += 3; AddEco(25); Snd(cDone, .7f); Toast("Fidan dikildi: bütçe +3, zamanla pasif karbon emilimi.");
    }

    void PlaceSolar()
    {
        if (!Has(Item.SolarPanel)) { Toast("Güneş panelin yok."); return; }
        Take(Item.SolarPanel, 1); var pos = pl.position + pl.forward * 2f; pos.y = 0;
        var pn = P(PrimitiveType.Cube, pos + Vector3.up * .9f, new Vector3(2f, .08f, 1.3f), new Color(.1f, .2f, .8f)); pn.transform.rotation = Quaternion.Euler(30, pl.eulerAngles.y, 0); Gloss(pn.GetComponent<Renderer>(), .9f);
        P(PrimitiveType.Cylinder, pos + Vector3.up * .4f, new Vector3(.1f, .4f, .1f), Color.gray);
        solar++; AddEco(40); Snd(cDone, .7f); Toast("Güneş paneli kuruldu: pasif karbon kredisi.");
    }

    // =====================================================================
    //  ÜRETİM
    // =====================================================================
    bool NearBench() { return benchT && Vector3.Distance(benchT.position, pl.position) < 4f; }
    float CostOf(Recipe r) { return r.carbon > 0 ? r.carbon * (NearBench() ? BENCH_MULT : 1f) : r.carbon; }
    bool CanCraft(Recipe r) { foreach (var i in r.inp) if (!Has(i.id, i.n)) return false; return true; }

    void Craft(Recipe r)
    {
        if (!CanCraft(r)) return;
        float cost = CostOf(r); bool bench = NearBench();
        foreach (var i in r.inp) Take(i.id, i.n);
        Add(r.outId, r.outN); budget -= cost;
        if (bench) benchCrafts++; if (r.outId == Item.Filter) madeFilter = true;
        AddEco(cost < 0 ? 6 : 2); Snd(cCraft);
        Toast(cost < 0 ? "Geri dönüşüm! Karbon bütçen +" + (-cost).ToString("0.0") + "kg" : "Üretildi: " + DB.I[r.outId].name + "  (-" + cost.ToString("0.00") + "kg CO2)");
    }

    // =====================================================================
    //  SİMÜLASYON
    // =====================================================================
    void Vitals(float dt)
    {
        bool run = Inp.Held(K.Sprint) && !craftOpen && Inp.Move().sqrMagnitude > .1f;
        thirst -= (run ? SPRINT_THIRST : THIRST) * dt;
        if (thirst <= 0) { thirst = 0; Hurt(2f * dt, "Susuzluk"); }

        float expo = 0; var p = pl.position;
        foreach (var f in facs) if (f.active) expo += Vector2.Distance(new Vector2(p.x, p.z), new Vector2(f.root.position.x, f.root.position.z)) < 30 ? EXPO_NEAR : EXPO_FAR;
        if (budget < 0) expo += SMOG_EXPO;
        if (maskT > 0) { expo *= .3f; maskT -= dt; }
        tox = Mathf.Clamp(tox + (expo - TOX_DECAY) * dt, 0, 100);

        inZone = false;
        foreach (var z in radZones) if (Vector2.Distance(new Vector2(p.x, p.z), new Vector2(z.position.x, z.position.z)) < z.localScale.x * .5f) inZone = true;
        rad = Mathf.Clamp(rad + (inZone ? (maskT > 0 ? RAD_RATE * .5f : RAD_RATE) : -RAD_DECAY) * dt, 0, 100);
        if (inZone && Random.value < dt * (4 + rad * .15f)) Snd(cGeiger, .5f);

        if (tox > 60) Hurt((tox - 60) * .05f * dt, "Zehirlenme (kirli hava)");
        if (rad > 60) Hurt((rad - 60) * .05f * dt, "Radyasyon");
        if (thirst > 60 && tox < 30 && rad < 30) health = Mathf.Min(100, health + .8f * dt);
        if (health <= 0) { health = 0; st = St.Dead; craftOpen = false; }
    }

    int Active() { int a = 0; foreach (var f in facs) if (f.active) a++; return a; }

    void Sim(float dt)
    {
        int act = Active();
        budget += (-FAC_DRAIN * act + TREE_GAIN * trees + SOLAR_GAIN * solar) * dt;
        if (facs.Count > 0) purity = Mathf.Clamp(purity + (act * -.2f + (facs.Count - act) * .5f) * dt, 0, 100);
    }

    // sadece görsel/atmosfer (menüde de çalışır)
    void Visuals(float dt)
    {
        foreach (var n in nodes)
            if (n.kind == Kind.Pickup && n.t && !n.done)
            { n.t.Rotate(0, 60 * dt, 0, Space.World); n.t.position = new Vector3(n.t.position.x, n.baseY + Mathf.Sin(Time.time * 2 + n.phase) * .06f, n.t.position.z); }
        if (benchLight) benchLight.intensity = 3f + Mathf.PerlinNoise(Time.time * 6f, 0) * 1.2f;
        if (fireLight) fireLight.intensity = 2.2f + Mathf.PerlinNoise(Time.time * 9f, 3f) * 1.4f;
        foreach (var f in facs)
            if (f.lamp) { bool on = !f.active || Mathf.Sin(Time.time * 4f) > 0; var c = f.active ? Color.red : Color.green; f.lamp.material.color = on ? c : Color.black; Glow(f.lamp, on ? c * 1.2f : Color.black); }
        if (lakeR) lakeR.material.color = Color.Lerp(new Color(.35f, .3f, .12f), new Color(.12f, .45f, .85f), purity / 100f);
        foreach (var t in treeObjs) { float s = Mathf.MoveTowards(t.localScale.x, 1f, dt / 60f); t.localScale = Vector3.one * s; }

        float smog = Mathf.Clamp01(Active() * .25f + (budget < 0 ? .5f : 0));
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, Mathf.Lerp(.012f, .03f, smog), dt);
        var fc = Color.Lerp(new Color(.6f, .58f, .5f), new Color(.45f, .45f, .2f), smog);
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fc, dt);
        if (skyMat)
        {
            skyMat.SetColor("_SkyTint", Color.Lerp(new Color(.6f, .55f, .45f), new Color(.5f, .55f, .2f), smog));
            skyMat.SetColor("_GroundColor", new Color(.3f, .27f, .22f)); skyMat.SetFloat("_AtmosphereThickness", Mathf.Lerp(1.6f, 2.6f, smog)); skyMat.SetFloat("_Exposure", .9f);
        }
        else if (cm) cm.backgroundColor = RenderSettings.fogColor;
    }

    int ValvesClosed() { int c = 0; foreach (var n in nodes) if (n.kind == Kind.Valve && n.done) c++; return c; }

    void Progress()
    {
        if (phase == 1 && collected >= 5 && madeFilter && benchCrafts >= 1 && drank) StartPhase2();
        // V2-HOOK: if (phase == 2 && ValvesClosed() >= 1) -> fabrika içi (Faz 3): canavarlar + vana bulmacası
        if (phase == 2 && facs.Count > 0 && ValvesClosed() == facs.Count && trees >= 3 && solar >= 1 && purity >= 70) { st = St.Win; craftOpen = false; Snd(cDone); }
    }

    // =====================================================================
    //  ARAYÜZ
    // =====================================================================
    void T(float x, float y, float w, float h, string s, int sz, Color c, TextAnchor a = TextAnchor.UpperLeft)
    {
        if (S == null) S = new GUIStyle(GUI.skin.label) { wordWrap = true };
        S.fontSize = sz; S.alignment = a;
        S.normal.textColor = Color.black; GUI.Label(new Rect(x + 1, y + 1, w, h), s, S);
        S.normal.textColor = c; GUI.Label(new Rect(x, y, w, h), s, S);
    }
    void Box(Rect r, Color c) { var o = GUI.color; GUI.color = c; GUI.DrawTexture(r, px); GUI.color = o; }
    void Panel(Rect r, string title, Color accent)
    {
        Box(r, new Color(0, 0, 0, .55f)); Box(new Rect(r.x, r.y, r.width, 22), new Color(accent.r * .5f, accent.g * .5f, accent.b * .5f, .9f)); Box(new Rect(r.x, r.y, 3, r.height), accent);
        T(r.x + 8, r.y + 2, r.width, 20, title, 13, Color.white);
    }
    void Bar(float x, float y, float w, float v, Color c, string l)
    {
        Box(new Rect(x - 1, y - 1, w + 2, 20), new Color(0, 0, 0, .8f)); Box(new Rect(x, y, w, 18), new Color(.12f, .12f, .12f, .9f));
        Box(new Rect(x + 2, y + 2, (w - 4) * Mathf.Clamp01(v), 14), c); T(x + 5, y, w, 18, l, 12, Color.white);
    }
    bool Btn(float cx, float y, string t, float w = 280) { return GUI.Button(new Rect(cx - w / 2, y, w, 42), t); }
    void Vig(float W, Color c, float a) { if (a <= .01f) return; var o = GUI.color; GUI.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(a)); GUI.DrawTexture(new Rect(0, 0, W, 720), vig); GUI.color = o; }

    void WorldLabels(float s)
    {
        float extra = thirdPerson ? CAM_DIST + .6f : 0f;
        foreach (var n in nodes)
        {
            if (n.done || !n.t) continue;
            bool pk = n.kind == Kind.Pickup;
            var sp = cm.WorldToScreenPoint(n.t.position + Vector3.up * (pk ? .7f : n.kind == Kind.Lake ? 2f : 2.2f));
            if (sp.z < 0 || sp.z > (pk ? 14 : 45) + extra) continue;
            float y = Screen.height - sp.y;
            if (pk)
            {
                var d = DB.I[n.item]; Box(new Rect(sp.x - 70 * s, y - 2 * s, 140 * s, 36 * s), new Color(0, 0, 0, .5f)); Box(new Rect(sp.x - 70 * s, y - 2 * s, 3 * s, 36 * s), d.color);
                T(sp.x - 66 * s, y, 134 * s, 20 * s, d.name, (int)(14 * s), d.color, TextAnchor.UpperCenter);
                T(sp.x - 66 * s, y + 17 * s, 134 * s, 16 * s, d.hint, (int)(11 * s), new Color(.85f, .85f, .85f), TextAnchor.UpperCenter);
            }
            else T(sp.x - 100 * s, y, 200 * s, 22 * s, n.label, (int)(14 * s), Color.yellow, TextAnchor.UpperCenter);
        }
        foreach (var f in facs)
        {
            var sp = cm.WorldToScreenPoint(f.root.position + Vector3.up * 18);
            if (sp.z > 0 && sp.z < 150) T(sp.x - 120 * s, Screen.height - sp.y, 240 * s, 22 * s, f.active ? "FABRİKA - ZEHİR SALIYOR" : "FABRİKA - DURDURULDU", (int)(15 * s), f.active ? Color.red : Color.green, TextAnchor.UpperCenter);
        }
        foreach (var z in radZones)
        {
            var sp = cm.WorldToScreenPoint(z.position + Vector3.up * 4);
            if (sp.z > 0 && sp.z < 80) T(sp.x - 100 * s, Screen.height - sp.y, 200 * s, 22 * s, "RADYASYON", (int)(15 * s), Color.yellow, TextAnchor.UpperCenter);
        }
    }

    string G(bool ok, string t) { return (ok ? "[x] " : "[ ] ") + t; }

    void MapDot(Vector3 wp, Color c, float sz, float mx, float my, float ms, float yaw)
    {
        var rel = wp - pl.position; float fx = Mathf.Sin(yaw), fz = Mathf.Cos(yaw);
        float u = rel.x * fz - rel.z * fx, v = rel.x * fx + rel.z * fz, half = ms / 2, range = 100f;
        float qx = Mathf.Clamp(u / range * half, -half + 4, half - 4), qy = Mathf.Clamp(-v / range * half, -half + 4, half - 4);
        Box(new Rect(mx + half + qx - sz / 2, my + half + qy - sz / 2, sz, sz), c);
    }

    void OnGUI()
    {
        if (px == null || cm == null) return;
        // tuş atama yakalama
        var ev = Event.current;
        if (waitKey >= 0 && ev.type == EventType.KeyDown && ev.keyCode != KeyCode.None)
        {
            if (ev.keyCode != KeyCode.Escape)
            {
                var nk = ev.keyCode; for (int j = 0; j < Inp.NB; j++) if (j != waitKey && Inp.bind[j] == nk) Inp.bind[j] = Inp.bind[waitKey]; // çakışmada yer değiştir
                Inp.bind[waitKey] = nk;
            }
            waitKey = -1; ev.Use();
        }

        float s = Screen.height / 720f;
        if (st == St.Play || st == St.Pause) WorldLabels(s);
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1)); GUI.skin.button.fontSize = 18;
        float W = Screen.width / s, cx = W / 2;

        if (st == St.Menu) { MenuScreen(W, cx); return; }
        if (st == St.Story)
        {
            Box(new Rect(0, 0, W, 720), new Color(0, 0, 0, .92f));
            T(0, 60, W, 60, "LAST RESOURCE", 44, new Color(.6f, 1f, .6f), TextAnchor.UpperCenter);
            T(W * .15f, 200, W * .7f, 300, Story[page], 22, Color.white, TextAnchor.UpperCenter);
            if (Btn(cx, 560, "Devam  (" + Kn(K.Confirm) + ")", 220)) NextPage();
            if (GUI.Button(new Rect(W - 150, 680, 130, 30), "Hikayeyi atla")) { page = Story.Length; NextPage(); }
            return;
        }
        if (st == St.Dead || st == St.Win)
        {
            Box(new Rect(0, 0, W, 720), new Color(0, 0, 0, .86f));
            bool win = st == St.Win;
            T(0, 120, W, 60, win ? "FAZ 1 TAMAMLANDI" : "ÖLDÜN", 44, win ? Color.green : Color.red, TextAnchor.UpperCenter);
            string sum = win
                ? "Fabrikaları durdurdun, gölü temizledin, fidan dikip enerji ürettin.\nKarbon bütçen: " + budget.ToString("0.0") + " kg   |   Eko puan: " + (int)eco + "  (" + Ranks[rankIdx].name + ")\n\nSıradaki: V2 - fabrika içi, canavarlar, vana bulmacası."
                : "Neden: " + cause + "\nBilinçsiz tüketim ve kirlilik dünyayı bu hale getirdi.\nKarbon bütçen: " + budget.ToString("0.0") + " kg   |   Eko puan: " + (int)eco;
            T(W * .15f, 240, W * .7f, 260, sum, 22, Color.white, TextAnchor.UpperCenter);
            if (Btn(cx, 540, "Yeniden başla  (" + Kn(K.Restart) + ")", 280)) NewGame(false);
            if (Btn(cx, 592, "Ana menü", 280)) NewGame(true);
            return;
        }

        // ---- ekran efektleri ----
        Vig(W, Color.black, .35f + (maskT > 0 ? .35f : 0));
        Vig(W, new Color(.4f, .9f, .1f), tox / 110f);
        Vig(W, new Color(1f, .9f, 0f), rad / 140f * (.7f + .3f * Mathf.Sin(Time.time * 8)));
        Vig(W, new Color(.9f, 0, 0), flash * .7f + (health < 30 ? (.25f + .15f * Mathf.Sin(Time.time * 5)) : 0));

        // ---- HUD ----
        Panel(new Rect(8, 8, 244, 188), "DURUM", new Color(.4f, .8f, .4f));
        Bar(14, 34, 232, health / 100, new Color(.85f, .2f, .2f), "CAN " + (int)health);
        Bar(14, 56, 232, thirst / 100, new Color(.2f, .5f, .95f), "SU " + (int)thirst);
        Bar(14, 78, 232, tox / 100, new Color(.4f, .8f, .2f), "ZEHİR " + (int)tox + (tox > 60 ? "  !" : ""));
        Bar(14, 100, 232, rad / 100, new Color(.95f, .85f, .1f), "RADYASYON " + (int)rad + (inZone ? "  (BÖLGEDE)" : ""));
        Color bc = budget > 20 ? Color.green : budget > 0 ? Color.yellow : Color.red;
        T(14, 124, 236, 22, "KARBON BÜTÇESİ: " + budget.ToString("0.0") + " kg", 15, bc);
        if (budget < 0) T(14, 144, 236, 20, "ZEHİRLİ SİS! Geri dönüştür, fidan dik.", 12, Color.red);
        else if (maskT > 0) T(14, 144, 236, 20, "Maske aktif: " + (int)maskT + " sn", 12, Color.cyan);
        T(14, 168, 236, 22, "Eko puan: " + (int)eco + "   " + Ranks[rankIdx].name, 13, Color.white);

        if (showQuests)
        {
            string g;
            if (phase == 1)
                g = G(collected >= 5, "Yerden 5 parça topla (" + Mathf.Min(collected, 5) + "/5)") + "\n" + G(madeFilter, Kn(K.Craft) + ": el kitabıyla Su Filtresi üret") + "\n" + G(benchCrafts >= 1, "Turuncu tezgâhta 1 üretim yap (karbon -%40)") + "\n" + G(drank, Kn(K.Drink) + ": su iç");
            else
                g = G(ValvesClosed() == facs.Count, "Atık vanalarını kapat (" + ValvesClosed() + "/" + facs.Count + ")  [" + Kn(K.Interact) + " basılı tut]") + "\n" + G(trees >= 3, "3 fidan dik (" + Mathf.Min(trees, 3) + "/3)") + "\n" + G(solar >= 1, "1 güneş paneli kur") + "\n" + G(purity >= 70, "Gölü %70 temizle (%" + (int)purity + ")");
            Panel(new Rect(W - 338, 8, 330, 112), (phase == 1 ? "GÖREVLER - FAZ 1 ÖĞRETİCİ" : "GÖREVLER - FAZ 2 AÇIK DÜNYA") + "  [" + Kn(K.Quest) + "]", new Color(1f, .8f, .2f));
            T(W - 330, 32, 318, 90, g, 13, Color.white);
        }
        else T(W - 338, 8, 330, 20, "[" + Kn(K.Quest) + "] Görevleri göster", 12, new Color(.8f, .8f, .8f), TextAnchor.UpperRight);

        if (showInv)
        {
            int cnt = 0; foreach (var kv in inv) if (kv.Value > 0) cnt++;
            float iy = 130; Panel(new Rect(W - 238, iy, 230, 30 + 20 * cnt), "ÇANTA " + Total() + "/" + Cap + "  [" + Kn(K.Inv) + "]", new Color(.5f, .7f, 1f)); iy += 26;
            foreach (var kv in inv)
            {
                if (kv.Value <= 0) continue;
                Box(new Rect(W - 230, iy + 3, 12, 12), DB.I[kv.Key].color); T(W - 214, iy, 200, 20, DB.I[kv.Key].name + "  x" + kv.Value, 13, Color.white); iy += 20;
            }
        }

        if (showMap) MiniMap(W);

        // ---- banner / mesaj / istem ----
        if (bannerT > 0) T(0, 150, W, 60, banner, 40, new Color(1, 1, 1, Mathf.Clamp01(bannerT)), TextAnchor.UpperCenter);
        if (msgT > 0) T(W * .2f, 600, W * .6f, 40, msg, 17, Color.white, TextAnchor.UpperCenter);
        if (tgt != null && !craftOpen)
        {
            string pr = "", ik = "[" + Kn(K.Interact) + "] ";
            switch (tgt.kind)
            {
                case Kind.Pickup: pr = ik + "Topla: " + DB.I[tgt.item].name; break;
                case Kind.Crate: pr = ik + "Kasayı aç"; break;
                case Kind.Bench: pr = ik + "Tezgâhı kullan"; break;
                case Kind.Lake: pr = purity >= 70 ? ik + "Temiz gölden iç" : ik + "Boş PET şişeye su doldur (arıtılmalı)"; break;
                case Kind.Valve: pr = "[" + Kn(K.Interact) + " basılı tut] Atık vanasını kapat"; break;
            }
            T(0, 555, W, 30, pr, 20, Color.yellow, TextAnchor.UpperCenter);
            if (tgt.kind == Kind.Valve && tgt.progress > 0) { Box(new Rect(cx - 100, 590, 200, 12), new Color(0, 0, 0, .7f)); Box(new Rect(cx - 98, 592, 196 * tgt.progress, 8), Color.green); }
        }
        T(12, 694, W - 200, 22, "Esc menü · " + Kn(K.Help) + " yardım · " + Kn(K.Cam) + " kamera · " + Kn(K.Interact) + " etkileşim · " + Kn(K.Craft) + " üretim · " + Kn(K.Drink) + " su · " + Kn(K.Heal) + " bandaj · " + Kn(K.Mask) + " maske · " + Kn(K.Plant) + " fidan · " + Kn(K.Solar) + " panel", 12, new Color(.85f, .85f, .85f));
        Box(new Rect(cx - 2, 358, 4, 4), new Color(1, 1, 1, .8f));

        if (craftOpen) CraftPanel(W);
        if (st == St.Pause) PauseScreen(W, cx);
    }

    void MiniMap(float W)
    {
        float ms = 130, mx = W - ms - 12, my = 720 - ms - 36; float yaw = pl.eulerAngles.y * Mathf.Deg2Rad;
        Box(new Rect(mx - 2, my - 2, ms + 4, ms + 4), new Color(0, 0, 0, .8f)); Box(new Rect(mx, my, ms, ms), new Color(.1f, .13f, .1f, .85f));
        if (benchT) MapDot(benchT.position, new Color(1f, .5f, .1f), 6, mx, my, ms, yaw);
        foreach (var n in nodes) if (n.kind == Kind.Crate && !n.done && n.t) MapDot(n.t.position, new Color(.1f, .8f, .8f), 4, mx, my, ms, yaw);
        if (phase == 2)
        {
            foreach (var n in nodes) if (n.kind == Kind.Lake && n.t) MapDot(n.t.position, new Color(.3f, .6f, 1f), 9, mx, my, ms, yaw);
            foreach (var f in facs) MapDot(f.root.position, f.active ? Color.red : Color.green, 8, mx, my, ms, yaw);
            foreach (var z in radZones) MapDot(z.position, Color.yellow, 7, mx, my, ms, yaw);
        }
        Box(new Rect(mx + ms / 2 - 3, my + ms / 2 - 3, 6, 6), Color.white);
        T(mx, my + ms + 2, ms, 18, "HARİTA [" + Kn(K.Map) + "]  ^ ileri", 11, new Color(.8f, .8f, .8f));
    }

    void CraftPanel(float W)
    {
        float x = W / 2 - 330, y = 90, w = 660, h = 40 + DB.R.Count * 46 + 30;
        Box(new Rect(x, y, w, h), new Color(0, 0, 0, .9f)); Box(new Rect(x, y, w, 30), new Color(.1f, .3f, .15f, .95f));
        bool nb = NearBench();
        T(x + 12, y + 5, w, 24, "COP31 EL KİTABI - ÜRETİM / GERİ DÖNÜŞÜM   " + (nb ? "(tezgâh: karbon -%40)" : "(tezgâhtan uzaksın)"), 15, nb ? Color.green : Color.white);
        float ry = y + 40;
        foreach (var r in DB.R)
        {
            string need = ""; foreach (var i in r.inp) need += DB.I[i.id].name + " " + Cnt(i.id) + "/" + i.n + "   ";
            float c = CostOf(r); bool can = CanCraft(r);
            T(x + 12, ry, 520, 20, r.name + "  ->  " + DB.I[r.outId].name + (r.outN > 1 ? " x" + r.outN : ""), 14, can ? Color.white : new Color(.6f, .6f, .6f));
            T(x + 12, ry + 20, 520, 20, need + "| CO2 " + (c < 0 ? "+" + (-c).ToString("0.0") + " kredi" : "-" + c.ToString("0.00") + " kg"), 12, c < 0 ? Color.green : new Color(1f, .75f, .4f));
            GUI.enabled = can; if (GUI.Button(new Rect(x + w - 110, ry + 4, 96, 32), "Üret")) Craft(r); GUI.enabled = true;
            ry += 46;
        }
        T(x + 12, ry + 4, w, 22, Kn(K.Craft) + " ile kapat. Karbon bütçen < 0 olursa zehirli sis seni öldürür.", 12, Color.yellow);
    }

    // ------------------------------ MENÜLER ------------------------------
    void MenuScreen(float W, float cx)
    {
        Box(new Rect(0, 0, W, 720), new Color(0, 0, 0, .45f));
        if (scr == Scr.Main)
        {
            T(0, 90, W, 70, "LAST RESOURCE", 56, new Color(.6f, 1f, .6f), TextAnchor.UpperCenter);
            T(0, 160, W, 30, "COP31 · Karbon ayak izi · Hayatta kalma prototipi", 18, Color.white, TextAnchor.UpperCenter);
            if (Btn(cx, 260, "OYNA")) { st = St.Story; page = 0; }
            if (Btn(cx, 314, "NASIL OYNANIR")) scr = Scr.Help;
            if (Btn(cx, 368, "TUŞ ATAMALARI")) scr = Scr.Keys;
            if (Btn(cx, 422, "ÇIKIŞ")) Quit();
            T(0, 680, W, 24, "V1.1 Prototype · Unity 6", 12, new Color(.7f, .7f, .7f), TextAnchor.UpperCenter);
        }
        else SubScreen(W, cx, false);
    }

    void PauseScreen(float W, float cx)
    {
        Box(new Rect(0, 0, W, 720), new Color(0, 0, 0, .7f));
        if (scr == Scr.Main)
        {
            T(0, 90, W, 60, "DURAKLATILDI", 44, Color.white, TextAnchor.UpperCenter);
            if (Btn(cx, 190, "DEVAM ET  (Esc)")) st = St.Play;
            if (Btn(cx, 244, "YARDIM / NASIL OYNANIR")) scr = Scr.Help;
            if (Btn(cx, 298, "TUŞ ATAMALARI")) scr = Scr.Keys;
            if (Btn(cx, 352, showQuests ? "GÖREV PANELİ: AÇIK" : "GÖREV PANELİ: KAPALI")) showQuests = !showQuests;
            if (Btn(cx, 406, thirdPerson ? "KAMERA: 3. KİŞİ" : "KAMERA: 1. KİŞİ")) thirdPerson = !thirdPerson;
            if (Btn(cx, 460, "YENİDEN BAŞLA")) NewGame(false);
            if (Btn(cx, 514, "ANA MENÜ")) NewGame(true);
            if (Btn(cx, 568, "ÇIKIŞ")) Quit();
        }
        else SubScreen(W, cx, true);
    }

    void SubScreen(float W, float cx, bool pause)
    {
        if (scr == Scr.Help)
        {
            T(0, 30, W, 50, "NASIL OYNANIR", 36, new Color(.6f, 1f, .6f), TextAnchor.UpperCenter);
            float y = 90, x = cx - 400;
            foreach (var r in HelpRows)
            {
                Box(new Rect(x, y, 800, 44), new Color(1, 1, 1, .06f)); T(x + 10, y + 11, 170, 24, r.h, 15, Color.yellow); T(x + 180, y + 3, 610, 44, r.t, 14, Color.white); y += 47;
            }
            T(0, y + 6, W, 24, "Yürü: WASD · Bak: fare · Esc: menü · Tüm diğer tuşlar 'Tuş atamaları' ekranında", 14, new Color(.8f, .8f, .8f), TextAnchor.UpperCenter);
            if (Btn(cx, 640, "GERİ", 200)) scr = Scr.Main;
        }
        else
        {
            T(0, 30, W, 50, "TUŞ ATAMALARI", 36, new Color(.6f, 1f, .6f), TextAnchor.UpperCenter);
            float y = 90, x = cx - 300;
            for (int i = 0; i < Inp.NB; i++)
            {
                Box(new Rect(x, y, 600, 34), new Color(1, 1, 1, i % 2 == 0 ? .06f : .02f));
                T(x + 12, y + 6, 340, 24, Inp.Names[i], 15, Color.white);
                if (GUI.Button(new Rect(x + 400, y + 2, 190, 30), waitKey == i ? "tuşa bas..." : Inp.bind[i].ToString())) waitKey = i;
                y += 36;
            }
            T(0, y + 8, W, 24, "Sabit: WASD hareket · Fare bak · Esc menü · Enter hikaye · R yeniden başla. Bir tuşa tıkla, sonra yeni tuşa bas (Esc = iptal).", 13, new Color(.8f, .8f, .8f), TextAnchor.UpperCenter);
            if (Btn(cx - 120, 640, "VARSAYILAN", 220)) Inp.Reset();
            if (Btn(cx + 120, 640, "GERİ", 220)) scr = Scr.Main;
        }
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

// =====================================================================
// V2-HOOK : CANAVAR FİKİRLERİ (fabrika içi, vana bulmacası)
//  Çamur Sürüngeni  - kirli su birikintilerinden çıkar, yavaş, zehirli temas
//  Duman Hayaleti   - sisle görünür, maske filtresi tüketir
//  Hurda Golem      - atık yığınlarından yapılı, sese tepki verir
//  Radyasyon Böceği - sürü halinde, radyasyon bölgelerinde çoğalır
//  Tasarım: vana başına bir canavar "bekçi"; yakalanırsan zehir +40.
// =====================================================================

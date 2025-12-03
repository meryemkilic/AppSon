using System;
using System.Collections.Generic;

namespace SpeechTherapy.Data
{
    // ----------------------------------------------------------------------
    // 1. AUTHENTICATION (Giriş)
    // ----------------------------------------------------------------------

    /// <summary>
    /// API'den gelen Giriş Cevabı yapısı.
    /// </summary>
    [Serializable]
    public class AuthResponse
    {
        public string Token;
        public string UserId;
        public string Username;
    }

    // ----------------------------------------------------------------------
    // 2. MENU VE LEVEL SEÇİMİ
    // ----------------------------------------------------------------------

    /// <summary>
    /// Harf Seçim Ekranı (MenuScene) için harf verisi.
    /// </summary>
    [Serializable]
    public class LetterItem
    {
        public string Char;
        public int Stars; // Toplam kazanılan yıldız
        public bool IsLocked;
    }

    /// <summary>
    /// Oyun Seçim Ekranı (LetterScene) için seviye verisi.
    /// </summary>
    [Serializable]
    public class GameLevelItem
    {
        public string GameId;
        public string Name;
        public int DifficultyLevel;
        public bool IsAssignedTask; // Terapist tarafından atanmış ödev rozeti için
        public bool IsLocked;
    }

    // ----------------------------------------------------------------------
    // 3. OYUN ASSETLERİ (Hafıza Oyunu Verisi)
    // ----------------------------------------------------------------------

    /// <summary>
    /// Tek bir oyun kartını veya varlığını (Asset) temsil eder. 
    /// Veriler API'den URL olarak gelir.
    /// </summary>
    [Serializable]
    public class AssetItem
    {
        public string Id;
        public string TextContent; // Kart üzerinde yazacak metin (Örn: KEDİ)
        public string ImageUrl;    // Kart ön yüz resmi (URL)
        public string SoundUrl;    // Kart sesi (URL)
        public bool IsTarget;      // Oyun hedef kelimesi mi? (İleride kullanılacak)
    }

    /// <summary>
    /// API'den gelen ve bir oyun seviyesi için gereken tüm varlıkları (Asset'leri) kapsayan ana yapı.
    /// </summary>
    [Serializable]
    public class AssetSetResponse
    {
        public string SetId;
        public string Letter;
        public string Type; // word, syllable, sentence
        public List<AssetItem> Assets; // Oyunda kullanılacak kart çifti sayısı kadar AssetItem (Hafıza Oyunu için 3 çift kart için 3 adet veri)
    }
}
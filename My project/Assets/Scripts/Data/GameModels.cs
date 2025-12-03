using System;
using System.Collections.Generic;
using Newtonsoft.Json; // Unity Package Manager'dan indirdiğimiz kütüphane

namespace SpeechTherapy.Data
{
    /// <summary>
    /// Backend'den gelen Login cevabı.
    /// </summary>
    [Serializable]
    public class AuthResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; } // JWT Token

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

    /// <summary>
    /// Menüde listelenecek her bir oyun satırı.
    /// Hem normal oyunları hem de Terapistin atadığı ÖDEVLERİ temsil eder.
    /// </summary>
    [Serializable]
    public class GameLevelItem
    {
        [JsonProperty("game_id")]
        public string GameId { get; set; } // Örn: "shadow_game", "matching_game"

        [JsonProperty("name")]
        public string Name { get; set; } // Örn: "Gölge Bulmaca"

        [JsonProperty("difficulty_level")]
        public int DifficultyLevel { get; set; } // 1: Kolay, 2: Orta, 3: Zor

        [JsonProperty("is_locked")]
        public bool IsLocked { get; set; } // Önceki seviye bitmediyse kilitli

        // 🌟 KRİTİK: Terapist bunu ödev olarak atadı mı?
        // UI'da üzerinde küçük bir "Rozet" çıkmasını sağlayacak.
        [JsonProperty("is_assigned_task")]
        public bool IsAssignedTask { get; set; } 
    }

    /// <summary>
    /// Bir oyun seçildiğinde Backend'den indirilecek İÇERİK PAKETİ.
    /// (Render Database mimarisine uygun)
    /// </summary>
    [Serializable]
    public class AssetSetResponse
    {
        [JsonProperty("set_id")]
        public string SetId { get; set; } // Örn: "set_k_word_medium"

        [JsonProperty("letter")]
        public string Letter { get; set; } // "K"

        [JsonProperty("type")]
        public string Type { get; set; } // "word", "syllable"

        [JsonProperty("assets")]
        public List<AssetItem> Assets { get; set; } // Kartların listesi
    }

    /// <summary>
    /// Tek bir oyun kartının verisi (Kedi, Araba vb.)
    /// </summary>
    [Serializable]
    public class AssetItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text_content")]
        public string TextContent { get; set; } // "KEDİ"

        // Backend bize tam URL verecek (https://.../kedi.png)
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; } 

        [JsonProperty("audio_url")]
        public string AudioUrl { get; set; }

        [JsonProperty("is_target")]
        public bool IsTarget { get; set; } // Doğru cevap bu mu?
    }

    
}

/// <summary>
    /// Ana menüde görülecek Harf kutusu.
    /// </summary>
    [Serializable]
    public class LetterItem
    {
        [JsonProperty("char")]
        public string Char { get; set; } // "A", "B", "K"

        [JsonProperty("is_locked")]
        public bool IsLocked { get; set; } // Kilitli mi?

        [JsonProperty("stars")]
        public int Stars { get; set; } // O harfteki genel başarısı (0-3)
    }
using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks; 
using Newtonsoft.Json;         
using SpeechTherapy.Data;      
using UnityEngine;
using UnityEngine.Networking;  

namespace SpeechTherapy.Services
{
    public class ApiDataService : MonoBehaviour
    {
        [Header("Geliştirici Ayarları")]
        [SerializeField] private bool _useMockData = true; // Simülasyon Şalteri
        
        private const string BASE_URL = "https://senin-backend-adresin.onrender.com/api";
        private string _jwtToken;

        // -------------------------------------------------------------------------
        // 1. LOGIN
        // -------------------------------------------------------------------------
        public async UniTask<AuthResponse> Login(string username, string password)
        {
            if (_useMockData)
            {
                // 🎭 MOCK DATA RETURN
                await UniTask.Delay(500);
                Debug.Log($"🎭 [MOCK] Giriş yapıldı: {username}");
                
                return new AuthResponse 
                { 
                    Token = "fake_jwt_token_123456", 
                    UserId = "user_can_101", 
                    Username = username 
                };
            }

            // 🌐 GERÇEK BAĞLANTI
            var url = $"{BASE_URL}/auth/login";
            var bodyData = new { username, password };
            string jsonBody = JsonConvert.SerializeObject(bodyData);

            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) throw new Exception(request.error);

                var authData = JsonConvert.DeserializeObject<AuthResponse>(request.downloadHandler.text);
                _jwtToken = authData.Token;
                return authData; // GERÇEK RETURN
            }
        }

        // -------------------------------------------------------------------------
        // 2. OYUN LİSTESİ ÇEKME (ÖDEV MANTIĞI BURADA)
        // -------------------------------------------------------------------------
        public async UniTask<GameLevelItem[]> GetGamesForLetter(string letter, string type)
        {
            if (_useMockData)
            {
                await UniTask.Delay(500);
                Debug.Log($"🎭 [MOCK] Oyun listesi oluşturuluyor: {letter} - {type}");
                
                // 🌟 MOCK DATA RETURN
                return new GameLevelItem[]
                {
                    new GameLevelItem 
                    { 
                        GameId = "matching_game_easy", 
                        Name = "Eşleştirme (Kolay)", 
                        DifficultyLevel = 1, 
                        IsLocked = false, 
                        IsAssignedTask = false 
                    },
                    new GameLevelItem 
                    { 
                        GameId = "shadow_game_medium", 
                        Name = "Gölge Bulmaca (Orta)", 
                        DifficultyLevel = 2, 
                        IsLocked = false, 
                        IsAssignedTask = true // TERAPİST ÖDEVİ
                    },
                    new GameLevelItem 
                    { 
                        GameId = "driving_game_hard", 
                        Name = "Araba Sürme (Zor)", 
                        DifficultyLevel = 3, 
                        IsLocked = true, 
                        IsAssignedTask = false 
                    }
                };
            }

            // 🌐 GERÇEK BAĞLANTI
            var url = $"{BASE_URL}/games?letter={letter}&type={type}";
            using (var request = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(_jwtToken)) request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success) throw new Exception(request.error);
                
                return JsonConvert.DeserializeObject<GameLevelItem[]>(request.downloadHandler.text); // GERÇEK RETURN
            }
        }

        // -------------------------------------------------------------------------
        // 3. ASSET SETİ İNDİRME
        // -------------------------------------------------------------------------
        public async UniTask<AssetSetResponse> GetAssetSet(string setId)
        {
            if (_useMockData)
            {
                await UniTask.Delay(1000);
                Debug.Log($"🎭 [MOCK] Asset Seti hazırlanıyor: {setId}");

                // 🌟 MOCK DATA RETURN
                return new AssetSetResponse
                {
                    SetId = setId,
                    Letter = "K",
                    Type = "word",
                    Assets = new List<AssetItem>
                    {
                        new AssetItem { Id = "asset_1", TextContent = "KEDİ", ImageUrl = "https://placehold.co/200x200/png?text=Kedi", IsTarget = true },
                        new AssetItem { Id = "asset_2", TextContent = "KALE", ImageUrl = "https://placehold.co/200x200/png?text=Kale", IsTarget = false }
                    }
                };
            }

            // 🌐 GERÇEK BAĞLANTI
            var url = $"{BASE_URL}/assets/{setId}";
            using (var request = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(_jwtToken)) request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success) throw new Exception(request.error);
                
                return JsonConvert.DeserializeObject<AssetSetResponse>(request.downloadHandler.text); // GERÇEK RETURN
            }
        }

        // -------------------------------------------------------------------------
        // 4. HARF LİSTESİ ÇEKME
        // -------------------------------------------------------------------------
        public async UniTask<LetterItem[]> GetAvailableLetters()
        {
            if (_useMockData)
            {
                await UniTask.Delay(100);
                Debug.Log("🎭 [MOCK] Tüm harfler açık olarak listeleniyor...");

                var mockList = new List<LetterItem>();
                string alphabet = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ";
                
                foreach (char c in alphabet)
                {
                    mockList.Add(new LetterItem 
                    { 
                        Char = c.ToString(), 
                        IsLocked = false, 
                        Stars = UnityEngine.Random.Range(0, 4) 
                    });
                }

                return mockList.ToArray(); // MOCK RETURN
            }

            // 🌐 GERÇEK BAĞLANTI
            var url = $"{BASE_URL}/letters"; 
            
            using (var request = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(_jwtToken)) request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success) 
                {
                     throw new Exception("Harf listesi alınamadı: " + request.error);
                }

                return JsonConvert.DeserializeObject<LetterItem[]>(request.downloadHandler.text); // GERÇEK RETURN
            }
        }
    }
}
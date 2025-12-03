using UnityEngine;
using Cysharp.Threading.Tasks; 
using SpeechTherapy.Data;      
using SpeechTherapy.Services;  
using System.Collections.Generic; // List için eklendi

namespace SpeechTherapy.Core // Eğer klasör yapınızdan dolayı Managers kullanıyorsanız, onu koruyun.
{
    public class GameConfigManager : MonoBehaviour
    {
        public static GameConfigManager Instance { get; private set; }

        private ApiDataService _apiService;

        // --- STATE ---
        public AuthResponse CurrentUser { get; private set; }
        public string CurrentLetter { get; private set; }
        public string CurrentFloorType { get; private set; }
        public AssetSetResponse ActiveAssetSet { get; private set; }

        private void Awake()
        {
            // 1. Singleton Kurulumu
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 2. DEPENDENCY COMPOSITION
            Debug.Log("⚙️ Sistem başlatılıyor: Servisler enjekte ediliyor...");
            _apiService = gameObject.AddComponent<ApiDataService>();
            
            if (_apiService == null)
            {
                Debug.LogError("🚨 KRİTİK HATA: ApiDataService oluşturulamadı!");
            }
        }

        // Seçilen harfi hafızaya kaydeder
        public void SetSelectedLetter(string letter)
        {
            CurrentLetter = letter;
            Debug.Log($"Patron: Seçilen harf hafızaya alındı -> {CurrentLetter}");
        }

        public async UniTask<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                CurrentUser = await _apiService.Login(username, password);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Patron: Giriş başarısız! - {ex.Message}");
                return false;
            }
        }

        // 🌟 DÜZELTİLDİ: Bu metot artık sınıfın içinde.
        public async UniTask<LetterItem[]> GetAvailableLetters()
        {
            try
            {
                return await _apiService.GetAvailableLetters();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Patron: Harfler alınamadı! - {ex.Message}");
                return null;
            }
        }

        public async UniTask<GameLevelItem[]> GetLevelList(string letter, string floorType)
        {
            CurrentLetter = letter;
            CurrentFloorType = floorType;

            try
            {
                return await _apiService.GetGamesForLetter(letter, floorType);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Patron: Liste çekilemedi! - {ex.Message}");
                return null;
            }
        }

        public async UniTask<bool> PrepareGameSession(string gameId)
        {
            try
            {
                ActiveAssetSet = await _apiService.GetAssetSet(gameId);
                
                if (ActiveAssetSet != null && ActiveAssetSet.Assets.Count > 0)
                {
                    Debug.Log($"Patron: {gameId} yüklendi. {ActiveAssetSet.Assets.Count} asset hazır.");
                    return true;
                }
                
                Debug.LogWarning("Patron: Asset seti boş!");
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Patron: Oyun hazırlanamadı! - {ex.Message}");
                return false;
            }
        }
    }
}
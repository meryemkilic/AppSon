using UnityEngine;
using Cysharp.Threading.Tasks; 
using SpeechTherapy.Data;      
using SpeechTherapy.Services;  

namespace SpeechTherapy.Core
{
    public class GameConfigManager : MonoBehaviour
    {
        public static GameConfigManager Instance { get; private set; }

        // Inspector'dan gizledik, çünkü kodla yöneteceğiz.
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

            // 2. DEPENDENCY COMPOSITION (Bağımlılık Kurulumu)
            // GetComponent kullanmıyoruz. 
            // AddComponent hem ekler hem referansı döndürür. Bellek dostudur.
            Debug.Log("⚙️ Sistem başlatılıyor: Servisler enjekte ediliyor...");
            
            _apiService = gameObject.AddComponent<ApiDataService>();
            
            // Eğer servis eklenemezse null döner, kontrol edelim (Opsiyonel ama güvenli)
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
            // Servis referansımız garanti, direkt kullanıyoruz.
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
    }// Assets/_Game/Scripts/Services/ApiDataService.cs içindeki ilgili metot:

        public async UniTask<LetterItem[]> GetAvailableLetters()
        {
            if (_useMockData)
            {
                await UniTask.Delay(100);
                Debug.Log("🎭 [MOCK] Tüm harfler açık olarak listeleniyor...");

                // Basit bir döngü ile A'dan Z'ye harf üretelim
                // Hepsi KİLİTSİZ (IsLocked = false)
                var mockList = new List<LetterItem>();
                string alphabet = "BCÇDFGĞHKLMNPRSŞTVYZ";
                
                foreach (char c in alphabet)
                {
                    mockList.Add(new LetterItem 
                    { 
                        Char = c.ToString(), 
                        IsLocked = false, // Hepsini açtık
                        Stars = UnityEngine.Random.Range(0, 4) // Rastgele yıldız (0-3 arası)
                    });
                }

                return mockList.ToArray();
            }

            // ... (Gerçek bağlantı kısmı aynı kalacak) ...
             var url = $"{BASE_URL}/letters"; 
            // ...
        }

        
}
using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask

namespace SpeechTherapy.Core
{
    public class ConsoleTester : MonoBehaviour
    {
        // Unity "Play" tuşuna basınca otomatik çalışır
        private void Start()
        {
            // Testi asenkron başlatmak için fire-and-forget yapıyoruz
            RunTests().Forget();
        }

        private async UniTaskVoid RunTests()
        {
            // Biraz bekleyelim ki diğer scriptler (Awake) kurulsun
            await UniTask.Delay(1000); 

            Debug.Log("🧪 TEST BAŞLIYOR...");

            var manager = GameConfigManager.Instance;

            // 1. TEST: Login Ol
            // (Buraya gerçek backendindeki geçerli bir kullanıcıyı yazmalısın)
            // Eğer backendin yoksa ApiDataService hata verecek, bu normal.
            bool loginSuccess = await manager.AuthenticateUser("testuser", "123456");

            if (!loginSuccess) 
            {
                Debug.LogError("❌ Login Testi Başarısız! (Backend ayakta mı?)");
                return;
            }
            Debug.Log("✅ Login Testi Başarılı!");

            // 2. TEST: Oyun Listesi Çek
            // Senaryo: K Harfi, Kelime Seviyesi
            var games = await manager.GetLevelList("K", "word");

            if (games != null)
            {
                Debug.Log($"✅ Oyun Listesi Alındı! Toplam Oyun: {games.Length}");
                foreach (var game in games)
                {
                    string status = game.IsAssignedTask ? " [ÖDEV 🌟]" : "";
                    Debug.Log($"   - {game.Name} (Zorluk: {game.DifficultyLevel}){status}");
                }
            }
            else
            {
                Debug.LogError("❌ Oyun Listesi Alınamadı!");
            }

            // 3. TEST: Asset Seti İndir
            // Mock bir ID ile deniyoruz
            bool isReady = await manager.PrepareGameSession("set_k_word_medium");

            if (isReady)
            {
                Debug.Log("✅ Asset Seti İndirildi! Oyun Sahnesine Geçilebilir.");
                Debug.Log($"   -> İlk Asset Resmi: {manager.ActiveAssetSet.Assets[0].ImageUrl}");
            }
            else
            {
                Debug.LogError("❌ Asset Seti İndirilemedi!");
            }

            Debug.Log("🏁 TEST TAMAMLANDI.");
        }
    }
}
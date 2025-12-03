using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using SpeechTherapy.Core; 
using SpeechTherapy.Data;     
using UnityEngine.SceneManagement;

namespace SpeechTherapy.UI
{
    public class LetterLevelController : MonoBehaviour
    {
        private UIDocument _doc;
        
        // UXML Elemanları
        private Label _lblHeader;
        private Button _tabSyllable, _tabWord, _tabSentence;
        private VisualElement _listContainer;
        private Button _btnBack;
        
        // ❌ KALDIRILDI: Floor görsel değişkenleri
        // private VisualElement _visualFloor; 
        // private Label _lblTheme;

        [SerializeField] private VisualTreeAsset _gameItemTemplate;

        private string _currentLetter;
        private string _currentFloorType = "syllable"; // Varsayılan: Hece

        private void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            var root = _doc.rootVisualElement;

            // 1. Elemanları Bul
            _lblHeader = root.Q<Label>("lbl-header");
            _tabSyllable = root.Q<Button>("tab-syllable");
            _tabWord = root.Q<Button>("tab-word");
            _tabSentence = root.Q<Button>("tab-sentence");
            _listContainer = root.Q<VisualElement>("list-container");
            _btnBack = root.Q<Button>("btn-back");
            
            // ❌ KALDIRILDI: Floor görsel element bulma satırları
            // _visualFloor = root.Q<VisualElement>("visual-floor");
            // _lblTheme = root.Q<Label>("lbl-theme");

            // 2. State'i Al (Güvenli erişim)
            if (GameConfigManager.Instance != null)
            {
                _currentLetter = GameConfigManager.Instance.CurrentLetter;
            }
            
            if (string.IsNullOrEmpty(_currentLetter)) 
            {
                _currentLetter = "A"; 
            }

            _lblHeader.text = $"{_currentLetter} Harfi - Oyunlar"; 

            // 3. Tab Eventlerini Bağla
            _tabSyllable.clicked += () => SetActiveFloor("syllable");
            _tabWord.clicked += () => SetActiveFloor("word");
            _tabSentence.clicked += () => SetActiveFloor("sentence");
            _btnBack.clicked += OnBackClicked;

            // 4. Varsayılan Listeyi Yükle
            SetActiveFloor("syllable");
        }

        private void OnBackClicked()
        {
            SceneManager.LoadScene("MenuScene");
        }

        private void SetActiveFloor(string floorType)
        {
            _currentFloorType = floorType;
            Debug.Log($"Sekme Değişti: {_currentLetter} harfi için {floorType} yükleniyor.");

            // Sekme Butonlarının Görsel Yönetimi
            Color activeColor = new Color32(255, 107, 107, 255); 
            Color inactiveColor = new Color32(136, 136, 136, 255);
            
            // 💡 DÜZELTME: Tüm butonların rengi aktifliğe göre ayarlanır. UXML'de kalan inline stil yok edildiği için artık düzgün çalışacaktır.

            // HECE Sekmesi
            _tabSyllable.style.backgroundColor = floorType == "syllable" ? activeColor : StyleKeyword.Null;
            _tabSyllable.style.color = floorType == "syllable" ? Color.white : inactiveColor;
            
            // KELİME Sekmesi
            _tabWord.style.backgroundColor = floorType == "word" ? activeColor : StyleKeyword.Null;
            _tabWord.style.color = floorType == "word" ? Color.white : inactiveColor;

            // CÜMLE Sekmesi
            _tabSentence.style.backgroundColor = floorType == "sentence" ? activeColor : StyleKeyword.Null;
            _tabSentence.style.color = floorType == "sentence" ? Color.white : inactiveColor;

            // ❌ KALDIRILDI: Floor görseli güncelleme satırları

            // Yeni listeyi çek
            LoadGameList().Forget();
        }

        // ❌ KALDIRILDI: GetFloorColor metodu

        private async UniTaskVoid LoadGameList()
        {
            _listContainer.Clear(); 

            GameLevelItem[] games = await GameConfigManager.Instance.GetLevelList(_currentLetter, _currentFloorType);

            if (games == null || games.Length == 0)
            {
                _listContainer.Add(new Label("Bu seviyede henüz oyun yok.") { style = { color = Color.gray, unityTextAlign = TextAnchor.MiddleCenter } });
                return;
            }

            // Oyunları ekrana bas
            foreach (var game in games)
            {
                if (_gameItemTemplate == null) break;

                TemplateContainer instance = _gameItemTemplate.Instantiate();
                var btnPlay = instance.Q<Button>("btn-play");
                var lblName = instance.Q<Label>("lbl-game-name");
                var lblStars = instance.Q<Label>("lbl-stars");
                var badgeTask = instance.Q<VisualElement>("badge-task"); // ÖDEV rozeti

                lblName.text = game.Name;
                
                // Zorluk Yıldızları
                lblStars.text = GetStarString(game.DifficultyLevel);

                // 🌟 ÖDEV Rozeti Kontrolü
                badgeTask.style.display = game.IsAssignedTask ? DisplayStyle.Flex : DisplayStyle.None; 
                
                // Kilit Kontrolü
                if (game.IsLocked)
                {
                    btnPlay.SetEnabled(false);
                    btnPlay.text = "🔒 KİLİTLİ";
                }
                else
                {
                    // Tıklama Eventi: Oyunu Başlat
                    btnPlay.clicked += () => OnGameSelected(game);
                }

                _listContainer.Add(instance);
            }
        }

        // 1, 2 veya 3 yıldız görseli oluşturur
        private string GetStarString(int difficulty)
        {
            string stars = "";
            for (int i = 1; i <= 3; i++)
            {
                stars += i <= difficulty ? "★" : "☆";
            }
            return stars;
        }

        private async void OnGameSelected(GameLevelItem game)
        {
            Debug.Log($"🎮 Oyun Başlatılıyor: {game.Name} (Set ID: {game.GameId})");
            
            // 1. Asset Setini İndir (Patron'a emir ver)
            bool ready = await GameConfigManager.Instance.PrepareGameSession(game.GameId);

            if (ready)
            {
                // 2. İndirme başarılıysa Game Scene'e geç
                Debug.Log("🎉 Assetler hazır. Oyun Sahnesi Yükleniyor...");
                // İleride buraya: SceneManager.LoadScene("GameScene");
            }
            else
            {
                // Hata mesajııı
            
                Debug.LogError("Oyun içeriği yüklenemedi. Lütfen tekrar deneyin.");
            }
        }
    }
}
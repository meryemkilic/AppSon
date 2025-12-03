using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SpeechTherapy.Core; 
using SpeechTherapy.Data;
using DG.Tweening;
using System.Linq;

namespace SpeechTherapy.Games.Memory
{
    public class MemoryGameUIController : MonoBehaviour
    {
        private UIDocument _doc;
        
        [Header("UI Toolkit Atamaları")]
        [SerializeField] private VisualTreeAsset _cardUxmlTemplate; 

        private Button _btnRestart;
        private Button _btnMainMenu;
        private VisualElement _cardContainer; 

        private GameManager _gameManager; 

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _gameManager = GameManager.Instance; 

            if (_doc == null || _gameManager == null)
            {
                Debug.LogError("🚨 MemoryGameUIController: UIDocument veya GameManager bulunamadı!");
                enabled = false; 
                return;
            }
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            _btnRestart = root.Q<Button>("btn-restart");
            _btnMainMenu = root.Q<Button>("btn-main-menu");
            _cardContainer = root.Q<VisualElement>("card-container"); 

            if (_btnRestart != null) _btnRestart.clicked += OnRestartClicked;
            if (_btnMainMenu != null) _btnMainMenu.clicked += OnMainMenuClicked;

            StartMemoryGame();
        }

        private void StartMemoryGame()
        {
            AssetSetResponse activeSet = GameConfigManager.Instance.ActiveAssetSet;

            if (activeSet == null || activeSet.Assets.Count == 0)
            {
                Debug.LogError("🚨 MemoryGame: Aktif Asset Seti (kart verisi) bulunamadı.");
                return;
            }

            _gameManager.cardContainerElement = _cardContainer;
            _gameManager.cardUxmlTemplate = _cardUxmlTemplate;
            
            int totalUniqueAssets = activeSet.Assets.Count;

            int gridHeight = 2; 
            int gridWidth = totalUniqueAssets; 

            Debug.Log($"🎉 MemoryGame Başlatılıyor: Grid {gridHeight}x{gridWidth}, {totalUniqueAssets} çift.");
            
            _gameManager.GenerateLevel(gridHeight, gridWidth, activeSet.Assets.ToArray());
        }

        private void OnRestartClicked()
        {
            // 💡 DÜZELTME: Hata vermeyen basit bir DOTween animasyonu kullanıldı.
            // Bu, MemoryGameUIController'ın bağlı olduğu GameObject'in transform'unu animasyona sokar.
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1)
                .OnComplete(() => _gameManager.RestartLevel());
        }

        private void OnMainMenuClicked()
        {
            DOTween.KillAll();
            SceneManager.LoadScene("MenuScene"); 
        }
        
        private void OnDisable()
        {
            if (_btnRestart != null) _btnRestart.clicked -= OnRestartClicked;
            if (_btnMainMenu != null) _btnMainMenu.clicked -= OnMainMenuClicked;
        }
    }
}
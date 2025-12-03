using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using SpeechTherapy.Core;
using SpeechTherapy.Data; // Modeller için

namespace SpeechTherapy.UI
{
    public class MainMenuController : MonoBehaviour
    {
        private UIDocument _doc;
        private VisualElement _gridContainer;
        
        // Template dosyasını Inspector'dan değil, Resources'tan yükleyeceğiz (Clean Code)
        // Ya da Inspector'a sürükleyebilirsin, ikisi de olur. Biz şimdilik Inspector kullanalım daha kolay.
        [SerializeField] private VisualTreeAsset _letterTemplate; 

        private void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            var root = _doc.rootVisualElement;
            _gridContainer = root.Q<VisualElement>("grid-container");

            // Ekran açılır açılmaz harfleri yükle
            LoadLetters().Forget();
        }

        private async UniTaskVoid LoadLetters()
        {
            // 1. Patron'dan listeyi iste
            // (GameConfigManager'a yeni metod eklemediğimiz için direkt servisten çekelim şimdilik,
            // ama doğrusu Manager üzerinden geçmektir. Hızlı test için böyle yapıyoruz.)
            // Doğrusu: var letters = await GameConfigManager.Instance.GetLetters();
            
            // Şimdilik Servise ulaşmak için küçük bir hile (Manager üzerinden servise erişim ekleyebilirsin)
            // Biz temiz olsun diye servisi buraya Dependency olarak alabiliriz veya Manager'a metod ekleriz.
            // Hadi Manager'a metod ekleyelim (Aşağıda anlatacağım).
            
            var letters = await GameConfigManager.Instance.GetAvailableLetters();

            if (letters == null) return;

            _gridContainer.Clear(); // Eskileri temizle

            // 2. Döngü ile butonları oluştur
            // Assets/_Game/Scripts/UI/MainMenuController.cs içindeki döngü:

            foreach (var item in letters)
            {
                TemplateContainer instance = _letterTemplate.Instantiate();
                
                var btn = instance.Q<Button>("btn-letter");
                var lblChar = instance.Q<Label>("lbl-char");
                var lblLock = instance.Q<Label>("lbl-lock");

                // Harfi yaz
                lblChar.text = item.Char;

                // --- REVİZE EDİLDİ: KİLİT MANTIĞI KALDIRILDI ---
                
                // Kilit ikonunu kesinlikle gizle (Template'de zaten gizli ama garanti olsun)
                if (lblLock != null) lblLock.style.display = DisplayStyle.None;
                
                // Rengi standart (Sarı) kalsın veya dinamik renk verebilirsin
                btn.SetEnabled(true); 

                // Her harf tıklanabilir
                btn.clicked += () => OnLetterClicked(item);
                
                _gridContainer.Add(instance);
            }
        }

        private void OnLetterClicked(LetterItem item)
        {
            Debug.Log($"Seçilen Harf: {item.Char}. Oyun Seçim ekranına gidiliyor...");
            
            // 1. Seçimi Patron'a bildir
            GameConfigManager.Instance.SetSelectedLetter(item.Char);
            
            // 2. Sahneye Geç (Sahne adını 'LetterScene' yapacağız)
            UnityEngine.SceneManagement.SceneManager.LoadScene("LetterScene");
        }
    }
}
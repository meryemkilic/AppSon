using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit kütüphanesi
using Cysharp.Threading.Tasks; // UniTask
using UnityEngine.SceneManagement; // 🎯 YENİ: SAHNE YÖNETİMİ İÇİN EKLENDİ
using SpeechTherapy.Core;      // GameConfigManager'a ulaşmak için

namespace SpeechTherapy.UI
{
    public class LoginController : MonoBehaviour
    {
        private UIDocument _doc;
        
        // UI Elemanları (UXML'deki isimlerle eşleşecek)
        private TextField _inputUsername;
        private TextField _inputPassword;
        private Button _btnLogin;
        private Button _btnRegister;
        private Label _lblError;
        private Label _lblLoading;

        // Script aktif olduğunda (Oyun başlayınca) çalışır
        private void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            
            if (_doc == null)
            {
                Debug.LogError("🚨 LoginController: UIDocument bulunamadı!");
                return;
            }

            // Görsel ağacın en tepesi (Root)
            var root = _doc.rootVisualElement;

            // 1. ELEMANLARI BUL (Query - Q Metodu)
            _inputUsername = root.Q<TextField>("input-username");
            _inputPassword = root.Q<TextField>("input-password");
            _btnLogin = root.Q<Button>("btn-login");
            _btnRegister = root.Q<Button>("btn-register");
            _lblError = root.Q<Label>("lbl-error");
            _lblLoading = root.Q<Label>("lbl-loading");

            // 2. BUTONLARI DİNLE (Event Binding)
            if (_btnLogin != null)
                _btnLogin.clicked += () => OnLoginClicked().Forget(); // Async metodu tetikle

            if (_btnRegister != null)
                _btnRegister.clicked += OnRegisterClicked;
        }

        // Giriş Butonuna Basılınca
        private async UniTaskVoid OnLoginClicked()
        {
            // Verileri al
            string username = _inputUsername.value;
            string password = _inputPassword.value;

            // Boş mu kontrol et
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Lütfen kullanıcı adı ve şifre giriniz.");
                return;
            }

            // Yükleniyor moduna geç (Butonları kilitle)
            SetLoadingState(true);
            
            // PATRONA GİT: "Bu kullanıcıyı içeri al"
            bool success = await GameConfigManager.Instance.AuthenticateUser(username, password);

            // Cevap geldi, yükleniyor modunu kapat
            SetLoadingState(false);

            if (success)
            {
                Debug.Log("🎉 UI: Giriş Başarılı! Ana Menüye geçiliyor...");
                ShowError(""); // Varsa hata mesajını sil
                
                // 🎯 SAHNE GEÇİŞ KOMUTU: MenuScene'i yüklüyoruz.
                SceneManager.LoadScene("MenuScene");
            }
            else
            {
                ShowError("Giriş başarısız. Bilgileri kontrol edin.");
                // Şifre alanını temizle
                _inputPassword.value = "";
            }
        }

        private void OnRegisterClicked()
        {
            Debug.Log("📝 Kayıt ol butonuna basıldı. (Henüz aktif değil)");
        }

        // Hata mesajını gösterir veya gizler
        private void ShowError(string message)
        {
            if (_lblError == null) return;

            _lblError.text = message;
            // Mesaj boşsa gizle, doluysa göster
            _lblError.style.display = string.IsNullOrEmpty(message) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // Yükleniyor animasyonunu yönetir
        private void SetLoadingState(bool isLoading)
        {
            if (_lblLoading != null) 
                _lblLoading.style.display = isLoading ? DisplayStyle.Flex : DisplayStyle.None;
            
            // İşlem sürerken butona tekrar basılmasın
            if (_btnLogin != null) 
                _btnLogin.SetEnabled(!isLoading); 
        }
    }
}
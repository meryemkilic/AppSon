using UnityEngine;
using UnityEngine.UIElements; 
using DG.Tweening;
using SpeechTherapy.Data; 
using Cysharp.Threading.Tasks; 
using UnityEngine.Networking;
using System;

namespace SpeechTherapy.Games.Memory
{
    public class Card : MonoBehaviour
    {
        private VisualElement _cardRoot; 
        private Button _cardButton;
        private VisualElement _frontImage; 
        private VisualElement _backImage;  

        private AssetItem _assetData; 
        private int _cardID; 

        private bool isRevealed = false;
        private bool isMatched = false;
        private float flipDuration = 0.4f;
        private Ease flipEase = Ease.OutQuad;

        public void Initialize(VisualElement root, AssetItem data, int cardID)
        {
            _cardRoot = root;
            _assetData = data;
            _cardID = cardID;

            _cardButton = root.Q<Button>("card-button");
            _frontImage = root.Q<VisualElement>("front-image");
            _backImage = root.Q<VisualElement>("back-image");
            
            _frontImage.style.display = DisplayStyle.None;
            _backImage.style.display = DisplayStyle.Flex; 
            
            _cardButton.clicked += OnClicked;

            // 💡 DÜZELTME: transform'un dönüşünü başlangıçta sıfırla
            transform.localRotation = Quaternion.identity; 
            
            isRevealed = false;
            isMatched = false;
            
            LoadImageFromUrl(_assetData.ImageUrl).Forget();
        }
        
        private async UniTaskVoid LoadImageFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    if (_frontImage != null)
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        _frontImage.style.backgroundImage = new StyleBackground(sprite);
                    }
                }
                else
                {
                    Debug.LogError($"Kart resmi yüklenemedi: {url} - {request.error}");
                }
            }
        }


        public void OnClicked()
        {
            if (GameManager.Instance == null || GameManager.Instance.CanPlayerSelectCard() == false)
                return;

            if (isRevealed)
                return;

            Reveal();
            GameManager.Instance.OnCardClicked(this);
        }

        public void Reveal()
        {
            _cardButton.SetEnabled(false);
            isRevealed = true; 

            // 💡 DÜZELTME: Kartın kendi transform'u kullanılır.
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2).SetEase(flipEase));
            sequence.AppendCallback(() =>
            {
                _backImage.style.display = DisplayStyle.None;
                _frontImage.style.display = DisplayStyle.Flex;
            });
            sequence.Append(transform.DORotate(new Vector3(0, 180, 0), flipDuration / 2).SetEase(flipEase));
        }

        public void Hide()
        {
            if (!isRevealed || isMatched) return;

            isRevealed = false;

            // 💡 DÜZELTME: Kartın kendi transform'u kullanılır.
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2).SetEase(flipEase));
            sequence.AppendCallback(() =>
            {
                _frontImage.style.display = DisplayStyle.None;
                _backImage.style.display = DisplayStyle.Flex;
            });
            sequence.Append(transform.DORotate(new Vector3(0, 0, 0), flipDuration / 2).SetEase(flipEase));
            sequence.OnComplete(() =>
            {
                transform.localRotation = Quaternion.identity; 
                _cardButton.SetEnabled(true);
            });
        }

        public void SetMatched()
        {
            isMatched = true;
            isRevealed = true;
            _cardButton.SetEnabled(false);
            
            // 💡 DÜZELTME: Kartın kendi transform'u kullanılır.
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(1.15f, 0.2f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(1f, 0.2f).SetEase(Ease.InQuad));
        }

        private void OnDestroy()
        {
            // İhtiyaç duyulan tüm animasyonları öldür
            DOTween.Kill(transform);
        }

        public int GetCardID() => _cardID;
        // 💡 DÜZELTME: Erişim hatasını önlemek için doğru public tanımı
        public bool IsRevealed() => isRevealed; 
    }
}
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks; 
using SpeechTherapy.Data;
using SpeechTherapy.Core; 
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace SpeechTherapy.Games.Memory
{
    public class GameManager : SingletonBehaviour<GameManager> 
    {
        [Header("Game Settings")]
        [SerializeField] private float cardRevealTime = 0.75f;
        
        // UI Toolkit Bağımlılıkları (MemoryGameUIController tarafından atanır)
        [HideInInspector] public VisualTreeAsset cardUxmlTemplate; 
        [HideInInspector] public VisualElement cardContainerElement; 

        private AssetItem[] currentAssetItems; 
        private List<Card> allCards = new List<Card>();
        private List<Card> revealedCards = new List<Card>();
        private int totalPairs;
        private int matchedPairs = 0;
        private bool isCheckingMatch = false;

        protected override void Awake()
        {
            base.Awake();
            DOTween.Init();
        }

        public void GenerateLevel(int Height, int Width, AssetItem[] assetSet)
        {
            if (assetSet == null || assetSet.Length == 0)
            {
                Debug.LogError("🚨 MemoryGame: Asset seti yüklenemedi!");
                return;
            }

            totalPairs = assetSet.Length; 
            this.currentAssetItems = assetSet; 

            InitializeGame();
        }

        private void InitializeGame()
        {
            if (cardContainerElement == null || cardUxmlTemplate == null)
            {
                Debug.LogError("🚨 MemoryGame: UI bağımlılıkları (Container/Template) atanmadı!");
                return;
            }
            
            cardContainerElement.Clear(); 
            allCards.Clear();
            matchedPairs = 0;
            
            List<int> cardIDs = new List<int>();
            for (int i = 0; i < totalPairs; i++)
            {
                cardIDs.Add(i);
                cardIDs.Add(i);
            }
            cardIDs = cardIDs.OrderBy(x => Guid.NewGuid()).ToList(); 

            for (int i = 0; i < cardIDs.Count; i++)
            {
                int assetIndex = cardIDs[i];
                AssetItem assetToUse = currentAssetItems[assetIndex];

                TemplateContainer cardInstance = cardUxmlTemplate.Instantiate();
                cardContainerElement.Add(cardInstance);
                
                // 💡 DÜZELTME: TemplateContainer'dan Card scriptini al. 
                // Bu, CardTemplate.uxml'in bağlı olduğu GameObject'e Card.cs'in atanmış olduğunu varsayar.
                Card newCard = cardInstance.GetComponent<Card>();
                
                // Eğer Card scripti template'de atanmamışsa, runtime'da ekle
                if (newCard == null)
                {
                    GameObject go = (cardInstance as VisualElement)?.gameObject;
                    if (go != null)
                    {
                        newCard = go.AddComponent<Card>();
                        Debug.LogWarning("Card component was missing on the template and was added at runtime.");
                    }
                }
                
                if (newCard == null) continue; // Hata durumunda döngüyü atla
                
                newCard.Initialize(cardInstance, assetToUse, assetIndex);
                allCards.Add(newCard);
                
                // Animasyon (Card.cs'deki transform animasyonunu tetikler)
                newCard.transform.localScale = Vector3.zero;
                newCard.transform.DOScale(1, 0.2f).SetEase(Ease.OutBack).SetDelay(i * 0.05f);
            }
        }

        public async void OnCardClicked(Card selectedCard)
        {
            // 💡 DÜZELTME: Card.IsRevealed() metoduna erişim artık doğru
            if (isCheckingMatch || selectedCard.IsRevealed()) 
                return;

            revealedCards.Add(selectedCard);

            if (revealedCards.Count == 2)
            {
                isCheckingMatch = true;
                await UniTask.Delay(TimeSpan.FromSeconds(cardRevealTime)); 
                
                CheckForMatch();
                isCheckingMatch = false;
            }
        }

        private void CheckForMatch()
        {
            if (revealedCards.Count < 2) return;

            bool isMatch = revealedCards[0].GetCardID() == revealedCards[1].GetCardID();

            if (isMatch)
            {
                matchedPairs++;
                
                foreach (Card card in revealedCards)
                {
                    card.SetMatched();
                }

                if (matchedPairs >= totalPairs)
                {
                    PlayVictoryAnimation();
                }
            }
            else
            {
                foreach (Card card in revealedCards)
                {
                    card.Hide();
                }
            }
            revealedCards.Clear();
        }

        public bool CanPlayerSelectCard() => !isCheckingMatch && revealedCards.Count < 2;

        public void RestartLevel()
        {
            if (currentAssetItems != null && currentAssetItems.Length > 0)
            {
                InitializeGame();
            }
            else
            {
                Debug.LogError("Oyun yeniden başlatılamıyor: Kart seti hafızada yok.");
            }
        }

        private void PlayVictoryAnimation()
        {
            // Zafer Animasyonu
            Sequence victorySequence = DOTween.Sequence();
            for (int i = 0; i < allCards.Count; i++)
            {
                Card currentCard = allCards[i];
                if (currentCard == null) continue;

                // 💡 DÜZELTME: Transform üzerinden animasyon
                currentCard.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo)
                    .SetDelay(i * 0.1f);
            }
            
            UniTask.Delay(TimeSpan.FromSeconds(3)).Forget(); 
        }
        
        private void OnDestroy()
        {
            DOTween.KillAll();
        }
    }
}
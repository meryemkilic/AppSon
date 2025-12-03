using UnityEngine;

namespace SpeechTherapy
{
    // T'nin bir MonoBehaviour olduğunu ve T'nin kendisi olduğunu kısıtlar.
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                // Instance zaten varsa onu döndür
                if (_instance != null)
                {
                    return _instance;
                }

                // Instance yoksa sahnede ara
                _instance = FindObjectOfType<T>();

                // Eğer hala bulunamadıysa (sahneye eklenmemişse) hata ver veya oluştur
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                    Debug.LogWarning($"SingletonBehaviour: '{typeof(T).Name}' sahnede bulunamadı. Yeni bir GameObject oluşturuldu.");
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                // Oyun yöneticilerinin sahne geçişlerinde yok olmamasını sağlar.
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                // Zaten bir instance varsa kendini yok et
                Destroy(gameObject);
            }
        }
    }
}
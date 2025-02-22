using UnityEngine;

namespace Utils
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if(_instance == null)
                    _instance = FindFirstObjectByType<T>();
                return _instance;
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
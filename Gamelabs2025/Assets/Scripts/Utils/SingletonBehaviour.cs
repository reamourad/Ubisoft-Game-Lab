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

        protected virtual void Awake()
        {
            if(_instance == null)
                _instance = this as T;
            else if(_instance != this)
                Destroy(gameObject);
            
            DontDestroyOnLoad(this);
        }
    }
}
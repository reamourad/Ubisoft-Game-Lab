using UnityEngine;

namespace StateManagement.MainMenu.UI
{
    [CreateAssetMenu(fileName = "UINavigationSounds", menuName = "UINavigationSounds")]
    public class UINavigationSounds : ScriptableObject
    {
        public AudioClip navigateSound;
        public AudioClip selectSound;
        public AudioClip tabSwitchSFX;
        
        private static UINavigationSounds _instance;

        public static UINavigationSounds Instance
        {
            get
            {
                if(_instance == null)
                    _instance = Resources.Load<UINavigationSounds>("UINavigationSounds");
                
                return _instance;
            }
        }
        
        
    }
}
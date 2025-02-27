using UnityEngine;

namespace Networking
{
    public class TimeManagerUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text timeText;

        public void SetTimerText(string text)
        {
            timeText.text = text;
        }
    }
}
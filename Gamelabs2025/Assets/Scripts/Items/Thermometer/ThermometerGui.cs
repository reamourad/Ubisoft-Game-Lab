using UnityEngine;

namespace Player.Items.Thermometer
{
    public class ThermometerGui : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text tempReading;

        public void SetTemperatureText(string tempText)
        {
            tempReading.text = tempText;
        }
    }
}
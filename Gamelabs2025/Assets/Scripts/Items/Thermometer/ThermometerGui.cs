using System;
using UnityEngine;

namespace Player.Items.Thermometer
{
    public class ThermometerGui : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text tempReading;
        [SerializeField] private TMPro.TMP_Text tempDistReading;

        public void SetTemperatureText(string tempText, float distance)
        {
            tempReading.text = tempText;
            tempDistReading.text = distance is float.PositiveInfinity ? "" : $" ( Dist: {(int)distance}m )";
        }
    }
}
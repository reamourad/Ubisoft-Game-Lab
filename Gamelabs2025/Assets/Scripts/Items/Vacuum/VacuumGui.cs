using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Items
{
    public class VacuumGui : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text powerText;
        [SerializeField] private Image powerBallFill;

        Coroutine fillCoroutine;
        public void SetPowerPercentage(float percentage)
        {
            powerText.text = $"Charge: {Mathf.RoundToInt(percentage*100.0f).ToString()}%";
            //powerBallFill.fillAmount = percentage;
            if(fillCoroutine != null)
                StopCoroutine(fillCoroutine);
            fillCoroutine = StartCoroutine(FillRoutine(percentage));
        }

        IEnumerator FillRoutine(float value)
        {
            float timeStep = 0;
            float curr = powerBallFill.fillAmount;
            float newValue = value;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / 0.15f;
                powerBallFill.fillAmount = Mathf.Lerp(curr, newValue, timeStep);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
using System.Collections;
using Player.Audio;
using UnityEngine;
using UnityEngine.Playables;

namespace Player.NotificationSystem
{
    public class NotificationObject : MonoBehaviour
    {
        [SerializeField] private AudioClip swooshClip;
        [SerializeField] private TMPro.TMP_Text notificationText;
        [SerializeField] private CanvasGroup objCanvasGroup;
        [SerializeField] private CanvasGroup notificationTextCG;

        public void Setup(string text)
        {
            notificationText.text = text;
            StartCoroutine(AnimateBody());
            StartCoroutine(AnimateText());
        }

        IEnumerator AnimateBody()
        {
            float timeStep = 0;
            var startSize = new Vector3(0,1,1);
            var newSize = new Vector3(1, 1, 1);
            AudioManager.Instance.PlaySFX(swooshClip);
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / 0.1f;
                transform.localScale = Vector3.Lerp(startSize, newSize, timeStep);
                objCanvasGroup.alpha = Mathf.Lerp(0, 1, timeStep);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(2f);
            timeStep = 0;
            var rect = GetComponent<RectTransform>();
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x * -1, rect.anchoredPosition.y);
            AudioManager.Instance.PlaySFX(swooshClip);
            yield return new WaitForEndOfFrame();
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / 0.1f;
                transform.localScale = Vector3.Lerp(newSize, startSize, timeStep);
                notificationTextCG.alpha = Mathf.Lerp(1,0, timeStep);
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator AnimateText()
        {
            float timeStep = 0;
            yield return new WaitForSeconds(0.1f);
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / 0.15f;
                notificationTextCG.alpha = Mathf.Lerp(0, 1, timeStep);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(2f);
            yield return new WaitForSeconds(0.1f);
            timeStep = 0;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / 0.1f;
                notificationTextCG.alpha = Mathf.Lerp(1,0, timeStep);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
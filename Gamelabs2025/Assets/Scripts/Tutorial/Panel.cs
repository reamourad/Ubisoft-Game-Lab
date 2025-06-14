using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

namespace Tutorial
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private List<VideoClip> videoClips;
        [SerializeField] private List<int> skippingPages = new() {2};
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private GameObject rightButton;
        [SerializeField] private Transform circleParent;
        [SerializeField] private GameObject leftButton;
        [SerializeField] private Circle circlePrefab;
        [SerializeField] private TMP_Text rightText;
        [SerializeField] private Transform pages;

        private List<Circle> circles = new();
        private Vector3 basePosition;
        private int pageCount;
        private int counter;

        private void Start()
        {
            basePosition = pages.localPosition;
            BindKeys();
            SetPageCount();
            CreateCircles();
            ChangeVideo();
            ToggleButton(leftButton, false);
        }

        private void BindKeys()
        {
            InputReader.Instance.SetToUIInputs();
            InputReader.Instance.OnTutorialNextEvent += DestroyPanel;
            InputReader.Instance.OnTutorialNextEvent += MoveRight;
            InputReader.Instance.OnTutorialBackEvent += MoveLeft;
        }

        private void UnBindKeys()
        {
            InputReader.Instance.SetToGameplayInputs();
            InputReader.Instance.OnTutorialNextEvent -= DestroyPanel;
            InputReader.Instance.OnTutorialNextEvent -= MoveRight;
            InputReader.Instance.OnTutorialBackEvent -= MoveLeft;
        }

        private void SetPageCount()
        {
            pageCount = pages.childCount;
        }

        private void CreateCircles()
        {
            for (var i = 0; i < pageCount; i++)
            {
                circles.Add(Instantiate(circlePrefab));
                circles[i].transform.SetParent(circleParent, false);
                circles[i].transform.SetAsFirstSibling();
            }

            circles.Reverse();
            circles.First().FillCircle();
        }

        private void MoveLeft()
        {
            if (!leftButton.activeSelf) return;
            UpdateCircles(-1);
            MovePage();
            ChangeVideo();
            ToggleButton(rightButton, true);
            ChangeRightButtonText("Next");
            if (counter == 0)
                ToggleButton(leftButton, false);
        }

        private void ChangeRightButtonText(string newName)
        {
            rightText.text = newName;
        }

        private void MoveRight()
        {
            if (counter != pageCount - 1)
            {
                UpdateCircles(1);
                MovePage();
                ChangeVideo();
                ToggleButton(leftButton, true);
            }

            if (counter == pageCount - 1)
                ChangeRightButtonText("Close");
        }

        private void MovePage()
        {
            pages.DOLocalMove(basePosition + Vector3.left * 1500 * counter, 1f);
        }

        private void ChangeVideo()
        {
            if(counter >= videoClips.Count) return;
            if(skippingPages.Contains(counter + 1)) return;
            videoPlayer.ChangeSlide(videoClips[counter]);
        }

        private void UpdateCircles(int direction)
        {
            circles[counter].EmptyCircle();
            counter += direction;
            circles[counter].FillCircle();
        }

        private void ToggleButton(GameObject button, bool state)
        {
            button.gameObject.SetActive(state);
        }

        private void DestroyPanel()
        {
            if (counter != pageCount - 1) return;
            UnBindKeys();
            DOTween.KillAll();
            Destroy(gameObject);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private GameObject confirmButton;
        [SerializeField] private GameObject rightButton;
        [SerializeField] private Transform circleParent;
        [SerializeField] private GameObject leftButton;
        [SerializeField] private Circle circlePrefab;
        [SerializeField] private Transform pages;


        private List<Circle> circles = new();
        private Vector3 basePosition;
        private int pageCount;
        private int counter;

        private void Start()
        {
            basePosition = pages.localPosition;
            SetPageCount();
            CreateCircles();
            ToggleButton(rightButton, false);
        }

        private void BindKeys()
        {
            // Bind input reader events to MoveLeft,MoveRight and Destroy panel here
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
                circles[i].transform.SetParent(circleParent,false);
                circles[i].transform.SetAsFirstSibling();
            }

            circles.First().FillCircle();
        }

        public void MoveLeft()
        {
            UpdateCircles(-1);
            MovePage();
            ToggleButton(leftButton, true);
            ToggleButton(confirmButton, false);
            if (counter == 0)
                ToggleButton(rightButton, false);
        }

        public void MoveRight()
        {
            UpdateCircles(1);
            MovePage();
            ToggleButton(rightButton, true);
            if (counter != pageCount - 1) return;
            ToggleButton(leftButton, false);
            ToggleButton(confirmButton, true);
        }

        private void MovePage()
        {
            pages.DOLocalMove(basePosition + Vector3.left * 500 * counter, 1f);
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

        public void DestroyPanel()
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class Circle : MonoBehaviour
    {
        [SerializeField] private Sprite filled;
        [SerializeField] private Sprite empty;
        [SerializeField] private Image circle;

        public void FillCircle()
        {
            circle.sprite = filled;
        }

        public void EmptyCircle()
        {
            circle.sprite = empty;
        }
    }
}

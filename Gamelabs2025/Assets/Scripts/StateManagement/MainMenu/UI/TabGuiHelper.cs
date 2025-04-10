using System;
using UnityEngine;
using UnityEngine.UI;

namespace StateManagement.MainMenu.UI
{
    [RequireComponent(typeof(Toggle))]
    public class TabGuiHelper : MonoBehaviour
    {
        [SerializeField] private GameObject selected;
        [SerializeField] private GameObject settingsToShow;
        private void Start()
        {
            GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);    
        }

        private void OnValueChanged(bool arg0)
        {
            selected.SetActive(arg0);
            settingsToShow.SetActive(arg0);
        }
    }
}
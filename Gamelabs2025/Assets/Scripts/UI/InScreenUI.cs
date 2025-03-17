using TMPro;
using UnityEngine;
using Utils;

public class InScreenUI : SingletonBehaviour<InScreenUI>
{
    [SerializeField] private TextMeshProUGUI toolTipText;

    public void SetToolTipText(string text)
    {
        toolTipText.text = text;
    }
}

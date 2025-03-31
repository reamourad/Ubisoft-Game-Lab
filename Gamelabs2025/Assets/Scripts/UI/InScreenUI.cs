using System;
using System.Collections.Generic;
using Player.UI.ControlPrompts;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class InScreenUI : SingletonBehaviour<InScreenUI>
{
    private const string INPUT_PROMPT_PATH = "DynamicInputPrompts";
    [SerializeField] private TextMeshProUGUI toolTipText;

    private DynamicInputPrompts inputPrompts;
    private void Start()
    {
        LoadDynamicPrompts();
    }

    private void LoadDynamicPrompts()
    {
        var go = Instantiate(Resources.Load<GameObject>(INPUT_PROMPT_PATH), this.transform);
        inputPrompts = go.GetComponent<DynamicInputPrompts>();
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 125f);
    }
    
    public void SetToolTipText(string text)
    {
        toolTipText.text = text;
    }

    public void AddToolTipText(string text)
    {
        toolTipText.text += "\n" + text;
    }

    public void ShowInputPrompt(InputAction action, string promptText)
    {
        if(action == null)
            return;
        inputPrompts.ShowInputPrompt(action, promptText);
    }

    public void RemoveInputPrompt(InputAction action)
    {
        inputPrompts.RemoveInputPrompt(action);
    }

    public void ClearInputPrompts()
    {
        inputPrompts.Clear();
    }
    
    public string GetToolTipText()
    {
        return toolTipText.text;
    }
}

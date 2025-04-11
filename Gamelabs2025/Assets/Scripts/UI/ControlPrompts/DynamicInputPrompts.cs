using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Player.UI.ControlPrompts
{
    public class DynamicInputPrompts : MonoBehaviour
    {
        [SerializeField] private GameObject referencePrompt;
        private Dictionary<InputAction, (GameObject go, TMPro.TMP_Text prompt)> inputActions = new Dictionary<InputAction, (GameObject, TMPro.TMP_Text)>();

        
        public void ShowInputPrompt(InputAction action, string text)
        {
            //we already have the key
            if (inputActions.ContainsKey(action))
            {
                inputActions[action].prompt.text = text;
                return;
            }

            var go = Instantiate(referencePrompt, this.transform);
            go.SetActive(true);
            go.GetComponent<ControlPromptDisplayer>().SetActionReference(action);
            var txt = go.GetComponentInChildren<TMPro.TMP_Text>();
            txt.text = text;
            inputActions.Add(action, (go, txt));

            StartCoroutine(RebuildLayout());
        }

        public void RemoveInputPrompt(InputAction action)
        {
            if (inputActions.ContainsKey(action))
            {
                var tuple = inputActions[action];
                Destroy(tuple.go);
                inputActions.Remove(action);
                StartCoroutine(RebuildLayout());
            }
        }

        private IEnumerator RebuildLayout()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);
        }

        public void Clear()
        {
            foreach (var action in inputActions)
            {
                Destroy(action.Value.go);
            }
            inputActions.Clear();
        }
    }
}
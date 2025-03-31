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
        private Dictionary<InputAction, GameObject> inputActions = new Dictionary<InputAction, GameObject>();

        
        public void AddShowInputPrompt(InputAction action, string text)
        {
            //we already have the key
            if(inputActions.ContainsKey(action))
                return;
        
            Instantiate(referencePrompt, this.transform);
            referencePrompt.SetActive(true);
            referencePrompt.GetComponent<ControlPromptDisplayer>().SetActionReference(action);
            referencePrompt.GetComponentInChildren<TMPro.TMP_Text>().text = text;
            inputActions.Add(action, referencePrompt);

            StartCoroutine(RebuildLayout());
        }

        public void RemoveInputPrompt(InputAction action)
        {
            if (inputActions.ContainsKey(action))
            {
                var go = inputActions[action];
                Destroy(go);
                inputActions.Remove(action);
                StartCoroutine(RebuildLayout());
            }
        }

        private IEnumerator RebuildLayout()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.MarkLayoutForRebuild(this.GetComponent<RectTransform>());
        }
    }
}
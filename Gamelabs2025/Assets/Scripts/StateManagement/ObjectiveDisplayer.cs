using System;
using Player;
using UnityEngine;

namespace StateManagement
{
    public class ObjectiveDisplayer : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text objectiveText;
        [SerializeField] private string prepareText;
        [SerializeField] private string hiderObjective;
        [SerializeField] private string seekerObjective;

        public void Start()
        {
            GameController.Instance.OnStageChanged += StageChanged;
        }

        private void OnDestroy()
        {
            GameController.Instance.OnStageChanged -= StageChanged;
        }

        private void StageChanged(GameController.GameStage state)
        {
            string text = "";
            if (state == GameController.GameStage.Preparing)
            {
               text = $"<color=yellow>Objective:</color> {prepareText}";
            }
            else
            {
                if(GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Hider)
                    text = $"<color=yellow>Objective:</color> {hiderObjective}";
                else if(GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Seeker)
                    text = $"<color=yellow>Objective:</color> {seekerObjective}";
            }
            objectiveText.text = text;
        }
    }
}
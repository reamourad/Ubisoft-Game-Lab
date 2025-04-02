using UnityEngine;

namespace Player.Inventory
{
    public interface ISeekerAttachable
    {
        /// <summary>
        /// Attaches to player according to a locator
        /// </summary>
        void OnAttach(Transform parent);
        /// <summary>
        /// Implement Hiding Logic.
        /// Handle Destroy Logic
        /// </summary>
        void OnDetach(Transform parent, bool spawnWorldDummy);

        string GetUsePromptText();
    }
}
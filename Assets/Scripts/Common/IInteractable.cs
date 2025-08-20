using UnityEngine;

namespace ZhouSoftware
{
    public interface IInteractable
    {
        // Determines if the interactor can interact with this object currently
        bool CanInteract(Transform interactor);

        // Perfrom the interaction
        void Interact(Transform interactor);

        // Optional visual feedback on focus
        void OnFocusEnter();
        void OnFocusExit();

        // Optional UI prompt text
        string PromptText { get; }

    }
}
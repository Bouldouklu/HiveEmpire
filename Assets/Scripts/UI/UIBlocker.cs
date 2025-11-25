using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Utility for checking if pointer is over UI elements.
/// Prevents world interactions from triggering when clicking on UI.
/// </summary>
public static class UIBlocker
{
    /// <summary>
    /// Returns true if the pointer is currently over any UI element.
    /// Use this to block world interactions when clicking on UI panels/buttons.
    /// </summary>
    /// <returns>True if pointer is over UI, false otherwise</returns>
    public static bool IsPointerOverUI()
    {
        // Check if EventSystem exists
        if (EventSystem.current == null)
        {
            Debug.LogWarning("UIBlocker: No EventSystem found in scene. UI blocking will not work.");
            return false;
        }

        // Check if pointer is over a UI GameObject
        return EventSystem.current.IsPointerOverGameObject();
    }
}

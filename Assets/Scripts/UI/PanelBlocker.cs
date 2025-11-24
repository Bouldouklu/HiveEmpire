using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Handles click-outside-to-close functionality for UI panels.
/// Detects when a click starts AND ends on the blocker (not on child UI elements).
/// This prevents sliders and other interactive elements from accidentally closing the panel.
/// </summary>
public class PanelBlocker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Events")]
    [Tooltip("Invoked when user clicks outside the panel (both pointer down and up on blocker)")]
    public UnityEvent OnClickedOutside;

    private bool pointerDownOnBlocker = false;
    private int enabledFrame = -1;

    private void OnEnable()
    {
        // Record the frame when the blocker is enabled
        // This prevents same-frame clicks from closing the panel immediately
        enabledFrame = Time.frameCount;
        pointerDownOnBlocker = false;
    }

    /// <summary>
    /// Called when pointer is pressed down.
    /// Records that the click started on the blocker.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Ignore clicks on the same frame the blocker was enabled
        // This prevents the panel from closing immediately when opened by clicking an object at the screen edge
        if (Time.frameCount == enabledFrame)
        {
            pointerDownOnBlocker = false;
            return;
        }

        // Check if the pointer is actually over this blocker (not a child element)
        if (eventData.pointerEnter == gameObject)
        {
            pointerDownOnBlocker = true;
        }
        else
        {
            pointerDownOnBlocker = false;
        }
    }

    /// <summary>
    /// Called when pointer is released.
    /// Only triggers the close event if both down and up happened on the blocker.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Ignore clicks on the same frame the blocker was enabled
        if (Time.frameCount == enabledFrame)
        {
            pointerDownOnBlocker = false;
            return;
        }

        // Only close if pointer down started on blocker AND pointer up is also on blocker
        if (pointerDownOnBlocker && eventData.pointerEnter == gameObject)
        {
            OnClickedOutside?.Invoke();
        }

        // Reset flag
        pointerDownOnBlocker = false;
    }
}

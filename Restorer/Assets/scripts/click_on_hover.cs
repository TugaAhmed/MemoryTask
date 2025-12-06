using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Tobii.XR;

public class click_on_hover : MonoBehaviour
{
    private XRBaseInteractor interactor; // The interactor (e.g., XRRayInteractor)
    private XRBaseInteractable currentHoveredInteractable; // Currently hovered object
    private const ControllerButton TriggerButton = ControllerButton.Trigger;

    public SimpleMemoryGame gameManager; // Drag your GameManager with SimpleMemoryGameVR script here

    void Start()
    {
        interactor = GetComponent<XRBaseInteractor>();

        if (interactor != null)
        {
            interactor.hoverEntered.AddListener(OnHoverEnter);
            interactor.hoverExited.AddListener(OnHoverExit);
        }
    }

    void Update()
    {
        // Check if hovering over a valid option cube and user presses trigger
        if (currentHoveredInteractable != null && ControllerManager.Instance.GetButtonPressDown(TriggerButton))
        {
            //Debug.Log("Trigger pressed on: " + currentHoveredInteractable.gameObject.name);

            if (gameManager != null)
            {
                gameManager.OnOptionSelected(currentHoveredInteractable.gameObject);
            }
        }
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        currentHoveredInteractable = args.interactableObject.transform.GetComponent<XRBaseInteractable>();
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (args.interactableObject.transform.GetComponent<XRBaseInteractable>() == currentHoveredInteractable)
        {
            currentHoveredInteractable = null;
        }
    }

    private void OnDestroy()
    {
        if (interactor != null)
        {
            interactor.hoverEntered.RemoveListener(OnHoverEnter);
            interactor.hoverExited.RemoveListener(OnHoverExit);
        }
    }
}

/*  Script made by Tturna
 *  version 0.1
*   
*   This script is designed to provide a base for 1st person interactions between the player and the world or items in the world.
*   The script uses a continuous raycast to check for objects to interact with.
*   When a specific key is pressed, the script will attempt an interaction.
*   The interaction will fail if the target object doesn't have a TturnaInteractable component.
*   Upon a successful interaction, an event will be fired for other scripts to subscribe to.
*/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tturna.Interaction
{
    public class Tt_Interactor : MonoBehaviour
    {
        /// <summary>
        /// Takes the interaction target and the interaction source as parameters.
        /// </summary>
        public static event Action<Tt_Interactable, GameObject> Interaction;

        enum RayStart { Camera, Custom}

        [Header("Interaction Settings")]
        [SerializeField, Tooltip("Preset starting point for the interaction ray.")]
        RayStart rayStart;

        [SerializeField, Tooltip("Custom starting point for the interaction ray. This has priority if not null")]
        Transform customStart;

        [SerializeField, Tooltip("Maximum interaction distance in meters.")]
        float maxInteractionDistance;

        [SerializeField, Tooltip("Show interaction key prompt when looking at an interactable.")]
        bool showInteractPrompt;

        [SerializeField, Tooltip("Should the target object get destroyed when the player interacts with it")]
        bool destroyInteractTarget;

        [Header("Required References")]
        [SerializeField] GameObject interactPromptPrefab;

        Camera cam;
        Text promptText;
        Vector3 rayOrigin;
        int interactLayer => 1 << LayerMask.NameToLayer("Interactable");
        protected Tt_Interactable targetInteractable;

        void Start()
        {
            cam = Camera.main;

            if (showInteractPrompt)
            {
                try
                {
                    promptText = GameObject.Find("Interact Prompt").GetComponent<Text>();
                }
                catch
                {
                    Debug.LogWarning("Couldn't find a game object with name 'Interact Prompt'. Making one...");
                    promptText = Instantiate(interactPromptPrefab).GetComponent<Text>();
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F)) Interact();

            #region Interaction Raycasting
            switch (rayStart)
            {
                case RayStart.Camera:
                    rayOrigin = cam.transform.position;
                    break;

                case RayStart.Custom:
                default:
                    rayOrigin = customStart.position;
                    break;
            }

            if (rayOrigin == Vector3.zero) return;

            bool rayHit = Physics.Raycast(rayOrigin, cam.transform.forward, out RaycastHit hitInfo, maxInteractionDistance, interactLayer);

            if (!rayHit)
            {
                //Debug.DrawLine(rayOrigin, rayOrigin + cam.transform.forward * maxInteractionDistance, Color.green);
                ClearTarget();
                return;
            }

            if (targetInteractable?.gameObject != hitInfo.transform.gameObject)
            {
                if (hitInfo.transform.TryGetComponent(out Tt_Interactable interactable))
                {
                    targetInteractable = interactable;
                    promptText.text = "[F]";
                }
                else
                {
                    ClearTarget();
                    return;
                }
            }

            //Debug.DrawLine(rayOrigin, hitInfo.point, Color.red);
            #endregion
        }

        void ClearTarget()
        {
            targetInteractable = null;

            if (showInteractPrompt) promptText.text = "";
        }

        protected virtual void Interact()
        {
            if (!targetInteractable) return;

            targetInteractable.Interact(gameObject);
            Interaction?.Invoke(targetInteractable, gameObject);
            if (destroyInteractTarget) Destroy(targetInteractable);
        }
    }
}
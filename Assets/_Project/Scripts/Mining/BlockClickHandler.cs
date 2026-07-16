using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastVein.Mining
{
    [RequireComponent(typeof(RectTransform))]
    public class BlockClickHandler : MonoBehaviour
    {
        const string MapName = "Mining";
        const string TapActionName = "Tap";
        const string PointerPositionActionName = "PointerPosition";

        [SerializeField] InputActionAsset inputActions;
        [SerializeField] RectTransform blockRect;

        InputAction tapAction;
        InputAction pointerPositionAction;

        public event Action OnBlockTapped;

        void Awake()
        {
            if (blockRect == null) blockRect = GetComponent<RectTransform>();

            InputActionMap map = inputActions.FindActionMap(MapName, throwIfNotFound: true);
            tapAction = map.FindAction(TapActionName, throwIfNotFound: true);
            pointerPositionAction = map.FindAction(PointerPositionActionName, throwIfNotFound: true);
        }

        void OnEnable()
        {
            tapAction.Enable();
            pointerPositionAction.Enable();
            tapAction.performed += HandleTap;
        }

        void OnDisable()
        {
            tapAction.performed -= HandleTap;
            tapAction.Disable();
            pointerPositionAction.Disable();
        }

        void HandleTap(InputAction.CallbackContext ctx)
        {
            Vector2 screenPos = pointerPositionAction.ReadValue<Vector2>();
            if (RectTransformUtility.RectangleContainsScreenPoint(blockRect, screenPos, null))
            {
                OnBlockTapped?.Invoke();
            }
        }
    }
}

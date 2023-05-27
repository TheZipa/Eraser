using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputService : MonoBehaviour
    {
        public event Action<Vector2, Vector2> OnLineDrag;
        
        private InputActions _playerInput;
        private Vector2 _lastPosition;
        private bool _isTouch;

        public void Start()
        {
            _playerInput = new InputActions();
            _playerInput.Enable();
            Subscribe();
        }

        private void Subscribe()
        {
            _playerInput.Main.Press.started += StartTouch;
            _playerInput.Main.Press.canceled += EndTouch;
        }
   
        private void StartTouch(InputAction.CallbackContext obj)
        {
            _isTouch = true;
            _lastPosition = GetScreenPosition();
            StartCoroutine(Drag());
        }

        private void EndTouch(InputAction.CallbackContext obj) => _isTouch = false;

        private IEnumerator Drag()
        {
            while (_isTouch)
            {
                yield return null;
                Vector2 newScreenPosition = GetScreenPosition();
                OnLineDrag?.Invoke(newScreenPosition, _lastPosition);
                _lastPosition = newScreenPosition;
            }
        }

        private Vector2 GetScreenPosition() => _playerInput.Main.Screen.ReadValue<Vector2>();

        private void OnDestroy()
        {
            _playerInput.Main.Press.started -= StartTouch;
            _playerInput.Main.Press.canceled -= EndTouch;
            _playerInput.Dispose();
        }
    }
}
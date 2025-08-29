using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTutorial : MonoBehaviour
{
    PlayerController playerController;
    InputSystem_Actions input;
    Vector2 touchStartPos;
    Vector2 touchCurrentPos;
    bool isTouching = false;
    Vector2 externalMoveInput = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        input = new InputSystem_Actions();

        input.UI.Enable();
        input.Player.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (TutorialManager.Instance != null)
        {
            var currentPage = TutorialManager.Instance.GetTutorialPages[TutorialManager.Instance.GetCurrentPageIndex];
            if (input.UI.Cancel.triggered)
            {

                if (currentPage.header == "First Off")
                {
                    TutorialManager.Instance.CompleteStep();
                }
            }


            Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();

            if (currentPage.header == "Movement")
            {
                if (moveInput.y >= .1f)
                    TutorialManager.Instance.CompleteStep();
                else if (moveInput.y <= -.1f)
                    TutorialManager.Instance.CompleteStep();
                if (moveInput.x >= .1f)
                    TutorialManager.Instance.CompleteStep();
                else if (moveInput.x <= -.1f)
                    TutorialManager.Instance.CompleteStep();


                bool isPC = Application.platform == RuntimePlatform.WindowsPlayer
             || Application.platform == RuntimePlatform.WindowsEditor
             || Application.platform == RuntimePlatform.OSXPlayer
             || Application.platform == RuntimePlatform.LinuxPlayer;

                if (isPC) return; // no touchscreen available

                int activeTouchCount = 0;
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed)
                        activeTouchCount++;
                }

                // --- Movement ---
                if (activeTouchCount >= 1)
                {
                    var primaryTouch = Touchscreen.current.primaryTouch;

                    if (!isTouching)
                    {
                        isTouching = true;
                        touchStartPos = primaryTouch.startPosition.ReadValue();
                    }

                    touchCurrentPos = primaryTouch.position.ReadValue();
                    Vector2 dragDelta = touchCurrentPos - touchStartPos;

                    // Normalize drag like joystick
                    Vector2 moveInputTouch = dragDelta.normalized;

                    if (moveInputTouch.x >= .1f)               
                        TutorialManager.Instance.CompleteStep();
                    else if (moveInputTouch.x <= -.1f)
                        TutorialManager.Instance.CompleteStep();
                    if (moveInputTouch.y >= .1f)               
                        TutorialManager.Instance.CompleteStep();
                    else if (moveInputTouch.y <= -.1f)
                        TutorialManager.Instance.CompleteStep();
                }
                else
                {
                    isTouching = false;
                    externalMoveInput = Vector2.zero;
                }
            }

            if(currentPage.header == "Sprinting")
            {
                bool sprintInput = input.Player.Sprint.triggered;

                if (sprintInput)
                {
                    TutorialManager.Instance.CompleteStep();
                }
                int activeTouchCount = 0;
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed)
                        activeTouchCount++;
                }
                // --- Sprint ---
                if (activeTouchCount >= 2)
                {
                    // keep sprinting as long as key is held
                    sprintInput = true;
                    TutorialManager.Instance.CompleteStep();
                }
            }
        }
    }
}

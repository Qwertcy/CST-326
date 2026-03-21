using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;

    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public ClearCounter selectedCounter;
    }

    private bool isWalking;
    private Vector3 lastInteractDir;
    private ClearCounter selectedCounter;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("There is more than one player instance");
        }
        Instance = this;
    }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }
    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.Interact();
        }

    }
    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized(); // read current movement input
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y); // convert input into a 3d direction

        if (moveDir != Vector3.zero) // only update stored interaction direction when the player is giving movement input
        {
            lastInteractDir = moveDir; // remember the last non-zero facing/input direction
        }

        float interactDistance = 2f; // max raycast distance for detecting counters

        if (lastInteractDir != Vector3.zero && Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) // cast in the last valid direction instead of the current one
        {
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)) // check whether the hit object has a ClearCounter component
            {
                if (clearCounter != selectedCounter) // only update if the selected counter actually changed
                {
                    SetSelectedCounter(clearCounter); // store and broadcast the new selected counter
                }
            }
            else
            {
                SetSelectedCounter(null); // hit something on that layer but it was not a ClearCounter
            }
        }
        else
        {
            SetSelectedCounter(null); // nothing was hit, so clear the selection
        }
    }
    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized(); // get 2d input from your input system, usually x = horizontal and y = vertical
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y); // convert 2d input into a 3d world-space movement direction on the xz plane

        float moveDistance = moveSpeed * Time.deltaTime; // distance to move this frame, using deltaTime so movement is frame-rate independent
        float playerRadius = .7f; // radius of the capsule used for collision checking
        float playerHeight = 2f; // height offset for the top point of the capsule

        bool canMove = !Physics.CapsuleCast( // check whether moving in the desired direction would hit something
            transform.position, // bottom point of the capsule
            transform.position + Vector3.up * playerHeight, // top point of the capsule
            playerRadius, // capsule radius
            moveDir, // direction to test
            moveDistance // how far ahead to test
        );

        if (!canMove) // if full diagonal movement is blocked
        {
            Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0f).normalized; // try moving only along the x axis
            canMove = moveDir.x != 0f && !Physics.CapsuleCast( // only test x movement if there is actual x input
                transform.position, // bottom point of the capsule
                transform.position + Vector3.up * playerHeight, // top point of the capsule
                playerRadius, // capsule radius
                moveDirX, // x-only movement direction
                moveDistance // test distance
            );

            if (canMove) // if x-only movement works
            {
                moveDir = moveDirX; // use x-only movement
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z).normalized; // try moving only along the z axis
                canMove = moveDir.z != 0f && !Physics.CapsuleCast( // only test z movement if there is actual z input
                    transform.position, // bottom point of the capsule
                    transform.position + Vector3.up * playerHeight, // top point of the capsule
                    playerRadius, // capsule radius
                    moveDirZ, // z-only movement direction
                    moveDistance // test distance
                );

                if (canMove) // if z-only movement works
                {
                    moveDir = moveDirZ; // use z-only movement
                }
            }
        }

        isWalking = canMove && moveDir != Vector3.zero; // walking should only be true when there is a valid movement direction and movement is allowed

        if (canMove) // only change position if the cast says movement is valid
        {
            transform.position += moveDir * moveDistance; // actually move the player in world space
        }

        if (moveDir != Vector3.zero) // only rotate when there is a valid movement direction, avoids slerping toward a zero vector
        {
            float rotateSpeed = 10f; // how quickly the player rotates toward the movement direction
            transform.forward = Vector3.Slerp( // smoothly rotate the forward vector
                transform.forward, // current forward direction
                moveDir, // target forward direction
                Time.deltaTime * rotateSpeed // interpolation factor scaled by frame time
            );
        }
    }

    private void SetSelectedCounter(ClearCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

}
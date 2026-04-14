using UnityEngine; // provides monobehaviour, camera, raycasting, and core unity types
using UnityEngine.InputSystem; // provides access to the new input system mouse api
using WarcraftChess.Runtime.Core; // imports gamestate, movecommand, boardcoord, and piece identity types
using WarcraftChess.Runtime.Rules; // imports move legality and execution systems used by the local input loop

namespace WarcraftChess.Runtime.Presentation // defines namespace for input handling systems
{
    public sealed class BoardInputController : MonoBehaviour // handles local mouse input and converts clicks into piece selection or move execution
    {
        private GameState gameState; // stores the active runtime game state being interacted with
        private BoardRenderer boardRenderer; // stores the renderer used to refresh visuals after moves execute

        private PieceId? selectedPiece; // tracks the currently selected piece identity if a piece has been chosen
        private BoardCoord? selectedCoord; // tracks the currently selected piece's logical board coordinate

        public void Initialize(GameState state, BoardRenderer renderer) // initializes the input controller with the active game state and renderer
        {
            gameState = state; // assigns the active runtime state used for legality checks and move execution
            boardRenderer = renderer; // assigns the board renderer used to redraw the board after state changes
        }

        private void Update() // runs every frame to detect local player click input
        {
            if (Mouse.current == null) // ensures a mouse device is available before trying to read button state
                return; // exits early when no mouse device is present

            if (Mouse.current.leftButton.wasPressedThisFrame) // detects a left mouse click using the new input system
            {
                HandleClick(); // processes the click into a selection or attempted move
            }
        }

        private void HandleClick() // processes a raycast click into selection or move execution behavior
        {
            Camera mainCamera = Camera.main; // retrieves the scene's main camera for screen-to-world ray generation
            if (mainCamera == null) // ensures a valid camera exists before attempting a raycast
            {
                Debug.LogError("main camera not found"); // reports missing camera setup clearly in the console
                return; // exits because clicking cannot work without a camera
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue(); // reads the current mouse screen position from the new input system
            Ray ray = mainCamera.ScreenPointToRay(mousePosition); // creates a ray from the camera through the clicked screen position

            if (!Physics.Raycast(ray, out RaycastHit hit)) // performs the raycast and exits if no collider was hit
                return; // exits early when the click does not hit a board tile or piece

            PieceView pieceView = hit.collider.GetComponent<PieceView>(); // checks whether the clicked object is a rendered piece placeholder
            if (pieceView != null) // handles clicking directly on a piece
            {
                TrySelectPiece(pieceView); // attempts to select the clicked piece if it belongs to the current player
                return; // exits because piece selection is handled separately from tile movement
            }

            BoardCoord targetCoord = boardRenderer.WorldToBoard(hit.point); // converts the world-space hit location into a logical board coordinate
            TryMoveSelectedPiece(targetCoord); // attempts to move the currently selected piece to the clicked board square
        }

        private void TrySelectPiece(PieceView pieceView) // selects a clicked piece when it belongs to the player whose turn it is
        {
            if (gameState == null) // ensures the controller has been initialized before allowing selection
                return; // exits defensively if initialization did not happen

            if (!gameState.Pieces.TryGetValue(pieceView.PieceId, out PieceState pieceState)) // resolves the clicked view back to the runtime piece state
                return; // exits if the runtime piece cannot be found

            if (!pieceState.IsAlive) // prevents selecting pieces that are no longer alive in the runtime state
                return; // exits because dead pieces should not be interactable

            if (pieceState.Owner != gameState.CurrentPlayer) // restricts local interaction to the side whose turn it currently is
            {
                Debug.Log("cannot select opponent piece on this turn"); // provides simple local feedback when clicking the wrong side
                return; // exits because command generation must respect turn ownership
            }

            selectedPiece = pieceView.PieceId; // stores the selected runtime piece id for the next movement click
            selectedCoord = pieceView.BoardCoord; // stores the selected piece's logical origin coordinate
            Debug.Log($"selected piece: {pieceView.Owner} {pieceView.PieceType} at {pieceView.BoardCoord}"); // prints a readable selection log for debugging
        }

        private void TryMoveSelectedPiece(BoardCoord targetCoord) // attempts to move the currently selected piece to the clicked target square
        {
            if (gameState == null || boardRenderer == null) // ensures the controller is initialized before trying to execute moves
                return; // exits defensively if required references are missing

            if (!selectedPiece.HasValue || !selectedCoord.HasValue) // ensures a piece has already been selected before interpreting a tile click as a move
                return; // exits because there is no selected origin for the move command

            MoveCommand move = new MoveCommand(selectedPiece.Value, selectedCoord.Value, targetCoord); // constructs a runtime move command from the selected origin to the clicked target
            if (LegalMoveService.IsMoveFullyLegal(gameState, move)) // validates the attempted move through the fully legal chess rules layer
            {
                PieceMoveExecutor.ExecuteMove(gameState, move); // mutates the runtime state by applying the validated move
                boardRenderer.Render(gameState); // redraws the board and pieces from the updated runtime state
                Debug.Log($"move executed: {selectedCoord.Value} -> {targetCoord}"); // prints a readable execution log for debugging and local feedback
            }
            else
            {
                Debug.Log($"illegal move: {selectedCoord.Value} -> {targetCoord}"); // prints simple feedback when the clicked move is not legal
            }

            selectedPiece = null; // clears piece selection after the attempt so the next click starts a new interaction cycle
            selectedCoord = null; // clears stored origin after the attempt so stale coordinates are not reused
        }
    }
}
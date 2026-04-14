using System.Collections.Generic; // provides dictionary support for tracking rendered piece objects by runtime piece id
using UnityEngine; // provides monobehaviour, transforms, primitive creation, and material coloring
using WarcraftChess.Runtime.Core; // imports gamestate, boardcoord, pieceid, and piece state used for rendering from simulation

namespace WarcraftChess.Runtime.Presentation // defines the namespace for board and piece visualization systems
{
    public sealed class BoardRenderer : MonoBehaviour // renders a logical gamestate as simple board tiles and placeholder piece objects
    {
        [SerializeField] private BoardViewConfig boardViewConfig = new BoardViewConfig(); // stores configurable board and piece rendering settings
        [SerializeField] private Transform tileRoot; // optional parent transform used to organize generated board tiles in the hierarchy
        [SerializeField] private Transform pieceRoot; // optional parent transform used to organize generated piece placeholders in the hierarchy

        private readonly Dictionary<PieceId, PieceView> pieceViews = new Dictionary<PieceId, PieceView>(); // tracks currently rendered piece views by runtime piece id for later refresh and sync
        private bool boardBuilt; // prevents rebuilding duplicate board tiles if render is called multiple times

        public void Render(GameState gameState) // renders the current logical board and pieces from the provided runtime match state
        {
            if (!boardBuilt) // builds the board only once because tiles are static presentation objects
            {
                EnsureRootsExist(); // creates fallback hierarchy roots if none were assigned in the inspector
                BuildBoardTiles(); // generates the full 8x8 board tile grid in world space
                boardBuilt = true; // marks board construction complete so later renders only refresh pieces
            }

            ClearPieceViews(); // removes any existing rendered piece placeholders before drawing the current state fresh
            BuildPieceViews(gameState); // generates placeholder piece visuals from the latest runtime match state
        }

        public Vector3 BoardToWorld(BoardCoord boardCoord) // converts a logical board coordinate into a world-space position centered on the board
        {
            float halfBoardSize = 3.5f * boardViewConfig.TileSize; // computes half the visual board width so coordinates can be centered around world origin
            float worldX = (boardCoord.X * boardViewConfig.TileSize) - halfBoardSize; // converts logical x into centered world-space x position
            float worldZ = (boardCoord.Y * boardViewConfig.TileSize) - halfBoardSize; // converts logical y into centered world-space z position
            float worldY = boardViewConfig.PieceHeight; // places pieces slightly above the board surface for visibility

            return new Vector3(worldX, worldY, worldZ); // returns the final world-space position for the logical board square
        }

        private void EnsureRootsExist() // creates hierarchy parent objects when optional roots were not assigned in the inspector
        {
            if (tileRoot == null) // checks whether a board-tile parent has been assigned already
            {
                GameObject tileRootObject = new GameObject("Tiles"); // creates a fallback container object for generated board tiles
                tileRootObject.transform.SetParent(transform, false); // parents the tile container under this renderer object for organization
                tileRoot = tileRootObject.transform; // stores the generated tile root for later use
            }

            if (pieceRoot == null) // checks whether a piece parent has been assigned already
            {
                GameObject pieceRootObject = new GameObject("Pieces"); // creates a fallback container object for generated piece placeholders
                pieceRootObject.transform.SetParent(transform, false); // parents the piece container under this renderer object for organization
                pieceRoot = pieceRootObject.transform; // stores the generated piece root for later use
            }
        }

        private void BuildBoardTiles() // generates all 64 visual board squares using primitive cubes
        {
            for (int x = 0; x < 8; x++) // iterates across the board files from left to right
            {
                for (int y = 0; y < 8; y++) // iterates across the board ranks from bottom to top
                {
                    BoardCoord coord = new BoardCoord(x, y); // constructs the logical board coordinate for this tile
                    GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // creates a cube primitive to act as a simple board square
                    tileObject.name = $"Tile_{x}_{y}"; // names the tile clearly for hierarchy readability and debugging
                    tileObject.transform.SetParent(tileRoot, false); // parents the tile under the tile root for scene organization

                    Vector3 tilePosition = BoardToWorld(coord); // converts the logical coordinate into a centered world-space position
                    tilePosition.y = 0.0f; // keeps the board tile itself at ground level rather than piece height
                    tileObject.transform.position = tilePosition; // places the tile at the correct board-square world position

                    tileObject.transform.localScale = new Vector3(boardViewConfig.TileSize, 0.1f, boardViewConfig.TileSize); // sizes the tile to the configured board square size with a thin board-like thickness

                    Renderer rendererComponent = tileObject.GetComponent<Renderer>(); // retrieves the tile renderer so its color can be set
                    if (rendererComponent != null) // ensures the tile has a renderer before changing material color
                    {
                        bool isLightTile = (x + y) % 2 == 0; // determines whether this square should use the light or dark alternating board color
                        rendererComponent.material.color = isLightTile ? boardViewConfig.LightTileColor : boardViewConfig.DarkTileColor; // applies alternating board colors
                    }
                }
            }
        }

        private void BuildPieceViews(GameState gameState) // generates simple placeholder piece visuals for every live piece in the provided runtime match state
        {
            foreach (PieceState pieceState in gameState.Pieces.Values) // iterates through every runtime piece in the simulation state
            {
                if (!pieceState.IsAlive) // skips captured or dead pieces because they should not be shown on the board
                    continue; // continues to the next runtime piece

                GameObject pieceObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // creates a cylinder primitive as a temporary placeholder piece mesh
                pieceObject.transform.SetParent(pieceRoot, false); // parents the generated piece under the piece root for scene organization
                pieceObject.transform.position = BoardToWorld(pieceState.Position); // places the placeholder on the correct square from logical board coordinates
                pieceObject.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f); // gives the placeholder a compact piece-like size that fits on a single tile

                PieceView pieceView = pieceObject.AddComponent<PieceView>(); // attaches the piece view component so the object carries runtime identity metadata
                Color pieceColor = pieceState.Owner == PlayerId.White ? boardViewConfig.WhitePieceColor : boardViewConfig.BlackPieceColor; // selects color based on owning player
                pieceView.Initialize(pieceState.Id, pieceState.Type, pieceState.Owner, pieceState.Position, pieceColor); // initializes the view with runtime identity and visual appearance

                pieceViews[pieceState.Id] = pieceView; // stores the generated view in the lookup dictionary for future refresh and sync operations
            }
        }

        private void ClearPieceViews() // destroys all currently rendered piece placeholders so the latest state can be drawn fresh
        {
            foreach (PieceView pieceView in pieceViews.Values) // iterates through every tracked rendered piece placeholder
            {
                if (pieceView != null) // ensures the view still exists before destroying its gameobject
                {
                    Destroy(pieceView.gameObject); // removes the old visual object so stale piece positions do not remain on screen
                }
            }

            pieceViews.Clear(); // clears the runtime piece-view lookup after all old visual objects are removed
        }

        public BoardCoord WorldToBoard(Vector3 worldPos) // converts a world-space point on the board surface into the nearest logical board coordinate
        {
            float halfBoardSize = 3.5f * boardViewConfig.TileSize; // computes half the visual board width so centered world coordinates can be shifted back into board-space
            int x = Mathf.RoundToInt((worldPos.x + halfBoardSize) / boardViewConfig.TileSize); // converts centered world x back into a zero-based board file index
            int y = Mathf.RoundToInt((worldPos.z + halfBoardSize) / boardViewConfig.TileSize); // converts centered world z back into a zero-based board rank index

            return new BoardCoord(x, y); // returns the reconstructed logical board coordinate nearest to the clicked world position
        }
    }
}
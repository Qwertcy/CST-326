using UnityEngine; // provides unity types like serializefield, color, vector3, and scriptable configuration support

namespace WarcraftChess.Runtime.Presentation // defines the namespace for visual board rendering configuration
{
    [System.Serializable] // allows this plain c# class to be editable in the unity inspector when embedded in a monobehaviour
    public sealed class BoardViewConfig // stores board rendering settings used by the board renderer
    {
        [SerializeField] private float tileSize = 1.0f; // controls the world-space size of each board square
        [SerializeField] private float pieceHeight = 0.5f; // controls how high piece placeholders sit above the board
        [SerializeField] private Color lightTileColor = new Color(0.9f, 0.9f, 0.9f); // sets the color used for light board squares
        [SerializeField] private Color darkTileColor = new Color(0.2f, 0.2f, 0.2f); // sets the color used for dark board squares
        [SerializeField] private Color whitePieceColor = new Color(0.95f, 0.95f, 0.95f); // sets the color used for white piece placeholders
        [SerializeField] private Color blackPieceColor = new Color(0.1f, 0.1f, 0.1f); // sets the color used for black piece placeholders

        public float TileSize => tileSize; // exposes the configured tile size as a read-only property for renderer use
        public float PieceHeight => pieceHeight; // exposes the configured piece height as a read-only property for renderer use
        public Color LightTileColor => lightTileColor; // exposes the configured light tile color as a read-only property for renderer use
        public Color DarkTileColor => darkTileColor; // exposes the configured dark tile color as a read-only property for renderer use
        public Color WhitePieceColor => whitePieceColor; // exposes the configured white piece color as a read-only property for renderer use
        public Color BlackPieceColor => blackPieceColor; // exposes the configured black piece color as a read-only property for renderer use
    }
}
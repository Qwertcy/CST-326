using UnityEngine; // provides monobehaviour and renderer access for visual piece placeholders
using WarcraftChess.Runtime.Core; // imports piece identity and ownership types used for view metadata

namespace WarcraftChess.Runtime.Presentation // defines the namespace for visual piece placeholder components
{
    public sealed class PieceView : MonoBehaviour // stores view-side metadata and appearance updates for a rendered piece placeholder
    {
        public PieceId PieceId { get; private set; } // stores which runtime piece this visual object represents
        public PieceType PieceType { get; private set; } // stores the base piece type for debugging and future visual swapping
        public PlayerId Owner { get; private set; } // stores which player owns the represented piece
        public BoardCoord BoardCoord { get; private set; } // stores the logical board coordinate this view currently represents

        public void Initialize(PieceId pieceId, PieceType pieceType, PlayerId owner, BoardCoord boardCoord, Color color) // initializes this piece view with runtime identity data and visual color
        {
            PieceId = pieceId; // stores the runtime piece id on the view so future sync systems can find it
            PieceType = pieceType; // stores the represented base piece type for future art/model decisions
            Owner = owner; // stores the represented owner for future interaction filtering
            BoardCoord = boardCoord; // stores the represented logical position for future sync and debugging

            Renderer rendererComponent = GetComponent<Renderer>(); // retrieves the renderer used to display the placeholder mesh
            if (rendererComponent != null) // ensures a renderer exists before applying visual color
            {
                rendererComponent.material.color = color; // colors the placeholder based on the owning player
            }

            gameObject.name = $"{owner}_{pieceType}_{boardCoord.X}_{boardCoord.Y}"; // gives the visual object a readable name in the hierarchy for debugging
        }
    }
}
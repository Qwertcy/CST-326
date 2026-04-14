using WarcraftChess.Runtime.Core; // imports core types like pieceid, boardcoord, and piecetype

namespace WarcraftChess.Runtime.Core // defines namespace for command data structures
{
    public readonly struct MoveCommand // represents a requested piece movement action with optional promotion data
    {
        public PieceId PieceId { get; } // identifies which piece is attempting to move
        public BoardCoord From { get; } // stores the starting coordinate of the move
        public BoardCoord To { get; } // stores the target coordinate of the move
        public PieceType? PromotionPieceType { get; } // stores the requested promotion result when a pawn reaches the final rank

        public MoveCommand(PieceId pieceId, BoardCoord from, BoardCoord to, PieceType? promotionPieceType = null) // constructs a move request with optional promotion information
        {
            PieceId = pieceId; // assigns the piece identity
            From = from; // assigns the origin position
            To = to; // assigns the destination position
            PromotionPieceType = promotionPieceType; // assigns the optional promotion target piece type
        }
    }
}
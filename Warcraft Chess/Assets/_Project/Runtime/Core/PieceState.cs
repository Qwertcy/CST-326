using System; // provides serializable support for unity-friendly data types

namespace WarcraftChess.Runtime.Core
{ // defines the namespace for core piece runtime state
    [Serializable] // lets unity serialize this class if it appears inside other serializable objects
    public sealed class PieceState // stores the runtime state of a single piece on or off the board
    {
        public bool HasMoved; // tracks whether the piece has moved at least once
        public PieceId Id; // unique identifier for this specific piece instance
        public PlayerId Owner; // identifies which player currently owns the piece
        public PieceType Type; // stores the base chess identity of the piece
        public PawnFaction Faction; // stores the pawn faction or none for non-pawns
        public bool IsAlive; // tracks whether the piece is still active in the match
        public BoardCoord Position; // stores the current board position when the piece is alive
        public int Armour; // stores current armour value for future warcraft effects
        public bool HasBattleKing; // tracks whether battle king is active on this king
        public bool HasMatriarchy; // tracks whether matriarchy is active on this queen
        public bool IsGhoul; // tracks whether this pawn is currently in ghoul state
        public bool IsFortified; // tracks whether this rook is currently fortified
        public bool IsOrcKnight; // tracks whether this pawn has the orc knight transformation
    }
}
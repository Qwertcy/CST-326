namespace WarcraftChess.Runtime.Core
{ // defines the namespace for shared core types
    public enum PlayerId // identifies which side owns a piece or turn
    {
        White = 0, // first player / white side
        Black = 1 // second player / black side
    }

    public enum PieceType // identifies the base chess piece category
    {
        Pawn = 0, // standard pawn piece
        Rook = 1, // standard rook piece
        Knight = 2, // standard knight piece
        Bishop = 3, // standard bishop piece
        Queen = 4, // standard queen piece
        King = 5 // standard king piece
    }

    public enum PawnFaction // faction tag used only for pawns in warcraft chess
    {
        None = 0, // used for non-pawn pieces or unassigned values
        Human = 1, // hearts faction
        Undead = 2, // spades faction
        Orc = 3, // clubs faction
        Elf = 4 // diamonds faction
    }
}
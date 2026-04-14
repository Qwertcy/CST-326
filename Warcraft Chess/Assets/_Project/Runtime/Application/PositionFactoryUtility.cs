using WarcraftChess.Runtime.Core; // imports core state types used for constructing custom scenarios

namespace WarcraftChess.Runtime.Application // defines namespace for shared application-layer scenario construction helpers
{
    public static class PositionFactoryUtility // provides reusable helper methods for creating custom board states in factories
    {
        public static PlayerState CreatePlayer(PlayerId playerId) // creates a minimal player state for custom scenario factories
        {
            PlayerState playerState = new PlayerState(); // allocates a new player state object
            playerState.Id = playerId; // assigns the owning player id
            playerState.MaterialCount = 0; // initializes material count to zero for custom scenarios
            playerState.CheckersPiecesRemaining = 7; // initializes checker markers to the project default value
            return playerState; // returns the initialized player state
        }

        public static void AddPiece(GameState gameState, PlayerId owner, PieceType type, PawnFaction faction, BoardCoord position) // creates and places a single piece into a custom scenario
        {
            PieceState pieceState = new PieceState(); // allocates a new runtime piece object
            pieceState.Id = PieceId.NewId(); // assigns a unique id to the piece
            pieceState.Owner = owner; // stores which player owns the piece
            pieceState.Type = type; // stores the base chess piece type
            pieceState.Faction = faction; // stores faction information or none for standard chess pieces
            pieceState.IsAlive = true; // marks the piece as alive on the board
            pieceState.Position = position; // stores the logical board position
            pieceState.Armour = 0; // initializes armour to zero for standard chess testing
            pieceState.HasBattleKing = false; // initializes battle king state as inactive
            pieceState.HasMatriarchy = false; // initializes matriarchy state as inactive
            pieceState.IsGhoul = false; // initializes ghoul state as inactive
            pieceState.IsFortified = false; // initializes fortify state as inactive
            pieceState.IsOrcKnight = false; // initializes orc knight state as inactive
            pieceState.HasMoved = false; // initializes the piece as never having moved so setup state is clean

            gameState.Pieces[pieceState.Id] = pieceState; // stores the piece in the runtime piece registry
            gameState.Board.PlacePiece(position, pieceState.Id); // places the piece onto the logical board occupancy map
        }

        public static PieceState FindPiece(GameState gameState, PlayerId owner, PieceType pieceType, BoardCoord position) // searches the piece registry for a live piece matching owner, type, and coordinate
        {
            foreach (PieceState piece in gameState.Pieces.Values) // iterates through every runtime piece in the current match
            {
                if (!piece.IsAlive) // skips dead pieces because they should not be used in scenario lookups
                    continue; // continues scanning the remaining pieces

                if (piece.Owner != owner) // skips pieces owned by the wrong side
                    continue; // continues scanning the remaining pieces

                if (piece.Type != pieceType) // skips pieces of the wrong base type
                    continue; // continues scanning the remaining pieces

                if (piece.Position != position) // skips pieces not located on the requested square
                    continue; // continues scanning the remaining pieces

                return piece; // returns the first piece that matches all requested criteria
            }

            return null; // returns null when no matching piece exists in the registry
        }
    }
}
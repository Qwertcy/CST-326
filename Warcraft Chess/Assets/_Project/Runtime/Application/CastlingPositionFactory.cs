using WarcraftChess.Runtime.Core; // imports core state types used to build custom castling scenarios

namespace WarcraftChess.Runtime.Application // defines namespace for application-layer custom position factories
{
    public static class CastlingPositionFactory // builds custom board states for castling tests
    {
        public static GameState CreateSimpleKingsideCastlingPosition() // creates a simple board where white can legally castle kingside
        {
            GameState gameState = new GameState(); // allocates a new game state for the custom scenario

            gameState.CurrentPlayer = PlayerId.White; // makes it white's turn so white castling can be evaluated
            gameState.TurnNumber = 1; // initializes the turn counter for the scenario
            gameState.RandomSeed = 2001; // assigns a placeholder deterministic seed for consistency

            gameState.Players[PlayerId.White] = PositionFactoryUtility.CreatePlayer(PlayerId.White); // creates the white player state using the shared scenario helper
            gameState.Players[PlayerId.Black] = PositionFactoryUtility.CreatePlayer(PlayerId.Black); // creates the black player state using the shared scenario helper

            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.King, PawnFaction.None, new BoardCoord(4, 0)); // places the white king on e1 so kingside castling can be tested
            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.Rook, PawnFaction.None, new BoardCoord(7, 0)); // places the white rook on h1 as the kingside castling rook
            PositionFactoryUtility.AddPiece(gameState, PlayerId.Black, PieceType.King, PawnFaction.None, new BoardCoord(4, 7)); // places the black king safely away so the position remains a valid chess state

            gameState.ActionLog.Add("simple kingside castling test position created"); // records scenario creation in the log for debugging
            return gameState; // returns the finished custom castling scenario
        }
    }
}
using WarcraftChess.Runtime.Core; // imports core state types used to build custom endgame scenarios

namespace WarcraftChess.Runtime.Application // defines namespace for application-layer custom position factories
{
    public static class CheckmatePositionFactory // builds custom board states for checkmate and stalemate testing
    {
        public static GameState CreateSimpleCheckmatePosition() // creates a simple checkmate position with black trapped in the corner by white king and queen
        {
            GameState gameState = new GameState(); // allocates a new game state for the custom scenario

            gameState.CurrentPlayer = PlayerId.Black; // makes it black's turn so black's legal move availability can be evaluated
            gameState.TurnNumber = 1; // initializes the turn counter for the scenario
            gameState.RandomSeed = 1001; // assigns a placeholder deterministic seed for consistency

            gameState.Players[PlayerId.White] = PositionFactoryUtility.CreatePlayer(PlayerId.White); // creates the white player state using the shared scenario helper
            gameState.Players[PlayerId.Black] = PositionFactoryUtility.CreatePlayer(PlayerId.Black); // creates the black player state using the shared scenario helper

            PositionFactoryUtility.AddPiece(gameState, PlayerId.Black, PieceType.King, PawnFaction.None, new BoardCoord(0, 7)); // places the black king on a8 as the checkmated side
            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.Queen, PawnFaction.None, new BoardCoord(1, 6)); // places the white queen on b7 delivering the mating attack
            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.King, PawnFaction.None, new BoardCoord(2, 5)); // places the white king on c6 supporting the queen and covering escape squares

            gameState.ActionLog.Add("simple checkmate test position created"); // records scenario creation in the log for debugging
            return gameState; // returns the finished custom checkmate scenario
        }
    }
}
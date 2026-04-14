using WarcraftChess.Runtime.Core; // imports core state types used to build custom promotion scenarios

namespace WarcraftChess.Runtime.Application // defines namespace for application-layer custom position factories
{
    public static class PromotionPositionFactory // builds custom board states for pawn promotion testing
    {
        public static GameState CreateSimplePromotionPosition() // creates a simple position where a white pawn can promote on the next move
        {
            GameState gameState = new GameState(); // allocates a new game state for the custom scenario

            gameState.CurrentPlayer = PlayerId.White; // makes it white's turn so the white pawn can be promoted immediately
            gameState.TurnNumber = 1; // initializes the turn counter for the scenario
            gameState.RandomSeed = 4001; // assigns a placeholder deterministic seed for consistency

            gameState.Players[PlayerId.White] = PositionFactoryUtility.CreatePlayer(PlayerId.White); // creates the white player state using the shared scenario helper
            gameState.Players[PlayerId.Black] = PositionFactoryUtility.CreatePlayer(PlayerId.Black); // creates the black player state using the shared scenario helper

            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.King, PawnFaction.None, new BoardCoord(4, 0)); // places the white king on e1 so the position remains a valid chess state
            PositionFactoryUtility.AddPiece(gameState, PlayerId.Black, PieceType.King, PawnFaction.None, new BoardCoord(4, 7)); // places the black king on e8 so the position remains a valid chess state
            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.Pawn, PawnFaction.None, new BoardCoord(0, 6)); // places the white pawn on a7 ready to promote on a8

            gameState.ActionLog.Add("simple promotion test position created"); // records scenario creation in the log for debugging
            return gameState; // returns the finished custom promotion scenario
        }
    }
}
using WarcraftChess.Runtime.Core; // imports core state types used to build custom en passant scenarios
using WarcraftChess.Runtime.Rules; // imports piece move execution used to simulate the prior pawn double move

namespace WarcraftChess.Runtime.Application // defines namespace for application-layer custom position factories
{
    public static class EnPassantPositionFactory // builds custom board states for en passant testing
    {
        public static GameState CreateEnPassantPosition() // creates a position where white can legally capture en passant on the next move
        {
            GameState gameState = new GameState(); // allocates a new game state for the custom scenario

            gameState.CurrentPlayer = PlayerId.Black; // makes it black's turn first so the required double-pawn move can be simulated legally
            gameState.TurnNumber = 1; // initializes the turn counter for the scenario
            gameState.RandomSeed = 3001; // assigns a placeholder deterministic seed for consistency

            gameState.Players[PlayerId.White] = PositionFactoryUtility.CreatePlayer(PlayerId.White); // creates the white player state using the shared helper
            gameState.Players[PlayerId.Black] = PositionFactoryUtility.CreatePlayer(PlayerId.Black); // creates the black player state using the shared helper

            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.King, PawnFaction.None, new BoardCoord(4, 0)); // places the white king on e1 so the position remains a valid chess state
            PositionFactoryUtility.AddPiece(gameState, PlayerId.Black, PieceType.King, PawnFaction.None, new BoardCoord(4, 7)); // places the black king on e8 so the position remains a valid chess state
            PositionFactoryUtility.AddPiece(gameState, PlayerId.White, PieceType.Pawn, PawnFaction.None, new BoardCoord(4, 4)); // places the white pawn on e5 ready to perform en passant
            PositionFactoryUtility.AddPiece(gameState, PlayerId.Black, PieceType.Pawn, PawnFaction.None, new BoardCoord(5, 6)); // places the black pawn on f7 ready for the double-step move

            PieceState blackPawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.Black, PieceType.Pawn, new BoardCoord(5, 6)); // finds the black pawn on f7 for the simulated setup move

            if (blackPawn == null) // guards against null lookup failures during scenario construction
            {
                return gameState; // returns early defensively if setup somehow failed
            }

            MoveCommand blackDoubleMove = new MoveCommand(blackPawn.Id, new BoardCoord(5, 6), new BoardCoord(5, 4)); // constructs the black pawn's two-square move from f7 to f5
            PieceMoveExecutor.ExecuteMove(gameState, blackDoubleMove); // executes the black double-step move so last-move state and turn order are set correctly for en passant
            gameState.ActionLog.Add("en passant test position created"); // records scenario creation in the log for debugging

            return gameState; // returns the finished custom en passant scenario where it is now white's turn
        }
    }
}
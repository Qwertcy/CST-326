using NUnit.Framework; // provides the unit test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports factories and shared scenario helpers used to build test states
using WarcraftChess.Runtime.Core; // imports core state types like gamestate, movecommand, and boardcoord
using WarcraftChess.Runtime.Rules; // imports move validation and execution systems under test

namespace WarcraftChess.Tests.EditMode // defines the namespace for edit mode rules-engine tests
{
    public sealed class PieceMoveExecutorTests // groups tests that verify state mutation, capture handling, and turn progression
    {
        [Test] // marks this method as a runnable unit test
        public void ExecuteMove_UpdatesTurnAndTurnNumber() // verifies that executing a valid move switches the turn and increments the turn counter
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated execution testing
            PieceState pawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1)); // finds the white pawn on a2

            Assert.That(pawn, Is.Not.Null); // confirms the expected pawn exists before executing the test move

            MoveCommand move = new MoveCommand(pawn.Id, new BoardCoord(0, 1), new BoardCoord(0, 3)); // constructs the white pawn opening double move from a2 to a4
            Assert.That(PieceMoveValidator.IsMoveLegal(gameState, move), Is.True); // confirms the move is legal before mutating game state

            PieceMoveExecutor.ExecuteMove(gameState, move); // applies the move to the game state so turn and board mutation can be verified

            Assert.That(gameState.CurrentPlayer, Is.EqualTo(PlayerId.Black)); // confirms the turn switches to black after white's move
            Assert.That(gameState.TurnNumber, Is.EqualTo(2)); // confirms the turn counter advances after execution
            Assert.That(pawn.Position, Is.EqualTo(new BoardCoord(0, 3))); // confirms the pawn's logical position updates to the destination square
        }

        [Test] // marks this method as a runnable unit test
        public void ExecuteMove_Capture_RemovesEnemyPiece() // verifies that a normal capture removes the enemy piece and records it as captured
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated execution testing

            PieceState whitePawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1)); // finds the white pawn on a2
            PieceState blackPawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.Black, PieceType.Pawn, new BoardCoord(1, 6)); // finds the black pawn on b7

            Assert.That(whitePawn, Is.Not.Null); // confirms the white pawn exists before the setup sequence
            Assert.That(blackPawn, Is.Not.Null); // confirms the black pawn exists before the setup sequence

            MoveCommand whiteDouble = new MoveCommand(whitePawn.Id, new BoardCoord(0, 1), new BoardCoord(0, 3)); // constructs white's opening double move from a2 to a4
            MoveCommand blackDouble = new MoveCommand(blackPawn.Id, new BoardCoord(1, 6), new BoardCoord(1, 4)); // constructs black's opening double move from b7 to b5

            PieceMoveExecutor.ExecuteMove(gameState, whiteDouble); // executes white's opening double move
            PieceMoveExecutor.ExecuteMove(gameState, blackDouble); // executes black's opening double move

            PieceState updatedWhitePawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 3)); // re-finds the white pawn on a4 after setup moves
            Assert.That(updatedWhitePawn, Is.Not.Null); // confirms the white pawn is in the expected pre-capture square

            MoveCommand capture = new MoveCommand(updatedWhitePawn.Id, new BoardCoord(0, 3), new BoardCoord(1, 4)); // constructs the normal pawn capture from a4 to b5
            Assert.That(PieceMoveValidator.IsMoveLegal(gameState, capture), Is.True); // confirms the capture is geometrically legal before execution

            PieceMoveExecutor.ExecuteMove(gameState, capture); // executes the pawn capture so board and piece state can be verified afterward

            PieceState capturedBlackPawn = gameState.Pieces[blackPawn.Id]; // retrieves the originally tracked black pawn instance from the registry after capture
            Assert.That(capturedBlackPawn.IsAlive, Is.False); // confirms the captured black pawn is marked dead
            Assert.That(PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(1, 4)), Is.Not.Null); // confirms the white pawn occupies the capture destination square
            Assert.That(gameState.Players[PlayerId.White].CapturedEnemyPieces.Contains(blackPawn.Id), Is.True); // confirms the capture is recorded in white's captured piece list
        }
    }
}
using NUnit.Framework; // provides the unit test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports match and scenario factories used to build test states
using WarcraftChess.Runtime.Core; // imports core data types like gamestate, movecommand, and boardcoord
using WarcraftChess.Runtime.Rules; // imports the piece move validator under test

namespace WarcraftChess.Tests.EditMode // defines the namespace for edit mode rules-engine tests
{
    public sealed class PieceMoveValidatorTests // groups tests that verify baseline geometric move validation
    {
        [Test] // marks this method as a runnable unit test
        public void WhitePawn_OneSquareForward_IsLegal() // verifies that a white pawn can move one square forward from its starting position
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated testing
            PieceState pawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1)); // finds the white pawn on a2 in zero-based coordinates

            Assert.That(pawn, Is.Not.Null); // confirms the setup contains the expected pawn before validating the move

            MoveCommand move = new MoveCommand(pawn.Id, new BoardCoord(0, 1), new BoardCoord(0, 2)); // constructs the pawn move from a2 to a3
            bool isLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // evaluates baseline geometric legality for the pawn move

            Assert.That(isLegal, Is.True); // confirms the validator accepts the normal one-square pawn advance
        }

        [Test] // marks this method as a runnable unit test
        public void WhitePawn_TwoSquareOpeningMove_IsLegal() // verifies that a white pawn can move two squares from its starting rank when unobstructed
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated testing
            PieceState pawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1)); // finds the white pawn on a2

            Assert.That(pawn, Is.Not.Null); // confirms the expected pawn exists before validating the move

            MoveCommand move = new MoveCommand(pawn.Id, new BoardCoord(0, 1), new BoardCoord(0, 3)); // constructs the pawn move from a2 to a4
            bool isLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // evaluates baseline geometric legality for the opening double move

            Assert.That(isLegal, Is.True); // confirms the validator accepts the normal two-square opening pawn advance
        }

        [Test] // marks this method as a runnable unit test
        public void WhiteKnight_ToC3_IsLegal() // verifies that the white knight on b1 can legally jump to c3
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated testing
            PieceState knight = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Knight, new BoardCoord(1, 0)); // finds the white knight on b1

            Assert.That(knight, Is.Not.Null); // confirms the expected knight exists before validating the move

            MoveCommand move = new MoveCommand(knight.Id, new BoardCoord(1, 0), new BoardCoord(2, 2)); // constructs the knight move from b1 to c3
            bool isLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // evaluates baseline geometric legality for the knight jump

            Assert.That(isLegal, Is.True); // confirms the validator accepts the legal knight jump
        }

        [Test] // marks this method as a runnable unit test
        public void WhiteBishop_BlockedAtStart_IsIllegal() // verifies that the white bishop on c1 cannot move through its blocking pawn in the starting position
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated testing
            PieceState bishop = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Bishop, new BoardCoord(2, 0)); // finds the white bishop on c1

            Assert.That(bishop, Is.Not.Null); // confirms the expected bishop exists before validating the move

            MoveCommand move = new MoveCommand(bishop.Id, new BoardCoord(2, 0), new BoardCoord(5, 3)); // constructs the bishop move from c1 to f4 through blocking pawns
            bool isLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // evaluates baseline geometric legality for the blocked bishop move

            Assert.That(isLegal, Is.False); // confirms the validator rejects blocked diagonal movement
        }

        [Test] // marks this method as a runnable unit test
        public void WhitePawn_DiagonalWithoutTarget_IsIllegal() // verifies that a pawn cannot move diagonally unless capturing or using en passant
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated testing
            PieceState pawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1)); // finds the white pawn on a2

            Assert.That(pawn, Is.Not.Null); // confirms the expected pawn exists before validating the move

            MoveCommand move = new MoveCommand(pawn.Id, new BoardCoord(0, 1), new BoardCoord(1, 2)); // constructs the diagonal pawn move from a2 to b3 without an enemy target
            bool isLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // evaluates baseline geometric legality for the empty diagonal pawn move

            Assert.That(isLegal, Is.False); // confirms the validator rejects diagonal pawn movement when no capture rule applies
        }
    }
}
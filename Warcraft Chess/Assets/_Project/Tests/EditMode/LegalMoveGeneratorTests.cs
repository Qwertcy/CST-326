using System.Collections.Generic; // provides list support for legal move collections returned by the generator
using NUnit.Framework; // provides the unit test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports factories and shared helpers used to build test positions
using WarcraftChess.Runtime.Core; // imports core state types like gamestate and pieceid
using WarcraftChess.Runtime.Rules; // imports the legal move generator under test

namespace WarcraftChess.Tests.EditMode // defines the namespace for edit mode rules-engine tests
{
    public sealed class LegalMoveGeneratorTests // groups tests that verify fully legal move generation
    {
        [Test] // marks this method as a runnable unit test
        public void StartingKnight_HasTwoLegalMoves() // verifies that the white knight on b1 has exactly two legal opening moves
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated move-generation testing
            PieceState knight = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Knight, new BoardCoord(1, 0)); // finds the white knight on b1

            Assert.That(knight, Is.Not.Null); // confirms the expected knight exists before generating moves

            List<MoveCommand> moves = LegalMoveGenerator.GetLegalMovesForPiece(gameState, knight.Id); // generates all fully legal moves for the knight

            Assert.That(moves.Count, Is.EqualTo(2)); // confirms the knight has the standard two opening moves from b1
        }

        [Test] // marks this method as a runnable unit test
        public void StartingBishop_HasZeroLegalMoves() // verifies that the white bishop on c1 has no legal opening moves because pawns block it
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated move-generation testing
            PieceState bishop = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Bishop, new BoardCoord(2, 0)); // finds the white bishop on c1

            Assert.That(bishop, Is.Not.Null); // confirms the expected bishop exists before generating moves

            List<MoveCommand> moves = LegalMoveGenerator.GetLegalMovesForPiece(gameState, bishop.Id); // generates all fully legal moves for the bishop

            Assert.That(moves.Count, Is.EqualTo(0)); // confirms the bishop has no legal moves in the starting position
        }

        [Test] // marks this method as a runnable unit test
        public void StartingWhitePlayer_HasTwentyLegalMoves() // verifies that white has the canonical twenty legal moves in the classical starting position
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard starting position for isolated move-generation testing

            List<MoveCommand> moves = LegalMoveGenerator.GetLegalMovesForPlayer(gameState, PlayerId.White); // generates all fully legal opening moves for white

            Assert.That(moves.Count, Is.EqualTo(20)); // confirms the standard opening move count of sixteen pawn moves plus four knight moves
        }
    }
}
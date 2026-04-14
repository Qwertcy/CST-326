using NUnit.Framework; // provides the unit test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports scenario factories used to build custom legality test states
using WarcraftChess.Runtime.Core; // imports core state types like gamestate, boardcoord, and movecommand
using WarcraftChess.Runtime.Rules; // imports the move validator and legal move service under test

namespace WarcraftChess.Tests.EditMode // defines the namespace for edit mode rules-engine tests
{
    public sealed class LegalMoveServiceTests // groups tests that verify full legal move filtering beyond basic geometry
    {
        [Test] // marks this method as a runnable unit test
        public void PinnedRookMove_IsBaseLegalButNotFullyLegal() // verifies that a pinned rook move remains geometrically legal but is rejected by king-safety legality checks
        {
            GameState gameState = PinnedPositionFactory.CreatePinnedRookPosition(); // creates the custom pinned-rook scenario used to test self-check prevention
            PieceState rook = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Rook, new BoardCoord(4, 1)); // finds the pinned white rook on e2

            Assert.That(rook, Is.Not.Null); // confirms the pinned rook exists before building the sideways move command

            MoveCommand move = new MoveCommand(rook.Id, new BoardCoord(4, 1), new BoardCoord(5, 1)); // constructs the rook move from e2 to f2 that would expose the white king

            bool baseLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // evaluates geometric legality without king-safety rules
            bool fullyLegal = LegalMoveService.IsMoveFullyLegal(gameState, move); // evaluates full chess legality including self-check prevention

            Assert.That(baseLegal, Is.True); // confirms the rook move is geometrically valid
            Assert.That(fullyLegal, Is.False); // confirms the move is rejected because it leaves the king in check
        }
    }
}
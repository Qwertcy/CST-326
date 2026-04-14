using NUnit.Framework; // provides the unit test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports factories used to construct test positions
using WarcraftChess.Runtime.Core; // imports core state types used by the evaluator
using WarcraftChess.Runtime.Rules; // imports the check evaluator under test

namespace WarcraftChess.Tests.EditMode // defines the namespace for edit mode rules-engine tests
{
    public sealed class CheckEvaluatorTests // groups tests that verify king-check threat detection
    {
        [Test] // marks this method as a runnable unit test
        public void StartingPosition_NeitherKingIsInCheck() // verifies that the classical chess starting position does not begin with either king in check
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh standard chess starting position for isolated check evaluation

            bool whiteInCheck = CheckEvaluator.IsKingInCheck(gameState, PlayerId.White); // evaluates whether white begins the game in check
            bool blackInCheck = CheckEvaluator.IsKingInCheck(gameState, PlayerId.Black); // evaluates whether black begins the game in check

            Assert.That(whiteInCheck, Is.False); // confirms the starting position is safe for white
            Assert.That(blackInCheck, Is.False); // confirms the starting position is safe for black
        }
    }
}
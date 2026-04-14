using NUnit.Framework; // provides the unit test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports the custom checkmate scenario factory used for endgame testing
using WarcraftChess.Runtime.Core; // imports core state types used by the endgame evaluator
using WarcraftChess.Runtime.Rules; // imports the check and endgame evaluators under test

namespace WarcraftChess.Tests.EditMode // defines the namespace for edit mode rules-engine tests
{
    public sealed class EndgameEvaluatorTests // groups tests that verify checkmate and stalemate evaluation
    {
        [Test] // marks this method as a runnable unit test
        public void SimpleCheckmatePosition_BlackIsInCheckmate() // verifies that the custom corner-mate scenario is recognized as checkmate for black
        {
            GameState gameState = CheckmatePositionFactory.CreateSimpleCheckmatePosition(); // creates the custom endgame scenario with black trapped in the corner

            bool blackInCheck = CheckEvaluator.IsKingInCheck(gameState, PlayerId.Black); // evaluates whether black's king is under attack in the scenario
            bool blackInCheckmate = EndgameEvaluator.IsCheckmate(gameState, PlayerId.Black); // evaluates whether the scenario is recognized as checkmate
            bool blackInStalemate = EndgameEvaluator.IsStalemate(gameState, PlayerId.Black); // evaluates whether the scenario is incorrectly recognized as stalemate

            Assert.That(blackInCheck, Is.True); // confirms the black king is actually in check in the mating position
            Assert.That(blackInCheckmate, Is.True); // confirms the endgame evaluator recognizes the position as checkmate
            Assert.That(blackInStalemate, Is.False); // confirms the mating position is not confused with stalemate
        }
    }
}
using NUnit.Framework; // provides the test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports the local match factory under test
using WarcraftChess.Runtime.Core; // imports the core runtime state types used in assertions

namespace WarcraftChess.Tests.EditMode
{ // defines the namespace for edit mode tests
    public sealed class GameStateFactoryTests // test container for initial match creation behavior
    {
        [Test] // marks this method as a runnable unit test
        public void CreateStandardChessMatch_CreatesThirtyTwoPieces() // verifies the standard opening setup creates the correct number of pieces
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh test match with a fixed seed
            Assert.That(gameState.Pieces.Count, Is.EqualTo(32)); // confirms all standard chess pieces were created
        }

        [Test] // marks this method as a runnable unit test
        public void CreateStandardChessMatch_StartsWithWhiteTurn() // verifies the default starting side is white
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh test match with a fixed seed
            Assert.That(gameState.CurrentPlayer, Is.EqualTo(PlayerId.White)); // confirms white is the starting player
        }

        [Test] // marks this method as a runnable unit test
        public void CreateStandardChessMatch_PlacesWhiteKingCorrectly() // verifies a key major piece starts on the expected square
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh test match with a fixed seed

            PieceState whiteKing = null; // stores the matching white king once found

            foreach (PieceState piece in gameState.Pieces.Values) // scans all pieces to find the white king
            {
                if (piece.Owner == PlayerId.White && piece.Type == PieceType.King) // filters down to the expected piece
                {
                    whiteKing = piece; // stores the matched piece for assertion
                    break; // exits the loop once the target is found
                }
            }

            Assert.That(whiteKing, Is.Not.Null); // confirms the white king exists in the piece registry
            Assert.That(whiteKing.Position, Is.EqualTo(new BoardCoord(4, 0))); // confirms the white king starts on e1 in zero-based coordinates
        }
    }
}
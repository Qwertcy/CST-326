using NUnit.Framework; // provides the test framework attributes and assertions
using WarcraftChess.Runtime.Core; // imports the type under test

namespace WarcraftChess.Tests.EditMode
{ // defines the namespace for edit mode tests
    public sealed class BoardCoordTests // test container for board coordinate behavior
    {
        [Test] // marks this method as a runnable unit test
        public void IsInBounds_ReturnsTrue_ForValidCoordinate() // verifies normal valid board positions work
        {
            BoardCoord coord = new BoardCoord(3, 5); // creates a coordinate inside the 8x8 board
            Assert.That(coord.IsInBounds(), Is.True); // confirms the coordinate is accepted as valid
        }

        [Test] // marks this method as a runnable unit test
        public void IsInBounds_ReturnsFalse_ForInvalidCoordinate() // verifies invalid positions are rejected
        {
            BoardCoord coord = new BoardCoord(8, 2); // creates a coordinate outside board width
            Assert.That(coord.IsInBounds(), Is.False); // confirms the coordinate is rejected
        }

        [Test] // marks this method as a runnable unit test
        public void Equality_Works_ForSameValues() // verifies value equality rather than reference identity
        {
            BoardCoord a = new BoardCoord(2, 2); // first coordinate instance
            BoardCoord b = new BoardCoord(2, 2); // second coordinate instance with same values
            Assert.That(a == b, Is.True); // confirms overloaded equality compares stored values
        }
    }
}
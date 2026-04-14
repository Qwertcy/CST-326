using System; // provides interfaces like iequatable for value comparison

namespace WarcraftChess.Runtime.Core
{ // defines the namespace for shared board coordinate logic
    [Serializable] // lets unity serialize this struct when embedded in other serializable data
    public readonly struct BoardCoord : IEquatable<BoardCoord> // immutable board coordinate value type
    {
        public int X { get; } // zero-based board file coordinate from 0 to 7
        public int Y { get; } // zero-based board rank coordinate from 0 to 7

        public BoardCoord(int x, int y) // constructs a coordinate from raw x and y values
        {
            X = x; // stores the x component
            Y = y; // stores the y component
        }

        public bool IsInBounds() // checks whether the coordinate is on the 8x8 board
        {
            return X >= 0 && X < 8 && Y >= 0 && Y < 8; // returns true only for valid board positions
        }

        public bool Equals(BoardCoord other) // typed equality check for efficient value comparisons
        {
            return X == other.X && Y == other.Y; // two coordinates are equal when both components match
        }

        public override bool Equals(object obj) // boxed equality override required for value semantics
        {
            return obj is BoardCoord other && Equals(other); // safely compares only when the object is a boardcoord
        }

        public override int GetHashCode() // allows this struct to be used in hash collections like dictionaries
        {
            return HashCode.Combine(X, Y); // combines x and y into a stable hash code
        }

        public override string ToString() // useful for logs and debugging output
        {
            return $"({X}, {Y})"; // prints a readable coordinate representation
        }

        public static bool operator ==(BoardCoord left, BoardCoord right) // equality operator for direct comparisons
        {
            return left.Equals(right); // delegates to the typed equality implementation
        }

        public static bool operator !=(BoardCoord left, BoardCoord right) // inequality operator for direct comparisons
        {
            return !left.Equals(right); // negates the typed equality result
        }
    }
}
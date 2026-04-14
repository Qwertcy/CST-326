using System; // provides guid support for unique identifiers

namespace WarcraftChess.Runtime.Core
{ // defines the namespace for shared core identity types
    [Serializable] // lets unity serialize this value when embedded in other serializable types
    public readonly struct PieceId : IEquatable<PieceId> // immutable unique identifier for a single piece instance
    {
        public string Value { get; } // stores the unique identifier text

        public PieceId(string value) // constructs a piece id from a provided string
        {
            Value = value; // stores the incoming identifier
        }

        public static PieceId NewId() // creates a brand new unique piece identifier
        {
            return new PieceId(Guid.NewGuid().ToString("N")); // uses a guid without dashes for compact stable ids
        }

        public bool Equals(PieceId other) // typed equality comparison for efficient value semantics
        {
            return Value == other.Value; // ids are equal when their stored string matches
        }

        public override bool Equals(object obj) // boxed equality override for collections and general comparisons
        {
            return obj is PieceId other && Equals(other); // safely compares only compatible values
        }

        public override int GetHashCode() // allows this value to work in hash-based collections
        {
            return Value != null ? Value.GetHashCode() : 0; // hashes the stored string or returns zero when null
        }

        public override string ToString() // useful for logs and debugging
        {
            return Value; // returns the stored identifier text
        }

        public static bool operator ==(PieceId left, PieceId right) // equality operator for direct comparisons
        {
            return left.Equals(right); // delegates to the typed equality implementation
        }

        public static bool operator !=(PieceId left, PieceId right) // inequality operator for direct comparisons
        {
            return !left.Equals(right); // negates the typed equality result
        }
    }
}
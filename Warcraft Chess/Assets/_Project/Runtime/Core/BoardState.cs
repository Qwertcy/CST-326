using System.Collections.Generic; // provides dictionary support for fast board occupancy lookup

namespace WarcraftChess.Runtime.Core
{ // defines the namespace for board occupancy state
    public sealed class BoardState // stores which pieces occupy which logical board coordinates
    {
        private readonly Dictionary<BoardCoord, PieceId> _occupancy = new Dictionary<BoardCoord, PieceId>(); // maps board squares to occupying piece ids

        public bool IsOccupied(BoardCoord coord) // checks whether a given square currently contains a piece
        {
            return _occupancy.ContainsKey(coord); // returns true when a piece id is mapped to that coordinate
        }

        public bool TryGetPieceIdAt(BoardCoord coord, out PieceId pieceId) // safely reads the occupying piece id at a square
        {
            return _occupancy.TryGetValue(coord, out pieceId); // returns whether a piece is present and outputs its id if found
        }

        public void PlacePiece(BoardCoord coord, PieceId pieceId) // places a piece id onto a square
        {
            _occupancy[coord] = pieceId; // inserts or replaces the occupant at the target square
        }

        public void RemovePiece(BoardCoord coord) // removes any occupant from a square
        {
            _occupancy.Remove(coord); // deletes the mapping if one exists
        }

        public IReadOnlyDictionary<BoardCoord, PieceId> GetAllOccupants() // exposes a read-only view of the full board occupancy map
        {
            return _occupancy; // returns the underlying dictionary as a read-only interface
        }
    }
}
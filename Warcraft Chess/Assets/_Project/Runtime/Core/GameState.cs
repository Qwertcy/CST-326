using System.Collections.Generic; // provides dictionary and list support for overall match state

namespace WarcraftChess.Runtime.Core
{ // defines the namespace for the full runtime match state
    public sealed class GameState // stores the complete deterministic state of a single match
    {
        public BoardState Board = new BoardState(); // stores board occupancy independent of presentation objects
        public Dictionary<PieceId, PieceState> Pieces = new Dictionary<PieceId, PieceState>(); // stores all piece instances by unique id
        public Dictionary<PlayerId, PlayerState> Players = new Dictionary<PlayerId, PlayerState>(); // stores both player states keyed by side

        public PlayerId CurrentPlayer = PlayerId.White; // stores which player currently has the turn
        public int TurnNumber = 1; // stores the match turn counter starting from one
        public int RandomSeed; // stores the match seed for future deterministic randomness
        public List<string> ActionLog = new List<string>(); // stores simple human-readable log entries for now
        public MoveCommand? LastMove; // stores the most recent executed move
    }
}
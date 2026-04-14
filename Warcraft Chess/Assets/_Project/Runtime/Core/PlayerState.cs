using System.Collections.Generic; // provides list support for player-owned runtime collections

namespace WarcraftChess.Runtime.Core
{ // defines the namespace for player runtime state
    public sealed class PlayerState // stores the runtime state for one player in a match
    {
        public PlayerId Id; // identifies which side this player state belongs to
        public int MaterialCount; // stores current material resource available to spend
        public int CheckersPiecesRemaining; // stores remaining checker markers available to place

        public List<string> DrawPile = new List<string>(); // stores the player's draw pile as simple card ids for now
        public List<string> Graveyard = new List<string>(); // stores used cards that may later be reshuffled
        public List<string> OutOfPlay = new List<string>(); // stores one-time-use cards removed from circulation
        public List<string> Hand = new List<string>(); // stores cards currently available to the player
        public List<PieceId> CapturedEnemyPieces = new List<PieceId>(); // tracks enemy pieces this player has captured
    }
}
using WarcraftChess.Runtime.Core; // imports core game state types used for evaluation

namespace WarcraftChess.Runtime.Rules // defines namespace for check and threat evaluation systems
{
    public static class CheckEvaluator // evaluates attack relationships and king safety
    {
        public static bool IsSquareUnderAttack(GameState gameState, BoardCoord targetSquare, PlayerId defendingPlayer) // determines whether a square is threatened by any enemy piece
        {
            PlayerId attackingPlayer = defendingPlayer == PlayerId.White ? PlayerId.Black : PlayerId.White; // identifies the opposing player

            foreach (PieceState piece in gameState.Pieces.Values) // iterates through all pieces in the game state
            {
                if (!piece.IsAlive) // skips pieces that are no longer active
                    continue; // dead pieces cannot attack

                if (piece.Owner != attackingPlayer) // filters to only enemy pieces
                    continue; // ignore friendly pieces

                MoveCommand simulatedAttack = new MoveCommand(piece.Id, piece.Position, targetSquare); // constructs a hypothetical attack move

                if (PieceMoveValidator.IsMoveLegal(gameState, simulatedAttack, true)) // checks whether this piece could legally attack the target square while ignoring turn ownership
                    return true; // if any enemy piece can attack the square, it is under attack
            }

            return false; // no enemy piece can attack the square
        }

        public static bool IsKingInCheck(GameState gameState, PlayerId player) // determines whether a given player's king is currently under attack
        {
            PieceState king = FindKing(gameState, player); // locates the king belonging to the specified player

            if (king == null) // ensures the king exists
                return false; // defensive fallback (should not happen in normal play)

            return IsSquareUnderAttack(gameState, king.Position, player); // checks whether the king's square is threatened
        }

        private static PieceState FindKing(GameState gameState, PlayerId player) // searches for the king belonging to a specific player
        {
            foreach (PieceState piece in gameState.Pieces.Values) // iterates through all pieces
            {
                if (!piece.IsAlive) // skips dead pieces
                    continue; // continue searching

                if (piece.Owner != player) // filters to pieces belonging to the target player
                    continue; // continue searching

                if (piece.Type == PieceType.King) // checks for king type
                    return piece; // returns the king when found
            }

            return null; // returns null if no king is found (should not happen in a valid game)
        }
    }
}
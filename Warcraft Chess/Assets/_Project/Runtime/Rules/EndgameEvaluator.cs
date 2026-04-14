using System.Collections.Generic; // provides list support for legal move collection
using WarcraftChess.Runtime.Core; // imports core state types used for endgame evaluation

namespace WarcraftChess.Runtime.Rules // defines namespace for checkmate and stalemate evaluation systems
{
    public static class EndgameEvaluator // determines whether a player is in checkmate or stalemate
    {
        public static bool IsCheckmate(GameState gameState, PlayerId player) // determines whether the specified player is in checkmate
        {
            if (!CheckEvaluator.IsKingInCheck(gameState, player)) // ensures the player's king is actually in check first
                return false; // a player cannot be in checkmate if they are not currently in check

            List<MoveCommand> legalMoves = LegalMoveGenerator.GetLegalMovesForPlayer(gameState, player); // generates all fully legal moves for the player

            return legalMoves.Count == 0; // checkmate occurs when the player is in check and has no legal moves
        }

        public static bool IsStalemate(GameState gameState, PlayerId player) // determines whether the specified player is in stalemate
        {
            if (CheckEvaluator.IsKingInCheck(gameState, player)) // ensures the player's king is not currently in check
                return false; // a player cannot be in stalemate if they are in check

            List<MoveCommand> legalMoves = LegalMoveGenerator.GetLegalMovesForPlayer(gameState, player); // generates all fully legal moves for the player

            return legalMoves.Count == 0; // stalemate occurs when the player is not in check and has no legal moves
        }
    }
}
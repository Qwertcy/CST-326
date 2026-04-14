using System.Collections.Generic; // provides dictionary support for cloning runtime state collections
using WarcraftChess.Runtime.Core; // imports core state types used for move simulation

namespace WarcraftChess.Runtime.Rules // defines namespace for higher-level chess legality checks
{
    public static class LegalMoveService // validates full chess legality including self-check prevention
    {
        public static bool IsMoveFullyLegal(GameState gameState, MoveCommand command) // checks whether a move is legal both geometrically and strategically
        {
            if (!PieceMoveValidator.IsMoveLegal(gameState, command)) // first checks baseline movement legality using the existing piece movement validator
                return false; // rejects moves that already fail basic movement rules

            PieceState originalPiece = gameState.Pieces[command.PieceId]; // retrieves the moving piece from the original game state before simulation

            if (IsCastlingMove(command, originalPiece)) // checks whether this move is a castling move that needs extra king-path attack validation
            {
                if (!IsCastlingFullyLegal(gameState, command, originalPiece)) // enforces the additional castling-specific legality rules before simulation
                    return false; // rejects castling when any of those special conditions fail
            }

            if (IsPromotionMove(originalPiece, command)) // checks whether this move requires a promotion choice because a pawn reaches the last rank
            {
                if (!IsValidPromotionPieceType(command.PromotionPieceType)) // ensures the command provides a legal promotion target piece
                    return false; // rejects promotion moves without a valid classical chess promotion piece selection
            }
            else if (command.PromotionPieceType.HasValue) // rejects promotion data on moves that are not actually promotion moves
            {
                return false; // prevents unrelated moves from carrying invalid extra promotion payload
            }

            GameState simulatedState = CloneGameState(gameState); // creates a detached copy of the current game state for safe simulation
            PieceMoveExecutor.ExecuteMove(simulatedState, command); // applies the move to the cloned state so king safety can be evaluated afterward

            PieceState movedPiece = simulatedState.Pieces[command.PieceId]; // retrieves the moved piece from the cloned state after execution
            PlayerId movingPlayer = movedPiece.Owner; // stores the side that made the move so their king can be checked

            return !CheckEvaluator.IsKingInCheck(simulatedState, movingPlayer); // move is fully legal only if the moving player's king is not left in check
        }

        private static bool IsCastlingMove(MoveCommand command, PieceState piece) // determines whether a move command represents a king castling move
        {
            return piece.Type == PieceType.King && command.From.Y == command.To.Y && System.Math.Abs(command.To.X - command.From.X) == 2; // identifies the king's two-square horizontal move as castling
        }

        private static bool IsCastlingFullyLegal(GameState gameState, MoveCommand command, PieceState king) // enforces the extra chess legality rules for castling beyond basic movement shape
        {
            if (CheckEvaluator.IsKingInCheck(gameState, king.Owner)) // castling is illegal while the king is currently in check
                return false; // rejects castling out of check

            int direction = command.To.X > command.From.X ? 1 : -1; // determines whether the castle is kingside or queenside from the target square
            BoardCoord stepSquare = new BoardCoord(command.From.X + direction, command.From.Y); // computes the square the king passes through on the way to the destination

            if (CheckEvaluator.IsSquareUnderAttack(gameState, stepSquare, king.Owner)) // ensures the king does not pass through an attacked square while castling
                return false; // rejects castling through check

            if (CheckEvaluator.IsSquareUnderAttack(gameState, command.To, king.Owner)) // ensures the king does not land on an attacked square while castling
                return false; // rejects castling into check

            return true; // returns true once all extra castling legality rules are satisfied
        }

        private static bool IsPromotionMove(PieceState piece, MoveCommand command) // determines whether this move causes a pawn to reach its final promotion rank
        {
            if (piece.Type != PieceType.Pawn) // only pawns can promote
                return false; // rejects all non-pawn moves

            int promotionRank = piece.Owner == PlayerId.White ? 7 : 0; // selects the final rank based on the pawn's owning side
            return command.To.Y == promotionRank; // promotion occurs when the pawn reaches its final board rank
        }

        private static bool IsValidPromotionPieceType(PieceType? promotionPieceType) // validates that the chosen promotion target is one of the four legal classical chess promotion pieces
        {
            if (!promotionPieceType.HasValue) // promotion requires an explicit piece choice
                return false; // rejects missing promotion choices

            return promotionPieceType.Value == PieceType.Queen // allows promotion to queen
                || promotionPieceType.Value == PieceType.Rook // allows promotion to rook
                || promotionPieceType.Value == PieceType.Bishop // allows promotion to bishop
                || promotionPieceType.Value == PieceType.Knight; // allows promotion to knight
        }

        private static GameState CloneGameState(GameState source) // creates a deep-enough clone of the game state for move simulation
        {
            GameState clone = new GameState(); // allocates a new root game state instance

            clone.CurrentPlayer = source.CurrentPlayer; // copies the current turn owner
            clone.TurnNumber = source.TurnNumber; // copies the current turn counter
            clone.RandomSeed = source.RandomSeed; // copies the deterministic seed value
            clone.LastMove = source.LastMove; // copies the latest executed move into the cloned state

            foreach (KeyValuePair<PlayerId, PlayerState> pair in source.Players) // copies both player state records into the clone
            {
                PlayerState copiedPlayer = new PlayerState(); // allocates a new player state instance for the clone
                copiedPlayer.Id = pair.Value.Id; // copies the player id
                copiedPlayer.MaterialCount = pair.Value.MaterialCount; // copies current material count
                copiedPlayer.CheckersPiecesRemaining = pair.Value.CheckersPiecesRemaining; // copies remaining checker markers

                copiedPlayer.DrawPile = new List<string>(pair.Value.DrawPile); // copies draw pile contents into a new list instance
                copiedPlayer.Graveyard = new List<string>(pair.Value.Graveyard); // copies graveyard contents into a new list instance
                copiedPlayer.OutOfPlay = new List<string>(pair.Value.OutOfPlay); // copies out-of-play pile contents into a new list instance
                copiedPlayer.Hand = new List<string>(pair.Value.Hand); // copies current hand contents into a new list instance
                copiedPlayer.CapturedEnemyPieces = new List<PieceId>(pair.Value.CapturedEnemyPieces); // copies captured piece ids into a new list instance

                clone.Players[pair.Key] = copiedPlayer; // stores the cloned player state in the new game state
            }

            foreach (KeyValuePair<PieceId, PieceState> pair in source.Pieces) // copies every runtime piece state into the clone
            {
                PieceState copiedPiece = new PieceState(); // allocates a new piece state object for the clone
                copiedPiece.Id = pair.Value.Id; // copies the unique piece id
                copiedPiece.Owner = pair.Value.Owner; // copies piece ownership
                copiedPiece.Type = pair.Value.Type; // copies base piece type
                copiedPiece.Faction = pair.Value.Faction; // copies pawn faction or none
                copiedPiece.IsAlive = pair.Value.IsAlive; // copies alive/dead state
                copiedPiece.Position = pair.Value.Position; // copies board position
                copiedPiece.Armour = pair.Value.Armour; // copies armour value
                copiedPiece.HasBattleKing = pair.Value.HasBattleKing; // copies battle king state
                copiedPiece.HasMatriarchy = pair.Value.HasMatriarchy; // copies matriarchy state
                copiedPiece.IsGhoul = pair.Value.IsGhoul; // copies ghoul state
                copiedPiece.IsFortified = pair.Value.IsFortified; // copies fortify state
                copiedPiece.IsOrcKnight = pair.Value.IsOrcKnight; // copies orc knight transformation state
                copiedPiece.HasMoved = pair.Value.HasMoved; // copies whether the piece has moved before

                clone.Pieces[pair.Key] = copiedPiece; // stores the cloned piece in the cloned piece registry
            }

            foreach (KeyValuePair<BoardCoord, PieceId> pair in source.Board.GetAllOccupants()) // copies all board occupancy mappings into the clone
            {
                clone.Board.PlacePiece(pair.Key, pair.Value); // recreates the same board occupancy in the cloned board state
            }

            clone.ActionLog = new List<string>(source.ActionLog); // copies action log entries into a new list instance

            return clone; // returns the completed cloned game state for isolated simulation
        }
    }
}
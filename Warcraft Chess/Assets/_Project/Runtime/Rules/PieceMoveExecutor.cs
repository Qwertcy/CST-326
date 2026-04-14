using WarcraftChess.Runtime.Core; // imports core state types

namespace WarcraftChess.Runtime.Rules // defines namespace for rule execution systems
{
    public static class PieceMoveExecutor // applies validated moves to the game state
    {
        public static void ExecuteMove(GameState gameState, MoveCommand command) // performs a move on the board including special handling for castling and en passant
        {
            PieceState piece = gameState.Pieces[command.PieceId]; // retrieves the moving piece from the piece registry

            bool isCastlingMove = piece.Type == PieceType.King && command.From.Y == command.To.Y && System.Math.Abs(command.To.X - command.From.X) == 2; // determines whether this move is a king castling move
            bool isEnPassantMove = IsEnPassantExecution(gameState, piece, command); // determines whether this move is specifically an en passant capture

            if (isEnPassantMove) // handles the special case where a pawn captures en passant onto an empty destination square
            {
                MoveCommand lastMove = gameState.LastMove.Value; // retrieves the last move that enabled en passant
                PieceState targetPawn = gameState.Pieces[lastMove.PieceId]; // retrieves the pawn being captured en passant

                targetPawn.IsAlive = false; // marks the captured pawn as dead
                gameState.Board.RemovePiece(targetPawn.Position); // removes the captured pawn from its current square on the board
                gameState.Players[piece.Owner].CapturedEnemyPieces.Add(targetPawn.Id); // records the capture for the moving player
            }
            else if (gameState.Board.TryGetPieceIdAt(command.To, out PieceId targetId)) // handles the normal capture case where the destination square is occupied
            {
                PieceState target = gameState.Pieces[targetId]; // retrieves the captured piece from the registry
                target.IsAlive = false; // marks the captured piece as dead
                gameState.Board.RemovePiece(command.To); // removes the captured piece from the destination square
                gameState.Players[piece.Owner].CapturedEnemyPieces.Add(targetId); // records the capture for the moving player
            }

            gameState.Board.RemovePiece(command.From); // clears the moving piece's original square from board occupancy
            gameState.Board.PlacePiece(command.To, command.PieceId); // places the moving piece onto its destination square
            piece.Position = command.To; // updates the moving piece's stored logical position

            if (piece.Type == PieceType.Pawn) // checks whether the moving piece is a pawn that may now require promotion handling
            {
                int promotionRank = piece.Owner == PlayerId.White ? 7 : 0; // selects the final rank based on the pawn's owning side

                if (piece.Position.Y == promotionRank && command.PromotionPieceType.HasValue) // promotes the pawn when it reaches the last rank with a valid promotion choice supplied
                {
                    piece.Type = command.PromotionPieceType.Value; // changes the pawn's base piece type to the chosen promotion type
                    piece.Faction = PawnFaction.None; // clears pawn-faction data because the promoted piece is no longer treated as a pawn in base chess
                }
            }

            if (isCastlingMove) // handles the rook side-effect when the king performs a castling move
            {
                ExecuteCastlingRookMove(gameState, piece, command); // moves the corresponding rook to its castled square
            }

            piece.HasMoved = true; // marks the moving piece as having moved at least once
            gameState.LastMove = command; // stores this move as the latest executed move in the match state

            gameState.CurrentPlayer = gameState.CurrentPlayer == PlayerId.White ? PlayerId.Black : PlayerId.White; // switches the active turn to the opposing player
            gameState.TurnNumber++; // advances the turn counter after the move completes

            gameState.ActionLog.Add($"{piece.Owner} moved {piece.Type} from {command.From} to {command.To}"); // appends a human-readable move entry to the action log
        }

        private static bool IsEnPassantExecution(GameState gameState, PieceState piece, MoveCommand command) // determines whether the current pawn move should execute as an en passant capture
        {
            if (piece.Type != PieceType.Pawn) // en passant only applies to pawns
                return false; // rejects all non-pawn moves

            if (!gameState.LastMove.HasValue) // en passant requires a previous move to evaluate
                return false; // rejects if there is no last move

            if (gameState.Board.IsOccupied(command.To)) // en passant only lands on an empty destination square
                return false; // rejects if the destination is occupied because that would be a normal capture instead

            int dx = command.To.X - command.From.X; // calculates horizontal movement distance for the current pawn move
            int dy = command.To.Y - command.From.Y; // calculates vertical movement distance for the current pawn move
            int direction = piece.Owner == PlayerId.White ? 1 : -1; // determines forward direction for the moving pawn

            if (System.Math.Abs(dx) != 1 || dy != direction) // en passant must be a one-file diagonal forward move
                return false; // rejects if the move shape does not match en passant capture geometry

            MoveCommand lastMove = gameState.LastMove.Value; // retrieves the previously executed move

            if (!gameState.Pieces.TryGetValue(lastMove.PieceId, out PieceState lastPiece)) // finds the piece that made the previous move
                return false; // rejects if the previous piece cannot be found

            if (lastPiece.Type != PieceType.Pawn) // en passant only applies when the previous move was a pawn move
                return false; // rejects if the previous mover was not a pawn

            if (lastPiece.Owner == piece.Owner) // en passant must target an enemy pawn
                return false; // rejects if the previous pawn belongs to the same player

            if (System.Math.Abs(lastMove.To.Y - lastMove.From.Y) != 2) // en passant only applies after a two-square pawn advance
                return false; // rejects if the previous pawn did not move two squares

            if (lastPiece.Position.Y != piece.Position.Y) // the enemy pawn must end adjacent on the same rank as the capturing pawn
                return false; // rejects if the pawns are not aligned on the same rank

            if (System.Math.Abs(lastPiece.Position.X - piece.Position.X) != 1) // the enemy pawn must be exactly one file away
                return false; // rejects if the enemy pawn is not horizontally adjacent

            if (command.To.X != lastPiece.Position.X || command.To.Y != piece.Position.Y + direction) // the capture destination must match the square behind the moved pawn
                return false; // rejects if the command does not target the exact en passant capture square

            return true; // confirms that this move should execute as an en passant capture
        }

        private static void ExecuteCastlingRookMove(GameState gameState, PieceState king, MoveCommand command) // moves the correct rook to its new square when a king castles
        {
            int direction = command.To.X > command.From.X ? 1 : -1; // determines whether the castle is kingside or queenside from the king destination
            int rookFromX = direction > 0 ? 7 : 0; // selects the starting rook file associated with the castling side
            int rookToX = command.To.X - direction; // computes the rook destination square adjacent to the king's final square

            BoardCoord rookFrom = new BoardCoord(rookFromX, command.From.Y); // computes the rook's starting coordinate on the same back rank as the king
            BoardCoord rookTo = new BoardCoord(rookToX, command.From.Y); // computes the rook's destination coordinate after castling

            if (!gameState.Board.TryGetPieceIdAt(rookFrom, out PieceId rookId)) // confirms that the expected rook is present on its starting square
                return; // exits defensively if the rook cannot be found even though validation should already have guaranteed it

            PieceState rook = gameState.Pieces[rookId]; // retrieves the rook piece state from the piece registry

            gameState.Board.RemovePiece(rookFrom); // clears the rook's original square from board occupancy
            gameState.Board.PlacePiece(rookTo, rookId); // places the rook onto its castled destination square
            rook.Position = rookTo; // updates the rook's stored logical position
            rook.HasMoved = true; // marks the rook as having moved so castling cannot be repeated with it later
        }
    }
}
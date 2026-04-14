using System; // provides math helpers like abs for movement calculations
using WarcraftChess.Runtime.Core; // imports core state types used for validation

namespace WarcraftChess.Runtime.Rules // defines namespace for movement validation systems
{
    public static class PieceMoveValidator // validates baseline chess movement legality for pieces
    {
        public static bool IsMoveLegal(GameState gameState, MoveCommand command, bool ignoreTurn = false) // adds optional flag to bypass turn ownership checks for simulations
        {
            if (!gameState.Pieces.TryGetValue(command.PieceId, out PieceState piece)) // tries to find the moving piece in the game state's registry
                return false; // invalid if the piece does not exist

            if (!piece.IsAlive) // checks whether the piece is still alive on the board
                return false; // dead pieces cannot move

            if (!ignoreTurn && piece.Owner != gameState.CurrentPlayer)
                return false;

            if (command.From != piece.Position) // confirms the command origin matches the piece's actual stored position
                return false; // invalid if the command claims the piece starts somewhere else

            if (!command.From.IsInBounds()) // confirms the origin coordinate is still on the board
                return false; // invalid if the origin is outside the 8x8 board

            if (!command.To.IsInBounds()) // confirms the destination coordinate is on the board
                return false; // invalid if the destination is outside the 8x8 board

            if (command.From == command.To) // rejects moves that do not change squares
                return false; // zero-distance moves are not legal moves

            return piece.Type switch // dispatches to the appropriate piece-specific movement rule
            {
                PieceType.Pawn => IsPawnMoveLegal(gameState, piece, command), // validates pawn movement and capture behavior
                PieceType.Rook => IsRookMoveLegal(gameState, piece, command), // validates rook straight-line movement
                PieceType.Knight => IsKnightMoveLegal(gameState, piece, command), // validates knight jumping movement
                PieceType.Bishop => IsBishopMoveLegal(gameState, piece, command), // validates bishop diagonal movement
                PieceType.Queen => IsQueenMoveLegal(gameState, piece, command), // validates queen rook-or-bishop style movement
                PieceType.King => IsKingMoveLegal(gameState, piece, command), // validates king single-step movement
                _ => false // rejects unknown piece types defensively
            };
        }

        private static bool IsPawnMoveLegal(GameState gameState, PieceState piece, MoveCommand command) // validates standard pawn movement rules
        {
            int direction = piece.Owner == PlayerId.White ? 1 : -1; // sets forward direction based on which side owns the pawn
            int startRank = piece.Owner == PlayerId.White ? 1 : 6; // sets the rank from which this pawn may move two squares initially

            int dx = command.To.X - command.From.X; // calculates horizontal movement distance
            int dy = command.To.Y - command.From.Y; // calculates vertical movement distance

            if (dx == 0 && dy == direction) // checks the standard one-square forward move
            {
                return !gameState.Board.IsOccupied(command.To); // pawn may move forward only if the destination square is empty
            }

            if (dx == 0 && dy == direction * 2 && command.From.Y == startRank) // checks the opening two-square forward move from the correct start rank
            {
                BoardCoord intermediate = new BoardCoord(command.From.X, command.From.Y + direction); // computes the square between origin and destination
                return !gameState.Board.IsOccupied(intermediate) && !gameState.Board.IsOccupied(command.To); // both the intermediate and destination squares must be empty
            }

            if (Math.Abs(dx) == 1 && dy == direction) // checks diagonal pawn movement pattern
            {
                if (IsDestinationOccupiedByEnemy(gameState, piece, command.To)) // normal diagonal capture case
                    return true; // valid when capturing an enemy piece on the destination square

                if (IsEnPassantLegal(gameState, piece, command)) // checks whether this diagonal move qualifies as an en passant capture
                    return true; // valid when en passant conditions are satisfied

                return false; // rejects diagonal movement when neither normal capture nor en passant applies
            }

            return false; // rejects all other pawn movement shapes
        }

        private static bool IsEnPassantLegal(GameState gameState, PieceState piece, MoveCommand command) // validates en passant capture conditions using the last move state
        {
            if (gameState.LastMove == null) // ensures there is a previous move to evaluate
                return false; // en passant cannot occur without a prior move

            MoveCommand lastMove = gameState.LastMove.Value; // retrieves the last executed move

            if (!gameState.Pieces.TryGetValue(lastMove.PieceId, out PieceState lastPiece)) // finds the piece that performed the last move
                return false; // fails safely if the last piece cannot be found

            if (lastPiece.Type != PieceType.Pawn) // en passant only applies to pawns
                return false; // rejects if the last move was not a pawn

            if (lastPiece.Owner == piece.Owner) // ensures the last move was made by the opponent
                return false; // rejects if it was the same player

            int lastMoveDistance = Math.Abs(lastMove.To.Y - lastMove.From.Y); // computes how far the last pawn moved vertically

            if (lastMoveDistance != 2) // en passant only applies to two-square pawn advances
                return false; // rejects if the last move was not a double advance

            if (lastMove.To.Y != piece.Position.Y) // ensures the enemy pawn is now adjacent horizontally on the same rank
                return false; // rejects if the pawns are not side-by-side

            if (Math.Abs(lastMove.To.X - piece.Position.X) != 1) // ensures the enemy pawn is exactly one file away
                return false; // rejects if not horizontally adjacent

            int direction = piece.Owner == PlayerId.White ? 1 : -1; // determines forward direction for the capturing pawn
            BoardCoord expectedTarget = new BoardCoord(lastMove.To.X, piece.Position.Y + direction); // computes the en passant capture square

            if (command.To != expectedTarget) // ensures the move matches the exact en passant capture square
                return false; // rejects if the move is not targeting the correct square

            return true; // returns true when all en passant conditions are satisfied
        }

        private static bool IsRookMoveLegal(GameState gameState, PieceState piece, MoveCommand command) // validates rook movement along ranks or files
        {
            int dx = command.To.X - command.From.X; // calculates horizontal movement distance
            int dy = command.To.Y - command.From.Y; // calculates vertical movement distance

            if (dx != 0 && dy != 0) // rook must move only horizontally or vertically
                return false; // diagonal movement is illegal for a rook

            if (!IsPathClear(gameState, command.From, command.To)) // checks that no piece blocks the path between origin and destination
                return false; // rook cannot jump over blocking pieces

            return IsDestinationEmptyOrEnemy(gameState, piece, command.To); // rook may end on an empty square or capture an enemy piece
        }

        private static bool IsKnightMoveLegal(GameState gameState, PieceState piece, MoveCommand command) // validates knight L-shaped movement
        {
            int dx = Math.Abs(command.To.X - command.From.X); // calculates absolute horizontal movement distance
            int dy = Math.Abs(command.To.Y - command.From.Y); // calculates absolute vertical movement distance

            bool isKnightPattern = (dx == 1 && dy == 2) || (dx == 2 && dy == 1); // checks the two valid L-shaped knight patterns

            if (!isKnightPattern) // ensures the move follows knight geometry
                return false; // invalid if not a proper knight move

            return IsDestinationEmptyOrEnemy(gameState, piece, command.To); // knight may land on an empty square or capture an enemy piece
        }

        private static bool IsBishopMoveLegal(GameState gameState, PieceState piece, MoveCommand command) // validates bishop diagonal movement
        {
            int dx = command.To.X - command.From.X; // calculates horizontal movement distance
            int dy = command.To.Y - command.From.Y; // calculates vertical movement distance

            if (Math.Abs(dx) != Math.Abs(dy)) // bishop must move the same distance horizontally and vertically
                return false; // invalid if the move is not diagonal

            if (!IsPathClear(gameState, command.From, command.To)) // checks that no piece blocks the diagonal path
                return false; // bishop cannot jump over pieces

            return IsDestinationEmptyOrEnemy(gameState, piece, command.To); // bishop may end on an empty square or capture an enemy piece
        }

        private static bool IsQueenMoveLegal(GameState gameState, PieceState piece, MoveCommand command) // validates queen movement by combining rook and bishop patterns
        {
            return IsRookMoveLegal(gameState, piece, command) || IsBishopMoveLegal(gameState, piece, command); // queen is legal if either rook-style or bishop-style movement is legal
        }

        private static bool IsKingMoveLegal(GameState gameState, PieceState piece, MoveCommand command) // validates standard king movement plus the castling-shaped king move
        {
            int dx = Math.Abs(command.To.X - command.From.X); // calculates absolute horizontal movement distance
            int dy = Math.Abs(command.To.Y - command.From.Y); // calculates absolute vertical movement distance

            bool isOneStepMove = dx <= 1 && dy <= 1 && (dx != 0 || dy != 0); // checks whether this is a normal one-square king move

            if (isOneStepMove) // handles the normal king movement case first
            {
                return IsDestinationEmptyOrEnemy(gameState, piece, command.To); // king may move one square to an empty square or capture an enemy
            }

            bool isCastlingShape = dy == 0 && dx == 2; // checks whether the king is attempting the two-square horizontal castling pattern

            if (isCastlingShape) // handles the geometric castling case separately
            {
                return IsCastlingShapeLegal(gameState, piece, command); // validates only the structural requirements for castling shape
            }

            return false; // rejects all other king movement patterns
        }

        private static bool IsCastlingShapeLegal(GameState gameState, PieceState king, MoveCommand command) // validates the structural board-state conditions for castling before full legality checks
        {
            if (king.HasMoved) // castling is illegal if the king has already moved earlier in the game
                return false; // rejects castling once king movement history is no longer pristine

            int direction = command.To.X > command.From.X ? 1 : -1; // determines whether the castle is kingside or queenside from the king's horizontal destination
            int rookX = direction > 0 ? 7 : 0; // selects the rook file involved in the castle based on the direction
            BoardCoord rookCoord = new BoardCoord(rookX, command.From.Y); // computes the rook's current coordinate on the same back rank as the king

            if (!gameState.Board.TryGetPieceIdAt(rookCoord, out PieceId rookId)) // checks whether a rook actually exists on the expected rook square
                return false; // rejects castling if the rook is missing

            PieceState rook = gameState.Pieces[rookId]; // retrieves the rook piece state from the piece registry

            if (!rook.IsAlive) // ensures the rook is still alive and on the board
                return false; // rejects castling with a captured rook

            if (rook.Owner != king.Owner) // ensures the rook belongs to the same player as the king
                return false; // rejects castling with an enemy rook

            if (rook.Type != PieceType.Rook) // ensures the corner piece is actually a rook
                return false; // rejects castling if the expected rook square contains some other piece

            if (rook.HasMoved) // castling is illegal if the involved rook has moved earlier in the game
                return false; // rejects castling once rook movement history is no longer pristine

            int x = command.From.X + direction; // starts checking the squares between the king and the rook beginning with the next square toward the rook

            while (x != rookX) // iterates over every square between the king and rook but excludes the rook square itself
            {
                BoardCoord between = new BoardCoord(x, command.From.Y); // computes the current square between king and rook

                if (gameState.Board.IsOccupied(between)) // ensures there are no pieces between the king and rook
                    return false; // rejects castling when any piece blocks the path

                x += direction; // advances one square farther toward the rook
            }

            return true; // returns true once the structural castling conditions are satisfied
        }

        private static bool IsDestinationEmptyOrEnemy(GameState gameState, PieceState movingPiece, BoardCoord destination) // checks whether the destination square is either empty or occupied by an enemy
        {
            if (!gameState.Board.TryGetPieceIdAt(destination, out PieceId targetId)) // checks whether any piece occupies the destination square
                return true; // empty destination squares are always allowed from an occupancy standpoint

            PieceState targetPiece = gameState.Pieces[targetId]; // retrieves the piece occupying the destination
            return targetPiece.Owner != movingPiece.Owner; // destination is valid only if the occupant belongs to the opposing player
        }

        private static bool IsDestinationOccupiedByEnemy(GameState gameState, PieceState movingPiece, BoardCoord destination) // checks whether the destination square contains an enemy piece specifically
        {
            if (!gameState.Board.TryGetPieceIdAt(destination, out PieceId targetId)) // checks whether any piece occupies the destination square
                return false; // pawn captures are illegal if there is no target to capture

            PieceState targetPiece = gameState.Pieces[targetId]; // retrieves the destination piece for ownership comparison
            return targetPiece.Owner != movingPiece.Owner; // pawn capture is valid only if the piece is an enemy
        }

        private static bool IsPathClear(GameState gameState, BoardCoord from, BoardCoord to) // checks whether all squares between origin and destination are empty
        {
            int dx = to.X - from.X; // calculates total horizontal distance between origin and destination
            int dy = to.Y - from.Y; // calculates total vertical distance between origin and destination

            int stepX = dx == 0 ? 0 : dx / Math.Abs(dx); // normalizes horizontal travel direction to -1, 0, or 1
            int stepY = dy == 0 ? 0 : dy / Math.Abs(dy); // normalizes vertical travel direction to -1, 0, or 1

            int x = from.X + stepX; // starts checking at the first square after the origin
            int y = from.Y + stepY; // starts checking at the first square after the origin

            while (x != to.X || y != to.Y) // iterates over every intermediate square before the destination
            {
                if (gameState.Board.IsOccupied(new BoardCoord(x, y))) // checks whether an intermediate square contains any blocking piece
                    return false; // path is not clear if any square between origin and destination is occupied

                x += stepX; // advances one square horizontally toward the destination
                y += stepY; // advances one square vertically toward the destination
            }

            return true; // path is clear if no intermediate square was occupied
        }
    }
}
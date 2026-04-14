using System.Collections.Generic; // provides list support for collecting generated move commands
using WarcraftChess.Runtime.Core; // imports core state types like gamestate, pieceid, piecestate, and boardcoord

namespace WarcraftChess.Runtime.Rules // defines namespace for move generation systems
{
    public static class LegalMoveGenerator // generates fully legal move commands for pieces and players
    {
        public static List<MoveCommand> GetLegalMovesForPiece(GameState gameState, PieceId pieceId) // generates all fully legal moves for a single piece instance
        {
            List<MoveCommand> legalMoves = new List<MoveCommand>(); // stores all fully legal moves discovered for the requested piece

            if (!gameState.Pieces.TryGetValue(pieceId, out PieceState piece)) // tries to find the requested piece in the piece registry
                return legalMoves; // returns an empty list if the piece does not exist

            if (!piece.IsAlive) // rejects dead pieces because they cannot generate moves
                return legalMoves; // returns an empty list for dead pieces

            List<BoardCoord> candidateDestinations = GetCandidateDestinationsForPiece(piece); // generates raw geometric candidate destinations based on the piece type

            foreach (BoardCoord destination in candidateDestinations) // evaluates each candidate destination one by one
            {
                MoveCommand move = new MoveCommand(piece.Id, piece.Position, destination); // constructs a move command from the piece's current position to the candidate destination

                if (LegalMoveService.IsMoveFullyLegal(gameState, move)) // filters the candidate through full legality including king safety
                {
                    legalMoves.Add(move); // stores the move only when it passes full legality checks
                }
            }

            return legalMoves; // returns the completed list of fully legal moves for the piece
        }

        public static List<MoveCommand> GetLegalMovesForPlayer(GameState gameState, PlayerId playerId) // generates all fully legal moves for every live piece owned by the specified player
        {
            List<MoveCommand> legalMoves = new List<MoveCommand>(); // stores all fully legal moves found for the player

            foreach (PieceState piece in gameState.Pieces.Values) // iterates through every piece in the match state
            {
                if (!piece.IsAlive) // skips dead pieces because they cannot move
                    continue; // continues scanning remaining pieces

                if (piece.Owner != playerId) // skips pieces that belong to the other player
                    continue; // continues scanning remaining pieces

                legalMoves.AddRange(GetLegalMovesForPiece(gameState, piece.Id)); // generates and appends all legal moves for this one owned piece
            }

            return legalMoves; // returns the complete list of fully legal moves for the requested player
        }

        private static List<BoardCoord> GetCandidateDestinationsForPiece(PieceState piece) // generates raw candidate board squares based only on piece movement patterns and board bounds
        {
            return piece.Type switch // selects the appropriate candidate generation method for the base piece type
            {
                PieceType.Pawn => GetPawnCandidateDestinations(piece), // generates pawn movement and capture candidates
                PieceType.Rook => GetSlidingCandidateDestinations(piece.Position, rookDirections), // generates rook-style sliding candidates
                PieceType.Knight => GetKnightCandidateDestinations(piece.Position), // generates knight jump candidates
                PieceType.Bishop => GetSlidingCandidateDestinations(piece.Position, bishopDirections), // generates bishop-style sliding candidates
                PieceType.Queen => GetSlidingCandidateDestinations(piece.Position, queenDirections), // generates queen-style sliding candidates
                PieceType.King => GetKingCandidateDestinations(piece.Position), // generates king one-step candidates
                _ => new List<BoardCoord>() // defensively returns an empty list for unknown piece types
            };
        }

        private static readonly BoardCoord[] rookDirections = new BoardCoord[] // stores the four cardinal direction vectors used by rook-style movement
        {
            new BoardCoord(1, 0), // one square step to the right
            new BoardCoord(-1, 0), // one square step to the left
            new BoardCoord(0, 1), // one square step upward
            new BoardCoord(0, -1) // one square step downward
        };

        private static readonly BoardCoord[] bishopDirections = new BoardCoord[] // stores the four diagonal direction vectors used by bishop-style movement
        {
            new BoardCoord(1, 1), // one square step up-right
            new BoardCoord(-1, 1), // one square step up-left
            new BoardCoord(1, -1), // one square step down-right
            new BoardCoord(-1, -1) // one square step down-left
        };

        private static readonly BoardCoord[] queenDirections = new BoardCoord[] // stores the eight direction vectors used by queen-style movement
        {
            new BoardCoord(1, 0), // one square step to the right
            new BoardCoord(-1, 0), // one square step to the left
            new BoardCoord(0, 1), // one square step upward
            new BoardCoord(0, -1), // one square step downward
            new BoardCoord(1, 1), // one square step up-right
            new BoardCoord(-1, 1), // one square step up-left
            new BoardCoord(1, -1), // one square step down-right
            new BoardCoord(-1, -1) // one square step down-left
        };

        private static List<BoardCoord> GetPawnCandidateDestinations(PieceState piece) // generates raw pawn candidate destinations without checking occupancy or full legality
        {
            List<BoardCoord> candidates = new List<BoardCoord>(); // stores all in-bounds pawn candidate squares

            int direction = piece.Owner == PlayerId.White ? 1 : -1; // determines forward direction from the owning player's perspective
            int startRank = piece.Owner == PlayerId.White ? 1 : 6; // identifies the rank where the pawn may attempt a two-square opening move

            BoardCoord oneForward = new BoardCoord(piece.Position.X, piece.Position.Y + direction); // computes the one-square forward candidate
            if (oneForward.IsInBounds()) // ensures the one-step forward square lies on the board
            {
                candidates.Add(oneForward); // adds the one-step forward square as a candidate for later legality filtering
            }

            BoardCoord twoForward = new BoardCoord(piece.Position.X, piece.Position.Y + (direction * 2)); // computes the two-square forward candidate
            if (piece.Position.Y == startRank && twoForward.IsInBounds()) // only allows the opening double-step candidate from the proper starting rank
            {
                candidates.Add(twoForward); // adds the two-step forward square as a candidate for later legality filtering
            }

            BoardCoord diagonalLeft = new BoardCoord(piece.Position.X - 1, piece.Position.Y + direction); // computes the forward-left diagonal capture candidate
            if (diagonalLeft.IsInBounds()) // ensures the forward-left diagonal square lies on the board
            {
                candidates.Add(diagonalLeft); // adds the forward-left diagonal as a candidate for later legality filtering
            }

            BoardCoord diagonalRight = new BoardCoord(piece.Position.X + 1, piece.Position.Y + direction); // computes the forward-right diagonal capture candidate
            if (diagonalRight.IsInBounds()) // ensures the forward-right diagonal square lies on the board
            {
                candidates.Add(diagonalRight); // adds the forward-right diagonal as a candidate for later legality filtering
            }

            return candidates; // returns the full set of raw pawn candidate destinations
        }

        private static List<BoardCoord> GetKnightCandidateDestinations(BoardCoord origin) // generates all possible knight jump candidates from an origin square
        {
            List<BoardCoord> candidates = new List<BoardCoord>(); // stores all in-bounds knight candidate squares

            int[] xOffsets = new int[] { -2, -1, 1, 2 }; // stores the possible horizontal offsets for knight movement
            int[] yOffsets = new int[] { -2, -1, 1, 2 }; // stores the possible vertical offsets for knight movement

            foreach (int xOffset in xOffsets) // iterates through each possible horizontal offset
            {
                foreach (int yOffset in yOffsets) // iterates through each possible vertical offset
                {
                    if (System.Math.Abs(xOffset) == System.Math.Abs(yOffset)) // removes offsets that do not form an l-shape because both magnitudes match
                        continue; // skips invalid non-knight offset combinations

                    BoardCoord candidate = new BoardCoord(origin.X + xOffset, origin.Y + yOffset); // computes one potential knight destination from the current offset pair

                    if (candidate.IsInBounds()) // keeps only candidates that remain on the board
                    {
                        candidates.Add(candidate); // stores the in-bounds knight destination for later legality filtering
                    }
                }
            }

            return candidates; // returns all in-bounds knight candidate destinations
        }

        private static List<BoardCoord> GetKingCandidateDestinations(BoardCoord origin) // generates all one-step king candidates plus castling-shaped destinations from an origin square
        {
            List<BoardCoord> candidates = new List<BoardCoord>(); // stores all in-bounds king candidate squares

            for (int xOffset = -1; xOffset <= 1; xOffset++) // iterates through one-square horizontal offsets around the king
            {
                for (int yOffset = -1; yOffset <= 1; yOffset++) // iterates through one-square vertical offsets around the king
                {
                    if (xOffset == 0 && yOffset == 0) // excludes the zero-offset because remaining on the same square is not a move
                        continue; // skips the origin square

                    BoardCoord candidate = new BoardCoord(origin.X + xOffset, origin.Y + yOffset); // computes one neighboring square around the king

                    if (candidate.IsInBounds()) // keeps only neighboring squares that remain on the board
                    {
                        candidates.Add(candidate); // stores the in-bounds king destination for later legality filtering
                    }
                }
            }

            BoardCoord kingsideCastle = new BoardCoord(origin.X + 2, origin.Y); // computes the standard kingside castling destination two files to the right
            if (kingsideCastle.IsInBounds()) // ensures the kingside castling destination remains on the board
            {
                candidates.Add(kingsideCastle); // adds the castling-shaped destination so later legality layers can decide whether it is valid
            }

            BoardCoord queensideCastle = new BoardCoord(origin.X - 2, origin.Y); // computes the standard queenside castling destination two files to the left
            if (queensideCastle.IsInBounds()) // ensures the queenside castling destination remains on the board
            {
                candidates.Add(queensideCastle); // adds the castling-shaped destination so later legality layers can decide whether it is valid
            }

            return candidates; // returns all in-bounds king candidate destinations including castling shapes
        }

        private static List<BoardCoord> GetSlidingCandidateDestinations(BoardCoord origin, BoardCoord[] directions) // generates all in-bounds sliding destinations in the provided directions until board edge
        {
            List<BoardCoord> candidates = new List<BoardCoord>(); // stores all in-bounds sliding candidate squares

            foreach (BoardCoord direction in directions) // iterates through each allowed sliding direction for the piece
            {
                int x = origin.X + direction.X; // starts one square away from the origin horizontally in the current direction
                int y = origin.Y + direction.Y; // starts one square away from the origin vertically in the current direction

                while (true) // continues stepping outward until the board boundary is reached
                {
                    BoardCoord candidate = new BoardCoord(x, y); // computes the current sliding destination under consideration

                    if (!candidate.IsInBounds()) // stops extending in this direction once the board edge is passed
                        break; // exits the current directional scan

                    candidates.Add(candidate); // stores the in-bounds sliding candidate for later legality filtering

                    x += direction.X; // advances one more square horizontally along the current direction
                    y += direction.Y; // advances one more square vertically along the current direction
                }
            }

            return candidates; // returns all in-bounds sliding destinations for the given movement directions
        }
    }
}
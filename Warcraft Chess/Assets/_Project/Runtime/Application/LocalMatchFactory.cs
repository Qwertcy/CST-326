using WarcraftChess.Runtime.Core; // imports the core state types used to assemble a new match

namespace WarcraftChess.Runtime.Application
{ // defines the namespace for application-layer match creation
    public static class LocalMatchFactory // builds local match state for the prototype scene
    {
        public static GameState CreateStandardChessMatch(int seed) // creates a fresh game state with the normal chess starting layout
        {
            GameState gameState = new GameState(); // allocates the root runtime match state
            gameState.RandomSeed = seed; // stores the provided deterministic seed for future systems

            gameState.Players[PlayerId.White] = CreatePlayer(PlayerId.White); // creates and registers the white player state
            gameState.Players[PlayerId.Black] = CreatePlayer(PlayerId.Black); // creates and registers the black player state

            AddBackRank(gameState, PlayerId.White, 0); // places the white major pieces on rank zero
            AddPawnRank(gameState, PlayerId.White, 1); // places the white pawns on rank one
            AddPawnRank(gameState, PlayerId.Black, 6); // places the black pawns on rank six
            AddBackRank(gameState, PlayerId.Black, 7); // places the black major pieces on rank seven

            gameState.ActionLog.Add($"match created with seed {seed}"); // records match creation in the log
            return gameState; // returns the fully initialized game state
        }

        private static PlayerState CreatePlayer(PlayerId playerId) // creates the initial runtime state for one player
        {
            PlayerState playerState = new PlayerState(); // allocates a new player state object
            playerState.Id = playerId; // assigns the owning side
            playerState.MaterialCount = 0; // starts material at zero until the card economy is implemented
            playerState.CheckersPiecesRemaining = 7; // initializes the player's checker marker pool from the rule set
            return playerState; // returns the initialized player state
        }

        private static void AddBackRank(GameState gameState, PlayerId owner, int y) // places rook knight bishop queen king bishop knight rook on the given rank
        {
            AddPiece(gameState, owner, PieceType.Rook, PawnFaction.None, new BoardCoord(0, y)); // places the left rook
            AddPiece(gameState, owner, PieceType.Knight, PawnFaction.None, new BoardCoord(1, y)); // places the left knight
            AddPiece(gameState, owner, PieceType.Bishop, PawnFaction.None, new BoardCoord(2, y)); // places the left bishop
            AddPiece(gameState, owner, PieceType.Queen, PawnFaction.None, new BoardCoord(3, y)); // places the queen
            AddPiece(gameState, owner, PieceType.King, PawnFaction.None, new BoardCoord(4, y)); // places the king
            AddPiece(gameState, owner, PieceType.Bishop, PawnFaction.None, new BoardCoord(5, y)); // places the right bishop
            AddPiece(gameState, owner, PieceType.Knight, PawnFaction.None, new BoardCoord(6, y)); // places the right knight
            AddPiece(gameState, owner, PieceType.Rook, PawnFaction.None, new BoardCoord(7, y)); // places the right rook
        }

        private static void AddPawnRank(GameState gameState, PlayerId owner, int y) // places eight pawns on the given rank
        {
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(0, y)); // places pawn on file zero
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(1, y)); // places pawn on file one
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(2, y)); // places pawn on file two
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(3, y)); // places pawn on file three
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(4, y)); // places pawn on file four
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(5, y)); // places pawn on file five
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(6, y)); // places pawn on file six
            AddPiece(gameState, owner, PieceType.Pawn, PawnFaction.None, new BoardCoord(7, y)); // places pawn on file seven
        }

        private static void AddPiece(GameState gameState, PlayerId owner, PieceType type, PawnFaction faction, BoardCoord position) // creates a piece instance and places it onto the board
        {
            PieceState pieceState = new PieceState(); // allocates a new runtime piece object
            pieceState.Id = PieceId.NewId(); // assigns a unique identity to this piece instance
            pieceState.Owner = owner; // stores which player owns the piece
            pieceState.Type = type; // stores the base chess identity of the piece
            pieceState.Faction = faction; // stores the pawn faction or none for non-pawns
            pieceState.IsAlive = true; // marks the piece as active on the board
            pieceState.Position = position; // stores the current logical board position
            pieceState.Armour = 0; // initializes armour to zero for standard chess setup
            pieceState.HasBattleKing = false; // initializes battle king state as inactive
            pieceState.HasMatriarchy = false; // initializes matriarchy state as inactive
            pieceState.IsGhoul = false; // initializes ghoul state as inactive
            pieceState.IsFortified = false; // initializes fortify state as inactive
            pieceState.IsOrcKnight = false; // initializes orc knight state as inactive
            pieceState.HasMoved = false; // initializes the piece as never having moved

            gameState.Pieces[pieceState.Id] = pieceState; // stores the piece in the global piece registry
            gameState.Board.PlacePiece(position, pieceState.Id); // marks the board square as occupied by this piece
        }
    }
}
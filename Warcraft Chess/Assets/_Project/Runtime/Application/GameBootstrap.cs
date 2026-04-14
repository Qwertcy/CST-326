using System.Collections.Generic;
using UnityEngine; // gives access to monobehaviour and unity logging
using WarcraftChess.Runtime.Core; // imports core state types used for match inspection and command creation
using WarcraftChess.Runtime.Presentation;
using WarcraftChess.Runtime.Rules; // imports movement validation systems

namespace WarcraftChess.Runtime.Application // defines the namespace for scene bootstrapping logic
{
    public sealed class GameBootstrap : MonoBehaviour // unity entry point for the local prototype scene
    {
        private void Start()
        {
            GameState gameState = LocalMatchFactory.CreateStandardChessMatch(12345);
            BoardRenderer boardRenderer = FindAnyObjectByType<BoardRenderer>();
            BoardInputController input = FindAnyObjectByType<BoardInputController>();

            boardRenderer.Render(gameState);

            input.Initialize(gameState, boardRenderer);
        }

        //private void Start() // runs automatically when the scene starts
        //{
        //    Debug.Log("warcraft chess bootstrap started"); // confirms the application layer is alive and the scene hook is working

        //    GameState startupState = LocalMatchFactory.CreateStandardChessMatch(12345); // creates a fresh deterministic standard chess match for startup visibility

        //    Debug.Log($"current player: {startupState.CurrentPlayer}"); // logs which side starts the match
        //    Debug.Log($"turn number: {startupState.TurnNumber}"); // logs the initial turn counter
        //    Debug.Log($"piece count: {startupState.Pieces.Count}"); // confirms that all 32 standard chess pieces were created

        //    foreach (PieceState piece in startupState.Pieces.Values) // iterates over all pieces in the starting position for visibility in the console
        //    {
        //        Debug.Log($"{piece.Owner} {piece.Type} at {piece.Position}"); // prints each piece owner, type, and starting coordinate
        //    }

        //    RunPureMoveValidationTests(LocalMatchFactory.CreateStandardChessMatch(12345)); // runs movement validation checks on a fresh untouched starting state
        //    RunMoveExecutionTests(LocalMatchFactory.CreateStandardChessMatch(12345)); // runs execution and capture checks on a separate fresh state
        //    RunCheckEvaluationTests(LocalMatchFactory.CreateStandardChessMatch(12345)); // runs check-state evaluation on a separate fresh state
        //    RunLegalMoveGenerationTests(LocalMatchFactory.CreateStandardChessMatch(12345)); // runs legal move generation checks on a separate fresh state
        //    RunPinnedRookLegalityTest(); // runs a custom scenario where geometric legality and full legality differ
        //    RunCheckmateEvaluationTest(); // runs a custom checkmate scenario to verify endgame detection
        //    RunCastlingTest(); // runs a custom kingside castling scenario to verify validation generation and execution
        //    RunEnPassantTest(); // runs a custom en passant scenario to verify validation generation and execution
        //    RunPromotionTest(); // runs a custom promotion scenario to verify validation generation and execution
        //}

        private static void RunPureMoveValidationTests(GameState gameState) // executes only non-mutating movement validation checks against a fresh starting board
        {
            Debug.Log("=== pure move validation tests begin ==="); // marks the start of the pure validation block in the console for readability

            TestMove(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1), new BoardCoord(0, 2), true, "white pawn one-square forward"); // checks normal single-square pawn movement
            TestMove(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1), new BoardCoord(0, 3), true, "white pawn two-square opening move"); // checks the initial two-square pawn advance
            TestMove(gameState, PlayerId.White, PieceType.Knight, new BoardCoord(1, 0), new BoardCoord(2, 2), true, "white knight to c3"); // checks one legal knight jump
            TestMove(gameState, PlayerId.White, PieceType.Knight, new BoardCoord(1, 0), new BoardCoord(0, 2), true, "white knight to a3"); // checks the knight's alternate legal jump
            TestMove(gameState, PlayerId.White, PieceType.Bishop, new BoardCoord(2, 0), new BoardCoord(5, 3), false, "white bishop blocked by pawn"); // checks that bishop movement is rejected when blocked at the start position
            TestMove(gameState, PlayerId.White, PieceType.Rook, new BoardCoord(0, 0), new BoardCoord(0, 2), false, "white rook blocked by pawn"); // checks that rook movement is rejected when blocked by its own pawn
            TestMove(gameState, PlayerId.White, PieceType.Queen, new BoardCoord(3, 0), new BoardCoord(3, 1), false, "white queen blocked by allied pawn"); // checks that the queen cannot move onto a square occupied by an allied piece
            TestMove(gameState, PlayerId.White, PieceType.King, new BoardCoord(4, 0), new BoardCoord(4, 1), false, "white king blocked by allied pawn"); // checks that the king cannot move onto a square occupied by an allied piece
            TestMove(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1), new BoardCoord(1, 2), false, "white pawn diagonal without enemy target"); // checks that diagonal pawn movement is rejected when no enemy is present

            Debug.Log("=== pure move validation tests end ==="); // marks the end of the pure validation block in the console for readability
        }

        private static void TestMove(GameState gameState, PlayerId owner, PieceType pieceType, BoardCoord from, BoardCoord to, bool expectedLegal, string label) // finds a specific piece, builds a move command, validates it, and reports the result
        {
            PieceState piece = FindPiece(gameState, owner, pieceType, from); // locates the exact piece instance to use for the requested move test

            if (piece == null) // guards against null references if the expected piece is not found in the current match state
            {
                Debug.LogError($"[FAIL] {label} | piece not found: {owner} {pieceType} at {from}"); // reports the lookup failure clearly in the console
                return; // exits because validation cannot continue without the source piece
            }

            MoveCommand move = new MoveCommand(piece.Id, from, to); // constructs the move command to test using the located piece id and requested coordinates
            bool actualLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // asks the validator whether the move is legal on the current board state
            bool passed = actualLegal == expectedLegal; // compares the actual validator result against the expected outcome for this test case

            string resultTag = passed ? "PASS" : "FAIL"; // selects a readable pass/fail label for console output
            Debug.Log($"[{resultTag}] {label} | from {from} to {to} | expected: {expectedLegal} | actual: {actualLegal}"); // prints the test summary and whether the validator matched expectations
        }

        private static void TestMoveAndExecute(GameState gameState, PlayerId owner, PieceType pieceType, BoardCoord from, BoardCoord to, string label) // validates then executes a move to test full state mutation
        {
            PieceState piece = FindPiece(gameState, owner, pieceType, from); // finds the correct piece instance for the move

            if (piece == null) // ensures piece exists
            {
                Debug.LogError($"[FAIL] {label} | piece not found"); // logs error if lookup fails
                return; // aborts test
            }

            MoveCommand move = new MoveCommand(piece.Id, from, to); // constructs move command

            bool isLegal = PieceMoveValidator.IsMoveLegal(gameState, move); // validates move legality

            if (!isLegal) // ensures move is valid before executing
            {
                Debug.LogError($"[FAIL] {label} | move was illegal"); // logs failure
                return; // aborts execution
            }

            PieceMoveExecutor.ExecuteMove(gameState, move); // executes move

            Debug.Log($"[EXECUTED] {label} | new current player: {gameState.CurrentPlayer} | turn: {gameState.TurnNumber}"); // logs updated game state
        }

        private static PieceState FindPiece(GameState gameState, PlayerId owner, PieceType pieceType, BoardCoord position) // searches the piece registry for a live piece matching owner, type, and coordinate
        {
            foreach (PieceState piece in gameState.Pieces.Values) // iterates through every runtime piece in the current match
            {
                if (!piece.IsAlive) // skips dead pieces because they should not participate in movement tests
                    continue; // continues scanning the remaining pieces

                if (piece.Owner != owner) // skips pieces owned by the wrong side
                    continue; // continues scanning the remaining pieces

                if (piece.Type != pieceType) // skips pieces of the wrong base type
                    continue; // continues scanning the remaining pieces

                if (piece.Position != position) // skips pieces not located on the requested source square
                    continue; // continues scanning the remaining pieces

                return piece; // returns the first piece that matches all requested criteria
            }

            return null; // returns null when no matching piece exists in the registry
        }

        private static void RunPinnedRookLegalityTest() // tests whether full legality correctly rejects moving a pinned rook away from the king file
        {
            Debug.Log("=== pinned rook legality test begin ==="); // marks the start of the pinned-piece test block

            GameState pinnedGameState = PinnedPositionFactory.CreatePinnedRookPosition(); // creates the custom pinned-rook scenario

            PieceState pinnedRook = FindPiece(pinnedGameState, PlayerId.White, PieceType.Rook, new BoardCoord(4, 1)); // finds the white rook on e2

            if (pinnedRook == null) // guards against null lookup failures
            {
                Debug.LogError("[FAIL] pinned rook legality test | pinned rook not found"); // reports setup failure clearly
                return; // exits early because the test cannot proceed without the rook
            }

            MoveCommand sidewaysMove = new MoveCommand(pinnedRook.Id, new BoardCoord(4, 1), new BoardCoord(5, 1)); // attempts to move the pinned rook sideways from e2 to f2

            bool baseLegal = PieceMoveValidator.IsMoveLegal(pinnedGameState, sidewaysMove); // checks geometric rook movement legality only
            bool fullLegal = LegalMoveService.IsMoveFullyLegal(pinnedGameState, sidewaysMove); // checks full chess legality including king safety

            Debug.Log($"pinned rook base legal: {baseLegal}"); // shows whether the move is geometrically valid
            Debug.Log($"pinned rook fully legal: {fullLegal}"); // shows whether the move remains legal after king-safety evaluation

            Debug.Log("=== pinned rook legality test end ==="); // marks the end of the pinned-piece test block
        }

        private static void RunMoveExecutionTests(GameState gameState) // executes state-mutating move and capture checks against a dedicated fresh starting board
        {
            Debug.Log("=== move execution tests begin ==="); // marks the start of the execution block in the console for readability

            TestMoveAndExecute(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 1), new BoardCoord(0, 3), "white pawn double move"); // moves a white pawn two squares forward from its starting rank
            TestMoveAndExecute(gameState, PlayerId.Black, PieceType.Pawn, new BoardCoord(1, 6), new BoardCoord(1, 4), "black pawn double move"); // moves a black pawn two squares forward from its starting rank
            TestMoveAndExecute(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 3), new BoardCoord(1, 4), "white pawn capture black pawn"); // captures the advanced black pawn to verify capture execution

            Debug.Log("=== move execution tests end ==="); // marks the end of the execution block in the console for readability
        }

        private static void RunCheckEvaluationTests(GameState gameState) // evaluates king-check status on a fresh untouched starting board
        {
            Debug.Log("=== check evaluation tests begin ==="); // marks the start of the check evaluation block in the console for readability

            Debug.Log($"white in check: {CheckEvaluator.IsKingInCheck(gameState, PlayerId.White)}"); // verifies that white is not in check at the standard starting position
            Debug.Log($"black in check: {CheckEvaluator.IsKingInCheck(gameState, PlayerId.Black)}"); // verifies that black is not in check at the standard starting position

            Debug.Log("=== check evaluation tests end ==="); // marks the end of the check evaluation block in the console for readability
        }

        private static void RunLegalMoveGenerationTests(GameState gameState) // generates fully legal moves from a fresh untouched starting board so turn order and board layout match expected opening counts
        {
            Debug.Log("=== legal move generation tests begin ==="); // marks the start of the move generation block in the console for readability

            PieceState whiteKnight = FindPiece(gameState, PlayerId.White, PieceType.Knight, new BoardCoord(1, 0)); // finds the white knight on b1 in the starting position
            List<MoveCommand> whiteKnightMoves = LegalMoveGenerator.GetLegalMovesForPiece(gameState, whiteKnight.Id); // generates all fully legal moves for the white knight on b1
            Debug.Log($"white knight at (1, 0) legal move count: {whiteKnightMoves.Count}"); // verifies that the starting knight has exactly two legal moves

            PieceState whiteBishop = FindPiece(gameState, PlayerId.White, PieceType.Bishop, new BoardCoord(2, 0)); // finds the white bishop on c1 in the starting position
            List<MoveCommand> whiteBishopMoves = LegalMoveGenerator.GetLegalMovesForPiece(gameState, whiteBishop.Id); // generates all fully legal moves for the white bishop on c1
            Debug.Log($"white bishop at (2, 0) legal move count: {whiteBishopMoves.Count}"); // verifies that the starting bishop has zero legal moves because pawns block it

            List<MoveCommand> whitePlayerMoves = LegalMoveGenerator.GetLegalMovesForPlayer(gameState, PlayerId.White); // generates all fully legal starting moves for white from a clean starting board
            Debug.Log($"white total legal move count at start: {whitePlayerMoves.Count}"); // verifies the total opening move count for white

            Debug.Log("=== legal move generation tests end ==="); // marks the end of the move generation block in the console for readability
        }

        private static void RunCheckmateEvaluationTest() // evaluates a custom checkmate scenario and reports the results in the console
        {
            Debug.Log("=== checkmate evaluation test begin ==="); // marks the start of the checkmate test block in the console for readability

            GameState checkmateState = CheckmatePositionFactory.CreateSimpleCheckmatePosition(); // creates the custom black-to-move checkmate scenario

            bool blackInCheck = CheckEvaluator.IsKingInCheck(checkmateState, PlayerId.Black); // checks whether the black king is currently in check
            bool blackCheckmate = EndgameEvaluator.IsCheckmate(checkmateState, PlayerId.Black); // checks whether black is in checkmate
            bool blackStalemate = EndgameEvaluator.IsStalemate(checkmateState, PlayerId.Black); // checks whether black is in stalemate

            Debug.Log($"black in check: {blackInCheck}"); // reports whether the black king is under attack in the custom scenario
            Debug.Log($"black in checkmate: {blackCheckmate}"); // reports whether the custom scenario is recognized as checkmate
            Debug.Log($"black in stalemate: {blackStalemate}"); // reports whether the custom scenario is incorrectly being treated as stalemate

            Debug.Log("=== checkmate evaluation test end ==="); // marks the end of the checkmate test block in the console for readability
        }

        private static void RunCastlingTest() // evaluates a simple kingside castling scenario and reports the results in the console
        {
            Debug.Log("=== castling test begin ==="); // marks the start of the castling test block in the console for readability

            GameState castlingState = CastlingPositionFactory.CreateSimpleKingsideCastlingPosition(); // creates a custom board where white should be able to castle kingside

            PieceState whiteKing = FindPiece(castlingState, PlayerId.White, PieceType.King, new BoardCoord(4, 0)); // finds the white king on e1 in the castling test position

            if (whiteKing == null) // guards against null lookup failures in the test setup
            {
                Debug.LogError("[FAIL] castling test | white king not found"); // reports setup failure clearly
                return; // exits early because the test cannot continue without the king
            }

            MoveCommand castleMove = new MoveCommand(whiteKing.Id, new BoardCoord(4, 0), new BoardCoord(6, 0)); // constructs the standard white kingside castling move from e1 to g1

            bool baseLegal = PieceMoveValidator.IsMoveLegal(castlingState, castleMove); // checks whether the castling-shaped king move is structurally legal
            bool fullLegal = LegalMoveService.IsMoveFullyLegal(castlingState, castleMove); // checks whether the castling move is fully legal including attack-path rules

            Debug.Log($"castling base legal: {baseLegal}"); // reports whether the castling-shaped move passes baseline validation
            Debug.Log($"castling fully legal: {fullLegal}"); // reports whether the move passes full castling legality

            List<MoveCommand> kingMoves = LegalMoveGenerator.GetLegalMovesForPiece(castlingState, whiteKing.Id); // generates all fully legal king moves in the castling test position

            bool generatorIncludesCastle = false; // tracks whether the generated legal move list contains the kingside castle move
            foreach (MoveCommand move in kingMoves) // scans all generated king moves for the castling destination
            {
                if (move.To == new BoardCoord(6, 0)) // checks whether this generated move lands on the standard kingside castling square
                {
                    generatorIncludesCastle = true; // records that castling is present in the generated legal move list
                    break; // stops scanning once the castle move is found
                }
            }

            Debug.Log($"castling generated: {generatorIncludesCastle}"); // reports whether legal move generation includes the castling move

            if (fullLegal) // only executes the move when full legality confirms castling is allowed
            {
                PieceMoveExecutor.ExecuteMove(castlingState, castleMove); // executes the castling move so both king and rook positions can be verified afterward

                PieceState kingAfterCastle = FindPiece(castlingState, PlayerId.White, PieceType.King, new BoardCoord(6, 0)); // finds the white king on its expected post-castle square g1
                PieceState rookAfterCastle = FindPiece(castlingState, PlayerId.White, PieceType.Rook, new BoardCoord(5, 0)); // finds the white rook on its expected post-castle square f1

                Debug.Log($"king on g1 after castling: {kingAfterCastle != null}"); // verifies that the king reached the correct castled destination
                Debug.Log($"rook on f1 after castling: {rookAfterCastle != null}"); // verifies that the rook moved to the correct castled destination
            }

            Debug.Log("=== castling test end ==="); // marks the end of the castling test block in the console for readability
        }

        private static void RunEnPassantTest() // evaluates a custom en passant scenario and reports the results in the console
        {
            Debug.Log("=== en passant test begin ==="); // marks the start of the en passant test block in the console for readability

            GameState enPassantState = EnPassantPositionFactory.CreateEnPassantPosition(); // creates a custom board where white should be able to capture en passant

            PieceState whitePawn = FindPiece(enPassantState, PlayerId.White, PieceType.Pawn, new BoardCoord(4, 4)); // finds the white pawn on e5 in the en passant test position

            if (whitePawn == null) // guards against null lookup failures in the test setup
            {
                Debug.LogError("[FAIL] en passant test | white pawn not found"); // reports setup failure clearly
                return; // exits early because the test cannot continue without the white pawn
            }

            MoveCommand enPassantMove = new MoveCommand(whitePawn.Id, new BoardCoord(4, 4), new BoardCoord(5, 5)); // constructs the white en passant capture move from e5 to f6

            bool baseLegal = PieceMoveValidator.IsMoveLegal(enPassantState, enPassantMove); // checks whether the en passant-shaped pawn move is structurally legal
            bool fullLegal = LegalMoveService.IsMoveFullyLegal(enPassantState, enPassantMove); // checks whether the move is fully legal including king safety

            Debug.Log($"en passant base legal: {baseLegal}"); // reports whether baseline move validation accepts the en passant move
            Debug.Log($"en passant fully legal: {fullLegal}"); // reports whether full legality accepts the en passant move

            List<MoveCommand> whitePawnMoves = LegalMoveGenerator.GetLegalMovesForPiece(enPassantState, whitePawn.Id); // generates all fully legal moves for the white pawn in the en passant scenario

            bool generatorIncludesEnPassant = false; // tracks whether the generated move list includes the en passant destination
            foreach (MoveCommand move in whitePawnMoves) // scans all generated pawn moves for the en passant destination square
            {
                if (move.To == new BoardCoord(5, 5)) // checks whether this generated move lands on the en passant capture square
                {
                    generatorIncludesEnPassant = true; // records that the generator includes the en passant move
                    break; // stops scanning once the move is found
                }
            }

            Debug.Log($"en passant generated: {generatorIncludesEnPassant}"); // reports whether legal move generation includes the en passant move

            if (fullLegal) // only executes the move when full legality confirms en passant is allowed
            {
                PieceMoveExecutor.ExecuteMove(enPassantState, enPassantMove); // executes the en passant move so board-state results can be verified afterward

                PieceState whitePawnAfterMove = FindPiece(enPassantState, PlayerId.White, PieceType.Pawn, new BoardCoord(5, 5)); // finds the white pawn on its expected post-capture square f6
                PieceState blackPawnStillOnBoard = FindPiece(enPassantState, PlayerId.Black, PieceType.Pawn, new BoardCoord(5, 4)); // checks whether the black pawn still exists on f5 after the en passant capture

                Debug.Log($"white pawn on f6 after en passant: {whitePawnAfterMove != null}"); // verifies that the white pawn reached the correct destination square
                Debug.Log($"black pawn removed from f5: {blackPawnStillOnBoard == null}"); // verifies that the captured black pawn was removed from its original square
            }

            Debug.Log("=== en passant test end ==="); // marks the end of the en passant test block in the console for readability
        }

        private static void RunPromotionTest() // evaluates a simple pawn promotion scenario and reports the results in the console
        {
            Debug.Log("=== promotion test begin ==="); // marks the start of the promotion test block in the console for readability

            GameState promotionState = PromotionPositionFactory.CreateSimplePromotionPosition(); // creates a custom board where white should be able to promote immediately

            PieceState whitePawn = FindPiece(promotionState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 6)); // finds the white pawn on a7 in the promotion test position

            if (whitePawn == null) // guards against null lookup failures in the test setup
            {
                Debug.LogError("[FAIL] promotion test | white pawn not found"); // reports setup failure clearly
                return; // exits early because the test cannot continue without the pawn
            }

            MoveCommand invalidPromotionMove = new MoveCommand(whitePawn.Id, new BoardCoord(0, 6), new BoardCoord(0, 7)); // constructs a promotion-reaching move without specifying a promotion target
            bool invalidFullLegal = LegalMoveService.IsMoveFullyLegal(promotionState, invalidPromotionMove); // checks that promotion is rejected when no promotion piece is supplied

            Debug.Log($"promotion without piece selection fully legal: {invalidFullLegal}"); // verifies that missing promotion selection is correctly rejected

            MoveCommand validPromotionMove = new MoveCommand(whitePawn.Id, new BoardCoord(0, 6), new BoardCoord(0, 7), PieceType.Queen); // constructs a promotion move that chooses queen as the promotion target

            bool baseLegal = PieceMoveValidator.IsMoveLegal(promotionState, validPromotionMove); // checks whether the promotion-reaching pawn move is geometrically valid
            bool fullLegal = LegalMoveService.IsMoveFullyLegal(promotionState, validPromotionMove); // checks whether the move is fully legal with a valid promotion selection

            Debug.Log($"promotion base legal: {baseLegal}"); // reports whether baseline movement validation accepts the promotion move shape
            Debug.Log($"promotion fully legal: {fullLegal}"); // reports whether full legality accepts the promotion move with a valid promotion choice

            if (fullLegal) // only executes the move when legality confirms promotion is allowed
            {
                PieceMoveExecutor.ExecuteMove(promotionState, validPromotionMove); // executes the promotion move so the resulting piece type can be verified afterward

                PieceState promotedPiece = PositionFactoryUtility.FindPiece(promotionState, PlayerId.White, PieceType.Queen, new BoardCoord(0, 7)); // finds the promoted white queen on a8 after execution
                Debug.Log($"white queen on a8 after promotion: {promotedPiece != null}"); // verifies that the pawn was transformed into the requested promotion piece on the correct square
            }

            Debug.Log("=== promotion test end ==="); // marks the end of the promotion test block in the console for readability
        }
    }
}
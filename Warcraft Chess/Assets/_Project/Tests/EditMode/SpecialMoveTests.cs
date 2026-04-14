using System.Collections.Generic; // provides list support for generated legal move collections
using NUnit.Framework; // provides the unit test framework attributes and assertions
using WarcraftChess.Runtime.Application; // imports custom scenario factories and shared helpers used for special move testing
using WarcraftChess.Runtime.Core; // imports core state types like movecommand, boardcoord, and piecetype
using WarcraftChess.Runtime.Rules; // imports validators, generator, service, and executor used in special move tests

namespace WarcraftChess.Tests.EditMode // defines the namespace for edit mode rules-engine tests
{
    public sealed class SpecialMoveTests // groups tests that verify castling, en passant, and promotion behavior
    {
        [Test] // marks this method as a runnable unit test
        public void KingsideCastling_IsGeneratedLegalAndExecutesCorrectly() // verifies that a simple kingside castling position supports validation generation and execution
        {
            GameState gameState = CastlingPositionFactory.CreateSimpleKingsideCastlingPosition(); // creates the custom board where white should be able to castle kingside
            PieceState whiteKing = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.King, new BoardCoord(4, 0)); // finds the white king on e1 in the castling scenario

            Assert.That(whiteKing, Is.Not.Null); // confirms the expected king exists before constructing the castling move

            MoveCommand castleMove = new MoveCommand(whiteKing.Id, new BoardCoord(4, 0), new BoardCoord(6, 0)); // constructs the standard white kingside castling move from e1 to g1

            bool baseLegal = PieceMoveValidator.IsMoveLegal(gameState, castleMove); // evaluates baseline castling-shape legality
            bool fullyLegal = LegalMoveService.IsMoveFullyLegal(gameState, castleMove); // evaluates full castling legality including attack-path rules

            List<MoveCommand> kingMoves = LegalMoveGenerator.GetLegalMovesForPiece(gameState, whiteKing.Id); // generates all fully legal king moves for the castling scenario
            bool generatorIncludesCastle = false; // tracks whether the generated legal move list contains the castling move

            foreach (MoveCommand move in kingMoves) // scans generated king moves for the castling destination
            {
                if (move.To == new BoardCoord(6, 0)) // checks whether the current generated move lands on the castling square g1
                {
                    generatorIncludesCastle = true; // records that castling is present in the generated move list
                    break; // stops scanning once the castling move is found
                }
            }

            Assert.That(baseLegal, Is.True); // confirms the castling-shaped move passes baseline king movement validation
            Assert.That(fullyLegal, Is.True); // confirms full legality accepts the castle
            Assert.That(generatorIncludesCastle, Is.True); // confirms legal move generation includes the castle

            PieceMoveExecutor.ExecuteMove(gameState, castleMove); // executes the castle so final king and rook positions can be verified

            Assert.That(PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.King, new BoardCoord(6, 0)), Is.Not.Null); // confirms the king ends on g1 after castling
            Assert.That(PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Rook, new BoardCoord(5, 0)), Is.Not.Null); // confirms the rook ends on f1 after castling
        }

        [Test] // marks this method as a runnable unit test
        public void EnPassant_IsGeneratedLegalAndExecutesCorrectly() // verifies that the en passant scenario supports validation generation and execution
        {
            GameState gameState = EnPassantPositionFactory.CreateEnPassantPosition(); // creates the custom board where white should be able to capture en passant
            PieceState whitePawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(4, 4)); // finds the white pawn on e5 in the en passant scenario

            Assert.That(whitePawn, Is.Not.Null); // confirms the expected white pawn exists before constructing the en passant move

            MoveCommand enPassantMove = new MoveCommand(whitePawn.Id, new BoardCoord(4, 4), new BoardCoord(5, 5)); // constructs the en passant capture move from e5 to f6

            bool baseLegal = PieceMoveValidator.IsMoveLegal(gameState, enPassantMove); // evaluates baseline pawn movement legality including en passant shape
            bool fullyLegal = LegalMoveService.IsMoveFullyLegal(gameState, enPassantMove); // evaluates full legality including king safety

            List<MoveCommand> whitePawnMoves = LegalMoveGenerator.GetLegalMovesForPiece(gameState, whitePawn.Id); // generates all fully legal moves for the white pawn
            bool generatorIncludesEnPassant = false; // tracks whether the generated move list contains the en passant move

            foreach (MoveCommand move in whitePawnMoves) // scans generated pawn moves for the en passant destination
            {
                if (move.To == new BoardCoord(5, 5)) // checks whether the current generated move lands on the en passant square f6
                {
                    generatorIncludesEnPassant = true; // records that en passant is present in the generated move list
                    break; // stops scanning once the en passant move is found
                }
            }

            Assert.That(baseLegal, Is.True); // confirms the en passant capture passes baseline pawn validation
            Assert.That(fullyLegal, Is.True); // confirms full legality accepts the en passant capture
            Assert.That(generatorIncludesEnPassant, Is.True); // confirms legal move generation includes the en passant capture

            PieceMoveExecutor.ExecuteMove(gameState, enPassantMove); // executes the en passant capture so board-state results can be verified

            Assert.That(PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(5, 5)), Is.Not.Null); // confirms the white pawn ends on f6 after the capture
            Assert.That(PositionFactoryUtility.FindPiece(gameState, PlayerId.Black, PieceType.Pawn, new BoardCoord(5, 4)), Is.Null); // confirms the captured black pawn is removed from f5
        }

        [Test] // marks this method as a runnable unit test
        public void Promotion_RequiresSelection_AndPromotesToRequestedPiece() // verifies that promotion is rejected without a selection and succeeds with a valid selected piece
        {
            GameState gameState = PromotionPositionFactory.CreateSimplePromotionPosition(); // creates the custom board where a white pawn can promote immediately
            PieceState whitePawn = PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Pawn, new BoardCoord(0, 6)); // finds the white pawn on a7 in the promotion scenario

            Assert.That(whitePawn, Is.Not.Null); // confirms the expected white pawn exists before constructing promotion moves

            MoveCommand invalidPromotionMove = new MoveCommand(whitePawn.Id, new BoardCoord(0, 6), new BoardCoord(0, 7)); // constructs a promotion-reaching move without specifying a promotion target
            bool invalidFullyLegal = LegalMoveService.IsMoveFullyLegal(gameState, invalidPromotionMove); // evaluates whether missing promotion data is correctly rejected

            Assert.That(invalidFullyLegal, Is.False); // confirms promotion cannot occur without a valid promotion piece selection

            MoveCommand validPromotionMove = new MoveCommand(whitePawn.Id, new BoardCoord(0, 6), new BoardCoord(0, 7), PieceType.Queen); // constructs a valid promotion move choosing queen as the promotion target
            bool baseLegal = PieceMoveValidator.IsMoveLegal(gameState, validPromotionMove); // evaluates baseline pawn movement legality for the promotion-reaching move
            bool fullyLegal = LegalMoveService.IsMoveFullyLegal(gameState, validPromotionMove); // evaluates full legality for the promotion move with a valid piece choice

            Assert.That(baseLegal, Is.True); // confirms the move geometry itself is legal for the pawn
            Assert.That(fullyLegal, Is.True); // confirms the promotion move is fully legal with a valid promotion choice

            PieceMoveExecutor.ExecuteMove(gameState, validPromotionMove); // executes the promotion move so the resulting promoted piece type can be verified

            Assert.That(PositionFactoryUtility.FindPiece(gameState, PlayerId.White, PieceType.Queen, new BoardCoord(0, 7)), Is.Not.Null); // confirms the pawn is replaced functionally by a queen on a8 after execution
        }
    }
}
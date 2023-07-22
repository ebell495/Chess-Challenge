using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
// Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 0 };

    int[] pieceScores = { 0, 100, 320, 330, 500, 900, 20000 };

    int numPositions = 0;
    Random rnd = new();

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Random rng = new();

        int highestValueCapture = int.MinValue;
        bool isWhite = board.IsWhiteToMove;
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        // Console.WriteLine("Current Board Eval, {0}", evaluateMove(isWhite, board, 0, 0, 0, true));
        numPositions = 0;

        foreach (Move move in allMoves)
        {  
            board.MakeMove(move);
            int minimaxVal = evaluateMove(isWhite, board, 2, int.MinValue, int.MaxValue, false);
            if (minimaxVal > highestValueCapture) {
                // Console.WriteLine("Found better move, {0}, {1}", move, minimaxVal);
                highestValueCapture = minimaxVal;
                moveToPlay = move;
            }
            board.UndoMove(move);
        }

        // Console.WriteLine("Move - {0} -> {1}", moveToPlay.StartSquare, moveToPlay.TargetSquare);
        // Console.WriteLine("Searched Positions: {0}", numPositions);

        return moveToPlay;
    }

    int evaluateMove(bool color, Board board, int depth, int alpha, int beta, bool maximizingPlayer) {
        if (board.IsDraw()) {
            return 0;
        } else if (board.IsInCheckmate()) {
            return maximizingPlayer ? int.MinValue : int.MaxValue;
        } else if (depth == 0) {
            // Maximize our moves
            // Minimize opp. moves
            // Minimize attacks on our pieces
            // Maximise attacks on opp. pieces
            // Maximise score of our pieces on board
            // Minimise score of opp. pieces on board

            int whitePieceScore = 0;
            int blackPieceScore = 0;
            for (int i = 1; i < 7; i++) {
                whitePieceScore += board.GetPieceList((PieceType)i, true).Count * pieceScores[i];
                blackPieceScore += board.GetPieceList((PieceType)i, false).Count * pieceScores[i];
            }
            Move[] currentLegalMoves = board.GetLegalMoves();
            int currMoves = currentLegalMoves.Length;
            int estOppMoves = 0;
            if (board.TrySkipTurn()) {
                estOppMoves = board.GetLegalMoves().Length;
                board.UndoSkipTurn();
            } else {
                Move rndMove = currentLegalMoves[rnd.Next(currentLegalMoves.Length)];
                board.MakeMove(rndMove);
                estOppMoves = board.GetLegalMoves().Length;
                board.UndoMove(rndMove);
            }
            
            int factor = 50 * (currMoves - estOppMoves);


            // Console.WriteLine("White Score: {0}, Black Score: {1}, Current Moves: {2}, Avg. Opp Moves: {3}, factor: {4}", whitePieceScore, blackPieceScore, currMoves, avgOppMoves, factor);

            int result = (whitePieceScore - blackPieceScore) + (color ? factor : -factor);
            numPositions++;

            // Console.WriteLine("Result {0}", result);

            if (color) {
                return result;
            } else if (!color) {
                return -result;
            } 
        } 
        if (maximizingPlayer) {
            int value = int.MinValue;
            // For each child
            foreach (Move nextMove in board.GetLegalMoves()) {
                board.MakeMove(nextMove);
                value = Math.Max(value, evaluateMove(color, board, depth - 1, alpha, beta, false));
                board.UndoMove(nextMove);
                if (value > beta) {
                    return value;
                }
                alpha = Math.Max(alpha, value);
            }
            return value;
        } else {
            int value = int.MaxValue;
            foreach (Move nextMove in board.GetLegalMoves()) {
                board.MakeMove(nextMove);
                value = Math.Min(value, evaluateMove(color, board, depth - 1, alpha, beta, true));
                board.UndoMove(nextMove);
                if (value < alpha) {
                    return value;
                }
                beta = Math.Min(beta, value);
            }
            return value;
        }
    }

        // Piece values: null, pawn, knight, bishop, rook, queen, king
        // int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        // public Move Think(Board board, Timer timer)
        // {
        //     Move[] allMoves = board.GetLegalMoves();

        //     // Pick a random move to play if nothing better is found
        //     Random rng = new();
        //     Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        //     int highestValueCapture = 0;

        //     foreach (Move move in allMoves)
        //     {
        //         // Always play checkmate in one
        //         if (MoveIsCheckmate(board, move))
        //         {
        //             moveToPlay = move;
        //             break;
        //         }

        //         // Find highest value capture
        //         Piece capturedPiece = board.GetPiece(move.TargetSquare);
        //         int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

        //         if (capturedPieceValue > highestValueCapture)
        //         {
        //             moveToPlay = move;
        //             highestValueCapture = capturedPieceValue;
        //         }
        //     }

        //     return moveToPlay;
        // }

        // // Test if this move gives checkmate
        // bool MoveIsCheckmate(Board board, Move move)
        // {
        //     board.MakeMove(move);
        //     bool isMate = board.IsInCheckmate();
        //     board.UndoMove(move);
        //     return isMate;
        // }
    }
}
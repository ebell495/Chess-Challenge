using ChessChallenge.API;
using System;
using System.Collections;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 0 };

    int[] pieceScores = { 0, 100, 320, 330, 500, 900, 20000 };

    Random rnd = new();

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Random rng = new();

        int highestValueCapture = int.MinValue;
        bool isWhite = board.IsWhiteToMove;
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        // Console.WriteLine("Current Board Eval, {0}", evaluateMove(isWhite, board, 0, 0, 0, true));

        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int minimaxVal = minimax(isWhite, board, 3, int.MinValue, int.MaxValue, false);
            if (minimaxVal > highestValueCapture)
            {
                highestValueCapture = minimaxVal;
                moveToPlay = move;
            }
            board.UndoMove(move);
        }
        Console.WriteLine("Best Evaluation: {0}", highestValueCapture);
        return moveToPlay;
    }

    int evaluateBoard(Board board)
    {
        int whitePieceScore = 0;
        int blackPieceScore = 0;
        for (int i = 1; i < 7; i++)
        {
            whitePieceScore += board.GetPieceList((PieceType)i, true).Count * pieceScores[i];
            blackPieceScore += board.GetPieceList((PieceType)i, false).Count * pieceScores[i];
        }
        Move[] currentLegalMoves = board.GetLegalMoves();
        int currMoves = currentLegalMoves.Length;
        int estOppMoves = 0;
        if (board.TrySkipTurn())
        {
            estOppMoves = board.GetLegalMoves().Length;
            board.UndoSkipTurn();
        }
        else
        {
            Move rndMove = currentLegalMoves[rnd.Next(currentLegalMoves.Length)];
            board.MakeMove(rndMove);
            estOppMoves = board.GetLegalMoves().Length;
            board.UndoMove(rndMove);
        }

        int factor = 0 * (currMoves - estOppMoves);

        return (whitePieceScore - blackPieceScore) + factor;
    }

    int minimax(bool color, Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (board.IsDraw())
        {
            return 0;
        }
        else if (board.IsInCheckmate())
        {
            return maximizingPlayer ? int.MinValue : int.MaxValue;
        }
        else if (depth == 0)
        {
            // Maximize our moves
            // Minimize opp. moves
            // Minimize attacks on our pieces
            // Maximise attacks on opp. pieces
            // Maximise score of our pieces on board
            // Minimise score of opp. pieces on board

            if (color)
            {
                return evaluateBoard(board);
            }
            else if (!color)
            {
                return -(evaluateBoard(board));
            }
        }
        if (maximizingPlayer)
        {
            int value = int.MinValue;
            // For each child
            foreach (Move nextMove in board.GetLegalMoves())
            {
                board.MakeMove(nextMove);
                value = Math.Max(value, minimax(color, board, depth - 1, alpha, beta, false));
                board.UndoMove(nextMove);
                if (value > beta)
                {
                    return value;
                }
                alpha = Math.Max(alpha, value);
            }
            return value;
        }
        else
        {
            int value = int.MaxValue;
            foreach (Move nextMove in board.GetLegalMoves())
            {
                board.MakeMove(nextMove);
                value = Math.Min(value, minimax(color, board, depth - 1, alpha, beta, true));
                board.UndoMove(nextMove);
                if (value < alpha)
                {
                    return value;
                }
                beta = Math.Min(beta, value);
            }
            return value;
        }
    }
}
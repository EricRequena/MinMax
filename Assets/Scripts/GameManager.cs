using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;
public enum States
{
    CanMove,
    CantMove
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BoxCollider2D collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    [SerializeField] private States state = States.CanMove;
    public Camera camera;

    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        Calculs.CalculateDistances(collider, Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Matrix[i, j] = 0; // 0: vacío, 1: jugador, -1: IA
            }
        }
    }

    private void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    state = States.CantMove;
                    if (Calculs.EvaluateWin(Matrix) == 2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }

    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        MinimaxAI();
    }

    public void MinimaxAI()
    {
        int bestScore = int.MinValue;
        int moveX = -1, moveY = -1;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Matrix[i, j] == 0)
                {
                    Matrix[i, j] = -1;
                    int score = Minimax(Matrix, 5, false);
                    Matrix[i, j] = 0;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        moveX = i;
                        moveY = j;
                    }
                }
            }
        }
        if (moveX != -1 && moveY != -1)
            DoMove(moveX, moveY, -1);

        state = States.CanMove;
    }

    private int Minimax(int[,] board, int depth, bool isMaximizing)
    {
        int result = Calculs.EvaluateWin(board);

        if (result == 1) return -15;
        else if (result == -1) return 15;
        else if (result == 0) return 0;
        else if (depth == 0) return 0; 
        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = -1;
                        int score = Minimax(board, depth + 1, false);
                        board[i, j] = 0;
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = 1;
                        int score = Minimax(board, depth + 1, true);
                        board[i, j] = 0;
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }

    public void DoMove(int x, int y, int team)
    {
        Matrix[x, y] = team;
        if (team == 1)
            Instantiate(token1, Calculs.CalculatePoint(x, y), Quaternion.identity);
        else
            Instantiate(token2, Calculs.CalculatePoint(x, y), Quaternion.identity);
        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                break;
            case 1:
                Debug.Log("You Win");
                break;
            case -1:
                Debug.Log("You Lose");
                break;
        }
    }
}

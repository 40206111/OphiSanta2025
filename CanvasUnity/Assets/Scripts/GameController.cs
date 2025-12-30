using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum eGameState
{
    PreStart = 0,
    Running = 1,
    Lost = 2,
}
public class GameController
{
    private static GameController _instance;

    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameController();
            }
            return _instance;
        }
        private set { }
    }

    private GameController()
    {
        LoadHighScores();
    }

    const int MaxScores = 10;

    public eGameState GameState = eGameState.PreStart;

    public Action Restart;
    public Action GameStarted;
    public Action GameLost;
    public Action<Paintball> MaxBallPop;

    public List<int> HighScores = new List<int>();
    public int CurrentScore;

    public PalletCreator PalletCreator = new PalletCreator();

    public void ChangeState(eGameState newState)
    {
        if (GameState == newState)
        {
            return;
        }

        switch (newState)
        {
            case eGameState.PreStart:
                CurrentScore = 0;
                Restart?.Invoke();
                break;
            case eGameState.Running:
                PalletCreator.CreatePallet();
                GameStarted?.Invoke();
                break;
            case eGameState.Lost:
                CheckScores();
                GameLost?.Invoke();
                break;
        }
        GameState = newState;
    }

    private void CheckScores()
    {
        HighScores.Sort();
        HighScores.Reverse();

        if (HighScores.Count < MaxScores || CurrentScore > HighScores.Last())
        {
            var scoreToMove = CurrentScore;
            for (int i = 0; i < HighScores.Count; i++)
            {
                var score = HighScores[i];
                if (scoreToMove > score)
                {
                    var temp = scoreToMove;
                    scoreToMove = score;
                    HighScores[i] = temp;
                }
            }

            if (HighScores.Count < MaxScores)
            {
                HighScores.Add(scoreToMove);
            }
        }

        SaveHighScores();
    }

    private void SaveHighScores()
    {
        for (int i = 0; i < HighScores.Count; i++)
        {
            PlayerPrefs.SetInt($"score_{i}", HighScores[i]);
        }
    }

    private void LoadHighScores()
    {
        for (int i = 0; i < MaxScores; i++)
        {
            var score = PlayerPrefs.GetInt($"score_{i}");
            if (score != 0)
            {
                HighScores.Add(score);
            }
        }
    }

    public void CauseMaxBallPop(Paintball ball)
    {
        MaxBallPop?.Invoke(ball);
    }
}


using System;
using System.Collections.Generic;

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


    public eGameState GameState = eGameState.PreStart;

    public Action Restart;
    public Action GameStarted;
    public Action GameLost;

    public Dictionary<string, Paintball> ActiveBalls = new Dictionary<string, Paintball>();
    public Dictionary<string, Paintball> PooledBalls = new Dictionary<string, Paintball>();

    public void AddToPool(Paintball paintball)
    {
        ActiveBalls.Remove(paintball.gameObject.name);
        PooledBalls[paintball.gameObject.name] = paintball;
    }

    public void ChangeState(eGameState newState)
    {
        if (GameState == newState)
        {
            return;
        }

        switch (newState)
        {
            case eGameState.PreStart:
                Restart?.Invoke();
                break;
            case eGameState.Running:
                GameStarted?.Invoke();
                break;
            case eGameState.Lost:
                GameLost?.Invoke();
                break;
        }
        GameState = newState;
    }

    public void SplatBalls()
    {
        foreach (var ball in ActiveBalls)
        {
            ball.Value.Splat();
        }
    }
}

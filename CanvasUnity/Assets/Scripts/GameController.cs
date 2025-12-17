
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

    public List<Paintball> ActiveBalls = new List<Paintball>();
    public List<Paintball> PooledBalls = new List<Paintball>();

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

}

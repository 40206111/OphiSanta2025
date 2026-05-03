using UnityEngine;

public class MenuHighScoreComponent : GameOverComponent
{
    protected override void Awake()
    {
        GameController.Instance.RefreshHighScores += PopulateScores;
    }

    protected override void OnDestroy()
    {
        GameController.Instance.RefreshHighScores -= PopulateScores;
    }

    private void OnEnable()
    {
        PopulateScores();
    }

}

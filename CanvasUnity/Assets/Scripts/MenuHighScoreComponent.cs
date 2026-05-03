using UnityEngine;

public class MenuHighScoreComponent : GameOverComponent
{
    protected override void Awake(){}

    protected override void OnDestroy(){}

    private void OnEnable()
    {
        PopulateScores();
    }

}

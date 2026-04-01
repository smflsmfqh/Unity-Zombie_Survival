using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    public ZombieSpawner Spawner;   

    private int score = 0;
    public bool IsGameOver { get; private set; }

    public void Start()
    {
        uiManager.SetScoreText(score);
    }
    public void AddScore(int add)
    {
        if (IsGameOver)
        {
            return;
        }
        score += add;
        uiManager.SetScoreText(score);
    }

    public void EndGame()
    {
        IsGameOver = true;
        Spawner.enabled = false;    
        uiManager.SetActiveGameOverUi(true);
    }
}

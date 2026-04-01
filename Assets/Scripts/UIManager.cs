using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{

    public Text ammoText;
    public Text scoreText;
    public Text waveText;

    public GameObject gameOverUi;

    public void OnEnable()
    {
        SetAmmoText(0, 0);
        SetScoreText(0);
        SetWaveText(0, 0);
        SetActiveGameOverUi(false);
    }

    public void SetAmmoText(int magAmmo, int remainAmmo)
    {
        ammoText.text = $"{magAmmo} / {remainAmmo}";
    }

    public void SetScoreText(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void SetWaveText(int wave, int count)
    {
        waveText.text = $"Wave: {wave}\nEnemy Left: {count}";
    }

    public void SetActiveGameOverUi(bool active)
    {
        gameOverUi.SetActive(active);
    }

    public void OnClickRestart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

}

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();

            }
            return m_instance;
        }
    }
    private static GameManager m_instance;
    private int score = 0;
    public bool isGameOver { get; private set; } = false;

    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

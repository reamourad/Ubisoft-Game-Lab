using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playButton;
    public Button quitButton;
    public TextMeshProUGUI timerText;
    public GameObject Logo;
    [FormerlySerializedAs("scoreText")] public TextMeshProUGUI hidersRemainingText;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button restartButton;

    private float timeRemaining = 300f;
    private bool timerRunning = false;
    private bool isPaused = false;

    public COMP476CharacterController CC;

    private void Start()
    {
        playButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);

        timerText.gameObject.SetActive(false);
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        int remainingGhosts = FindObjectsByType<COMP476HiderMovement>(FindObjectsSortMode.None).Length;
        hidersRemainingText.text = $"Ghosts remaining: {remainingGhosts}";
        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape) && timerRunning)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (timerRunning && !isPaused)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;
                UpdateTimerDisplay(timeRemaining);
            }
        }
    }

    void StartGame()
    {
        playButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

        timerText.gameObject.SetActive(true);
        timeRemaining = 300f;
        timerRunning = true;

        Logo.SetActive(false);
        CC.isPlaying = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        CC.isPlaying = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        CC.isPlaying = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}

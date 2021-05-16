using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using UnityEngine.EventSystems;

/// <summary>In-game UI Manager.</summary>
public class UIManager : MonoBehaviour
{   
    public GameObject pauseMenu;
    public GameObject pauseMenuFirstSelected;
    public GameObject winMenu;
    public GameObject winMenuFirstSelected;
    public GameObject endRoundMenu;
    public GameObject endRoundMenuFirstSelected;
    public GameObject countdownUI;
    public GameObject ingameUI;
    public Text winningPlayer;
    public Text survivingPlayer;

    string[] menuScoreTemplates = new string[8] {
        "Player 1 wins!",
        "Player 2 wins!",
        "Player 3 wins!",
        "Player 4 wins!",
        "Player 1 scores a point",
        "Player 2 scores a point",
        "Player 3 scores a point",
        "Player 4 scores a point"
    };

    /// <summary>To set first selected gameobject.</summary>
    EventSystem eventSystem;

    GameObject prefabScoreDisplay;
    Transform scoreDisplay;
    Text[] scoreTexts;
    LifeDisplay[] lifeDisplays;
    Text countdownText;

    /// <summary>
    /// Displays GameManager.Instance.Countdown.val on screen.
    /// </summary>
    public IEnumerator UpdateCountdown()
    {
        while (GameManager.Instance.Countdown.val > 0.05f)
        {
            countdownText.text = Mathf.Ceil(GameManager.Instance.Countdown.val).ToString();
            yield return new WaitForSeconds(Time.deltaTime);
        }
        countdownText.text = "Go!";
    }  

    void Awake() 
    {
        eventSystem = EventSystem.current;
        countdownText = countdownUI.transform.GetChild(0).GetComponent<Text>();
        prefabScoreDisplay = Resources.Load<GameObject>("ScoreDisplay");
    }

    /// <summary>
    /// Fills ingame menu the needed amount scoredisplays etc.
    /// </summary>
    public void InitializeIngameMenu()
    {
        int nPlayers = GameManager.Instance.NumPlayers;
        scoreTexts = new Text[nPlayers];
        lifeDisplays = new LifeDisplay[nPlayers];
        for (int i = 0; i < nPlayers; i++) 
        {
            scoreDisplay = Instantiate(prefabScoreDisplay, Vector3.zero, Quaternion.identity).transform;
            scoreDisplay.SetParent(ingameUI.transform, false);
            scoreDisplay.transform.localPosition = new Vector2(50f*i - 25f*(nPlayers-1), 0f);
            scoreDisplay.Find("PlayerImage").GetComponent<Image>().color = GameManager.Instance.playerColors[i];
            scoreTexts[i] = scoreDisplay.Find("Score").GetComponent<Text>();
            lifeDisplays[i] = scoreDisplay.GetComponent<LifeDisplay>();
        }
    }
    
    /// <summary>
    /// Update score displays.
    /// </summary>
    public void UpdateIngameUI()
    {
        for (int i = 0; i < GameManager.Instance.NumPlayers; i++)
        {
            scoreTexts[i].text = GameManager.Instance.Score[i].ToString();
            lifeDisplays[i].SetLives(GameManager.Instance.Players[i].GetComponent<DrillCharacterController>().LivesLeft);
        }
    }

    /// <summary>
    /// Show win menu.
    /// </summary>
    /// <param name="i">id of player that has won.</param>
    public void Win(int i)
    {
        winningPlayer.text = menuScoreTemplates[i];
        winMenu.SetActive(true);
        eventSystem.SetSelectedGameObject(winMenuFirstSelected);
    }

    /// <summary>
    /// Show round end menu.
    /// </summary>
    /// <param name="i">id of player that scored a point.</param>
    public void EndRound(int i)
    {
        survivingPlayer.text = menuScoreTemplates[i+4];
        endRoundMenu.SetActive(true);
        eventSystem.SetSelectedGameObject(endRoundMenuFirstSelected);
    }

    /// <summary>
    /// Show pause menu.
    /// </summary>
    public void Pause() 
    {
        pauseMenu.SetActive(true);
        eventSystem.SetSelectedGameObject(pauseMenuFirstSelected);
    }

    /// <summary>
    /// Start showing the countdown on screen.
    /// </summary>
    public void StartCountdown() 
    {
        ShowCountdown();
        StartCoroutine(UpdateCountdown());
    }

    /// <summary>Activate countdown on UI</summary>
    public void ShowCountdown() { countdownUI.SetActive(true); }
    /// <summary>Deactivate countdown on UI</summary>
    public void HideCountdown() { countdownUI.SetActive(false); }

    /// <summary>
    /// Clear menues to only show ingame UI.
    /// </summary>
    public void Clear() 
    {
        pauseMenu.SetActive(false);
        winMenu.SetActive(false);
        endRoundMenu.SetActive(false);
        countdownUI.SetActive(false);
    }

    /// <summary>
    /// Clears UI and calls GameManager.Instance.SetupAndStartGame();
    /// </summary>
    public void NextMatch()
    {
        Clear();
        GameManager.Instance.SetupAndStartGame();
    }


    /// <summary>
    /// Clears UI and calls GameManager.Instance.RestartGame();
    /// </summary>
    public void RestartGame()
    {
        Clear();
        GameManager.Instance.RestartGame();
    }


    /// <summary>
    /// Clears UI and calls GameManager.Instance.ContinueGame();
    /// </summary>
    public void ContinueGame() 
    {
        Clear();
        GameManager.Instance.ContinueGame();
    }

    /// <summary>
    /// Sets up inMainMenu and loads main menu scene.
    /// </summary>
    public void ReturnToMenu() 
    {
        GameManager.Instance.inMainMenu = true;
        InputManager.Instance.inMainMenu = true;
        SceneManager.LoadScene("mainmenu");
    }

    /// <summary>
    /// Quits application.
    /// </summary>
    public void QuitGame() 
    {
        Application.Quit();
    }
}

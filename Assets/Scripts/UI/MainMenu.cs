using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    Text playerCountText;
    string[] playerCountTextTemplate = new string[3] {
        "2 Players", "3 Players", "4 Players"
    };
    string[] readyTextTemplate = new string[5] {
        "DRILL", "DERBY", "PRESS", "START", "ENTER"
    };

    bool inLobby = false;
    InputMappingCursor inputMappingCursor;
    public InputInfoDisplay[] InputInfoDisplays { get; private set; } = new InputInfoDisplay[4];
    public GameObject mainMenuUI;
    public Text titleFirstLine;
    public Text titleSecondLine;

    void Awake()
    {
        mainMenuUI.SetActive(true);
        playerCountText = mainMenuUI.transform.Find("Player Count").GetComponent<Text>();

        GameObject prefabInfoDisplay = Resources.Load<GameObject>("InputInfoDisplay");
        for (int i = 0; i < 4; i++)
        {
            InputInfoDisplays[i] = Instantiate(prefabInfoDisplay).GetComponent<InputInfoDisplay>();
            InputInfoDisplays[i].transform.SetParent(transform, false);
            InputInfoDisplays[i].Setup(i);
        }

        inputMappingCursor = new InputMappingCursor(gameObject);
    }

    /// <summary>
    /// Handles lobby menu logic and calls inputMappingCursor.Check().
    /// </summary>
    void Update()
    {
        if (inLobby)
        {
            int lobbyState = inputMappingCursor.Check();

            if (lobbyState == -1)
                BackToMainMenu();
            
            if (lobbyState == 1)
            {
                for (int i = 0; i < InputManager.Instance.joyButtonStart.Length; i++)
                {
                    if (Input.GetKeyDown(InputManager.Instance.joyButtonStart[i])) StartGame();
                }
                if (Input.GetKeyDown(KeyCode.Return)) StartGame();
            }
        }
    }

    /// <summary>
    /// Move from main menu to lobby menu.
    /// </summary>
    public void ToLobby()
    {
        int nPlayers = GameManager.Instance.NumPlayers;
        inLobby = true;
        mainMenuUI.SetActive(false);

        for (int i = 0; i < nPlayers; i++)
        {
            GameManager.Instance.Players[i].gameObject.SetActive(true);
            GameManager.Instance.Players[i].GetComponent<DrillCharacterController>().Reset();
            GameManager.Instance.Players[i].position = 
                new Vector2(2.5f*i - 1.25f*(nPlayers-1), 0.2f);
            InputInfoDisplays[i].Enable();
        }

        inputMappingCursor.Activate();
    }

    /// <summary>
    /// Sets the title "DRILL DERBY" to Press Start / Enter
    /// </summary>
    /// <param name="gamepad">True if we should write Press Start</param>
    public void SetTitleToReadyPrompt(bool gamepad)
    {
        titleFirstLine.text = readyTextTemplate[2];
        titleSecondLine.text = readyTextTemplate[gamepad ? 3 : 4];
    }

    /// <summary>
    /// Sets the title to "DRILL DERBY"
    /// </summary>
    public void SetTitleToDefault()
    {
        titleFirstLine.text = readyTextTemplate[0];
        titleSecondLine.text = readyTextTemplate[1];
    }

    /// <summary>
    /// Return from lobby menu to main menu.
    /// </summary>
    public void BackToMainMenu()
    {
        int nPlayers = GameManager.Instance.NumPlayers;

        SetTitleToDefault();

        inLobby = false;
        mainMenuUI.SetActive(true);
        inputMappingCursor.Hide();
        for (int i = 0; i < nPlayers; i++)
        {
            GameManager.Instance.Players[i].gameObject.SetActive(false);
            InputInfoDisplays[i].HideAll();
        }
    }

    /// <summary>
    /// Sets up inMainMenu and loads game scene.
    /// </summary>
    public void StartGame()
    {
        GameManager.Instance.inMainMenu = false;
        InputManager.Instance.inMainMenu = false;
        SceneManager.LoadScene("game");
    }

    /// <summary>
    /// Increases GameManager.NumPlayers and updates text.
    /// </summary>
    public void AddPlayer()
    {
        if (GameManager.Instance.NumPlayers < 4)
            GameManager.Instance.NumPlayers++;
        playerCountText.text = playerCountTextTemplate[GameManager.Instance.NumPlayers-2];
    }

    /// <summary>
    /// Decreases GameManager.NumPlayers and updates text.
    /// </summary>
    public void RemovePlayer()
    {
        if (GameManager.Instance.NumPlayers > 2)
            GameManager.Instance.NumPlayers--;
        playerCountText.text = playerCountTextTemplate[GameManager.Instance.NumPlayers-2];
    }

    /// <summary>
    /// Quits application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
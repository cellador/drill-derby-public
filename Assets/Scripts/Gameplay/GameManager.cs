using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public bool inMainMenu = false;

    #region gameplayvars
    public int NumPlayers {get; set;} = 2;
    /// <summary>
    /// How many lives each player has each round.
    /// </summary>
    public int Lives {get; set;} = 5;
    readonly public Color32[] playerColors = {
        new Color32(255,31,46,255),
        new Color32(0,153,116,255),
        new Color32(204,71,25,255),
        new Color32(69,255,47,255)
    };
    #endregion

    #region singletonvars
    public int[] Score { get; private set; }
    // Both from worldmanager but store the values
    // here so that everyone can access it.
    public float ViscosityLand { get; private set; }
    public float Gravity { get; private set; }
    public Transform[] Players {get; private set;}
    public bool[] PlayersAlive {get; set;}
    public GameState CurrentState {get; private set; }
    public Countdown Countdown { get; private set; } = new Countdown();
    #endregion

    #region internalvars
    float gameCountdown;
    GameState previousState;
    WorldManager worldManager;
    GameObject prefabPlayer;
    UIManager UI;
    readonly String[] characterTags = new string[] {"Character1","Character2","Character3","Character4"};
    bool coldStart;
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        prefabPlayer = Resources.Load<GameObject>("Player");
        gameCountdown = 0.1f;
        coldStart = true;

        Players = new Transform[4];
        for (int i = 0; i < 4; i++) 
        {
            Players[i] = Instantiate(prefabPlayer, Vector3.zero, Quaternion.identity).transform;
            Players[i].GetComponent<DrillCharacterController>().PlayerNumber = i;
            Players[i].GetComponent<DrillCharacterController>().SetTag(characterTags[i]);
            DontDestroyOnLoad(Players[i].gameObject);
        }

        //Add OnSceneChange delegate such that is called when scene changes.
        SceneManager.sceneLoaded += OnSceneChange;
    }

    /// <summary>
    /// Handle either the setup of the game in main menu 
    /// or a cold start when running game scene directly.
    /// </summary>
    void OnSceneChange(Scene scene, LoadSceneMode mode) 
    {
        // This is run when changing from game scene to menu.
        if (inMainMenu)
        {
            if (NumPlayers < 2)
                NumPlayers = 2;
            gameCountdown = 3f;
            coldStart = false;
            for (int i = 0; i < NumPlayers; i++)
            {
                Players[i].gameObject.SetActive(false);
            }
        }
        // Initialize game
        else
        {
            Score = new int[NumPlayers];
            PlayersAlive = new bool[NumPlayers];
            worldManager = GameObject.Find("World").GetComponent<WorldManager>();
            Gravity = worldManager.gravity;
            ViscosityLand = worldManager.viscosityLand;
            for (int i = 0; i < NumPlayers; i++)
            {
                Players[i].GetComponent<DrillCharacterController>().PositionOnSpawn =
                    worldManager.spawnPos + new Vector2(2.5f * i - 1.25f * (NumPlayers - 1), 0f);
            }
            UI = GameObject.Find("UI").GetComponent<UIManager>();
            UI.InitializeIngameMenu();

            if (coldStart) SetupColdStart();

            SetupAndStartGame();
        }
    }

    /// <summary>
    /// Resets score and starts game.
    /// This is used when starting a completely new match.
    /// </summary>
    public void RestartGame()
    {
        for (int i = 0; i < NumPlayers; i++)
            Score[i] = 0;
        SetupAndStartGame();
    }

    /// <summary>
    /// Sets up next round and starts it using a coroutine.
    /// </summary>
    public void SetupAndStartGame()
    {
        worldManager.NewWorld();
        CurrentState = GameState.COUNTDOWN;
        Time.timeScale = 1f;
        for (int i = 0; i < NumPlayers; i++)
        {
            PlayersAlive[i] = true;
            Players[i].gameObject.SetActive(true);
            Players[i].GetComponent<DrillCharacterController>().LivesLeft = Lives;
            Players[i].GetComponent<DrillCharacterController>().Reset();
        }
        UI.UpdateIngameUI();
        StartCoroutine(Countdown.Start(gameCountdown, StartGame));
        UI.StartCountdown();
    }

    /// <summary>
    /// Set first player to WASD keyboard controls and all others to "AI" movement.
    /// </summary>
    void SetupColdStart()
    {
        InputManager.Instance.characterInputs[0].MapPlayer(InputMode.KEYBOARD, -1);
        for (int i = 1; i < NumPlayers; i++)
            AIManager.Instance.ToggleAI(i);
    }

    /// <summary>
    /// Changes game state to PLAYING after countdown is over.
    /// Is called in a coroutine from SetupAndStartGame.
    /// </summary>
    void StartGame() {
        UI.HideCountdown();
        for (int i = 0; i < NumPlayers; i++)
            Players[i].GetComponent<DrillCharacterController>().Unfreeze();
        previousState = GameState.PLAYING;
        ContinueGame();
    }

    /// <summary>
    /// Clears menu UI and makes time move.
    /// </summary>
    public void ContinueGame() {
        UI.Clear();
        // If the game was continued after being paused during the countdown,
        // show the countdown.
        if (previousState == GameState.COUNTDOWN)
            UI.ShowCountdown();
        CurrentState = previousState;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Shows menu UI and stops time.
    /// </summary>
    public void PauseGame() {
        previousState = CurrentState;
        // If the game is paused during the countdown, hide the countdown.
        if (previousState == GameState.COUNTDOWN)
            UI.HideCountdown();
        CurrentState = GameState.PAUSED;
        UI.Pause();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Add delta to player's score.
    /// </summary>
    /// <param name="playerNumber">id of player whose score should be updated.</param>
    /// <param name="delta">Number that should be added to score.</param>
    public void AddToScore(int playerNumber, int delta)
    {
        Score[playerNumber] += delta;
        UI.UpdateIngameUI();
        CheckWinState(playerNumber);
    }

    /// <summary>
    /// Check if player has more or equal to 3 points and moves to win screen.
    /// </summary>
    /// <param name="playerNumber">id of player whose score should be checked.</param>
    void CheckWinState(int playerNumber) 
    {
        if(Score[playerNumber] >= 3)
        {
            CurrentState = GameState.WON;
            UI.Win(playerNumber);
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Update score and handle game logic if a player lost a life.
    /// </summary>
    /// <param name="playerNumber">PlayerNumber of player that has lost life.</param>
    /// <param name="lastCollider">The last collider that player has touched.</param>
    public void LostLife(int playerNumber, Collider2D lastCollider)
    {
        if (Players[playerNumber].GetComponent<DrillCharacterController>().LivesLeft == 0)
            PlayersAlive[playerNumber] = false;
        else 
            Players[playerNumber].GetComponent<DrillCharacterController>().Respawn();
        
        UI.UpdateIngameUI();
        
        // If only one player is left
        // Award 1 point and check win condition (AddToScore handles that
        // or finish round.
        if (PlayersAlive.Count(x => x) == 1)
        {
            int i = PlayersAlive.ToList().IndexOf(true);
            AddToScore(i, 1);
            if (Score[i] < 3)
            {
                CurrentState = GameState.ENDROUND;
                UI.EndRound(i);
                Time.timeScale = 0f;
            }
        }
    }
}
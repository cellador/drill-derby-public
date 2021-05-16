/// <summary>
/// Stores Character Input, must be assigned PlayerNumber via
/// constructor and InputMappingCursor assigns Input.
/// GetInput is called in Update() of InputManager.
/// </summary>
using UnityEngine;

public class CharacterInput 
{
    /// <summary>Keyboard or Gamepad</summary>
    public InputMode inputMode {get; private set;}
    public int padNumber {get; private set;}
    /// <summary>Accelerate, Brake, Left, Right, Escape</summary>
    public KeyCode[] buttons {get; private set;}
    
    int playerNumber;

    bool pauseButtonHeld;

    InputCommands.Turn turn;
    InputCommands.Accelerate accelerate;
    
    ///<Summary>Construct characterinput and assign it to player.</Summary>
    /// <param name="i">ID of player that this input is controlling.</param>
    public CharacterInput(int i)
    {
        playerNumber = i;
        pauseButtonHeld = true;

        buttons = new KeyCode[]{KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Escape};
        inputMode = InputMode.NULL;

        accelerate = new InputCommands.Accelerate();
        turn = new InputCommands.Turn();
    }

    /// <Summary>
    /// Gets input assigned to this player and passes it to player.
    /// To be called every frame.
    /// </Summary>
    public void GetInput()
    {
        // Only allow first player to pause, else pause gets toggled twice
        if (playerNumber == 0 && Input.GetKeyDown(KeyCode.Escape)) TogglePause();

        if (inputMode == InputMode.GAMEPAD)
        {
            if (pauseButtonHeld && !Input.GetKey(InputManager.Instance.joyButtonStart[padNumber]))
                pauseButtonHeld = false;
            if (!pauseButtonHeld && Input.GetKeyDown(InputManager.Instance.joyButtonStart[padNumber])) TogglePause();

            turn.Execute(GameManager.Instance.Players[playerNumber]
                        , Input.GetAxis(InputManager.Instance.joyXAxis[padNumber]));
            OppositeKeys(InputManager.Instance.joyButton0[padNumber]
                        , InputManager.Instance.joyButton1[padNumber]
                        , accelerate, GameManager.Instance.Players[playerNumber]);
        }
        if (inputMode == InputMode.KEYBOARD)
        {
            OppositeKeys(buttons[0], buttons[1]
                        , accelerate, GameManager.Instance.Players[playerNumber]);
            OppositeKeys(buttons[3], buttons[2]
                        , turn, GameManager.Instance.Players[playerNumber]);
        }
    }

    /// <summary>Resets all information stored in this input.</summary>
    public void UnmapPlayer()
    {
        inputMode = InputMode.NULL;
        padNumber = -1;
        for (int i = 0; i < 4; i++)
            buttons[i] = KeyCode.None;
    }

    /// <summary>Set input mode to keyboard or gamepad.</summary>
    /// <param name="mode">Keyboard or Gamepad input mode.</param>
    /// <param name="pad">Number of gamepad that should be assigned.</param>
    public void MapPlayer(InputMode mode, int pad)
    {
        inputMode = mode;
        padNumber = pad;
    }

    /// <summary>Map button to keycode.</summary>
    /// <param name="i">
    /// ID of button to be mapped.
    /// 0: Accelerate, 1: Brake, 2: Left, 3: Right
    /// </param>
    /// <param name="key">Keycode that is being mapped to button.</param>
    public void MapButton(int i, KeyCode key)
    {
        buttons[i] = key;
    }

    /// <summary>Free up button.</summary>
    public void UnmapButton(int n)
    {
        buttons[n] = KeyCode.None;
    }

    /// <summary>
    /// Get key and execute command on player.
    /// Handles opposite buttons such that they don't interfere when
    /// pressed simultaneously.
    /// </summary>
    /// <param name="buttonPositive">Keycode that should pass 1 to command.</param>
    /// <param name="buttonNegative">Keycode that should pass -1 to command.</param>
    /// <param name="command">Command that should be executed.</param>
    /// <param name="player">Player that command should be executed on.</param>
    void OppositeKeys(KeyCode buttonPositive,KeyCode buttonNegative, InputCommands.Command command, Transform player)
    {
        
        if (Input.GetKey(buttonPositive))
        {
            if (Input.GetKey(buttonNegative))
                command.Execute(player, 0F);
            else
                command.Execute(player, 1F);
        }
        if (Input.GetKeyUp(buttonPositive))
        {
            if (Input.GetKey(buttonNegative))
                command.Execute(player, -1F);
            else
                command.Execute(player, 0F);
        }

        if (Input.GetKey(buttonNegative))
        {
            if (Input.GetKey(buttonPositive))
                command.Execute(player, 0F);
            else
                command.Execute(player, -1F);
        }
        if (Input.GetKeyUp(buttonNegative))
        {
            if (Input.GetKey(buttonNegative))
                command.Execute(player, -1F);
            else
                command.Execute(player, 0F);
        }
    }

    /// <summary>Toggle pause depending on current game state.</summary>
    void TogglePause()
    {
        if (GameManager.Instance.CurrentState == GameState.COUNTDOWN || 
            GameManager.Instance.CurrentState == GameState.PLAYING)  
            GameManager.Instance.PauseGame();
        else if (GameManager.Instance.CurrentState == GameState.PAUSED)
            GameManager.Instance.ContinueGame();
    }
}
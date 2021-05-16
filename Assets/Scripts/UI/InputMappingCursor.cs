using UnityEngine;

enum MappingState 
{
    PLAYER,
    BUTTON
}

/// <summary>
/// Invoked to control button mapping sequence in main menu lobby.
/// Runs either in Player or Button Mapping state, will be initalized
/// via the constructor and Check() will be called in Update().
/// Is turned off until Activate() is called.
/// 
/// Keeps track of cursor via either playerToMap and buttonToMap.
/// </summary>
public class InputMappingCursor
{
    int playerToMap;
    /// <summary> 0: accelerate, 1: decelerate, 2: left, 3: right</summary>
    int buttonToMap;
    bool buttonIsBeingHeld;
    bool joyButton0InUse;
    MappingState map;
    InputManager inputManager;
    MainMenu mainMenu;

    readonly Vector2 inputMethodTextSize = new Vector2(52f, 6.2f);
    readonly Vector2 actionTextSize = new Vector2(15f, 6.2f);

    public SimpleQuad cursor;

    public InputMappingCursor(GameObject go)
    {
        inputManager = InputManager.Instance;
        mainMenu = go.GetComponent<MainMenu>();
        cursor = go.AddComponent<SimpleQuad>();
        cursor.enabled = false;
    }

    /// <summary>
    /// Enable cursor and begin assigning procedure.
    /// </summary>
    public void Activate()
    {
        for (int i = 0; i < 4; i++)
            inputManager.characterInputs[i].UnmapPlayer();

        playerToMap = 0;
        buttonToMap = -1;
        buttonIsBeingHeld = true;

        map = MappingState.PLAYER;

        cursor.ShowAtWorld(
            mainMenu.InputInfoDisplays[0].ShowNextKey(),
            inputMethodTextSize.x, 
            inputMethodTextSize.y);
        cursor.enabled = true;
    }

    /// <summary>
    /// Hide cursor.
    /// </summary>
    public void Hide()
    {
        cursor.enabled = false;
    }

    /// <summary>
    /// Check at every frame (called in MainMenu's update) which input
    /// is being pressed and act accordingly.
    /// </summary>
    /// <returns>0: do nothing, -1: back to main menu, 1: start game</returns>
    public int Check()
    {
        // Finished mapping, we can signal to start the game.
        if (playerToMap == GameManager.Instance.NumPlayers)
        {
            if (!buttonIsBeingHeld && Input.GetAxisRaw("Cancel") == 1)
            {
                buttonIsBeingHeld = true;
                playerToMap--;
                buttonToMap = -1;
                inputManager.characterInputs[playerToMap].UnmapPlayer();
                cursor.enabled = true;
                cursor.ShowAtWorld(
                    mainMenu.InputInfoDisplays[playerToMap].Reset(),
                    inputMethodTextSize.x,
                    inputMethodTextSize.y);
                mainMenu.SetTitleToDefault();
                return 0;
            }
            else
            {
                return 1;
            }
        }
        // Check if any controller is holding A/button0
        joyButton0InUse = false;
        for (int i = 0; i < 8; i++)
            if(Input.GetKeyDown(inputManager.joyButton0[i]))
                joyButton0InUse = true;
        // Check that undo/proceed buttons are not being held such that we only
        // act on them once
        if (Input.GetAxisRaw("Cancel") == 0 && Input.GetKey(KeyCode.Return) == false && !joyButton0InUse)
            buttonIsBeingHeld = false;

        if (map == MappingState.PLAYER)
        {
            if (UndoPlayer())
                return -1;
            if (buttonIsBeingHeld)
                return 0;
            if (PlayerCheckGamepad())
                return 0;
            if (PlayerCheckKeyboard())
                return 0;
        }
        if (map == MappingState.BUTTON)
        {
            if (UndoButton())
                return 0;
            if (ButtonCheck())
                return 0;
        }

        return 0;
    }

    /// <summary>
    /// Checks for the undo button and jump back one action of go back to player mapping.
    /// </summary>
    /// <returns>true if the undo button was pressed; else false.</returns>
    bool UndoButton()
    {
        if (Input.GetAxisRaw("Cancel") == 1)
        {
            if (!buttonIsBeingHeld)
            {
                buttonIsBeingHeld = true;
                buttonToMap--;
                if (buttonToMap == -1)
                {
                    map = MappingState.PLAYER;
                    cursor.ShowAtWorld(
                        mainMenu.InputInfoDisplays[playerToMap].UndoKey(),
                        inputMethodTextSize.x,
                        inputMethodTextSize.y);
                }
                else
                {
                    inputManager.characterInputs[playerToMap].UnmapButton(buttonToMap);
                    cursor.ShowAtWorld(
                        mainMenu.InputInfoDisplays[playerToMap].UndoKey() + Vector3.left*0.1f,
                        actionTextSize.x,
                        actionTextSize.y);
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Loops over all available buttons to see if any one should be assigned.
    /// We assign the button to the current buttonToMap and increment.
    /// </summary>
    /// <returns>true if any button was assigned; else false.</returns>
    bool ButtonCheck()
    {
        // Loop over all buttons
        for (int i = 0; i < inputManager.allKeys.Length; i++)
        {
            // If one was pressed
            if (Input.GetKeyDown(inputManager.allKeys[i]))
            {
                // And it hasn't been assigned yet
                if (!inputManager.isButtonMapped(inputManager.allKeys[i]))
                {
                    // We map it, increment buttonToMap and update cursor
                    inputManager.characterInputs[playerToMap].MapButton(
                        buttonToMap, 
                        inputManager.allKeys[i]
                    );
                    buttonToMap++;
                    if (buttonToMap == 4)
                    {
                        mainMenu.InputInfoDisplays[playerToMap].ShowNextKey();
                        playerToMap++;
                        map = MappingState.PLAYER;
                        if (playerToMap != GameManager.Instance.NumPlayers)
                        {
                            cursor.ShowAtWorld(
                                mainMenu.InputInfoDisplays[playerToMap].ShowNextKey(),
                                inputMethodTextSize.x,
                                inputMethodTextSize.y);
                        } 
                        else 
                        {
                            cursor.enabled = false;
                            mainMenu.SetTitleToReadyPrompt(
                                InputManager.Instance.characterInputs[0].inputMode == InputMode.GAMEPAD);
                        }
                    }
                    else
                    {
                        cursor.ShowAtWorld(
                            mainMenu.InputInfoDisplays[playerToMap].ShowNextKey() + Vector3.left * 0.1f,
                            actionTextSize.x,
                            actionTextSize.y);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check if we pressed the undo key and jump back one character 
    /// or signal that we can go back to the main menu.
    /// </summary>
    /// <returns>
    /// true if we press undo at playerToMap == 0; 
    /// false if we didn't press anything or went back one player;
    /// </returns>
    bool UndoPlayer()
    {
        if (Input.GetAxisRaw("Cancel") == 1)
        {
            if (!buttonIsBeingHeld)
            {
                buttonIsBeingHeld = true;
                if (playerToMap == 0)
                {
                    return true;
                }
                else
                {
                    mainMenu.InputInfoDisplays[playerToMap].HideAllButName();
                    playerToMap--;
                    inputManager.characterInputs[playerToMap].UnmapPlayer();
                    cursor.ShowAtWorld(
                        mainMenu.InputInfoDisplays[playerToMap].Reset(),
                        inputMethodTextSize.x,
                        inputMethodTextSize.y);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Check if we pressed any new button0 and map that player.
    /// </summary>
    /// <returns>
    /// true if a new controller pressed button0 and we mapped it; false if no new button0 was pressed.
    /// </returns>
    bool PlayerCheckGamepad()
    {
        for (int i = 0; i < 8; i++)
        {
            if(Input.GetKeyDown(inputManager.joyButton0[i]))
            {
                if (!inputManager.isJoyMapped(i))
                {
                    inputManager.characterInputs[playerToMap]
                                .MapPlayer(InputMode.GAMEPAD, i);
                    mainMenu.InputInfoDisplays[playerToMap].SetToGamepad();
                    playerToMap++;
                    buttonToMap = -1;
                    if (playerToMap < GameManager.Instance.NumPlayers)
                    {
                        cursor.ShowAtWorld(
                            mainMenu.InputInfoDisplays[playerToMap].ShowNextKey(),
                            inputMethodTextSize.x,
                            inputMethodTextSize.y);
                    }
                    else
                    {
                        cursor.enabled = false;
                        mainMenu.SetTitleToReadyPrompt(
                            InputManager.Instance.characterInputs[0].inputMode == InputMode.GAMEPAD);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if we pressed return and moves to buttonUI if that happened.
    /// </summary>
    /// <returns>true if we pressed return on the keyboard; false otherwise.</returns>
    bool PlayerCheckKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            inputManager.characterInputs[playerToMap].MapPlayer(InputMode.KEYBOARD, -1);
            map = MappingState.BUTTON;
            buttonToMap = 0;
            cursor.ShowAtWorld(
                mainMenu.InputInfoDisplays[playerToMap].ShowNextKey() + Vector3.left * 0.1f,
                actionTextSize.x,
                actionTextSize.y);
            return true;
        }
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages mapping and querying of input
/// in menus and during gameplay, respectively.
/// </summary>
public class InputManager : Singleton<InputManager> 
{
    public bool inMainMenu = false;
    public CharacterInput[] characterInputs = new CharacterInput[4];
    
    // Store all necessary inputs for 8 Xbox 360 gamepads
    public string[] joyXAxis {get; private set;}
    public KeyCode[] joyButton0 {get; private set;}
    public KeyCode[] joyButton1 {get; private set;}
    public KeyCode[] joyButtonStart {get; private set;}
    public KeyCode[] allKeys {get; private set;}

     void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < 4; i++)
            characterInputs[i] = new CharacterInput(i);

        joyXAxis = new string[8];
        joyButton0 = new KeyCode[8];
        joyButton1 = new KeyCode[8];
        joyButtonStart = new KeyCode[8];
        for (int i = 0; i < 8; i++)
        {
            joyXAxis[i] = "Joy"+(i+1)+"X";
            // Reference for joystick buttons: http://wiki.unity3d.com/index.php?title=Xbox360Controller
            joyButton0[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), 
                                                       "Joystick"+(i+1)+"Button0");
            joyButton1[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), 
                                                       "Joystick"+(i+1)+"Button1");
            joyButtonStart[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), 
                                                       "Joystick"+(i+1)+"Button7");
        }
        allKeys = (KeyCode[]) System.Enum.GetValues(typeof(KeyCode)) ;

        SceneManager.sceneLoaded += OnSceneChange;
    }

    /// <summary>Resets all characterinputs on menu load.</summary>
    void OnSceneChange(Scene scene, LoadSceneMode mode) 
    {
        if (inMainMenu)
        {
            for (int i = 0; i < 4; i++)
                if (characterInputs != null)
                    characterInputs[i].UnmapPlayer();
        }
    }

    /// <summary>Query input once per frame in-game.</summary>
    void Update ()
    {
        if (!inMainMenu)
        {
            for (int i = 0; i < GameManager.Instance.NumPlayers; i++)
            {
                characterInputs[i].GetInput();
            }
        }
    }

    /// <summary>Checks if gamepad is mapped.</summary>
    /// <param name="joyNumber">Gamepad number to check.</param>
    /// <returns>true if it is already in use, else false.</returns>
    public bool isJoyMapped(int joyNumber)
    {
        for (int i = 0; i < 4; i++)
        {
            if (characterInputs[i].padNumber == joyNumber)
                return true;
        }
        return false;
    }

    /// <summary>checks if keycode is already mapped.</summary>
    /// <param name="key">KeyCode to check</param>
    /// <returns>true if it is already in use, else false.</returns>
    public bool isButtonMapped(KeyCode key)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < characterInputs[i].buttons.Length; j++)
                if (characterInputs[i].buttons[j] == key) return true;
        }
        return false;
    }
}
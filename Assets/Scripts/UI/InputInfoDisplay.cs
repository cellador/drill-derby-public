using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputInfoDisplay : MonoBehaviour
{
    public Text playerText;
    public Text inputMethodText;
    public Text actionAccelerate;
    public Text actionBrake;
    public Text actionLeft;
    public Text actionRight;
    public Text inputActionLabel;
    public Text chooseControllerText;
    public Text assignButtonText;

    int playerNumber;
    /// <summary>
    /// 0: Show player name (default)
    /// 1: Input Method
    /// 2: Accelerate
    /// 3: Brake
    /// 4: Left
    /// 5: Right
    /// 6: Finished
    /// </summary>
    int state;

    readonly string[] infoTextArray = new string[12] {
        "Player 1", "Player 2", "Player 3", "Player 4", 
        "Keyboard or Gamepad?", "Gamepad", "Keyboard", 
        "None", "A", "B", "Stick L", "Stick R"
    };

    public void Setup(int _playerNumber)
    {
        playerNumber = _playerNumber;
        playerText.text = infoTextArray[playerNumber];
    }

    public void Enable()
    {
        HideAllButName();
        transform.position = GameManager.Instance.Players[playerNumber].position;
        playerText.gameObject.SetActive(true);
        state = 0;
    }

    public void HideAllButName()
    {
        state = 0;
        inputMethodText.gameObject.SetActive(false);
        actionAccelerate.gameObject.SetActive(false);
        actionBrake.gameObject.SetActive(false);
        actionLeft.gameObject.SetActive(false);
        actionRight.gameObject.SetActive(false);
        inputActionLabel.gameObject.SetActive(false);
        chooseControllerText.gameObject.SetActive(false);
        assignButtonText.gameObject.SetActive(false);
    }

    public void HideAll()
    {
        HideAllButName();
        playerText.gameObject.SetActive(false);
    }

    public Vector3 Reset()
    {
        HideAllButName();
        inputMethodText.text = infoTextArray[4];
        inputMethodText.gameObject.SetActive(true);
        chooseControllerText.gameObject.SetActive(true);
        state = 1;
        return inputMethodText.rectTransform.position;
    }

    public void SetToGamepad()
    {
        inputMethodText.text = infoTextArray[5];
        inputMethodText.gameObject.SetActive(true);

        inputActionLabel.gameObject.SetActive(true);
        actionAccelerate.text = infoTextArray[8];
        actionAccelerate.gameObject.SetActive(true);
        actionBrake.text = infoTextArray[9];
        actionBrake.gameObject.SetActive(true);
        actionLeft.text = infoTextArray[10];
        actionLeft.gameObject.SetActive(true);
        actionRight.text = infoTextArray[11];
        actionRight.gameObject.SetActive(true);

        chooseControllerText.gameObject.SetActive(false);
        assignButtonText.gameObject.SetActive(false);
    }

    public Vector3 ShowNextKey()
    {
        Vector3 pos;
        state++;
        switch (state)
        {
            // Just entered state 1 (ask for input method)
            case 1:
                // Show corresponding text and put cursor on input method text
                inputMethodText.gameObject.SetActive(true);
                chooseControllerText.gameObject.SetActive(true);
                pos = inputMethodText.rectTransform.position;
                break;
            // Just entered state 2 (ask for accelerate)
            case 2:
                // We choose keyboard (since this is GetNextKey)
                // reset actionAccelerate to none
                // Show corresponding text
                // Cursor on accelerate
                chooseControllerText.gameObject.SetActive(false);
                inputMethodText.text = infoTextArray[6];
                actionAccelerate.text = infoTextArray[7];
                actionAccelerate.gameObject.SetActive(true);
                inputActionLabel.gameObject.SetActive(true);
                assignButtonText.gameObject.SetActive(true);
                pos = actionAccelerate.rectTransform.position;
                break;
            // Just entered state 3 (ask for brake)
            case 3:
                // We update accelerate text
                // We reset actionBrake to none & show text
                // Cursor on actionBrake
                actionAccelerate.text = InputManager.Instance.characterInputs[playerNumber].buttons[0].ToString();
                actionBrake.text = infoTextArray[7];
                actionBrake.gameObject.SetActive(true);
                pos = actionBrake.rectTransform.position;
                break;
            // Just entered state 4 (ask for left)
            case 4:
                // We update brake text
                // We reset actionLeft to none & show text
                // Cursor on actionLeft
                actionBrake.text = InputManager.Instance.characterInputs[playerNumber].buttons[1].ToString();
                actionLeft.text = infoTextArray[7];
                actionLeft.gameObject.SetActive(true);
                pos = actionLeft.rectTransform.position;
                break;
            // Just entered state 5 (ask for right)
            case 5:
                // We update left text
                // We reset actionRight to none & show text
                // Update cursor
                actionLeft.text = InputManager.Instance.characterInputs[playerNumber].buttons[2].ToString();
                actionRight.text = infoTextArray[7];
                actionRight.gameObject.SetActive(true);
                pos = actionRight.rectTransform.position;
                break;
            // Just entered state 6 (finished)
            case 6:
                // We update right text
                // Hide button prompt
                actionRight.text = InputManager.Instance.characterInputs[playerNumber].buttons[3].ToString();
                assignButtonText.gameObject.SetActive(false);
                pos = Vector3.negativeInfinity;
                break;
            default:
                pos = Vector3.negativeInfinity;
                break;
        }

        return pos;
    }

    public Vector3 UndoKey()
    {
        Vector3 pos;

        switch (state)
        {
            // Undo from inputmethod
            case 1:
                inputMethodText.gameObject.SetActive(false);
                chooseControllerText.gameObject.SetActive(false);
                pos = Vector3.negativeInfinity;
                break;
            // Undo from accelerate
            case 2:
                actionAccelerate.gameObject.SetActive(false);
                inputActionLabel.gameObject.SetActive(false);
                assignButtonText.gameObject.SetActive(false);
                chooseControllerText.gameObject.SetActive(true);
                inputMethodText.text = infoTextArray[4];
                pos = inputMethodText.rectTransform.position;
                break;
            // Undo from brake
            case 3:
                actionBrake.gameObject.SetActive(false);
                actionAccelerate.text = infoTextArray[7];
                pos = actionAccelerate.rectTransform.position;
                break;
            // Undo from left
            case 4:
                actionLeft.gameObject.SetActive(false);
                actionBrake.text = infoTextArray[7];
                pos = actionBrake.rectTransform.position;
                break;
            // Undo from right
            case 5:
                actionRight.gameObject.SetActive(false);
                actionLeft.text = infoTextArray[7];
                pos = actionLeft.rectTransform.position;
                break;
            // Undo from finish
            case 6:
                actionRight.text = infoTextArray[7];
                assignButtonText.gameObject.SetActive(true);
                pos = actionRight.rectTransform.position;
                break;
            default:
                pos = Vector3.negativeInfinity;
                break;
        }

        state--;
        return pos;
    }
}

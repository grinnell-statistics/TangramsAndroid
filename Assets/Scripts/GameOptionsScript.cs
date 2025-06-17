using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/* Class: GameOptions
 * Purpose: Managage the options selected by the user
 */
public class GameOptionsScript : MonoBehaviour
{
    public InputField[] vars;
    public GameObject startButton;
    public Dropdown puzzleType;
    public Toggle noLimit;
    public Toggle shortTime;
    public Toggle mediumTime;
    public Toggle longTime;
    public Toggle hint;
    public Toggle timer;
    public GameObject startErrorWarning;
    public static bool hintOn;
    public static bool timerOn;
    public static float time;
    public static string timeTag = "";
    public static int buildIndexScene;
    string[] puzzleArr;

    public void Start()
    {
        // if not the first time playing, puzzle type dropdown displays previously played game
        if (DataManager.gameData.puzzleIndex != 0)
            puzzleType.value = DataManager.gameData.puzzleIndex;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (vars[0].isFocused)
                vars[2].ActivateInputField();
            if (vars[2].isFocused)
                vars[1].ActivateInputField();
            if (vars[1].isFocused)
                startButton.GetComponent<Button>().Select();
        }
    }

    // initializes the hint selection, time selection, and loads game scene
    public void StartGame()
    {
        hintOn = hint.isOn;
        timerOn = timer.isOn;
        DataManager.gameData.puzzleIndex = puzzleType.value;
        DataManager.gameData.displayTime = timer.isOn;
        DataManager.gameData.hintOn = hint.isOn;
        TimeSelection();

        if (puzzleType.value == 0){
            StartError();
            return;
        }
        else {
            startErrorWarning.SetActive(false);
            buildIndexScene = SceneManager.GetActiveScene().buildIndex + puzzleType.value;
            SceneManager.LoadScene(buildIndexScene);
        }
        
        
    }

    //displays an error message if user tries to proceed without clicking a puzzle
    public void StartError()
    {
        startErrorWarning.SetActive(true);
    }

    // assigns time depending on time selection
    public void TimeSelection()
    {
        if (noLimit.isOn)
        {
            time = 0;
            timeTag = "no limit"; 
        }

        else if (shortTime.isOn)
        {
            time = 60;
            timeTag = "other";
        }

        else if (mediumTime.isOn)
        {
            time = 120;
            timeTag = "other";
        }

        else if (longTime.isOn)
        {
            time = 180;
            timeTag = "other";
        }
        DataManager.gameData.regTime = (int) time;
    }

    public void SetPattern(int num)
    {
        //Patterns.currentPattern = GameManager.instance.GetComponent<Patterns>().patterns[num];
        //GameManager.instance.gameData.puzzle = puzzleType.options[num].text.ToString();
        
        /*sends information to 'Patterns' about which Pattern was chosen*/
        /* -1 is present as while the drop down menu is '1' indexed,
            arrays are zero-indexed */
        Patterns.currentPatternNum = (puzzleType.value - 1);
        //Debug.Log("The scene in SetPattern: " + puzzleType.value);
        //Debug.Log("The scene in SetPattern (sent to patterns): " + Patterns.currentPatternNum);
    }

    public void GetVariable1(string s)
    {
        DataManager.gameData.var1 = s;
    }

    public void GetVariable2(string s)
    {
        DataManager.gameData.var2 = s;
    }

    public void GetVariable3(string s)
    {
        DataManager.gameData.var3 = s;
    }
}

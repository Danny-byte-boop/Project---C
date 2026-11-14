using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// There is one line of dialogue, the speaker is either Chuck or BARBARA as they are the main characters.
// Lines are filled in on the TrialManager for both BARBARA and then Chuck. The scene then plays after dialogue is finished.
[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 4)] public string text;
}

/// <summary>
/// Shows dialogue one line at a time with a typewriter effect.
/// Players press Space, Enter, click, or the gamepad A button to continue. Whichever the player prefers :)
/// the first press finishes the line that is typing, the next press moves on.
/// When the last line is done it runs whatever was passed in as onComplete.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    // A shared reference so that the TrialManager can retrieve it more efficiently and easier.
    public static DialogueManager Instance { get; private set; }
    // the dialogue box (hidden when not in use)
    [SerializeField] GameObject panel;
    // shows the speaker's name, either Chuck or Barbara in STRANDED TOGETHER
    [SerializeField] TMP_Text speakerText;
    // shows the dialogue itself, shows the actual words either BARBARA or Chuck are saying.
    [SerializeField] TMP_Text lineText;
    // This is how fast or slow the typing speed is for the characters on the screen.
    [SerializeField] float charsPerSecond = 40f;
    // This is for the "continue" keys for the player to cycle through the dialogue.
    InputAction advanceAction;
    // This is just a boolean statement set to true when the player presses a "continue" key
    bool advancePressed;
    // the dialogue currently playing on the screen
    Coroutine routine;
    // This remains true while a conversation is showing on the players' screen.
    public bool IsPlaying { get; private set; }   

    void Awake()
    {
        Instance = this;

        // Any of these keys moves the dialogue forward, so there are a lot of options when it comes to pushing the dialogue
        //forward
        advanceAction = new InputAction("Advance", InputActionType.Button);
        advanceAction.AddBinding("<Keyboard>/space");
        advanceAction.AddBinding("<Keyboard>/enter");
        advanceAction.AddBinding("<Mouse>/leftButton");
        advanceAction.AddBinding("<Gamepad>/buttonSouth");
        // The panel starts hidden and then pops up when the dialogue is in view
        if (panel != null) panel.SetActive(false);   
    }

    void OnEnable()
    {
        advanceAction.Enable();
        advanceAction.performed += OnAdvance;
    }

    void OnDisable()
    {
        advanceAction.performed -= OnAdvance;
        advanceAction.Disable();
    }

    void OnDestroy()
    {
        advanceAction?.Dispose();
    }

    // The continue was pressed, so the dialogue checks this flag to make sure it ac
    void OnAdvance(InputAction.CallbackContext ctx)
    {
        advancePressed = true;
    }

    // This starts a conversation. OnComplete is called when the last line is dismissed from the screen.
    public void Play(DialogueLine[] lines, Action onComplete)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayRoutine(lines, onComplete));
    }

    // This goes through each line; types it out, and then waits for the player to press a "continue" key before it moves on.
    IEnumerator PlayRoutine(DialogueLine[] lines, Action onComplete)
    {
        IsPlaying = true;
        panel.SetActive(true);

        foreach (var line in lines)
        {
            speakerText.text = line.speaker;
            lineText.text = line.text;
            // This makes it so it starts with no letters showing
            lineText.maxVisibleCharacters = 0;
            // This makes sure that the text knows how many characters it has
            lineText.ForceMeshUpdate();             
            int total = lineText.textInfo.characterCount;

            advancePressed = false;
            float shown = 0f;

            // This is the Typewriter effect where it reveals letters over time, and a press key makes it go to the 
            //end of the line.
            while (lineText.maxVisibleCharacters < total)
            {
                if (advancePressed) break;
                shown += charsPerSecond * Time.unscaledDeltaTime;
                lineText.maxVisibleCharacters = Mathf.Min(total, (int)shown);
                yield return null;
            }
            //This makes sure that the whole line is visible
            lineText.maxVisibleCharacters = total;  

            // This waits for a press before moving on to the next line
            advancePressed = false;
            while (!advancePressed) yield return null;
        }

        panel.SetActive(false);
        IsPlaying = false;
        // This tells whoever started the dialogue that it's finished
        onComplete?.Invoke();    
    }
}
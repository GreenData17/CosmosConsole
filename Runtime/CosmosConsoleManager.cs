using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using System.Linq;

public class CosmosConsoleManager : MonoBehaviour
{
    public static CosmosConsoleManager instance { get; private set; }

    public bool printInitialize = true;
    public bool CaptureFramesPerSecond = true;
    [Space]
    public TMP_InputField consoleInput;
    public GameObject consoleOutputContent;
    public GameObject consoleOutputItem;

    // <private>
    private static List<Command> Commands = new List<Command>();
    private static List<float> FrameDeltaTimeArray = new List<float>();

    private CanvasGroup m_consoleGroup;
    private List<GameObject> m_consoleHistory = new List<GameObject>();

    private void Awake()
    {
        m_consoleGroup = GetComponent<CanvasGroup>();
        if (instance == null) instance = this; else Destroy(gameObject);

        if (printInitialize) SendToConsole("Initialize CosmosConsole...", "#00AA00");

        AddCommand("help", "Shows the help list", Cmd_SendHelp);
        AddCommand("test", "Sends a \"Hello World!\" to the console.", Cmd_SendTest);
        AddCommand("quit", "Quits the game.", Cmd_QuitGame);
        AddCommand("fps", "Shows the current FPS.", Cmd_SendFramesPerSecond);
        AddCommand("clear", "Clears the console.", Cmd_ClearHistory);

        if (printInitialize) SendToConsole("Initialized!", "#00AA00");
    }

    private void Start() {
        CloseConsole();
    }

    private void Update()
    {
        UpdateInput();
        UpdateFramesPerSecondList();

        if (Input.GetKey(KeyCode.F12)) OpenConsole();
    }

    private void UpdateFramesPerSecondList()
    {
        if (!CaptureFramesPerSecond) return;

        FrameDeltaTimeArray.Add(Time.unscaledDeltaTime);

        if(FrameDeltaTimeArray.Count > 50)
        {
            FrameDeltaTimeArray.RemoveAt(0);
        }
    }

    private void UpdateInput()
    {
        if (!string.IsNullOrEmpty(consoleInput.text))
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                HandleInput(consoleInput.text);
                consoleInput.text = "";
                consoleInput.Select();
                consoleInput.ActivateInputField();
            }
        }
    }

    private void OnEnable() =>
        Application.logMessageReceived += HandleLog;
    

    private void OnDisable() =>
        Application.logMessageReceived -= HandleLog;
    

    // <Functions>

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (string.IsNullOrWhiteSpace(logString)) return;

        switch (type)
        {
            case LogType.Log:
                SendToConsole($"[LOG] {logString}");
                break;
            case LogType.Warning:
                SendToConsole($"[WARN] {logString}", "#DDDD00");
                break;
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                SendToConsole($"[ERROR] {logString}", "#DD0000");
                break;
            default:
                SendToConsole(logString);
                break;
        }
    }

    private void HandleInput(string input)
    {
        var arguments = input.Split(' ');

        if (arguments.Length > 1)
        {
            List<string> argumentsSeperated = input.Split(' ').ToList();
            argumentsSeperated.RemoveAt(0);
            
            CallCommand(arguments[0], argumentsSeperated.ToArray());
        }
        else CallCommand(arguments[0], new string[] { });
    }

    private void CallCommand(string alias, string[] arguments)
    {
        bool valid = false;

        foreach (Command command in Commands)
        {
            if (alias != command.alias) continue;

            command.method(arguments);
            valid = true;
        }

        if (!valid)
        {
            SendToConsole($"There is no Command with the alias \"{alias}\".", "#DD0000");
        }
    }

    public static void SendToConsole(string content, string colorInHex = "#FFFFFF")
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var tempOutputItem = Instantiate(instance.consoleOutputItem, instance.consoleOutputContent.transform);
        tempOutputItem.GetComponent<TMP_Text>().text = $"<color={colorInHex}>{content}";
        tempOutputItem.SetActive(true);
        var consoleOutputScrollRect = instance.consoleOutputContent.transform.parent.parent.gameObject.GetComponent<ScrollRect>();
        instance.m_consoleHistory.Add(tempOutputItem);
        Canvas.ForceUpdateCanvases();
        consoleOutputScrollRect.verticalNormalizedPosition = 0;
    }
    
    public void CloseConsole()
    {
        m_consoleGroup.alpha = 0;
        m_consoleGroup.interactable = false;
        m_consoleGroup.blocksRaycasts = false;
    }

    public void OpenConsole()
    {
        m_consoleGroup.alpha = 1;
        m_consoleGroup.interactable = true;
        m_consoleGroup.blocksRaycasts = true;
    }

    public static int GetFPS()
    {
        float total = 0f;

        foreach (float deltaTime in FrameDeltaTimeArray)
        {
            total += deltaTime;
        }

        return Mathf.RoundToInt(50f / total);
    }

    // <Command realated Methods>

    public static void AddCommand(string alias, Action<string[]> methodeToExecute)
    {
        if (methodeToExecute.GetMethodInfo().GetParameters().Length == 0) return;

        Commands.Add(new Command() { alias = alias, method = methodeToExecute });
    }

    public static void AddCommand(string alias, string description, Action<string[]> methodeToExecute)
    {
        if (methodeToExecute.GetMethodInfo().GetParameters().Length == 0) return;

        Commands.Add(new Command() { alias = alias, description = description, method = methodeToExecute });
    }

    public static void RemoveCommand(string alias)
    {
        foreach(Command command in Commands)
        {
            if (command.alias != alias) continue;
            Commands.Remove(command);
        }
    }

    #region Commands

    private void Cmd_SendHelp(string[] args)
    {
        if (args.Length == 0) // show help list
        {
            string[] helpContent =
            {
                "---------------------------------",
                "=> <color=#00DD00>Thank you for using<color=#FFFFFF> CosmosConsole<color=#00DD00>!<color=#FFFFFF> <=",
            };

            foreach (string content in helpContent) { SendToConsole(content); }

            foreach (Command command in Commands)
            {
                if (string.IsNullOrEmpty(command.description)) continue;

                SendToConsole($"<color=#00DD00>{command.alias}<color=#FFFFFF> = <color=#DDDD00>{command.description}<color=#FFFFFF>");
            }
            SendToConsole("---------------------------------");
        }
        else // search help for a specifide command
        {
            foreach (Command command in Commands)
            {
                if (command.alias != args[0]) continue;

                if (string.IsNullOrEmpty(command.description))
                {
                    SendToConsole("<color=#AA8800>There is no help defined for this command.<color=#FFFFFF>");
                    continue;
                }

                SendToConsole($"<color=#00DD00>{command.alias}<color=#FFFFFF> = <color=#DDDD00>{command.description}<color=#FFFFFF>");
            }
        }
    }

    private void Cmd_SendTest(string[] args)
    {
        SendToConsole("Hello World!");
    }

    private void Cmd_QuitGame(string[] args)
    {
        Application.Quit();
        SendToConsole("Quitting Failed...", "#DD0000");
        SendToConsole("I'll never let you go! (~0o0)~ Uuuuuu~", "#AA0000");
    }

    private void Cmd_SendFramesPerSecond(string[] args)
    {
        int fps = GetFPS();
        string color = "#DD0000";

        if(fps > 30)
        {
            color = "#DDDD00";
            if(fps > 60)
            {
                color = "#00DD00";
            }
        }

        SendToConsole($"FPS: <color={color}>{fps}<color=#FFFFFF>");
    }

    private void Cmd_ClearHistory(string[] args)
    {
        foreach(GameObject consoleOutputitem in m_consoleHistory)
        {
            Destroy(consoleOutputitem);
        }
    }

    #endregion

}

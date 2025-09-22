using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class DeveloperConsole : MonoBehaviour
{
    public GameObject consoleUI;
    public TMP_InputField inputField;
    public TMP_Text outputText;

    
    private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();

    private bool isOpen = false;

    public PlayerHealth player;
    public NoClip noclip;
    public PlayerMovement playerMovement;

    [System.Serializable]
    public class EnemyEntry
    {
        public string name;
        public GameObject prefab;
    }

    public List<EnemyEntry> enemyPrefabs = new List<EnemyEntry>();
    public Transform spawnPoint;

    void Start()
    {
        
        commands.Add("god", args => ToggleGodMode());
        commands.Add("noclip", args => ToggleNoClip());
        commands.Add("help", args => ShowHelp());
        commands.Add("spawn", SpawnEnemy);

        commands.Add("forward", args => playerMovement.MoveForward());
        commands.Add("back", args => playerMovement.MoveBack());
        commands.Add("left", args => playerMovement.MoveLeft());
        commands.Add("right", args => playerMovement.MoveRight());

        consoleUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            isOpen = !isOpen;
            consoleUI.SetActive(isOpen);

            if (isOpen) inputField.ActivateInputField();
        }
    }

    public void OnCommandEntered()
    {
        string input = inputField.text.ToLower().Trim();
        inputField.text = "";

        if (string.IsNullOrEmpty(input))
        {
            inputField.ActivateInputField();
            return;
        }

        string[] parts = input.Split(' ');
        string cmd = parts[0];
        string[] args = parts.Length > 1 ? parts[1..] : new string[0];

        if (commands.ContainsKey(cmd))
        {
            Log("Running command: " + input);
            commands[cmd].Invoke(args);
        }
        else
        {
            Log("Unknown command: " + cmd);
        }

        inputField.ActivateInputField();
    }

    private void Log(string message)
    {
        outputText.text += "\n" + message;
    }

    public void ToggleGodModeButton()
    {
        ToggleGodMode();
    }

    public void ToggleNoClipButton()
    {
        ToggleNoClip();
    }

    private void ToggleGodMode()
    {
        if (player == null)
        {
            Log("No player assigned!");
            return;
        }

        player.invincible = !player.invincible;
        Log("God mode: " + player.invincible);
    }

    private void ToggleNoClip()
    {
        if (noclip == null)
        {
            Log("No NoClip script assigned!");
            return;
        }

        noclip.ToggleNoClip();
        Log("NoClip: " + noclip.noclipActive);
    }

    private void ShowHelp()
    {
        Log("Available commands: " + string.Join(", ", commands.Keys));
    }

    private void SpawnEnemy(string[] args)
    {
        if (args.Length == 0)
        {
            Log("Gebruik: spawn <enemyNaam>");
            return;
        }

        string enemyName = args[0];

        EnemyEntry entry = enemyPrefabs.Find(e => e.name.ToLower() == enemyName.ToLower());
        if (entry == null || entry.prefab == null)
        {
            Log("Geen enemy gevonden met naam: " + enemyName);
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 3f;
        Instantiate(entry.prefab, pos, Quaternion.identity);
        Log("Spawned enemy: " + enemyName);
    }
}

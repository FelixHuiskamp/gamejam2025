using UnityEngine;
using UnityEngine.UI;

public class JumpButtonUI : MonoBehaviour
{
    [Header("Verwijzing naar de CommandExecuter in de scene")]
    public CommandExecutor commandExecutor;

    [Header("Command die moet worden uitgevoerd")]
    public string commandToExecute = "PlayerJump Jump";

    private void Start()
    {
        if (commandExecutor == null)
        {
            commandExecutor = FindObjectOfType<CommandExecutor>();
            if (commandExecutor == null)
            {
                Debug.LogError("[JumpButtonUI] Geen CommandExecutor gevonden in de scene!");
            }
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("[JumpButtonUI] Geen Button component gevonden. Voeg dit script toe aan een UI Button.");
        }
    }

    private void OnButtonClicked()
    {
        if (commandExecutor != null)
        {
            Debug.Log($"[JumpButtonUI] Uitvoeren van commando: {commandToExecute}");
            commandExecutor.ExecuteCommand(commandToExecute);
        }
        else 
        {
            Debug.LogError("[JumpButtonUI] Geen CommandExecutor gekoppeld!");
        }
    }
}
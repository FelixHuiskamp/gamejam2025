using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpItem : MonoBehaviour
{
    public TMP_Text labelText;
    public Button button;

    private string commandName;
    private string description;
    private Action<string> onClickCallback;

    public void Setup(string commandName, string description, Action<string> onClick)
    {
        this.commandName = commandName;
        this.description = description;
        this.onClickCallback = onClick;

        labelText.text = $"{commandName} - {description}";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(commandName));
    }
}

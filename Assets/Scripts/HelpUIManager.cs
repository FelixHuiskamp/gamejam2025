using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class HelpUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject helpPanel;            
    public Transform contentParent;         
    public GameObject helpItemPrefab;       
    public Button closeButton;              
    public Button overlayButton;            

    
    private List<GameObject> spawnedItems = new List<GameObject>();

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (overlayButton != null)
            overlayButton.onClick.AddListener(Close);

        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    
    public void ShowAll(IDictionary<string, string> commands, Action<string> onItemClicked)
    {
        ClearItems();

        foreach (var kv in commands)
        {
            string name = kv.Key;
            string desc = string.IsNullOrEmpty(kv.Value) ? "-" : kv.Value;

            GameObject go = Instantiate(helpItemPrefab, contentParent);
            HelpItem item = go.GetComponent<HelpItem>();
            if (item != null)
            {
                item.Setup(name, desc, onItemClicked);
            }

            spawnedItems.Add(go);
        }

        Open();
    }

    public void Open()
    {
        if (helpPanel != null)
            helpPanel.SetActive(true);
    }

    public void Close()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    private void ClearItems()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] != null)
                Destroy(spawnedItems[i]);
        }
        spawnedItems.Clear();
    }
}

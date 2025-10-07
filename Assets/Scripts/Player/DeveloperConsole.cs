using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class DeveloperConsole : MonoBehaviour
{
    public GameObject consoleUI;
    public TMP_InputField inputField;
    public TMP_Text outputText;

    private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();
    private Dictionary<string, string> commandDescriptions = new Dictionary<string, string>();

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

    [Header("Help UI")]
    public HelpUIManager helpUIManager;

    [Header("Texture Command Settings")]
    public string texturesResourceFolder = "Textures"; // Resources/Textures/<name>
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<SpriteRenderer, Sprite> originalSprites = new Dictionary<SpriteRenderer, Sprite>();
    private bool texturesApplied = false;

    [Header("Audio (Music) Settings")]
    public string audioResourceFolder = "Audio"; // Assets/Resources/Audio/<name>
    public AudioSource musicSource;              // optional: sleep zelf een AudioSource in, anders wordt er één aangemaakt
    private AudioClip currentClip;

    void Start()
    {
        // base commands
        AddCommand("god", args => ToggleGodMode(), "Toggle god mode (invincible). Usage: god");
        AddCommand("noclip", args => ToggleNoClip(), "Toggle noclip (fly through walls). Usage: noclip");
        AddCommand("help", ShowHelp, "Show help UI. Usage: help OR help <command>");
        AddCommand("spawn", SpawnEnemy, "Spawn an enemy by name. Usage: spawn <enemyName>");
        AddCommand("list_enemies", args => ListEnemies(), "Show all available enemy names for spawn.");
        AddCommand("forward", args => playerMovement?.MoveForward(), "Move player one step forward (console step).");
        AddCommand("back", args => playerMovement?.MoveBack(), "Move player one step back (console step).");
        AddCommand("left", args => playerMovement?.MoveLeft(), "Move player one step left (console step).");
        AddCommand("right", args => playerMovement?.MoveRight(), "Move player one step right (console step).");
        AddCommand("spawnobj", SpawnObjectCommand, "Spawn a prefab from Resources/Prefabs. Usage: spawnobj <name> [count]");
        AddCommand("spawnobj_at", SpawnObjectAtCommand, "Spawn a prefab at world position. Usage: spawnobj_at <name> <x> <y> <z>");

        // texture commands
        AddCommand("settexture", SetTextureCommand, "Set a Texture2D from Resources/Textures by name on all Renderers. Usage: settexture <name>");
        AddCommand("cleartextures", ClearTextures, "Restore original materials/sprites. Usage: cleartextures");

        // audio / music commands
        AddCommand("playmusic", PlayMusicCommand, "Play an audio clip from Resources/Audio. Usage: playmusic <name> [loop]");
        AddCommand("stopmusic", StopMusicCommand, "Stop current music. Usage: stopmusic");
        AddCommand("pausemusic", PauseMusicCommand, "Pause current music. Usage: pausemusic");
        AddCommand("resumemusic", ResumeMusicCommand, "Resume paused music. Usage: resumemusic");
        AddCommand("setvolume", SetMusicVolumeCommand, "Set music volume (0..1). Usage: setvolume 0.5");
        AddCommand("listmusic", ListMusicCommand, "List available audio clips in Resources/Audio. Usage: listmusic");

        // ensure console UI hidden
        consoleUI.SetActive(false);

        // ensure we have an AudioSource
        if (musicSource == null)
        {
            // maak een AudioSource op hetzelfde object
            musicSource = gameObject.GetComponent<AudioSource>();
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();

            musicSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            isOpen = !isOpen;
            consoleUI.SetActive(isOpen);

            if (isOpen) inputField.ActivateInputField();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
    }

    public void OnCommandEntered()
    {
        string input = inputField.text.Trim();
        inputField.text = "";

        if (string.IsNullOrEmpty(input))
        {
            inputField.ActivateInputField();
            return;
        }

        // keep original input for logging (to preserve case if user typed it)
        string originalInput = input;
        string lower = input.ToLower();
        string[] parts = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0];
        string[] args = parts.Length > 1 ? parts[1..] : new string[0];

        if (commands.ContainsKey(cmd))
        {
            Log("> " + originalInput);
            try
            {
                commands[cmd].Invoke(args);
            }
            catch (Exception ex)
            {
                Log("Error while executing command: " + ex.Message);
            }
        }
        else
        {
            Log("Unknown command: " + cmd + "  (type 'help' to list commands)");
        }

        inputField.ActivateInputField();
    }

    private void Log(string message)
    {
        outputText.text += "\n" + message;
    }

    private void AddCommand(string name, Action<string[]> action, string description = "")
    {
        name = name.ToLower();
        if (!commands.ContainsKey(name))
        {
            commands.Add(name, action);
            commandDescriptions[name] = description ?? "";
        }
    }

    // ---------- command implementations ----------
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

    private void ShowHelp(string[] args)
    {
        if (args.Length > 0)
        {
            string q = args[0].ToLower();
            if (commandDescriptions.ContainsKey(q))
            {
                Log($"help {q} : {commandDescriptions[q]}");
            }
            else
            {
                Log("No help available for command: " + q);
            }
            return;
        }

        if (helpUIManager == null)
        {
            Log("=== Command List ===");
            foreach (var kv in commandDescriptions)
            {
                Log($"{kv.Key} : {kv.Value}");
            }
            return;
        }

        helpUIManager.ShowAll(commandDescriptions, OnHelpItemClicked);
    }

    private void OnHelpItemClicked(string commandName)
    {
        inputField.text = commandName;
        inputField.ActivateInputField();

        if (commands.ContainsKey(commandName))
        {
            try
            {
                commands[commandName].Invoke(new string[0]);
                Log($"Executed: {commandName}");
            }
            catch (Exception)
            {
                Log($"Command {commandName} requires arguments or failed to run automatically. Filled input field for you.");
            }
        }
    }

    private void ListEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Log("No enemy prefabs assigned.");
            return;
        }

        Log("=== Enemy List ===");
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            var e = enemyPrefabs[i];
            Log($"{i + 1}. {e.name}");
        }
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
            Log("Geen enemy gevonden met naam: " + enemyName + ". Gebruik 'list_enemies' om names te zien.");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 3f;
        Instantiate(entry.prefab, pos, Quaternion.identity);
        Log("Spawned enemy: " + enemyName);
    }

    // ================= Texture commands =================
    private void SetTextureCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Log("Gebruik: settexture <resourceName>  (plaats textures in Assets/Resources/" + texturesResourceFolder + "/ )");
            return;
        }

        string name = args[0];
        Texture2D tex = Resources.Load<Texture2D>(texturesResourceFolder + "/" + name);
        if (tex == null)
        {
            Log("Texture niet gevonden in Resources/" + texturesResourceFolder + "/" + name);
            return;
        }

        ApplyTextureToAll(tex);
        Log("Applied texture '" + name + "' to scene renderers and sprites.");
    }

    private void ApplyTextureToAll(Texture2D tex)
    {
        if (texturesApplied)
        {
            Log("Er waren al textures toegepast — originele materialen worden eerst hersteld.");
            ClearTextures(null);
        }

        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            if (!originalMaterials.ContainsKey(r))
                originalMaterials[r] = r.sharedMaterials;

            Material[] shared = r.sharedMaterials;
            Material[] newMats = new Material[shared.Length];

            for (int i = 0; i < shared.Length; i++)
            {
                Material baseMat = shared[i];
                if (baseMat == null)
                {
                    newMats[i] = new Material(Shader.Find("Standard"));
                }
                else
                {
                    newMats[i] = new Material(baseMat);
                }

                if (newMats[i].HasProperty("_MainTex"))
                {
                    newMats[i].mainTexture = tex;
                }
                else
                {
                    try { newMats[i].SetTexture("_MainTex", tex); } catch { }
                }
            }

            r.materials = newMats;
        }

        SpriteRenderer[] srs = FindObjectsOfType<SpriteRenderer>();
        foreach (SpriteRenderer sr in srs)
        {
            if (sr == null) continue;
            if (!originalSprites.ContainsKey(sr))
                originalSprites[sr] = sr.sprite;

            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            sr.sprite = newSprite;
        }

        texturesApplied = true;
    }

    private void ClearTextures(string[] args)
    {
        foreach (var kv in originalMaterials)
        {
            Renderer r = kv.Key;
            Material[] mats = kv.Value;
            if (r == null) continue;
            try
            {
                r.sharedMaterials = mats;
            }
            catch
            {
                try { r.materials = mats; } catch { }
            }
        }
        originalMaterials.Clear();

        foreach (var kv in originalSprites)
        {
            SpriteRenderer sr = kv.Key;
            Sprite s = kv.Value;
            if (sr == null) continue;
            sr.sprite = s;
        }
        originalSprites.Clear();

        texturesApplied = false;
        Log("Original materials and sprites restored.");
    }

    // ================= Audio / Music commands =================

    // playmusic <name> [loop]
    private void PlayMusicCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Log("Gebruik: playmusic <name> [loop]. Plaats audiobestanden in Assets/Resources/" + audioResourceFolder + "/");
            return;
        }

        // support multi-word names: if last arg is "loop" -> enable loop
        bool loop = false;
        List<string> nameParts = new List<string>(args);
        if (nameParts.Count > 1 && nameParts[nameParts.Count - 1].ToLower() == "loop")
        {
            loop = true;
            nameParts.RemoveAt(nameParts.Count - 1);
        }

        string name = string.Join(" ", nameParts).Trim();
        if (string.IsNullOrEmpty(name))
        {
            Log("Geen geldige naam opgegeven.");
            return;
        }

        // probeer direct Resources.Load
        AudioClip clip = Resources.Load<AudioClip>(audioResourceFolder + "/" + name);

        // fallback: case-insensitive zoek in Resources folder
        if (clip == null)
        {
            AudioClip[] all = Resources.LoadAll<AudioClip>(audioResourceFolder);
            foreach (var c in all)
            {
                if (c != null && string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    clip = c;
                    break;
                }
            }
        }

        if (clip == null)
        {
            Log($"AudioClip '{name}' niet gevonden in Resources/{audioResourceFolder}. Gebruik 'listmusic' om beschikbare namen te zien.");
            return;
        }

        currentClip = clip;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
        Log($"Playing '{clip.name}' (loop={loop})");
    }

    private void StopMusicCommand(string[] args)
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            Log("Music stopped.");
        }
        else
        {
            Log("No music is playing.");
        }
    }

    private void PauseMusicCommand(string[] args)
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            Log("Music paused.");
        }
        else
        {
            Log("No music is playing to pause.");
        }
    }

    private void ResumeMusicCommand(string[] args)
    {
        if (musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
            Log("Music resumed.");
        }
        else if (musicSource.isPlaying)
        {
            Log("Music is already playing.");
        }
        else
        {
            Log("No music to resume.");
        }
    }

    // setvolume <0..1>
    private void SetMusicVolumeCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Log("Gebruik: setvolume <0..1>");
            return;
        }

        if (float.TryParse(args[0], out float v))
        {
            musicSource.volume = Mathf.Clamp01(v);
            Log($"Music volume set to {musicSource.volume}");
        }
        else
        {
            Log("Kon volume niet parsen. Voorbeeld: setvolume 0.5");
        }
    }

    private void ListMusicCommand(string[] args)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(audioResourceFolder);
        if (clips == null || clips.Length == 0)
        {
            Log("Geen audio clips gevonden in Resources/" + audioResourceFolder);
            return;
        }

        Log("=== Music list ===");
        foreach (var c in clips)
            Log("- " + c.name);
    }

    // ---------- Spawn generic prefab commands (NavMesh-aware) ----------

    // spawnobj <name> [count]
    // probeert ieder object op het dichtstbijzijnde NavMesh punt te plaatsen (binnen sampleDistance)
    private void SpawnObjectCommand(string[] args)
    {
        if (args.Length == 0)
        {
            Log("Gebruik: spawnobj <prefabName> [count]");
            return;
        }

        int count = 1;
        List<string> parts = new List<string>(args);

        if (parts.Count > 0 && int.TryParse(parts[parts.Count - 1], out int parsedCount))
        {
            count = Mathf.Max(1, parsedCount);
            parts.RemoveAt(parts.Count - 1);
        }

        string name = string.Join(" ", parts).Trim();
        if (string.IsNullOrEmpty(name))
        {
            Log("Geen geldige prefab naam opgegeven.");
            return;
        }

        string folder = "Prefabs"; // Resources/Prefabs/
        GameObject prefab = Resources.Load<GameObject>(folder + "/" + name);

        if (prefab == null)
        {
            GameObject[] all = Resources.LoadAll<GameObject>(folder);
            foreach (var p in all)
            {
                if (p != null && string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    prefab = p;
                    break;
                }
            }
        }

        if (prefab == null)
        {
            Log($"Prefab '{name}' niet gevonden in Resources/{folder}. Controleer naam of gebruik 'list'.");
            return;
        }

        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 3f;
        float sampleDistance = 5f; // maximaal zoeken naar NavMesh binnen 5 meter

        for (int i = 0; i < count; i++)
        {
            // spreid ze licht zodat ze niet op elkaar stapelen
            Vector3 offset = new Vector3((i % 5) * 1.2f, 0f, (i / 5) * 1.2f);
            Vector3 intendedPos = basePos + offset;

            // probeer sample op NavMesh
            NavMeshHit hit;
            bool found = NavMesh.SamplePosition(intendedPos, out hit, sampleDistance, NavMesh.AllAreas);

            GameObject instance = Instantiate(prefab, intendedPos, Quaternion.identity);

            // als prefab heeft NavMeshAgent, warp naar NavMesh positie (indien gevonden)
            NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                if (found)
                {
                    // Warpen zet agent direct op de NavMesh positie (veilig)
                    agent.Warp(hit.position);
                }
                else
                {
                    Log($"Waarschuwing: geen NavMesh in de buurt van {intendedPos}. Enemy kan niet meteen navigeren.");
                    // als je wil kun je nog NavMesh.SamplePosition met grotere radius proberen hier
                }
            }
            else
            {
                // geen agent: positioneer GameObject zelf op NavMesh indien gevonden
                if (found)
                    instance.transform.position = hit.position;
                else
                    instance.transform.position = intendedPos; // fallback
            }
        }

        Log($"Spawned {count}x '{prefab.name}' (nav-sampled within {sampleDistance}m).");
    }

    // spawnobj_at <name> <x> <y> <z>
    private void SpawnObjectAtCommand(string[] args)
    {
        if (args.Length < 4)
        {
            Log("Gebruik: spawnobj_at <prefabName> <x> <y> <z>");
            return;
        }

        int n = args.Length;
        string name = string.Join(" ", args, 0, n - 3);

        if (!float.TryParse(args[n - 3], out float x) ||
            !float.TryParse(args[n - 2], out float y) ||
            !float.TryParse(args[n - 1], out float z))
        {
            Log("Kon positie niet parsen. Voorbeeld: spawnobj_at tree 10 0 5");
            return;
        }

        string folder = "Prefabs";
        GameObject prefab = Resources.Load<GameObject>(folder + "/" + name);

        if (prefab == null)
        {
            GameObject[] all = Resources.LoadAll<GameObject>(folder);
            foreach (var p in all)
            {
                if (p != null && string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    prefab = p;
                    break;
                }
            }
        }

        if (prefab == null)
        {
            Log($"Prefab '{name}' niet gevonden in Resources/{folder}.");
            return;
        }

        Vector3 intendedPos = new Vector3(x, y, z);
        float sampleDistance = 5f;
        NavMeshHit hit;
        bool found = NavMesh.SamplePosition(intendedPos, out hit, sampleDistance, NavMesh.AllAreas);

        GameObject instance = Instantiate(prefab, intendedPos, Quaternion.identity);

        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (found)
                agent.Warp(hit.position);
            else
                Log($"Waarschuwing: geen NavMesh in de buurt van {intendedPos}. Agent staat mogelijk buiten NavMesh.");
        }
        else
        {
            if (found)
                instance.transform.position = hit.position;
            else
                instance.transform.position = intendedPos;
        }

        Log($"Spawned '{prefab.name}' at {(found ? hit.position : intendedPos)} (nav-sampled within {sampleDistance}m).");
    }


}


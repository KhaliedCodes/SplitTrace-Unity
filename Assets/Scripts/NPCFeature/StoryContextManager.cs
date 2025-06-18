using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

[CreateAssetMenu(fileName = "StoryContext", menuName = "RPG/Story Context Manager")]
public class StoryContextManager : ScriptableObject
{
    [Header("Current Scene Information")]
    public string currentLocation;
    public string currentTime;
    public string currentObjective;
    
    [Header("Story Progress")]
    [Range(0, 100)] public int storyProgressPercentage = 0;
    public string currentChapter = "Chapter 1";
    
    [Header("Discovered Clues")]
    public List<string> discoveredClues = new List<string>();
    
    [Header("Known Suspects")]
    public List<string> knownSuspects = new List<string>();
    
    [Header("NPC Secrets Unlocked")]
    [SerializeField] private List<string> npcNames = new List<string>();
    [SerializeField] private List<string> npcSecrets = new List<string>();
    
    [Header("Important Events")]
    public List<string> keyEvents = new List<string>();
    
    [Header("Player Choices History")]
    public List<string> playerChoices = new List<string>();

    // Dictionary for runtime use (since Unity can't serialize Dictionary directly)
    private Dictionary<string, string> _npcRevealedInfo;
    
    public Dictionary<string, string> npcRevealedInfo
    {
        get
        {
            if (_npcRevealedInfo == null)
            {
                _npcRevealedInfo = new Dictionary<string, string>();
                // Populate from serialized lists
                for (int i = 0; i < Mathf.Min(npcNames.Count, npcSecrets.Count); i++)
                {
                    _npcRevealedInfo[npcNames[i]] = npcSecrets[i];
                }
            }
            return _npcRevealedInfo;
        }
    }

    private void OnEnable()
    {
        // Initialize dictionary from serialized data
        if (_npcRevealedInfo == null)
        {
            _npcRevealedInfo = new Dictionary<string, string>();
            for (int i = 0; i < Mathf.Min(npcNames.Count, npcSecrets.Count); i++)
            {
                _npcRevealedInfo[npcNames[i]] = npcSecrets[i];
            }
        }
    }

    // Combine the current context into a string for Gemini
    public string GetContextString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Current Story Progress: {storyProgressPercentage}% - {currentChapter}");
        sb.AppendLine($"Location: {currentLocation}");
        sb.AppendLine($"Time: {currentTime}");
        sb.AppendLine($"Current Objective: {currentObjective}");
        
        if (discoveredClues.Count > 0)
            sb.AppendLine("Clues discovered: " + string.Join(", ", discoveredClues));
        
        if (knownSuspects.Count > 0)
            sb.AppendLine("Known suspects: " + string.Join(", ", knownSuspects));
        
        if (keyEvents.Count > 0)
            sb.AppendLine("Important events: " + string.Join(", ", keyEvents));
        
        if (npcRevealedInfo.Count > 0)
        {
            sb.AppendLine("Information revealed by NPCs:");
            foreach (var pair in npcRevealedInfo)
                sb.AppendLine($" - {pair.Key}: {pair.Value}");
        }
        
        if (playerChoices.Count > 0)
        {
            sb.AppendLine("Recent player choices: " + string.Join(", ", playerChoices.GetRange(
                Mathf.Max(0, playerChoices.Count - 3), Mathf.Min(3, playerChoices.Count))));
        }
        
        return sb.ToString();
    }

    // Get a condensed version for shorter context
    public string GetCondensedContextString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Progress: {storyProgressPercentage}% | Location: {currentLocation} | Objective: {currentObjective}");
        
        if (discoveredClues.Count > 0)
            sb.AppendLine("Key clues: " + string.Join(", ", discoveredClues.GetRange(
                Mathf.Max(0, discoveredClues.Count - 3), Mathf.Min(3, discoveredClues.Count))));
        
        if (knownSuspects.Count > 0)
            sb.AppendLine("Suspects: " + string.Join(", ", knownSuspects));
        
        return sb.ToString();
    }

    // Add a clue and check for story progression
    public void AddClue(string clue)
    {
        if (!discoveredClues.Contains(clue))
        {
            discoveredClues.Add(clue);
            UpdateStoryProgress();
            
            // Trigger event for other systems
            OnClueDiscovered?.Invoke(clue);
        }
    }

    public void AddSuspect(string suspect)
    {
        if (!knownSuspects.Contains(suspect))
        {
            knownSuspects.Add(suspect);
            OnSuspectIdentified?.Invoke(suspect);
        }
    }

    public void RevealNPCInfo(string npcName, string info)
    {
        if (!npcRevealedInfo.ContainsKey(npcName))
        {
            npcRevealedInfo.Add(npcName, info);
            // Update serialized lists
            npcNames.Add(npcName);
            npcSecrets.Add(info);
            
            OnNPCInfoRevealed?.Invoke(npcName, info);
        }
    }

    public void AddKeyEvent(string eventDescription)
    {
        keyEvents.Add($"[{currentTime}] {eventDescription}");
        UpdateStoryProgress();
        OnKeyEventOccurred?.Invoke(eventDescription);
    }

    public void RecordPlayerChoice(string choice)
    {
        playerChoices.Add($"[{DateTime.Now:HH:mm}] {choice}");
        
        // Keep only last 10 choices to prevent memory bloat
        if (playerChoices.Count > 10)
        {
            playerChoices.RemoveAt(0);
        }
    }

    private void UpdateStoryProgress()
    {
        // Simple progress calculation based on discovered content
        int totalDiscovered = discoveredClues.Count + knownSuspects.Count + keyEvents.Count;
        int newProgress = Mathf.Min(100, totalDiscovered * 10); // Adjust multiplier as needed
        
        if (newProgress > storyProgressPercentage)
        {
            storyProgressPercentage = newProgress;
            OnStoryProgressUpdated?.Invoke(storyProgressPercentage);
        }
    }

    // Check if specific information has been discovered
    public bool HasDiscoveredClue(string clue)
    {
        return discoveredClues.Contains(clue);
    }

    public bool HasIdentifiedSuspect(string suspect)
    {
        return knownSuspects.Contains(suspect);
    }

    public bool HasNPCRevealedInfo(string npcName)
    {
        return npcRevealedInfo.ContainsKey(npcName);
    }

    // Get information that should be available to NPCs based on story progress
    public string GetNPCAvailableInfo(NPCPersonality npcPersonality)
    {
        if (npcPersonality == null) return "";
        
        StringBuilder availableInfo = new StringBuilder();
        
        // Based on NPC's story knowledge level
        if (npcPersonality.storyKnowledge > 0.7f)
        {
            // High knowledge NPCs know about most discoveries
            if (discoveredClues.Count > 0)
                availableInfo.AppendLine("Recent discoveries: " + string.Join(", ", discoveredClues));
            if (knownSuspects.Count > 0)
                availableInfo.AppendLine("Known suspects: " + string.Join(", ", knownSuspects));
        }
        else if (npcPersonality.storyKnowledge > 0.3f)
        {
            // Medium knowledge NPCs know about major events
            if (keyEvents.Count > 0)
                availableInfo.AppendLine("Recent events: " + string.Join(", ", keyEvents.GetRange(
                    Mathf.Max(0, keyEvents.Count - 2), Mathf.Min(2, keyEvents.Count))));
        }
        
        return availableInfo.ToString();
    }

    // Events for other systems to subscribe to
    public event Action<string> OnClueDiscovered;
    public event Action<string> OnSuspectIdentified;
    public event Action<string, string> OnNPCInfoRevealed;
    public event Action<string> OnKeyEventOccurred;
    public event Action<int> OnStoryProgressUpdated;

    // Reset for new game
    [ContextMenu("Reset Story Context")]
    public void ResetStoryContext()
    {
        currentLocation = "";
        currentTime = "";
        currentObjective = "";
        storyProgressPercentage = 0;
        currentChapter = "Chapter 1";
        
        discoveredClues.Clear();
        knownSuspects.Clear();
        keyEvents.Clear();
        playerChoices.Clear();
        
        npcNames.Clear();
        npcSecrets.Clear();
        _npcRevealedInfo?.Clear();
    }
}
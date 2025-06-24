using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Linq;

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
    public List<ClueData> discoveredClues = new List<ClueData>();
    
    [Header("Known Suspects")]
    public List<SuspectData> knownSuspects = new List<SuspectData>();
    
    [Header("Evidence Cross-References")]
    public List<EvidenceConnection> evidenceConnections = new List<EvidenceConnection>();
    
    [Header("Contradictions Tracking")]
    public List<ContradictionData> trackedContradictions = new List<ContradictionData>();
    
    [Header("NPC Secrets Unlocked")]
    [SerializeField] private List<string> npcNames = new List<string>();
    [SerializeField] private List<string> npcSecrets = new List<string>();
    [SerializeField] private List<float> informationReliability = new List<float>(); // 0-1 reliability score
    
    [Header("Important Events")]
    public List<string> keyEvents = new List<string>();
    
    [Header("Player Choices History")]
    public List<string> playerChoices = new List<string>();

    // Dictionary for runtime use (since Unity can't serialize Dictionary directly)
    private Dictionary<string, string> _npcRevealedInfo;
    private Dictionary<string, float> _npcReliabilityScores;
    
    public Dictionary<string, string> npcRevealedInfo
    {
        get
        {
            if (_npcRevealedInfo == null)
            {
                _npcRevealedInfo = new Dictionary<string, string>();
                _npcReliabilityScores = new Dictionary<string, float>();
                
                // Populate from serialized lists
                for (int i = 0; i < Mathf.Min(npcNames.Count, npcSecrets.Count); i++)
                {
                    _npcRevealedInfo[npcNames[i]] = npcSecrets[i];
                    if (i < informationReliability.Count)
                        _npcReliabilityScores[npcNames[i]] = informationReliability[i];
                    else
                        _npcReliabilityScores[npcNames[i]] = 0.5f; // Default reliability
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
            _npcReliabilityScores = new Dictionary<string, float>();
            
            for (int i = 0; i < Mathf.Min(npcNames.Count, npcSecrets.Count); i++)
            {
                _npcRevealedInfo[npcNames[i]] = npcSecrets[i];
                if (i < informationReliability.Count)
                    _npcReliabilityScores[npcNames[i]] = informationReliability[i];
                else
                    _npcReliabilityScores[npcNames[i]] = 0.5f;
            }
        }
    }

    // Enhanced clue addition with source tracking and contradiction detection
    public void AddClue(string clue, string source = "Unknown", float reliability = 0.5f)
    {
        var existingClue = discoveredClues.FirstOrDefault(c => c.clueText.Equals(clue, StringComparison.OrdinalIgnoreCase));
        
        if (existingClue == null)
        {
            var newClue = new ClueData
            {
                clueText = clue,
                source = source,
                discoveryTime = DateTime.Now.ToString("HH:mm"),
                reliability = reliability,
                relatedSuspects = new List<string>(),
                tags = ExtractClueTags(clue)
            };
            
            discoveredClues.Add(newClue);
            UpdateStoryProgress();
            CheckForContradictions();
            UpdateEvidenceConnections();
            
            // Trigger event for other systems
            OnClueDiscovered?.Invoke(clue);
        }
        else
        {
            // Clue already exists, but maybe from different source - check for contradictions
            if (existingClue.source != source && existingClue.reliability != reliability)
            {
                CheckForSourceContradiction(existingClue, source, reliability);
            }
        }
    }

    public void AddSuspect(string suspect, string description = "", List<string> associatedClues = null)
    {
        var existingSuspect = knownSuspects.FirstOrDefault(s => s.name.Equals(suspect, StringComparison.OrdinalIgnoreCase));
        
        if (existingSuspect == null)
        {
            var newSuspect = new SuspectData
            {
                name = suspect,
                description = description,
                suspicionLevel = 1,
                associatedClues = associatedClues ?? new List<string>(),
                alibis = new List<string>(),
                contradictoryStatements = new List<string>()
            };
            
            knownSuspects.Add(newSuspect);
            OnSuspectIdentified?.Invoke(suspect);
        }
        else
        {
            // Update existing suspect with new information
            if (!string.IsNullOrEmpty(description) && existingSuspect.description != description)
            {
                // Check for contradictory descriptions
                CheckForSuspectDescriptionContradiction(existingSuspect, description);
            }
            
            if (associatedClues != null)
            {
                foreach (var clue in associatedClues)
                {
                    if (!existingSuspect.associatedClues.Contains(clue))
                        existingSuspect.associatedClues.Add(clue);
                }
            }
        }
    }

    public void RevealNPCInfo(string npcName, string info, float reliability = 0.5f)
    {
        if (!npcRevealedInfo.ContainsKey(npcName))
        {
            npcRevealedInfo.Add(npcName, info);
            _npcReliabilityScores[npcName] = reliability;
            
            // Update serialized lists
            npcNames.Add(npcName);
            npcSecrets.Add(info);
            informationReliability.Add(reliability);
            
            // Check if this new information contradicts existing knowledge
            CheckForInformationContradictions(npcName, info, reliability);
            
            OnNPCInfoRevealed?.Invoke(npcName, info);
        }
        else
        {
            // Information already exists, check for contradictions
            string existingInfo = npcRevealedInfo[npcName];
            if (existingInfo != info)
            {
                RecordContradiction(npcName, existingInfo, info, "NPC provided different information");
            }
        }
    }

    // New method to record alibis and check for contradictions
    public void RecordAlibi(string suspectName, string alibi, string source)
    {
        var suspect = knownSuspects.FirstOrDefault(s => s.name.Equals(suspectName, StringComparison.OrdinalIgnoreCase));
        if (suspect != null)
        {
            // Check if this alibi contradicts existing ones
            foreach (var existingAlibi in suspect.alibis)
            {
                if (DoAlibisContradict(existingAlibi, alibi))
                {
                    RecordContradiction(suspectName, existingAlibi, alibi, $"Contradictory alibis from {source}");
                }
            }
            
            suspect.alibis.Add($"[{source}] {alibi}");
        }
    }

    private void CheckForContradictions()
    {
        // Check for contradictions between clues
        for (int i = 0; i < discoveredClues.Count; i++)
        {
            for (int j = i + 1; j < discoveredClues.Count; j++)
            {
                if (DoCluesContradict(discoveredClues[i], discoveredClues[j]))
                {
                    RecordContradiction(
                        $"Clue contradiction",
                        discoveredClues[i].clueText,
                        discoveredClues[j].clueText,
                        $"Contradictory evidence from {discoveredClues[i].source} and {discoveredClues[j].source}"
                    );
                }
            }
        }
    }

    private void CheckForSourceContradiction(ClueData existingClue, string newSource, float newReliability)
    {
        if (Mathf.Abs(existingClue.reliability - newReliability) > 0.3f)
        {
            RecordContradiction(
                existingClue.source,
                $"Reliability: {existingClue.reliability:F1}",
                $"Reliability: {newReliability:F1}",
                $"Different sources provide conflicting reliability for: {existingClue.clueText}"
            );
        }
    }

    private void CheckForSuspectDescriptionContradiction(SuspectData suspect, string newDescription)
    {
        if (!string.IsNullOrEmpty(suspect.description) && suspect.description != newDescription)
        {
            RecordContradiction(
                suspect.name,
                suspect.description,
                newDescription,
                "Conflicting descriptions of suspect"
            );
            
            suspect.contradictoryStatements.Add($"Description conflict: '{suspect.description}' vs '{newDescription}'");
        }
    }

    private void CheckForInformationContradictions(string npcName, string info, float reliability)
    {
        // Check if NPC's information contradicts existing clues or other NPC statements
        foreach (var clue in discoveredClues)
        {
            if (DoesInformationContradictClue(info, clue.clueText))
            {
                RecordContradiction(
                    npcName,
                    info,
                    clue.clueText,
                    $"NPC statement contradicts discovered evidence"
                );
            }
        }
    }

    private void RecordContradiction(string subject, string statement1, string statement2, string description)
    {
        var contradiction = new ContradictionData
        {
            subject = subject,
            statement1 = statement1,
            statement2 = statement2,
            description = description,
            discoveryTime = DateTime.Now.ToString("HH:mm"),
            isResolved = false
        };
        
        trackedContradictions.Add(contradiction);
        OnContradictionDiscovered?.Invoke(contradiction);
        
        Debug.Log($"[CONTRADICTION DETECTED] {description}: '{statement1}' vs '{statement2}'");
    }

    private void UpdateEvidenceConnections()
    {
        // Create connections between related pieces of evidence
        foreach (var clue in discoveredClues)
        {
            foreach (var suspect in knownSuspects)
            {
                if (IsEvidenceRelatedToSuspect(clue.clueText, suspect.name))
                {
                    var connection = new EvidenceConnection
                    {
                        evidenceType1 = "Clue",
                        evidence1 = clue.clueText,
                        evidenceType2 = "Suspect",
                        evidence2 = suspect.name,
                        connectionStrength = CalculateConnectionStrength(clue.clueText, suspect.name),
                        discoveryTime = DateTime.Now.ToString("HH:mm")
                    };
                    
                    if (!evidenceConnections.Any(ec => ec.IsSameConnection(connection)))
                    {
                        evidenceConnections.Add(connection);
                        if (!suspect.associatedClues.Contains(clue.clueText))
                            suspect.associatedClues.Add(clue.clueText);
                    }
                }
            }
        }
    }

    // Helper methods for contradiction detection
    private bool DoCluesContradict(ClueData clue1, ClueData clue2)
    {
        // Simple contradiction detection - you can make this more sophisticated
        string[] contradictoryPairs = {
            "alive|dead", "present|absent", "guilty|innocent", "true|false",
            "inside|outside", "before|after", "yes|no"
        };
        
        string text1 = clue1.clueText.ToLower();
        string text2 = clue2.clueText.ToLower();
        
        foreach (string pair in contradictoryPairs)
        {
            string[] words = pair.Split('|');
            if ((text1.Contains(words[0]) && text2.Contains(words[1])) ||
                (text1.Contains(words[1]) && text2.Contains(words[0])))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool DoAlibisContradict(string alibi1, string alibi2)
    {
        // Check for time contradictions, location contradictions, etc.
        // This is a simplified version
        return alibi1.ToLower().Contains("was not") && alibi2.ToLower().Contains("was at") ||
               alibi1.ToLower().Contains("at home") && alibi2.ToLower().Contains("at work");
    }

    private bool DoesInformationContradictClue(string info, string clue)
    {
        // Check if NPC information contradicts physical evidence
        string lowerInfo = info.ToLower();
        string lowerClue = clue.ToLower();
        
        // Add more sophisticated logic here
        return (lowerInfo.Contains("impossible") && lowerClue.Contains("evidence")) ||
               (lowerInfo.Contains("never") && lowerClue.Contains("always"));
    }

    private bool IsEvidenceRelatedToSuspect(string evidence, string suspect)
    {
        string lowerEvidence = evidence.ToLower();
        string lowerSuspect = suspect.ToLower();
        
        return lowerEvidence.Contains(lowerSuspect) || 
               evidence.Split(' ').Any(word => suspect.Split(' ').Contains(word, StringComparer.OrdinalIgnoreCase));
    }

    private float CalculateConnectionStrength(string evidence, string suspect)
    {
        // Simple connection strength calculation
        string lowerEvidence = evidence.ToLower();
        string lowerSuspect = suspect.ToLower();
        
        if (lowerEvidence.Contains(lowerSuspect)) return 0.8f;
        
        int commonWords = evidence.Split(' ').Intersect(suspect.Split(' '), StringComparer.OrdinalIgnoreCase).Count();
        return Mathf.Clamp01(commonWords * 0.2f);
    }

    private List<string> ExtractClueTags(string clue)
    {
        List<string> tags = new List<string>();
        string lowerClue = clue.ToLower();
        
        // Auto-tag based on content
        if (lowerClue.Contains("blood") || lowerClue.Contains("weapon")) tags.Add("Physical Evidence");
        if (lowerClue.Contains("time") || lowerClue.Contains("when")) tags.Add("Timeline");
        if (lowerClue.Contains("where") || lowerClue.Contains("location")) tags.Add("Location");
        if (lowerClue.Contains("said") || lowerClue.Contains("told")) tags.Add("Testimony");
        if (lowerClue.Contains("motive") || lowerClue.Contains("reason")) tags.Add("Motive");
        
        return tags;
    }

    // Get contradiction summary for AI context
    public string GetContradictionSummary()
    {
        if (trackedContradictions.Count == 0) return "";
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CONTRADICTIONS DETECTED:");
        
        foreach (var contradiction in trackedContradictions.Where(c => !c.isResolved))
        {
            sb.AppendLine($"- {contradiction.description}");
            sb.AppendLine($"  Statement 1: {contradiction.statement1}");
            sb.AppendLine($"  Statement 2: {contradiction.statement2}");
        }
        
        return sb.ToString();
    }

    // Enhanced context string with contradictions and evidence connections
    public string GetContextString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Current Story Progress: {storyProgressPercentage}% - {currentChapter}");
        sb.AppendLine($"Location: {currentLocation}");
        sb.AppendLine($"Time: {currentTime}");
        sb.AppendLine($"Current Objective: {currentObjective}");
        
        if (discoveredClues.Count > 0)
        {
            sb.AppendLine("Clues discovered:");
            foreach (var clue in discoveredClues)
            {
                sb.AppendLine($" - {clue.clueText} (Source: {clue.source}, Reliability: {clue.reliability:F1})");
            }
        }
        
        if (knownSuspects.Count > 0)
        {
            sb.AppendLine("Known suspects:");
            foreach (var suspect in knownSuspects)
            {
                sb.AppendLine($" - {suspect.name}: {suspect.description} (Suspicion: {suspect.suspicionLevel}/5)");
                if (suspect.alibis.Count > 0)
                    sb.AppendLine($"   Alibis: {string.Join("; ", suspect.alibis)}");
            }
        }
        
        if (trackedContradictions.Count > 0)
        {
            sb.AppendLine(GetContradictionSummary());
        }
        
        if (evidenceConnections.Count > 0)
        {
            sb.AppendLine("Evidence connections:");
            foreach (var connection in evidenceConnections.OrderByDescending(c => c.connectionStrength))
            {
                sb.AppendLine($" - {connection.evidence1} ↔ {connection.evidence2} (Strength: {connection.connectionStrength:F1})");
            }
        }
        
        if (npcRevealedInfo.Count > 0)
        {
            sb.AppendLine("Information revealed by NPCs:");
            foreach (var pair in npcRevealedInfo)
            {
                float reliability = _npcReliabilityScores.ContainsKey(pair.Key) ? _npcReliabilityScores[pair.Key] : 0.5f;
                sb.AppendLine($" - {pair.Key}: {pair.Value} (Reliability: {reliability:F1})");
            }
        }
        
        if (playerChoices.Count > 0)
        {
            sb.AppendLine("Recent player choices: " + string.Join(", ", playerChoices.GetRange(
                Mathf.Max(0, playerChoices.Count - 3), Mathf.Min(3, playerChoices.Count))));
        }
        
        return sb.ToString();
    }

    // Method to resolve contradictions
    public void ResolveContradiction(int contradictionIndex, string resolution)
    {
        if (contradictionIndex >= 0 && contradictionIndex < trackedContradictions.Count)
        {
            trackedContradictions[contradictionIndex].isResolved = true;
            trackedContradictions[contradictionIndex].resolution = resolution;
            OnContradictionResolved?.Invoke(trackedContradictions[contradictionIndex]);
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
        // Enhanced progress calculation
        int totalDiscovered = discoveredClues.Count + knownSuspects.Count + keyEvents.Count;
        int contradictionsFound = trackedContradictions.Count;
        int connectionsFound = evidenceConnections.Count;
        
        // Progress includes finding contradictions and making connections
        int newProgress = Mathf.Min(100, (totalDiscovered * 8) + (contradictionsFound * 5) + (connectionsFound * 3));
        
        if (newProgress > storyProgressPercentage)
        {
            storyProgressPercentage = newProgress;
            OnStoryProgressUpdated?.Invoke(storyProgressPercentage);
        }
    }

    // Enhanced checking methods
    public bool HasDiscoveredClue(string clue)
    {
        return discoveredClues.Any(c => c.clueText.Equals(clue, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasIdentifiedSuspect(string suspect)
    {
        return knownSuspects.Any(s => s.name.Equals(suspect, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasNPCRevealedInfo(string npcName)
    {
        return npcRevealedInfo.ContainsKey(npcName);
    }

    public List<ContradictionData> GetUnresolvedContradictions()
    {
        return trackedContradictions.Where(c => !c.isResolved).ToList();
    }

    public float GetNPCReliability(string npcName)
    {
        return _npcReliabilityScores.ContainsKey(npcName) ? _npcReliabilityScores[npcName] : 0.5f;
    }

    // Get information that should be available to NPCs based on story progress
    public string GetNPCAvailableInfo(NPCPersonality npcPersonality)
    {
        if (npcPersonality == null) return "";
        
        StringBuilder availableInfo = new StringBuilder();
        
        // Based on NPC's story knowledge level
        if (npcPersonality.storyKnowledge > 0.7f)
        {
            // High knowledge NPCs know about most discoveries and contradictions
            if (discoveredClues.Count > 0)
                availableInfo.AppendLine("Recent discoveries: " + string.Join(", ", discoveredClues.Select(c => c.clueText)));
            if (knownSuspects.Count > 0)
                availableInfo.AppendLine("Known suspects: " + string.Join(", ", knownSuspects.Select(s => s.name)));
            if (trackedContradictions.Count > 0)
                availableInfo.AppendLine("Known contradictions: " + trackedContradictions.Count);
        }
        else if (npcPersonality.storyKnowledge > 0.3f)
        {
            // Medium knowledge NPCs know about major events and some contradictions
            if (keyEvents.Count > 0)
                availableInfo.AppendLine("Recent events: " + string.Join(", ", keyEvents.GetRange(
                    Mathf.Max(0, keyEvents.Count - 2), Mathf.Min(2, keyEvents.Count))));
        }
        
        return availableInfo.ToString();
    }

    // Enhanced events for other systems to subscribe to
    public event Action<string> OnClueDiscovered;
    public event Action<string> OnSuspectIdentified;
    public event Action<string, string> OnNPCInfoRevealed;
    public event Action<string> OnKeyEventOccurred;
    public event Action<int> OnStoryProgressUpdated;
    public event Action<ContradictionData> OnContradictionDiscovered;
    public event Action<ContradictionData> OnContradictionResolved;

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
        trackedContradictions.Clear();
        evidenceConnections.Clear();
        
        npcNames.Clear();
        npcSecrets.Clear();
        informationReliability.Clear();
        _npcRevealedInfo?.Clear();
        _npcReliabilityScores?.Clear();
    }
}

// Data structures for enhanced story tracking
[System.Serializable]
public class ClueData
{
    public string clueText;
    public string source;
    public string discoveryTime;
    public float reliability; // 0-1 scale
    public List<string> relatedSuspects;
    public List<string> tags;
}

[System.Serializable]
public class SuspectData
{
    public string name;
    public string description;
    public int suspicionLevel; // 1-5 scale
    public List<string> associatedClues;
    public List<string> alibis;
    public List<string> contradictoryStatements;
}

[System.Serializable]
public class ContradictionData
{
    public string subject;
    public string statement1;
    public string statement2;
    public string description;
    public string discoveryTime;
    public bool isResolved;
    public string resolution;
}

[System.Serializable]
public class EvidenceConnection
{
    public string evidenceType1; // "Clue", "Suspect", "NPC Statement", etc.
    public string evidence1;
    public string evidenceType2;
    public string evidence2;
    public float connectionStrength; // 0-1 scale
    public string discoveryTime;
    
    public bool IsSameConnection(EvidenceConnection other)
    {
        return (evidence1 == other.evidence1 && evidence2 == other.evidence2) ||
               (evidence1 == other.evidence2 && evidence2 == other.evidence1);
    }
}
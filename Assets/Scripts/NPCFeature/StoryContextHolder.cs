using UnityEngine;

/// <summary>
/// MonoBehaviour wrapper for StoryContextManager ScriptableObject
/// This allows the story context to be easily accessed throughout the scene
/// </summary>
public class StoryContextHolder : MonoBehaviour
{
    [Header("Story Context")]
    [SerializeField] private StoryContextManager storyContext;
    
    public static StoryContextHolder Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Create a new story context if none is assigned
        if (storyContext == null)
        {
            storyContext = ScriptableObject.CreateInstance<StoryContextManager>();
            Debug.LogWarning("No StoryContextManager assigned, created a new one at runtime.");
        }
    }

    public StoryContextManager GetStoryContext()
    {
        return storyContext;
    }

    public void SetStoryContext(StoryContextManager newContext)
    {
        storyContext = newContext;
        
        // Notify all NPCs to update their context
        NPCController[] npcs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
        foreach (var npc in npcs)
        {
            var geminiAccessor = npc.GetComponent<GeminiAccessor>();
            if (geminiAccessor != null)
            {
                geminiAccessor.SetStoryContext(storyContext);
            }
        }
    }

    // Convenience methods for updating story context
    public void AddClue(string clue)
    {
        storyContext?.AddClue(clue);
        Debug.Log($"Clue added to story: {clue}");
    }

    public void AddSuspect(string suspect)
    {
        storyContext?.AddSuspect(suspect);
        Debug.Log($"Suspect added to story: {suspect}");
    }

    public void UpdateLocation(string location)
    {
        if (storyContext != null)
        {
            storyContext.currentLocation = location;
            Debug.Log($"Location updated to: {location}");
        }
    }

    public void UpdateObjective(string objective)
    {
        if (storyContext != null)
        {
            storyContext.currentObjective = objective;
            Debug.Log($"Objective updated to: {objective}");
        }
    }

    public void RevealNPCInfo(string npcName, string info)
    {
        storyContext?.RevealNPCInfo(npcName, info);
        Debug.Log($"NPC {npcName} revealed: {info}");
    }

    // Method to manually refresh all NPCs with current context
    [ContextMenu("Refresh All NPCs")]
    public void RefreshAllNPCs()
    {
        NPCController[] npcs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
        foreach (var npc in npcs)
        {
            var geminiAccessor = npc.GetComponent<GeminiAccessor>();
            if (geminiAccessor != null)
            {
                geminiAccessor.SetStoryContext(storyContext);
            }
        }
        Debug.Log($"Refreshed {npcs.Length} NPCs with current story context");
    }
}
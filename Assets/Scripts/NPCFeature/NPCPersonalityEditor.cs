#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCPersonality))]
public class NPCPersonalityEditor : Editor
{
    private bool showBasicInfo = true;
    private bool showPersonalityTraits = true;
    private bool showKnowledge = true;
    private bool showStoryContext = true;
    private bool showBehavior = true;
    private bool showConversationStyle = true;
    private bool showEmotions = true;
    private bool showPreview = true;

    public override void OnInspectorGUI()
    {
        NPCPersonality personality = (NPCPersonality)target;

        // Foldout sections for better organization
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "Basic Information", true);
        if (showBasicInfo)
        {
            EditorGUI.indentLevel++;
            personality.npcName = EditorGUILayout.TextField("NPC Name", personality.npcName);
            EditorGUILayout.LabelField("Character Description");
            personality.characterDescription = EditorGUILayout.TextArea(personality.characterDescription, GUILayout.Height(60));
            EditorGUILayout.LabelField("Initial Greeting");
            personality.initialGreeting = EditorGUILayout.TextArea(personality.initialGreeting, GUILayout.Height(60));
            EditorGUI.indentLevel--;
        }

        showPersonalityTraits = EditorGUILayout.Foldout(showPersonalityTraits, "Personality Traits", true);
        if (showPersonalityTraits)
        {
            EditorGUI.indentLevel++;
            personality.friendliness = EditorGUILayout.Slider("Friendliness", personality.friendliness, 0, 1);
            personality.intelligence = EditorGUILayout.Slider("Intelligence", personality.intelligence, 0, 1);
            personality.patience = EditorGUILayout.Slider("Patience", personality.patience, 0, 1);
            personality.openness = EditorGUILayout.Slider("Openness", personality.openness, 0, 1);
            EditorGUI.indentLevel--;
        }

        showKnowledge = EditorGUILayout.Foldout(showKnowledge, "Knowledge & Background", true);
        if (showKnowledge)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Background Story");
            personality.backgroundStory = EditorGUILayout.TextArea(personality.backgroundStory, GUILayout.Height(60));

            // Knowledge topics
            EditorGUILayout.LabelField("Knowledge Topics");

            // Add knowledge topics
            if (personality.knowledgeTopics == null)
                personality.knowledgeTopics = new System.Collections.Generic.List<string>();

            for (int i = 0; i < personality.knowledgeTopics.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                personality.knowledgeTopics[i] = EditorGUILayout.TextField(personality.knowledgeTopics[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    personality.knowledgeTopics.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Knowledge Topic"))
            {
                personality.knowledgeTopics.Add("");
            }

            EditorGUI.indentLevel--;
        }

        // New Story Context Awareness section
        showStoryContext = EditorGUILayout.Foldout(showStoryContext, "Story Context Awareness", true);
        if (showStoryContext)
        {
            EditorGUI.indentLevel++;
            
            personality.storyKnowledge = EditorGUILayout.Slider("Story Knowledge Level", personality.storyKnowledge, 0, 1);
            EditorGUILayout.HelpBox("0 = Limited knowledge, 0.5 = Some knowledge, 1 = Well-informed", MessageType.Info);

            // Story trigger words
            EditorGUILayout.LabelField("Story Trigger Words");
            EditorGUILayout.HelpBox("Keywords that prompt this NPC to reveal story information", MessageType.Info);

            if (personality.storyTriggerWords == null)
                personality.storyTriggerWords = new System.Collections.Generic.List<string>();

            for (int i = 0; i < personality.storyTriggerWords.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                personality.storyTriggerWords[i] = EditorGUILayout.TextField(personality.storyTriggerWords[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    personality.storyTriggerWords.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Trigger Word"))
            {
                personality.storyTriggerWords.Add("");
            }

            // Secret information
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Secret Information");
            EditorGUILayout.HelpBox("Information this NPC knows but won't reveal immediately", MessageType.Info);
            personality.secretInformation = EditorGUILayout.TextArea(personality.secretInformation, GUILayout.Height(40));

            // Known evidence
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Known Evidence");
            EditorGUILayout.HelpBox("Evidence or clues this NPC is aware of", MessageType.Info);

            if (personality.knownEvidence == null)
                personality.knownEvidence = new System.Collections.Generic.List<string>();

            for (int i = 0; i < personality.knownEvidence.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                personality.knownEvidence[i] = EditorGUILayout.TextField(personality.knownEvidence[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    personality.knownEvidence.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Evidence"))
            {
                personality.knownEvidence.Add("");
            }

            // Suspect information
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Suspect Information");
            EditorGUILayout.HelpBox("What this NPC knows about potential suspects", MessageType.Info);

            if (personality.suspectInformation == null)
                personality.suspectInformation = new System.Collections.Generic.List<string>();

            for (int i = 0; i < personality.suspectInformation.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                personality.suspectInformation[i] = EditorGUILayout.TextField(personality.suspectInformation[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    personality.suspectInformation.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Suspect Info"))
            {
                personality.suspectInformation.Add("");
            }

            EditorGUI.indentLevel--;
        }

        showBehavior = EditorGUILayout.Foldout(showBehavior, "Behavioral Settings", true);
        if (showBehavior)
        {
            EditorGUI.indentLevel++;
            personality.usesEmotions = EditorGUILayout.Toggle("Uses Emotions", personality.usesEmotions);
            EditorGUILayout.LabelField("Unique Quirks");
            personality.uniqueQuirks = EditorGUILayout.TextArea(personality.uniqueQuirks, GUILayout.Height(60));
            EditorGUILayout.LabelField("Custom System Prompt (Optional)");
            EditorGUILayout.HelpBox("If filled, this will override all other settings. Use only if you need full control over the prompt.", MessageType.Info);
            personality.customSystemPrompt = EditorGUILayout.TextArea(personality.customSystemPrompt, GUILayout.Height(100));
            EditorGUI.indentLevel--;
        }

        showConversationStyle = EditorGUILayout.Foldout(showConversationStyle, "Conversation Style", true);
        if (showConversationStyle)
        {
            EditorGUI.indentLevel++;
            personality.maxResponseLength = EditorGUILayout.IntSlider("Max Response Length (Sentences)", personality.maxResponseLength, 1, 10);
            personality.usesSlang = EditorGUILayout.Toggle("Uses Slang", personality.usesSlang);
            personality.isFormal = EditorGUILayout.Toggle("Is Formal", personality.isFormal);
            EditorGUI.indentLevel--;
        }

        showEmotions = EditorGUILayout.Foldout(showEmotions, "Emotions & Expressions", true);
        if (showEmotions)
        {
            EditorGUI.indentLevel++;
            personality.defaultEmotion = EditorGUILayout.TextField("Default Emotion", personality.defaultEmotion);

            // Available emotions
            EditorGUILayout.LabelField("Available Emotions");

            if (personality.availableEmotions == null)
                personality.availableEmotions = new System.Collections.Generic.List<string> { "neutral", "happy", "sad", "angry" };

            for (int i = 0; i < personality.availableEmotions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                personality.availableEmotions[i] = EditorGUILayout.TextField(personality.availableEmotions[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    personality.availableEmotions.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Emotion"))
            {
                personality.availableEmotions.Add("");
            }

            EditorGUI.indentLevel--;
        }

        showPreview = EditorGUILayout.Foldout(showPreview, "System Prompt Preview", true);
        if (showPreview)
        {
            EditorGUILayout.HelpBox(personality.GenerateSystemPrompt(), MessageType.None);

            if (GUILayout.Button("Copy System Prompt"))
            {
                GUIUtility.systemCopyBuffer = personality.GenerateSystemPrompt();
                EditorUtility.DisplayDialog("System Prompt Copied", "The system prompt has been copied to your clipboard.", "OK");
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
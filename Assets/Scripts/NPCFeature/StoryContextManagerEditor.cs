#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(StoryContextManager))]
public class StoryContextManagerEditor : Editor
{
    private StoryContextManager storyContext;

    // Foldout states
    private bool showSceneInfo = true;
    private bool showStoryProgress = true;
    private bool showClues = true;
    private bool showSuspects = true;
    private bool showContradictions = true;
    private bool showEvidenceConnections = true;
    private bool showNPCSecrets = true;
    private bool showKeyEvents = true;
    private bool showPlayerChoices = true;
    private bool showDebugTools = false;

    // Add new item states
    private bool showAddClue = false;
    private bool showAddSuspect = false;
    private bool showAddNPCInfo = false;

    // New item input fields
    private string newClueText = "";
    private string newClueSource = "";
    private float newClueReliability = 0.5f;
    private string newSuspectName = "";
    private string newSuspectDescription = "";
    private string newNPCName = "";
    private string newNPCSecret = "";
    private float newNPCReliability = 0.5f;

    // Colors for different reliability levels
    private Color highReliabilityColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);
    private Color mediumReliabilityColor = new Color(0.8f, 0.8f, 0.2f, 0.3f);
    private Color lowReliabilityColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);

    private void OnEnable()
    {
        storyContext = (StoryContextManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Story Context Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Scene Information Section
        DrawSceneInformation();

        // Story Progress Section
        DrawStoryProgress();

        // Clues Section
        DrawCluesSection();

        // Suspects Section
        DrawSuspectsSection();

        // Contradictions Section
        DrawContradictionsSection();

        // Evidence Connections Section
        DrawEvidenceConnectionsSection();

        // NPC Secrets Section
        DrawNPCSecretsSection();

        // Key Events Section
        DrawKeyEventsSection();

        // Player Choices Section
        DrawPlayerChoicesSection();

        // Debug Tools Section
        DrawDebugToolsSection();

        //        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(storyContext);
        }
    }

    private void DrawSceneInformation()
    {
        showSceneInfo = EditorGUILayout.Foldout(showSceneInfo, "Current Scene Information", true);
        if (showSceneInfo)
        {
            EditorGUI.indentLevel++;

            storyContext.currentLocation = EditorGUILayout.TextField("Current Location", storyContext.currentLocation);
            storyContext.currentTime = EditorGUILayout.TextField("Current Time", storyContext.currentTime);
            storyContext.currentObjective = EditorGUILayout.TextField("Current Objective", storyContext.currentObjective);

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawStoryProgress()
    {
        showStoryProgress = EditorGUILayout.Foldout(showStoryProgress, "Story Progress", true);
        if (showStoryProgress)
        {
            EditorGUI.indentLevel++;

            // Progress bar
            Rect progressRect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(progressRect, storyContext.storyProgressPercentage / 100f,
                $"Story Progress: {storyContext.storyProgressPercentage}%");

            storyContext.storyProgressPercentage = EditorGUILayout.IntSlider("Progress Percentage",
                storyContext.storyProgressPercentage, 0, 100);
            storyContext.currentChapter = EditorGUILayout.TextField("Current Chapter", storyContext.currentChapter);

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawCluesSection()
    {
        showClues = EditorGUILayout.Foldout(showClues, $"Discovered Clues ({storyContext.discoveredClues.Count})", true);
        if (showClues)
        {
            EditorGUI.indentLevel++;

            // Add new clue section
            showAddClue = EditorGUILayout.Foldout(showAddClue, "Add New Clue", true);
            if (showAddClue)
            {
                EditorGUI.indentLevel++;
                newClueText = EditorGUILayout.TextField("Clue Text", newClueText);
                newClueSource = EditorGUILayout.TextField("Source", newClueSource);
                newClueReliability = EditorGUILayout.Slider("Reliability", newClueReliability, 0f, 1f);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Clue") && !string.IsNullOrEmpty(newClueText))
                {
                    storyContext.AddClue(newClueText, newClueSource, newClueReliability);
                    newClueText = "";
                    newClueSource = "";
                    newClueReliability = 0.5f;
                }
                if (GUILayout.Button("Clear"))
                {
                    newClueText = "";
                    newClueSource = "";
                    newClueReliability = 0.5f;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Display existing clues
            for (int i = 0; i < storyContext.discoveredClues.Count; i++)
            {
                var clue = storyContext.discoveredClues[i];
                Color bgColor = GetReliabilityColor(clue.reliability);

                EditorGUILayout.BeginVertical(GetReliabilityStyle(clue.reliability));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Clue {i + 1}:", EditorStyles.boldLabel, GUILayout.Width(60));
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    storyContext.discoveredClues.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                clue.clueText = EditorGUILayout.TextField("Text", clue.clueText);
                clue.source = EditorGUILayout.TextField("Source", clue.source);
                clue.reliability = EditorGUILayout.Slider("Reliability", clue.reliability, 0f, 1f);

                if (clue.tags != null && clue.tags.Count > 0)
                {
                    EditorGUILayout.LabelField("Tags: " + string.Join(", ", clue.tags), EditorStyles.miniLabel);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawSuspectsSection()
    {
        showSuspects = EditorGUILayout.Foldout(showSuspects, $"Known Suspects ({storyContext.knownSuspects.Count})", true);
        if (showSuspects)
        {
            EditorGUI.indentLevel++;

            // Add new suspect section
            showAddSuspect = EditorGUILayout.Foldout(showAddSuspect, "Add New Suspect", true);
            if (showAddSuspect)
            {
                EditorGUI.indentLevel++;
                newSuspectName = EditorGUILayout.TextField("Suspect Name", newSuspectName);
                newSuspectDescription = EditorGUILayout.TextField("Description", newSuspectDescription);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Suspect") && !string.IsNullOrEmpty(newSuspectName))
                {
                    storyContext.AddSuspect(newSuspectName, newSuspectDescription);
                    newSuspectName = "";
                    newSuspectDescription = "";
                }
                if (GUILayout.Button("Clear"))
                {
                    newSuspectName = "";
                    newSuspectDescription = "";
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Display existing suspects
            for (int i = 0; i < storyContext.knownSuspects.Count; i++)
            {
                var suspect = storyContext.knownSuspects[i];

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Suspect {i + 1}:", EditorStyles.boldLabel, GUILayout.Width(80));
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    storyContext.knownSuspects.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                suspect.name = EditorGUILayout.TextField("Name", suspect.name);
                suspect.description = EditorGUILayout.TextField("Description", suspect.description);
                suspect.suspicionLevel = EditorGUILayout.IntSlider("Suspicion Level", suspect.suspicionLevel, 1, 5);

                if (suspect.associatedClues != null && suspect.associatedClues.Count > 0)
                {
                    EditorGUILayout.LabelField("Associated Clues:", EditorStyles.boldLabel);
                    foreach (var clue in suspect.associatedClues)
                    {
                        EditorGUILayout.LabelField("• " + clue, EditorStyles.miniLabel);
                    }
                }

                if (suspect.alibis != null && suspect.alibis.Count > 0)
                {
                    EditorGUILayout.LabelField("Alibis:", EditorStyles.boldLabel);
                    foreach (var alibi in suspect.alibis)
                    {
                        EditorGUILayout.LabelField("• " + alibi, EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawContradictionsSection()
    {
        int unresolvedCount = storyContext.trackedContradictions.Count(c => !c.isResolved);
        showContradictions = EditorGUILayout.Foldout(showContradictions,
            $"Contradictions ({unresolvedCount} unresolved / {storyContext.trackedContradictions.Count} total)", true);

        if (showContradictions)
        {
            EditorGUI.indentLevel++;

            for (int i = 0; i < storyContext.trackedContradictions.Count; i++)
            {
                var contradiction = storyContext.trackedContradictions[i];
                Color bgColor = contradiction.isResolved ? Color.green : Color.red;
                bgColor.a = 0.2f;

                var style = new GUIStyle("box");
                style.normal.background = MakeTex(1, 1, bgColor);

                EditorGUILayout.BeginVertical(style);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Contradiction {i + 1} - {contradiction.subject}", EditorStyles.boldLabel);
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    storyContext.trackedContradictions.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(contradiction.description, EditorStyles.wordWrappedLabel);

                EditorGUILayout.LabelField("Statement 1:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(contradiction.statement1, EditorStyles.wordWrappedLabel);

                EditorGUILayout.LabelField("Statement 2:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(contradiction.statement2, EditorStyles.wordWrappedLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Discovered: {contradiction.discoveryTime}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Resolved: {(contradiction.isResolved ? "Yes" : "No")}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                if (!contradiction.isResolved && GUILayout.Button("Mark as Resolved"))
                {
                    storyContext.ResolveContradiction(i, "Manually resolved via editor");
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawEvidenceConnectionsSection()
    {
        showEvidenceConnections = EditorGUILayout.Foldout(showEvidenceConnections,
            $"Evidence Connections ({storyContext.evidenceConnections.Count})", true);

        if (showEvidenceConnections)
        {
            EditorGUI.indentLevel++;

            foreach (var connection in storyContext.evidenceConnections.OrderByDescending(c => c.connectionStrength))
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(connection.evidence1, GUILayout.Width(150));
                EditorGUILayout.LabelField("↔", GUILayout.Width(20));
                EditorGUILayout.LabelField(connection.evidence2, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Strength: {connection.connectionStrength:F2}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Discovered: {connection.discoveryTime}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawNPCSecretsSection()
    {
        showNPCSecrets = EditorGUILayout.Foldout(showNPCSecrets,
            $"NPC Secrets ({storyContext.npcRevealedInfo.Count})", true);

        if (showNPCSecrets)
        {
            EditorGUI.indentLevel++;

            // Add new NPC info section
            showAddNPCInfo = EditorGUILayout.Foldout(showAddNPCInfo, "Add New NPC Information", true);
            if (showAddNPCInfo)
            {
                EditorGUI.indentLevel++;
                newNPCName = EditorGUILayout.TextField("NPC Name", newNPCName);
                newNPCSecret = EditorGUILayout.TextField("Secret/Info", newNPCSecret);
                newNPCReliability = EditorGUILayout.Slider("Reliability", newNPCReliability, 0f, 1f);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Info") && !string.IsNullOrEmpty(newNPCName) && !string.IsNullOrEmpty(newNPCSecret))
                {
                    storyContext.RevealNPCInfo(newNPCName, newNPCSecret, newNPCReliability);
                    newNPCName = "";
                    newNPCSecret = "";
                    newNPCReliability = 0.5f;
                }
                if (GUILayout.Button("Clear"))
                {
                    newNPCName = "";
                    newNPCSecret = "";
                    newNPCReliability = 0.5f;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Display existing NPC info
            foreach (var npcInfo in storyContext.npcRevealedInfo)
            {
                float reliability = storyContext.GetNPCReliability(npcInfo.Key);

                EditorGUILayout.BeginVertical(GetReliabilityStyle(reliability));

                EditorGUILayout.LabelField(npcInfo.Key, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(npcInfo.Value, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField($"Reliability: {reliability:F2}", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawKeyEventsSection()
    {
        showKeyEvents = EditorGUILayout.Foldout(showKeyEvents, $"Key Events ({storyContext.keyEvents.Count})", true);
        if (showKeyEvents)
        {
            EditorGUI.indentLevel++;

            foreach (var keyEvent in storyContext.keyEvents)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(keyEvent, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawPlayerChoicesSection()
    {
        showPlayerChoices = EditorGUILayout.Foldout(showPlayerChoices, $"Player Choices ({storyContext.playerChoices.Count})", true);
        if (showPlayerChoices)
        {
            EditorGUI.indentLevel++;

            foreach (var choice in storyContext.playerChoices)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(choice, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private void DrawDebugToolsSection()
    {
        showDebugTools = EditorGUILayout.Foldout(showDebugTools, "Debug Tools", true);
        if (showDebugTools)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Test Clue"))
            {
                storyContext.AddClue("Test clue discovered at " + System.DateTime.Now.ToString("HH:mm"),
                    "Debug", UnityEngine.Random.Range(0.1f, 1.0f));
            }
            if (GUILayout.Button("Add Test Suspect"))
            {
                storyContext.AddSuspect("Test Suspect " + UnityEngine.Random.Range(1, 100),
                    "A suspicious individual spotted near the scene");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Test Event"))
            {
                storyContext.AddKeyEvent("Test event occurred at " + System.DateTime.Now.ToString("HH:mm"));
            }
            if (GUILayout.Button("Force Contradiction Check"))
            {
                // This would trigger the private method, but we can't access it from here
                EditorGUILayout.HelpBox("Contradiction checking happens automatically when adding clues", MessageType.Info);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Context String"))
            {
                string context = storyContext.GetContextString();
                Debug.Log("Story Context:\n" + context);
                EditorGUILayout.HelpBox("Context string printed to console", MessageType.Info);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Reset All Data", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Story Context",
                    "Are you sure you want to reset all story data? This cannot be undone.",
                    "Reset", "Cancel"))
                {
                    storyContext.ResetStoryContext();
                }
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    private Color GetReliabilityColor(float reliability)
    {
        if (reliability >= 0.7f) return highReliabilityColor;
        if (reliability >= 0.4f) return mediumReliabilityColor;
        return lowReliabilityColor;
    }

    private GUIStyle GetReliabilityStyle(float reliability)
    {
        var style = new GUIStyle("box");
        style.normal.background = MakeTex(1, 1, GetReliabilityColor(reliability));
        return style;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
#endif
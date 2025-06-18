// HostilityTracker.cs
using System;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class HostilityTracker
{
    [Header("Enemy Conversion Settings")]
    public bool canBecomeEnemy = true;
    public int hostilityThreshold = 3;

    
    [Header("Hostility Detection")]
    public float aggressiveResponseMultiplier = 1.5f;
    public float threateningResponseMultiplier = 2.0f;
    public float playerAggressionMultiplier = 1.0f;
    
    // Patterns for different types of aggressive responses
   private readonly string[] threateningPatterns = {
    // Direct physical threats (refined and expanded)
    @"\b(kill|murder|destroy|annihilate|eliminate|assassinate|slay|execute|hurt|harm|attack|assault|punch|stab|shoot|beat|hit|wound|strangle|choke|smash|crush|maim|torture|break|ruin)\s+(you|u|me|us|them|him|her|player|character|detective|officer|investigator)\b",
    @"\b(come\s+at\s+you|going\s+to\s+end\s+you|put\s+you\s+down|make\s+you\s+bleed|break\s+your\s+neck|slice\s+you|rip\s+you\s+apart|tear\s+you\s+limb\s+from\s+limb)\b",
    
    // Consequences and warnings (expanded)
    @"\b(die|suffer|regret|pay|burn|perish|rot|destroyed|dead|eliminated|wiped\s+out|erased|gone|finish)\b",
    @"\b(you('ll| will)?\s+(regret|pay|suffer|die|be\s+sorry)|watch\s+(out|your\s+back)|be\s+careful|or\s+else|i'm\s+warning\s+you|this\s+isn't\s+over|you've\s+made\s+a\s+mistake)\b",
    
    // Power-imbalanced threats (expanded)
    @"\b(teach\s+you\s+a\s+lesson|show\s+you\s+your\s+place|put\s+you\s+in\s+your\s+place|make\s+you\s+cry|wipe\s+that\s+smile\s+off|i'm\s+in\s+charge|you're\s+nothing|don't\s+cross\s+me)\b",
    
    // Weapon references (more generic and implied)
    @"\b(gun|knife|blade|sword|weapon|bullet|bomb|explosive|poison|piece|tool)\s+(to|for|on|with)\s+(you|your|detective)\b",
    @"\b(i\s+have\s+a\s+surprise|i've\s+got\s+something\s+for\s+you|this\s+will\s+hurt|you\s+won't\s+like\s+this)\b", // Implied threats
    
    // Targeted destruction (broader scope)
    @"\b(destroy|ruin|wreck|break|smash|end|take\s+away)\s+(your|everything|life|world|existence|family|friends|career|reputation|future|chance)\b",
    
    // Commands with implied threat
    @"\b(don't\s+push\s+me|don't\s+test\s+me|get\s+out\s+of\s+my\s+sight|stay\s+away|back\s+off)\b"
};

private readonly string[] aggressivePatterns = {
    // Personal attacks (expanded and more derogatory terms)
    @"\b(you('re| are)?\s+(stupid|idiot|fool|moron|imbecile|retard|dumb|dummy|fucking|shit|worthless|useless|pathetic|disgusting|horrible|awful|terrible|scum|trash|garbage|filth|bastard|bitch|asshole|dick|prick|cunt|clown|wuss|loser|freak|psycho|creep|degenerate))\b",
    @"\b(i\s+(hate|despise|loathe|detest|abhor|can't\s+stand|am\s+sick\s+of|am\s+tired\s+of|am\s+done\s+with))\s+(you|u|your\s+guts|everything\s+about\s+you|your\s+existence|your\s+face|your\s+voice)\b",
    
    // Dismissive commands (expanded and more forceful)
    @"\b(shut\s+up|shut\s+it|fuck\s+off|fuck\s+you|fuck\s+u|piss\s+off|go\s+away|get\s+lost|get\s+out|leave\s+me\s+alone|buzz\s+off|sod\s+off|drop\s+dead|scram|beat\s+it|get\s+out\s+of\s+here)\b",
    @"\b(i\s+don't\s+care|none\s+of\s+your\s+business|mind\s+your\s+own\s+business|it's\s+not\s+your\s+concern|stay\s+out\s+of\s+it)\b", // Dismissive phrases
    
    // Emotional intensity (expanded)
    @"\b(angry|furious|livid|enraged|infuriated|incensed|irate|seething|fuming|pissed|outraged|maddened|raging|frustrated|annoyed|disgusted|displeased)\b",
    @"\b(you\s+(make\s+me|are\s+making\s+me)\s+(angry|mad|sick|furious|annoyed|frustrated)|i'm\s+about\s+to\s+lose\s+it|you're\s+testing\s+my\s+patience)\b",
    
    // Behavioral accusations (more direct and accusatory)
    @"\b(always|constantly|continually|repeatedly)\s+(wrong|annoying|irritating|bothering|fucking\s+up|ruining|screwing\s+up|mistaking|ignoring|lying|deceiving|manipulating)\b",
    @"\b(why\s+(do|must)\s+you|stop\s+(being|acting)|quit\s+it|enough\s+already|can('t|not)\s+stand\s+you|sick\s+of\s+you|fed\s+up\s+with|you\s+never|you\s+always)\b",
    @"\b(liar|cheat|fraud|scammer|deceiver|manipulator|hypocrite|coward|traitor)\b" // Direct insults
};

private readonly string[] hostileEmotionalCues = {
    // Extreme punctuation (already good, but ensure regex is efficient)
    @"!{2,}",   // 2+ exclamation marks (reduced from 3 to catch more)
    @"\?{2,}",   // 2+ question marks (reduced from 3)
    @"\.{3,}",   // 3+ periods (for tension, ellipses)
    
    // Capitalization patterns (slightly adjusted)
    @"\b[A-Z]{2,}\b",   // All-caps words (2+ letters)
    @"(?<![a-z])[A-Z\s\d!?-]+(?![a-z])", // Entire message in caps, more robust
    
    // Aggressive formatting (broadened)
    @"\*[^*]+\*",     // Text wrapped in single asterisks (bolding/emphasis)
    @"\*\*+[^*]+\*\*+", // Text wrapped in double asterisks
    @"_+[^_]+_+",    // Text wrapped in underscores
    
    // Mocking repetition (expanded)
    @"\b(no+|stop+|go+|ugh+|ha+)\b",   // Elongated words/sounds
    @"(\w)\1{2,}",      // Repeated characters (heyyy, nooo, cooool)
    
    // Sarcasm indicators (expanded)
    @"\b(sure|great|wonderful|lovely|fine|of\s+course)\b\s*[!?.]{2,}",   // Positive words with aggressive punctuation
    @"\b(oh)\s+please\b|\b(yeah)\s+right\b|\b(as\s+if)\b|\b(i'm\s+sure)\b", // Dismissive/sarcastic phrases
    
    // Sudden shifts
    @"\b(listen|look)\s+here" // Attempts to assert dominance or change tone abruptly
};
    
    [Header("Current State")]
    [SerializeField] private float currentHostility;
    [SerializeField] private int hostileInteractionCount;
    [SerializeField] private bool hasBeenMarkedAsEnemy;
    
    private float lastDecayCheckTime;
    private float lastInteractionTime;

    public float CurrentHostility => currentHostility;
    public int HostileInteractionCount => hostileInteractionCount;
    public bool IsEnemy => hasBeenMarkedAsEnemy;

    public void Initialize()
    {
        currentHostility = 0f;
        hostileInteractionCount = 0;
        hasBeenMarkedAsEnemy = false;
        lastDecayCheckTime = Time.time;
        lastInteractionTime = Time.time;
    }

    public void AnalyzePlayerText(string text)
    {
        if (!canBecomeEnemy || hasBeenMarkedAsEnemy) return;
        
        float hostilityIncrease = AnalyzeTextForHostility(text, playerAggressionMultiplier);
        
        if (hostilityIncrease > 0)
        {
            currentHostility += hostilityIncrease;
            hostileInteractionCount++;
            Debug.Log($"[HOSTILITY] Player aggression detected: +{hostilityIncrease:F2} hostility");
        }
        else
        {
            Debug.Log("[HOSTILITY] Player text did not trigger hostility increase.");
        }
        
        lastInteractionTime = Time.time;
    }

    public void AnalyzeAIResponse(string response)
    {
        if (!canBecomeEnemy || hasBeenMarkedAsEnemy) return;
        
        float hostilityIncrease = DetectAIAggression(response);
        
        if (hostilityIncrease > 0)
        {
            currentHostility += hostilityIncrease;
            hostileInteractionCount++;
            Debug.Log($"[HOSTILITY] AI responded aggressively: '{response.Substring(0, Math.Min(50, response.Length))}...'");
            Debug.Log($"[HOSTILITY] Hostility increased by: +{hostilityIncrease:F2}");
            Debug.Log($"[HOSTILITY] Total hostility now: {currentHostility:F2}/{hostilityThreshold}");
        }
        
        lastInteractionTime = Time.time;
    }
    
    private float DetectAIAggression(string response)
    {
        if (string.IsNullOrEmpty(response)) return 0f;
        
        string lowerResponse = response.ToLower();
        float aggressionScore = 0f;
        
        // Check for AI expressing anger or hostility toward the player
        string[] aiHostilityIndicators = {
            // Direct expressions of dislike/hate (expanded targets)
    @"\bi\s+(hate|despise|can't stand|am sick of|am tired of|am done with|loathe|detest|abhor)\s+(you|this|talking|your\s+lies|your\s+face|your\s+questions)\b",
    
    // Direct insults/accusations (expanded terms)
    @"\byou\s+(are|'re)\s+(stupid|annoying|pathetic|worthless|useless|irritating|a\s+liar|a\s+fool|a\s+joke|a\s+disappointment|an\s+idiot|a\s+pest)\b",
    @"\byou('ve| have)?\s+(lied|deceived|manipulated|betrayed)\s+(me|us)\b", // Accusations of wrongdoing
    
    // Refusal to cooperate/dismissal (stronger language)
    @"\bi\s+(don't want|refuse|won't|will\s+not)\s+(to\s+)?(talk|speak|help|deal with you|answer\s+that|cooperate|say\s+another\s+word)\b",
    @"\bget\s+(away|out|lost)|leave\s+me\s+alone|get\s+out\s+of\s+my\s+sight|don't\s+touch\s+me\b",
    @"\bshut\s+up|be\s+quiet|stop\s+(talking|bothering|asking|interfering)\b",
    @"\b(i\s+don't\s+care|none\s+of\s+your\s+business|mind\s+your\s+own\s+business|it's\s+not\s+your\s+concern|stay\s+out\s+of\s+it)\b", // Dismissive and hostile
    
    // Explicit statements of anger/frustration (expanded)
    @"\bi'm\s+(angry|furious|mad|enraged|livid|fed\s+up|pissed\s+off|disgusted)\b",
    @"\byou\s+(make\s+me|are\s+making\s+me)\s+(angry|mad|sick|furious|frustrated|lose\s+my\s+temper)\b",
    @"\benough\s+of\s+(you|this)|i've\s+had\s+enough|this\s+is\s+over|i'm\s+done\s+with\s+this\b",
    @"\b(this\s+is\s+ridiculous|i\s+can't\s+believe\s+this|what\s+do\s+you\s+want\s+from\s+me)\b", // Exasperated/frustrated
    
    // Challenging statements
    @"\b(what\s+are\s+you\s+going\s+to\s+do\s+about\s+it|try\s+me|just\s+you\s+try)\b"
};
        
        // Check for AI making threats
        string[] aiThreatIndicators = {
            // Direct physical harm/violence (expanded)
    @"\bi'll\s+(hurt|harm|get|kill|murder|destroy|attack|beat|punch|stab|shoot|maim|torture|break|ruin)\s+(you|u|player|detective)\b",
    @"\b(going\s+to\s+end\s+you|put\s+you\s+down|make\s+you\s+bleed|break\s+your\s+neck|slice\s+you|rip\s+you\s+apart|tear\s+you\s+limb\s+from\s+limb)\b",
    @"\b(touch\s+you|lay\s+a\s+hand\s+on\s+you|teach\s+you\s+a\s+lesson)\b", // Implied physical threat

    // Warnings of severe consequences (expanded)
    @"\byou'll\s+(regret|pay|be\s+sorry|suffer|die|see)\s+(this|for\s+this|what\s+happens)\b",
    @"\bwatch\s+(out|yourself|your\s+back)|be\s+careful|i'm\s+warning\s+you|you've\s+been\s+warned\b",
    @"\bor\s+else|you\s+don't\s+want\s+to|this\s+isn't\s+a\s+game|you\s+have\s+no\s+idea\s+what\s+you're\s+doing\b",
    @"\b(there\s+will\s+be\s+consequences|you'll\s+face\s+the\s+repercussions|this\s+won't\s+end\s+well)\b",

    // Threats to reputation, freedom, or well-being
    @"\bi'll\s+(expose|ruin|destroy)\s+(your\s+reputation|your\s+career|everything\s+you\s+have)\b",
    @"\byou'll\s+(never\s+work\s+again|end\s+up\s+in\s+jail|be\s+locked\s+up|rot\s+in\s+a\s+cell)\b",
    @"\bi\s+(know\s+where\s+you\s+live|know\s+your\s+family|know\s+your\s+secrets)\b", // Personal intimidation
    @"\b(you're\s+making\s+an\s+enemy|you've\s+crossed\s+the\s+line|don't\s+push\s+me|don't\s+test\s+me)\b",

    // Commands with implied threat
    @"\b(get\s+out\s+of\s+my\s+sight|leave\s+me\s+alone|back\s+off|stay\s+away)\s+or\s+else\b",
    @"\b(do\s+as\s+i\s+say|obey\s+me|listen\s+to\s+me)\s+or\s+else\b", // Asserting dominance
    
    // Statements implying power imbalance or control
    @"\b(i\s+control\s+this|i'm\s+in\s+charge|you're\s+in\s+my\s+territory|you're\s+on\s+my\s+turf)\b",
    @"\b(you\s+won't\s+get\s+away\s+with\s+this|i\s+won't\s+forget\s+this)\b"
        };
        
        // Check for AI using hostile tone (already good, but ensure regex is efficient)
        string[] aiHostileToneIndicators = {
           // Extreme punctuation (more flexible and impactful)
    @"!{2,}",        // 2 or more exclamation marks (shouting, extreme anger)
    @"\?{2,}",       // 2 or more question marks (extreme frustration, disbelief, challenge)
    @"\.{3,}",       // 3 or more periods (e.g., "...", indicating tension, trailing off in anger, implied threat)
    @"\!\?",         // Combined exclamation and question mark (sarcastic disbelief, aggressive questioning)

    // Capitalization patterns (more comprehensive)
    @"\b[A-Z]{3,}\b", // All-caps words (3+ letters), e.g., "NEVER", "STOP"
    @"(?<![a-z])[A-Z\s\d!?-]{4,}(?![a-z])", // Entire phrases/sentences in ALL CAPS (more robust, min 4 chars to avoid single letters)
    
    // Aggressive formatting (broader detection)
    @"\*[^*]+\*",    // Text wrapped in single asterisks (e.g., *idiot*, for aggressive emphasis/mocking)
    @"\*\*+[^*]+\*\*+", // Text wrapped in double asterisks (stronger aggressive emphasis)
    @"_+[^_]+_+",   // Text wrapped in underscores (aggressive emphasis)
    
    // Mocking repetition and elongated words
    @"\b(no+|stop+|go+|ugh+|ha+)\b", // Elongated words/sounds (e.g., "Nooo!", "Ughhh!")
    @"(\w)\1{2,}",  // Repeated characters (e.g., "heyyy", "cooool", to catch sarcastic/mocking tones)
    
    // Sarcasm indicators (expanded)
    @"\b(sure|great|wonderful|lovely|fine)\b\s*[!?.]{2,}", // Positive words followed by aggressive punctuation
    @"\b(oh)\s+please\b", // Dismissive "oh please"
    @"\b(yeah)\s+right\b", // Sarcastic "yeah right"
    @"\b(as\s+if)\b",      // Sarcastic "as if"
    @"\b(i'm\s+sure)\b",   // Sarcastic "I'm sure"
    
    // Frustration and exasperation sounds/phrases
    @"\b(huff|sigh|grrr|tsk)\b", // Onomatopoeia for frustration/anger
    @"\b(honestly|seriously|really)\b[\.!?]*$", // Exasperated adverbs, often at end of sentence
    @"\b(what\s+do\s+you\s+want\s+now|are\s+you\s+serious|you've\s+got\s+to\s+be\s+kidding)\b", // Frustrated/exasperated questions
    
    // Attempts to assert dominance or cut off conversation
    @"\b(listen\s+here|look\s+here|that's\s+enough|i'm\s+done)\b"
        };
        
        // Score hostility indicators (medium severity)
        foreach (string pattern in aiHostilityIndicators)
        {
            if (Regex.IsMatch(lowerResponse, pattern, RegexOptions.IgnoreCase))
            {
                aggressionScore += 1.5f;
                Debug.Log($"[AI AGGRESSION] Hostility pattern detected: {pattern}");
                break; // Only count one major hostility indicator
            }
        }
        
        // Score threat indicators (high severity)
        foreach (string pattern in aiThreatIndicators)
        {
            if (Regex.IsMatch(lowerResponse, pattern, RegexOptions.IgnoreCase))
            {
                aggressionScore += 2.5f;
                Debug.Log($"[AI AGGRESSION] Threat pattern detected: {pattern}");
                break; // Only count one threat per response
            }
        }
        
        // Score tone indicators (can stack, but lower individual scores)
        int toneIndicatorCount = 0;
        foreach (string pattern in aiHostileToneIndicators)
        {
            if (Regex.IsMatch(response, pattern)) // Use original case for tone patterns
            {
                toneIndicatorCount++;
                Debug.Log($"[AI AGGRESSION] Hostile tone detected: {pattern}");
            }
        }
        
        if (toneIndicatorCount > 0)
        {
            aggressionScore += 0.5f * toneIndicatorCount; // 0.5 per tone indicator
        }
        
        // Apply multiplier
        return aggressionScore * aggressiveResponseMultiplier;
    }

    private float AnalyzeTextForHostility(string text, float multiplier)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        
        string lowerText = text.ToLower();
        float hostilityScore = 0f;
        
        // Check for threatening language (highest severity)
        foreach (string pattern in threateningPatterns)
        {
            if (Regex.IsMatch(lowerText, pattern, RegexOptions.IgnoreCase))
            {
                hostilityScore += 2.0f * threateningResponseMultiplier;
                Debug.Log($"[HOSTILITY] Threatening pattern detected: {pattern}");
                break; // Only count one threatening pattern per text
            }
        }
        
        // Check for aggressive language (medium severity)
        if (hostilityScore == 0f) // Only if no threatening language found
        {
            foreach (string pattern in aggressivePatterns)
            {
                if (Regex.IsMatch(lowerText, pattern, RegexOptions.IgnoreCase))
                {
                    hostilityScore += 1.5f;
                    Debug.Log($"[HOSTILITY] Aggressive pattern detected: {pattern}");
                    break;
                }
            }
        }
        
        // Check for hostile emotional cues (lower severity, but can stack)
        foreach (string pattern in hostileEmotionalCues)
        {
            if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
            {
                hostilityScore += 0.5f;
                Debug.Log($"[HOSTILITY] Hostile emotional cue detected: {pattern}");
            }
        }
        
        // Apply multiplier and context adjustments
        return hostilityScore * multiplier;
    }
    
    public void AddHostility(float amount, string reason = "")
    {
        currentHostility = Mathf.Clamp(currentHostility + amount, 0, hostilityThreshold);
        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log($"[HOSTILITY] Manual hostility increase: +{amount} ({reason})");
        }
    }
    
    public bool CheckEnemyConversion()
    {
        if (hasBeenMarkedAsEnemy) return false;
        
        if (currentHostility >= hostilityThreshold || hostileInteractionCount >= 3)
        {
            hasBeenMarkedAsEnemy = true;
            Debug.Log($"[HOSTILITY] ENEMY CONVERSION TRIGGERED! Hostility: {currentHostility}, Interactions: {hostileInteractionCount}");
            return true;
        }
        return false;
    }
    
    // Method to get current hostility status for debugging
    public string GetHostilityStatus()
    {
        return $"Hostility: {currentHostility:F2}/{hostilityThreshold}, Interactions: {hostileInteractionCount}, Enemy: {hasBeenMarkedAsEnemy}";
    }
}
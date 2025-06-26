using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class HostilityTracker
{
    [Header("Enemy Conversion Settings")]
    public bool canBecomeEnemy = true;
    public float hostilityThreshold = 10f;

    [Header("Hostility Detection Multipliers")]
    public float playerAggressionMultiplier = 1.0f;
    public float aiAggressionMultiplier = 1.2f;
    public float aiThreatMultiplier = 1.8f;
    public float contextualMultiplier = 1.5f;

    [Header("Personality Modifiers")]
    public float temperMultiplier = 1.0f;
    public float toleranceMultiplier = 1.0f;

    [Header("Current State")]
    [SerializeField] private float currentHostility;
    [SerializeField] private int consecutiveHostileInteractions;
    [SerializeField] private bool hasBeenMarkedAsEnemy;

    [Header("Investigation Pressure Settings")]
    public bool enableInvestigationPressure = true;
    public float pressureIncreaseRate = 0.2f;
    public float maxPressureHostility = 3.0f;
    private float accumulatedPressureHostility = 0f;

    private static Dictionary<string, Regex> cachedPatterns = new Dictionary<string, Regex>();

    private static readonly Dictionary<string, float> threatPatterns = new Dictionary<string, float>
    {
        {@"\b(kill|murder|destroy|eliminate|assassinate|slay|execute|annihilate|massacre|wipe\s+out|take\s+down|crush|obliterate|decimate|terminate|neutralize|eradicate|slaughter|butcher|dismember|maim|torture|enslave|vanquish)\s+(you|u|me|us|them|him|her|player|detective|officer)\b", 4.0f},
        {@"\b(i\s+will\s+make\s+you\s+pay|you\s+will\s+suffer|i'm\s+going\s+to\s+enjoy\s+this|your\s+end\s+is\s+near|i\s+will\s+make\s+you\s+regret)\b", 3.2f},
        {@"\b(this\s+is\s+a\s+threat|you've\s+been\s+warned|you\s+are\s+in\s+danger)\b", 2.8f},
        {@"\byou('ll| will)?\s+(regret|pay|suffer|die|be\s+sorry)\b", 2.5f},
        {@"\b(this\s+interrogation\s+is\s+a\s+mistake|you('re)?\s+making\s+a\s+big\s+mistake|you('ll)?\s+be\s+sorry\s+for\s+this)\b", 2.5f},
        {@"\b(keep\s+pushing\s+and\s+see\s+what\s+happens|last\s+warning|you're\s+not\s+safe|watch\s+your\s+step)\b", 2.8f},
        {@"\b(gun|knife|sword|weapon|bullet|bomb|poison)\s+(to|on|for)\s+(you|detective|me)\b", 3.0f}
    };

    private static readonly Dictionary<string, float> aggressionPatterns = new Dictionary<string, float>
    {
        {@"\b(you('re)?\s+(stupid|idiot|fool|pathetic|worthless|loser|freak|creep|trash|filth))\b", 2.0f},
        {@"\b(i\s+(hate|despise|can't\s+stand|am\s+tired\s+of)\s+(you|your\s+voice|your\s+presence))\b", 1.8f},
        {@"\b(you\s+don’t\s+know\s+who\s+you’re\s+dealing\s+with|you’re\s+insignificant)\b", 1.9f},
        {@"\b(this\s+is\s+harassment|i\s+don’t\s+have\s+to\s+talk\s+to\s+you|stop\s+accusing\s+me|you're\s+wasting\s+my\s+time)\b", 1.8f},
        {@"\b(who\s+do\s+you\s+think\s+you\s+are|you\s+can’t\s+prove\s+anything|you\s+cop|you\s+rat)\b", 2.0f},
        {@"\b(you’re\s+harassing\s+me|you\s+won’t\s+break\s+me|i\s+won’t\s+be\s+your\s+scapegoat)\b", 1.7f}
    };

    private static readonly Dictionary<string, float> hostileTonePatterns = new Dictionary<string, float>
    {
        {@"!{3,}", 0.5f}, {@"\?{3,}", 0.4f}, {@"\.{4,}", 0.3f},
        {@"(\!\?){2,}|\?\!\?|!\?\!", 0.5f},
        {@"\b[A-Z]{4,}\b", 0.8f},
        {@"(?<![a-z])[A-Z\s\d!?-]{8,}(?![a-z])", 1.0f},
        {@"\b(sure|great|fine)\b\s*[!?.]{2,}", 0.6f},
        {@"\b(oh\s+please|yeah\s+right|whatever|as\s+if)\b", 0.4f}
    };

    public float CurrentHostility => currentHostility;
    public int ConsecutiveHostileInteractions => consecutiveHostileInteractions;
    public bool IsEnemy => hasBeenMarkedAsEnemy;
    public float HostilityPercentage => Mathf.Clamp01(currentHostility / hostilityThreshold);

    public void Initialize()
    {
        currentHostility = 0f;
        consecutiveHostileInteractions = 0;
        hasBeenMarkedAsEnemy = false;
        accumulatedPressureHostility = 0f;
        PrecompilePatterns();
    }

    public void ApplyInvestigationPressure(float secondsElapsed)
    {
        if (!enableInvestigationPressure || hasBeenMarkedAsEnemy) return;

        float increase = secondsElapsed * pressureIncreaseRate;
        accumulatedPressureHostility = Mathf.Min(accumulatedPressureHostility + increase, maxPressureHostility);

        if (increase > 0f)
        {
            ApplyHostilityIncrease(increase, "Investigation pressure buildup");
        }
    }

    private static void PrecompilePatterns()
    {
        if (cachedPatterns.Count > 0) return;

        foreach (var kvp in threatPatterns) cachedPatterns[kvp.Key] = new Regex(kvp.Key, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        foreach (var kvp in aggressionPatterns) cachedPatterns[kvp.Key] = new Regex(kvp.Key, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        foreach (var kvp in hostileTonePatterns) cachedPatterns[kvp.Key] = new Regex(kvp.Key, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public void AnalyzePlayerText(string text)
    {
        if (!canBecomeEnemy || hasBeenMarkedAsEnemy || string.IsNullOrEmpty(text)) return;

        float hostilityIncrease = AnalyzeTextForHostility(text, playerAggressionMultiplier);

        if (hostilityIncrease > 0)
        {
            ApplyHostilityIncrease(hostilityIncrease, "Player aggression");
        }
        else
        {
            consecutiveHostileInteractions = Mathf.Max(0, consecutiveHostileInteractions - 1);
        }
    }

    public void AnalyzeAIResponse(string response)
    {
        if (!canBecomeEnemy || hasBeenMarkedAsEnemy || string.IsNullOrEmpty(response)) return;

        float hostilityIncrease = AnalyzeTextForHostility(response, aiAggressionMultiplier);
        if (hostilityIncrease > 0)
        {
            ApplyHostilityIncrease(hostilityIncrease, "AI aggression");
        }
    }

    private float AnalyzeTextForHostility(string text, float multiplier)
    {
        float total = 0f;
        bool majorThreat = false;

        foreach (var kvp in threatPatterns)
        {
            if (cachedPatterns[kvp.Key].IsMatch(text))
            {
                total += kvp.Value * multiplier;
                majorThreat = true;
                break;
            }
        }

        if (!majorThreat)
        {
            foreach (var kvp in aggressionPatterns)
            {
                if (cachedPatterns[kvp.Key].IsMatch(text))
                {
                    total += kvp.Value * multiplier;
                    break;
                }
            }
        }

        float toneHostility = 0f;
        int toneCount = 0;

        foreach (var kvp in hostileTonePatterns)
        {
            if (cachedPatterns[kvp.Key].IsMatch(text))
            {
                toneHostility += kvp.Value * Mathf.Pow(0.8f, toneCount);
                toneCount++;
            }
        }

        total += toneHostility * multiplier;
        total *= temperMultiplier;
        total /= toleranceMultiplier;

        return total;
    }

    private void ApplyHostilityIncrease(float amount, string reason)
    {
        currentHostility += amount;
        consecutiveHostileInteractions++;
        currentHostility = Mathf.Min(currentHostility, hostilityThreshold * 1.5f);
        Debug.Log($"[HOSTILITY] {reason}: +{amount:F2} (Total: {currentHostility:F2}/{hostilityThreshold})");
    }

    public void AddHostility(float amount, string reason = "") => ApplyHostilityIncrease(amount, reason);

    public void SetPersonalityModifiers(float temper, float tolerance)
    {
        temperMultiplier = Mathf.Clamp(temper, 0.5f, 3f);
        toleranceMultiplier = Mathf.Clamp(tolerance, 0.5f, 3f);
    }

    public bool CheckEnemyConversion()
    {
        if (hasBeenMarkedAsEnemy) return false;

        bool shouldConvert = currentHostility >= hostilityThreshold ||
                             consecutiveHostileInteractions >= 5 ||
                             (currentHostility >= hostilityThreshold * 0.8f && consecutiveHostileInteractions >= 3);

        if (shouldConvert)
        {
            hasBeenMarkedAsEnemy = true;
            Debug.Log($"[HOSTILITY] ENEMY CONVERSION TRIGGERED.");
            return true;
        }

        return false;
    }

    public HostilityLevel GetHostilityLevel()
    {
        if (hasBeenMarkedAsEnemy) return HostilityLevel.Enemy;
        float percent = HostilityPercentage;
        if (percent >= 0.8f) return HostilityLevel.Hostile;
        if (percent >= 0.6f) return HostilityLevel.Agitated;
        if (percent >= 0.4f) return HostilityLevel.Annoyed;
        if (percent >= 0.2f) return HostilityLevel.Irritated;
        return HostilityLevel.Neutral;
    }

    public string GetHostilityStatus() =>
        $"Hostility: {currentHostility:F1}/{hostilityThreshold} | Level: {GetHostilityLevel()} | " +
        $"Consecutive: {consecutiveHostileInteractions} | Enemy: {hasBeenMarkedAsEnemy}";

    public void ResetHostility()
    {
        currentHostility = 0f;
        consecutiveHostileInteractions = 0;
        hasBeenMarkedAsEnemy = false;
        accumulatedPressureHostility = 0f;
    }
}

public enum HostilityLevel
{
    Neutral,
    Irritated,
    Annoyed,
    Agitated,
    Hostile,
    Enemy
}

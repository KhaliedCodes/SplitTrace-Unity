// HostilityTracker.cs
using System;
using UnityEngine;

[Serializable]
public class HostilityTracker
{
    [Header("Enemy Conversion Settings")]
    public bool canBecomeEnemy = true;
    public int hostilityThreshold = 3;
    public float hostilityDecayRate = 0.1f;
    public float decayInterval = 5f; // Seconds between decay checks
    public string[] hostileTriggerWords = { "threaten", "attack", "kill", "hurt", "fight", "enemy", "hate" };
    public string[] peacefulTriggerWords = { "sorry", "peace", "friend", "help", "apologize", "calm" };

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

    public void Update()
    {
        if (Time.time - lastDecayCheckTime > decayInterval)
        {
            currentHostility = Mathf.Max(0, currentHostility - hostilityDecayRate);
            lastDecayCheckTime = Time.time;
        }
    }

    public void AnalyzeText(string text)
    {
        if (!canBecomeEnemy || hasBeenMarkedAsEnemy) return;

        string lowerText = text.ToLower();
        bool foundHostile = false;
        bool foundPeaceful = false;

        foreach (string word in hostileTriggerWords)
        {
            if (lowerText.Contains(word))
            {
                currentHostility += 1f;
                hostileInteractionCount++;
                foundHostile = true;
                break;
            }
        }

        if (!foundHostile)
        {
            foreach (string word in peacefulTriggerWords)
            {
                if (lowerText.Contains(word))
                {
                    currentHostility = Mathf.Max(0, currentHostility - 0.5f);
                    foundPeaceful = true;
                    break;
                }
            }
        }

        lastInteractionTime = Time.time;
    }
     public void AddHostility(float amount)
    {
        currentHostility = Mathf.Clamp(currentHostility + amount, 0, hostilityThreshold);
    }
    public bool CheckEnemyConversion()
    {
        if (hasBeenMarkedAsEnemy) return false;
        
        if (currentHostility >= hostilityThreshold || 
            hostileInteractionCount >= 2)
        {
            hasBeenMarkedAsEnemy = true;
            return true;
        }
        return false;
    }
}
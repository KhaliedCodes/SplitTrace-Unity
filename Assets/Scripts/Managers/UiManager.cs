using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    [Header("Tutorial Settings")]
    public GameObject tutorialPanel;

    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private GameObject TutCollider;

    private bool isTutorialActive = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            HideTutorial();
        } 
        
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            ResetTutorials();
        }
    }

    public void ShowTutorial(TutorialData data)
    {
        // Check if already shown
        if (HasShownTutorial(data.uniqueKey)) return;

        isTutorialActive = true;

        tutorialPanel?.SetActive(true);
        if (tutorialText != null)
            tutorialText.text = data.tutorialText;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Save shown state
        PlayerPrefs.SetInt(data.uniqueKey, 1);
        PlayerPrefs.Save();
    }

    public void HideTutorial()
    {
        if (!isTutorialActive) return;

        isTutorialActive = false;
        tutorialPanel?.SetActive(false);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

    }
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
    public bool HasShownTutorial(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    [ContextMenu("Reset Tutorial Flags")]
    public void ResetTutorials()
    {
        Debug.Log("Prefs Reset");
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();
    }

}

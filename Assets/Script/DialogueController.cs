using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    [Header("Choices")]
    public GameObject choicesPanel;
    public GameObject choiceButtonPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.text = npcName;
        portraitImage.sprite = portrait;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    // מוחק כפתורים
    public void ClearChoices()
    {
        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // יוצר כפתור
    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btn = Instantiate(choiceButtonPrefab, choicesPanel.transform);

        btn.GetComponentInChildren<TMP_Text>().text = choiceText;

        Button button = btn.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        return btn;
    }

    // מציג בחירות
    public void ShowChoices(string[] choices, System.Action<int> onChoiceSelected)
    {
        ClearChoices();
        choicesPanel.SetActive(true);

        for (int i = 0; i < choices.Length; i++)
        {
            int index = i;

            CreateChoiceButton(choices[i], () =>
            {
                choicesPanel.SetActive(false);
                onChoiceSelected(index);
            });
        }
    }

    public void HideChoices()
    {
        choicesPanel.SetActive(false);
    }
}
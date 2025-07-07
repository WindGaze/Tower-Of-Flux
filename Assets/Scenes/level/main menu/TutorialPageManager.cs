using UnityEngine;
using UnityEngine.UI;

public class TutorialPageManager : MonoBehaviour
{
    public GameObject[] tutorialPages; // Assign your tutorial UI pages here
    public Button nextButton;
    public Button backButton;

    private int currentPage = 0;

    void Start()
    {
        UpdatePage();
        nextButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(PreviousPage);
    }

    void UpdatePage()
    {
        // Disable all pages
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(i == currentPage);
        }

        // Enable/Disable navigation buttons
        backButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < tutorialPages.Length - 1;
    }

    public void NextPage()
    {
        if (currentPage < tutorialPages.Length - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }
}

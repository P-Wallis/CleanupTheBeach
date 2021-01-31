using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuID
{
    NONE,
    Title,
    Instructions,
    Credits
}

public class MenuManager : MonoBehaviour
{
    private bool isShowingMenu;
    public bool IsShowingMenu { get { return isShowingMenu; } }
    public GameObject title, instructions, credits;

    public void ShowMenu(MenuID menu)
    {
        title.SetActive(menu == MenuID.Title);
        instructions.SetActive(menu == MenuID.Instructions);
        credits.SetActive(menu == MenuID.Credits);

        isShowingMenu = menu == MenuID.Title || menu == MenuID.Credits;
    }

    public void ShowInstructions() { ShowMenu(MenuID.Instructions); }
    public void ReloadScene() { UnityEngine.SceneManagement.SceneManager.LoadScene("Main"); }
}

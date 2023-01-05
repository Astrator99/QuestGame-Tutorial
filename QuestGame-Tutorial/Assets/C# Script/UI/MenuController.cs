using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static event Action BaseMenuOpening;
    public static event Action BaseMenuClosed;
    
    #region Inspector

    [SerializeField] private string startScene = "Scenes/GameScene";
    [SerializeField] private string menuScene = "Scenes/MenuScene";

    [Tooltip("Base to be opened/closed with the ToggleMenu Action.\nThis is usually the main or pause menu.")]
    [SerializeField] private Menu baseMenu;

    [Tooltip("prevent the base menu from being closed. E.g. in the main menu.")]
    [SerializeField] private bool preventBaseClosing;
    
    #endregion

    private GameInput input;

    private Stack<Menu> openedMenus;

    #region Unity Event Functions

    private void Awake()
    {
        input = new GameInput();

        input.UI.ToggleMenu.performed += ToggleMenu;
        input.UI.GoBackMenu.performed += GoBackMenu;

        openedMenus = new Stack<Menu>();
        
        // Reset timescale on scene start.
        Time.timeScale = 1;
    }

    private void Start()
    {
        //Add base menu to stack in case it was open on start. E.g. the main menu.
        if (baseMenu.gameObject.activeSelf)
        {
            openedMenus.Push(baseMenu);
        }
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void OnDestroy()
    {
        input.UI.ToggleMenu.performed -= ToggleMenu;
        input.UI.GoBackMenu.performed -= GoBackMenu;
    }

    #endregion

    #region Menu Functions

    public void StartGame()
    {
        SceneManager.LoadScene(startScene);
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene(menuScene);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenMenu(Menu menu)
    {
        if (menu == baseMenu)
        {
            BaseMenuOpening?.Invoke();
        }
        
        menu.Open();
        // Add menu to the stack.
        openedMenus.Push(menu);
    }

    public void CloseMenu()
    {
        if (openedMenus.Count == 0) { return; }
        
        // Remove base menu from closing.
        if (preventBaseClosing &&
            openedMenus.Count == 1 &&
            openedMenus.Peek() == baseMenu) // Look at the top most menu on the stack without removing it.
        {
            return;
        }
        
        // Remove top most menu from the stack.
        Menu closingMenu = openedMenus.Pop();
        closingMenu.Close();

        if (closingMenu == baseMenu)
        {
            BaseMenuClosed?.Invoke();
        }
    }

    private void ToggleMenu(InputAction.CallbackContext _)
    {
        if (!baseMenu.gameObject.activeSelf)
        {
            OpenMenu(baseMenu);
        }
        else
        {
            GoBackMenu(_);
        }
    }

    private void GoBackMenu(InputAction.CallbackContext _)
    {
        CloseMenu();
    }

    #endregion
}

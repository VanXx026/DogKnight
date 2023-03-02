using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscUI : MonoBehaviour
{
    private bool isOpenEscPanel;
    public GameObject escUICanvas;
    private BladeModeController bladeModeController;
    private PlayerController playerController;

    private void Awake()
    {

        escUICanvas.SetActive(false);
        isOpenEscPanel = false;
    }

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        bladeModeController = FindObjectOfType<BladeModeController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isOpenEscPanel)
        {
            OpenEscPanel();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isOpenEscPanel)
        {
            CloseEscPanel();
        }
    }

    public void OpenEscPanel()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        playerController.enabled = false;
        bladeModeController.enabled = false;

        Debug.Log("open");
        escUICanvas.SetActive(true);
        isOpenEscPanel = true;
        Time.timeScale = 0f;
    }

    public void CloseEscPanel()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        playerController.enabled = true;
        bladeModeController.enabled = true;

        Debug.Log("close");
        escUICanvas.SetActive(false);
        isOpenEscPanel = false;
        Time.timeScale = 1.0f;
    }

    public void QuitGame()
    {
        Time.timeScale = 1.0f;
        SaveManager.Instance.SavePlayerData();
        SceneController.Instance.TransitionToMainMenu();
    }

    public void SaveGame()
    {
        Debug.Log("?");
        playerController.enabled = true;
        // playerController.GetPosAndRotate();
        SaveManager.Instance.SavePlayerData();
        playerController.enabled = false;
    }

}

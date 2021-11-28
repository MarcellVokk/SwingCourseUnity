using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject crosshair, menuCamera;
    public GameObject chatPanel, pausePanel, startMenu, settingsPanel, privateServersPanel;
    public InputField usernameField, privateServersMenuUsernameField;
    public InputField ipAddressField;
    public TMP_Dropdown windowModeDropdown, resolutionDropdown;
    public TMP_Text FPSLabel;
    public Slider maxFPSSlider;
    public Toggle vsyncToggle;

    public enum state
	{
        mainMenu,
        pause,
        game,
        chat,
        settings,
        animation
	}

    public static state controlState = state.mainMenu;
    public static bool blockUserInput;

	private void Update()
	{
        if(controlState == state.game && Cursor.visible == true)
		{
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (controlState == state.game)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                chatPanel.SetActive(true);
                GameManager.ChatFocus(true);
                controlState = state.chat;
                blockUserInput = true;
            }
        }
		else if (Input.GetKeyDown(KeyCode.Escape))
		{
            if(controlState == state.chat)
			{
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                GameManager.ChatFocus(false);
                chatPanel.SetActive(false);
                controlState = state.game;
                blockUserInput = false;
            }
            else if(controlState == state.game)
			{
                //open menu
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                crosshair.SetActive(false);
                pausePanel.SetActive(true);
                controlState = state.pause;
                blockUserInput = true;
            }
            else if (controlState == state.pause)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                crosshair.SetActive(true);
                pausePanel.SetActive(false);
                controlState = state.game;
                blockUserInput = false;
            }
			else if(controlState == state.settings)
			{
                CancelSettings();
            }
        }

        if(controlState == state.animation)
		{
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            instance.crosshair.SetActive(false);
            instance.pausePanel.SetActive(false);
            instance.settingsPanel.SetActive(false);
            instance.chatPanel.SetActive(false);
            instance.startMenu.SetActive(false);
            blockUserInput = true;
        }
    }

    public bool connecting = false;

    public void ConnectAuto()
    {
        if (!connecting)
        {
            connecting = true;
            FirebaseManagger.GetListData("servers", result =>
            {
                Client.instance.ip = IPAddress.Parse(result[0]);
                Client.instance.ConnectToServer(result[0], usernameField.text);

                startMenu.SetActive(false);
                crosshair.SetActive(true);
                menuCamera.SetActive(false);

                Debug.Log("Connected");
                controlState = state.game;
            });
        }
    }

    public void ShowPrivateServersMenu(bool visible)
    {
        startMenu.SetActive(!visible);
        privateServersPanel.SetActive(visible);
    }

    public static void BackToGame()
	{
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        instance.crosshair.SetActive(true);
        instance.pausePanel.SetActive(false);
        instance.settingsPanel.SetActive(false);
        instance.chatPanel.SetActive(false);
        instance.startMenu.SetActive(false);
        controlState = state.game;
        blockUserInput = false;
    }

	private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    
    public void ShowSettings()
	{
        settingsPanel.SetActive(true);
        pausePanel.SetActive(false);
        controlState = state.settings;
        blockUserInput = true;

        if(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
		{
            windowModeDropdown.value = 0;
        }
        else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            windowModeDropdown.value = 1;
        }
        else if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow)
        {
            windowModeDropdown.value = 2;
        }
        else if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            windowModeDropdown.value = 3;
        }

        resolutionDropdown.options = new List<TMP_Dropdown.OptionData>();

        foreach(Resolution s in Screen.resolutions)
		{
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(s.width + "x" + s.height + " @ " + s.refreshRate));
        }

        resolutionDropdown.value = Screen.resolutions.ToList().IndexOf(Screen.currentResolution);

        if(QualitySettings.vSyncCount == 1)
		{
            vsyncToggle.isOn = true;
		}
		else
		{
            vsyncToggle.isOn = false;
		}

		if (!vsyncToggle.isOn)
		{
            if (Application.targetFrameRate < 0)
            {
                maxFPSSlider.value = 241;
            }
            else if (Application.targetFrameRate < maxFPSSlider.minValue)
            {
                maxFPSSlider.value = maxFPSSlider.minValue;
            }
            else if (Application.targetFrameRate > maxFPSSlider.maxValue)
            {
                maxFPSSlider.value = maxFPSSlider.maxValue;
            }
            else
            {
                maxFPSSlider.value = Application.targetFrameRate;
            }

            UpdateFPSLabel();
        }
		else
		{
            maxFPSSlider.value = 241;
            FPSLabel.text = "VSync";
		}
    }

    public void ApplySettings()
	{
        if (windowModeDropdown.value == 0)
		{
            Screen.SetResolution(Screen.resolutions[resolutionDropdown.value].width, Screen.resolutions[resolutionDropdown.value].height, FullScreenMode.ExclusiveFullScreen, Screen.resolutions[resolutionDropdown.value].refreshRate);
		}
        else if (windowModeDropdown.value == 1)
        {
            Screen.SetResolution(Screen.resolutions[resolutionDropdown.value].width, Screen.resolutions[resolutionDropdown.value].height, FullScreenMode.FullScreenWindow, Screen.resolutions[resolutionDropdown.value].refreshRate);
        }
        else if (windowModeDropdown.value == 2)
        {
            Screen.SetResolution(Screen.resolutions[resolutionDropdown.value].width, Screen.resolutions[resolutionDropdown.value].height, FullScreenMode.MaximizedWindow, Screen.resolutions[resolutionDropdown.value].refreshRate);
        }
        else if (windowModeDropdown.value == 3)
        {
            Screen.SetResolution(Screen.resolutions[resolutionDropdown.value].width, Screen.resolutions[resolutionDropdown.value].height, FullScreenMode.Windowed, Screen.resolutions[resolutionDropdown.value].refreshRate);
        }

		if (!vsyncToggle.isOn)
		{
            if (maxFPSSlider.value == 241)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 0;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = (int)maxFPSSlider.value;
            }
        }
		else
		{
            QualitySettings.vSyncCount = 1;
        }

        QualitySettings.antiAliasing = 2;
    }

    public void CancelSettings()
	{
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshair.SetActive(true);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        controlState = state.game;
        blockUserInput = false;
    }

    public void ConnectToServer()
    {
        if (!connecting)
        {
            connecting = true;
            Client.instance.ip = IPAddress.Parse(ipAddressField.text);
            Client.instance.ConnectToServer(ipAddressField.text, privateServersMenuUsernameField.text);

            startMenu.SetActive(false);
            crosshair.SetActive(true);
            menuCamera.SetActive(false);

            Debug.Log("Connected");
            controlState = state.game;
        }
    }

    public void DisconnectFromServer()
	{
        Client.instance.Disconnect();
        SceneManager.LoadScene(SceneManager.GetSceneByName("MainScene").buildIndex);
    }

    public void UpdateFPSLabel()
	{
        if(maxFPSSlider.value == 241)
		{
            FPSLabel.text = "Unlimited";
        }
		else
		{
            FPSLabel.text = maxFPSSlider.value.ToString();
        }
	}

    public void VSyncToggle(Toggle toggle)
	{
		if (toggle.isOn)
		{
            maxFPSSlider.interactable = false;
            FPSLabel.text = "VSync";
		}
		else
		{
            maxFPSSlider.interactable = true;
            UpdateFPSLabel();
        }
	}
}

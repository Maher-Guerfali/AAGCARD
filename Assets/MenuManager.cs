using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject settingsPanel;

    [Header("Grid Settings UI")]
    public Slider rowsSlider;
    public Slider colsSlider;
    public TMP_Text rowsValueText;
    public TMP_Text colsValueText;
    public Button startGameButton;

    [Header("Menu Buttons")]
    public Button settingsButton;
    public Button backToMenuButton;
    public Button quitButton;

  

    [Header("Grid Constraints")]
    [SerializeField] private int minRows = 2;
    [SerializeField] private int maxRows = 8;
    [SerializeField] private int minCols = 2;
    [SerializeField] private int maxCols = 8;

    // Current settings
    private int currentRows = 4;
    private int currentCols = 4;

    private void Start()
    {
        InitializeMenu();
        SetupButtonListeners();
        LoadSettings();
        ShowMainMenu();
    }

    private void InitializeMenu()
    {
        // Setup sliders
        if (rowsSlider != null)
        {
            rowsSlider.minValue = minRows;
            rowsSlider.maxValue = maxRows;
            rowsSlider.wholeNumbers = true;
            rowsSlider.value = currentRows;
            rowsSlider.onValueChanged.AddListener(OnRowsChanged);
        }

        if (colsSlider != null)
        {
            colsSlider.minValue = minCols;
            colsSlider.maxValue = maxCols;
            colsSlider.wholeNumbers = true;
            colsSlider.value = currentCols;
            colsSlider.onValueChanged.AddListener(OnColsChanged);
        }

        // Setup sound sliders
        

        UpdateGridSizeDisplay();
       
    }

    private void SetupButtonListeners()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(ShowMainMenu);

       

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void LoadSettings()
    {
        currentRows = PlayerPrefs.GetInt("GridRows", 4);
        currentCols = PlayerPrefs.GetInt("GridCols", 4);

        // Clamp values to valid range
        currentRows = Mathf.Clamp(currentRows, minRows, maxRows);
        currentCols = Mathf.Clamp(currentCols, minCols, maxCols);

        if (rowsSlider != null)
            rowsSlider.value = currentRows;

        if (colsSlider != null)
            colsSlider.value = currentCols;

        UpdateGridSizeDisplay();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("GridRows", currentRows);
        PlayerPrefs.SetInt("GridCols", currentCols);
        PlayerPrefs.Save();
    }

    #region UI Event Handlers

    private void OnRowsChanged(float value)
    {
        currentRows = Mathf.RoundToInt(value);
        UpdateGridSizeDisplay();
        ValidateGridSize();
        PlayButtonSound();
    }

    private void OnColsChanged(float value)
    {
        currentCols = Mathf.RoundToInt(value);
        UpdateGridSizeDisplay();
        ValidateGridSize();
        PlayButtonSound();
    }

    private void ValidateGridSize()
    {
        // Ensure the total number of cards is even for pairing
        int totalCards = currentRows * currentCols;
        bool isValidSize = totalCards % 2 == 0 && totalCards >= 4;

        if (startGameButton != null)
        {
            startGameButton.interactable = isValidSize;
        }

        // Update display color based on validity
        if (rowsValueText != null)
        {
            rowsValueText.color = isValidSize ? Color.white : Color.red;
        }
        if (colsValueText != null)
        {
            colsValueText.color = isValidSize ? Color.white : Color.red;
        }
    }

    private void UpdateGridSizeDisplay()
    {
        if (rowsValueText != null)
            rowsValueText.text = currentRows.ToString();

        if (colsValueText != null)
            colsValueText.text = currentCols.ToString();

        ValidateGridSize();
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicVolume(value);
        }
    }

    #endregion

    #region Menu Navigation

    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
        PlayButtonSound();
    }

    public void ShowSettings()
    {
        SetActivePanel(settingsPanel);
        ;
        PlayButtonSound();
    }

    public void ShowGame()
    {
        SetActivePanel(gamePanel);
    }

    private void SetActivePanel(GameObject activePanel)
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(activePanel == mainMenuPanel);

        if (gamePanel != null)
            gamePanel.SetActive(activePanel == gamePanel);

        if (settingsPanel != null)
            settingsPanel.SetActive(activePanel == settingsPanel);
    }

    #endregion

    #region Game Control

    public void StartGame()
    {
        SaveSettings();

        // Update game settings
        if (GameManager.Instance != null && GameManager.Instance.gridBuilder != null)
        {
            GameManager.Instance.rows = currentRows;
            GameManager.Instance.cols = currentCols;

            // Update the settings in GridBuilder
            if (GameManager.Instance.gridBuilder.settings != null)
            {
                GameManager.Instance.gridBuilder.settings.rows = currentRows;
                GameManager.Instance.gridBuilder.settings.cols = currentCols;
            }
        }

        ShowGame();
        PlayButtonSound();

        // Start the game with preview
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGameWithPreview();
        }
    }

    public void ReturnToMenu()
    {
        ShowMainMenu();
        PlayButtonSound();
    }

    #endregion

    #region Sound Controls

    public void ToggleSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleSound();
            
            PlayButtonSound();
        }
    }

 

    private void PlayButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayFlip();
        }
    }

    #endregion

    #region Utility

    public void QuitGame()
    {
        PlayButtonSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Public method to get current grid settings
    public Vector2Int GetGridSize()
    {
        return new Vector2Int(currentCols, currentRows);
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    [ContextMenu("Test Grid Size 2x2")]
    private void TestGrid2x2()
    {
        currentRows = 2;
        currentCols = 2;
        UpdateGridSizeDisplay();
        if (rowsSlider != null) rowsSlider.value = currentRows;
        if (colsSlider != null) colsSlider.value = currentCols;
    }

    [ContextMenu("Test Grid Size 6x5")]
    private void TestGrid6x5()
    {
        currentRows = 6;
        currentCols = 5;
        UpdateGridSizeDisplay();
        if (rowsSlider != null) rowsSlider.value = currentRows;
        if (colsSlider != null) colsSlider.value = currentCols;
    }
#endif

    #endregion
}
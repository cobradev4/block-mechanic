using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

public class MenuController : MonoBehaviour {
    public float fadeDuration = 1.0f;
    public GameObject transitionHandler;
    public GameObject game;
    private GameObject settings;
    private GameObject mainMenu;
    private Button createWorldButton;
    private Button loadWorldButton;
    private Button saveWorldButton;
    private Button settingsButton;
    private Button quitButton;
    private GameObject world;
    private GameObject gameCanvas;
    private GameObject player;
    private GameObject sun;
    private GameObject loadingText;
    private bool worldCreated = false;
    private bool waitForWorldLoad = false;
    private bool generatingWorld;
    private Vector3 playerPosition;
    
    void Start() {
        Cursor.lockState = CursorLockMode.Confined; //keep cursor within window

        settings = transform.GetChild(0).GetChild(2).gameObject; //can't use tag since it is deactivated
        loadingText = transform.GetChild(0).GetChild(3).gameObject;
        mainMenu = GameObject.FindGameObjectWithTag("Main Menu");  

        world = game.transform.GetChild(0).gameObject;
        gameCanvas = game.transform.GetChild(1).gameObject;
        player = game.transform.GetChild(2).gameObject;
        sun = game.transform.GetChild(3).gameObject;

        createWorldButton = mainMenu.transform.GetChild(0).GetComponent<Button>();
        loadWorldButton = mainMenu.transform.GetChild(1).GetComponent<Button>();
        saveWorldButton = mainMenu.transform.GetChild(2).GetComponent<Button>();
        settingsButton = mainMenu.transform.GetChild(3).GetComponent<Button>();
        quitButton = mainMenu.transform.GetChild(4).GetComponent<Button>();
    }

    void Update() {
        if (waitForWorldLoad) {
            if (!world.GetComponent<World>().loading) {
                
                if (!generatingWorld) player.transform.GetChild(1).position = playerPosition;
                
                gameCanvas.SetActive(true);
                player.SetActive(true);
                sun.SetActive(true);
                
                gameObject.SetActive(false);
                worldCreated = true;

                AddSaveButton();

                waitForWorldLoad = false;
                loadingText.SetActive(false);
            }
        }
        if (world.GetComponent<World>().showLoadingIndicator) loadingText.SetActive(true);
    }

    IEnumerator fade(Button button, bool fadeIn) {
        float counter = 0;
        Color currentColor = button.colors.disabledColor;

        while (counter < fadeDuration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp((fadeIn ? 0 : 1), (fadeIn ? 1 : 0), counter / fadeDuration);
            
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            colors.normalColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            button.colors = colors;

            yield return null;
        }
    }
    IEnumerator fade(TMP_InputField input, bool fadeIn) {
        float counter = 0;
        Color currentColor = input.colors.disabledColor;

        while (counter < fadeDuration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp((fadeIn ? 0 : 1), (fadeIn ? 1 : 0), counter / fadeDuration);
            
            ColorBlock colors = input.colors;
            colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            colors.normalColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            input.colors = colors;

            yield return null;
        }
    }

    IEnumerator fade(Slider slider, bool fadeIn) {
        float counter = 0;
        Color currentColor = slider.colors.disabledColor;

        while (counter < fadeDuration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp((fadeIn ? 0 : 1), (fadeIn ? 1 : 0), counter / fadeDuration);
            
            ColorBlock colors = slider.colors;
            colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            colors.normalColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            slider.colors = colors;

            yield return null;
        }
    }

    IEnumerator fade(TMP_Dropdown dropdown, bool fadeIn) {
        float counter = 0;
        Color currentColor = dropdown.colors.disabledColor;

        while (counter < fadeDuration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp((fadeIn ? 0 : 1), (fadeIn ? 1 : 0), counter / fadeDuration);
            
            ColorBlock colors = dropdown.colors;
            colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            colors.normalColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
            dropdown.colors = colors;

            yield return null;
        }
    }

    IEnumerator fade(TextMeshProUGUI text, bool fadeIn) {
        float counter = 0;
        Color currentColor = text.color;

        while (counter < fadeDuration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp((fadeIn ? 0 : 1), (fadeIn ? 1 : 0), counter / fadeDuration);

            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            
            yield return null;
        }
    }

    IEnumerator fade(Image image, bool fadeIn) {
        float counter = 0;
        Color currentColor = image.color;

        while (counter < fadeDuration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp((fadeIn ? 0 : 1), (fadeIn ? 1 : 0), counter / fadeDuration);

            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            
            yield return null;
        }
    }

    IEnumerator deactivateMenu() {
        yield return new WaitForSeconds(fadeDuration);
        mainMenu.SetActive(false);
    }

    IEnumerator deactivateSettings() {
        yield return new WaitForSeconds(fadeDuration);
        settings.SetActive(false);
    }

    private void AddSaveButton() {
        //move settings button and quit button down 75 to make room for save button
        Vector3 settingsPos = settingsButton.GetComponent<RectTransform>().localPosition;
        Vector3 quitPos = quitButton.GetComponent<RectTransform>().localPosition;
        settingsPos.y -= 75.0f;
        quitPos.y -= 75.0f;

        settingsButton.GetComponent<RectTransform>().localPosition = settingsPos;
        quitButton.GetComponent<RectTransform>().localPosition = quitPos;

        saveWorldButton.gameObject.SetActive(true);
    }

    public void ShowMainMenu(bool fadeIn) {
        StartCoroutine(fade(createWorldButton, fadeIn));
        StartCoroutine(fade(createWorldButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), fadeIn));

        StartCoroutine(fade(loadWorldButton, fadeIn));
        StartCoroutine(fade(loadWorldButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), fadeIn));

        StartCoroutine(fade(saveWorldButton, fadeIn));
        StartCoroutine(fade(saveWorldButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), fadeIn));

        StartCoroutine(fade(settingsButton, fadeIn));
        StartCoroutine(fade(settingsButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), fadeIn));

        StartCoroutine(fade(quitButton, fadeIn));
        StartCoroutine(fade(quitButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), fadeIn));
    }
    public void OnQuitClick() {
        Application.Quit(); //Quit the game
    }

    public void OnCreateWorldClick() {
        if (!worldCreated) {
            Debug.Log("create world");
            generatingWorld = true;

            world.SetActive(true);
            waitForWorldLoad = true; //run actions after world as loaded via update
        } else OnResumeWorldClick();
    }

    public void OnResumeWorldClick() {
        Debug.Log("resume world");
        transitionHandler.GetComponent<TransitionHandler>().MenuToGame();
    }

    public void OnLoadWorldClick() {
        Debug.Log("load world");
        generatingWorld = false;

        /* using https://github.com/gkngkc/UnityStandaloneFileBrowser */
        string path = StandaloneFileBrowser.OpenFilePanel("Open File", "./", "world", false)[0];
        playerPosition = world.GetComponent<World>().FileToWorld(path); //player position in world being loaded, also loads world to world map 

        if (!worldCreated) { 
            world.GetComponent<World>().fromLoad = true; //indicate the the world is being created first time from world save   
            worldCreated = true;
        } else {
            world.GetComponent<World>().LoadPlayerChunks(player.transform.GetChild(1).GetComponent<PlayerController>().renderDistance, playerPosition);
        }
        OnCreateWorldClick();
        player.transform.GetChild(1).position = playerPosition;
    }

    public void OnSaveWorldClick() {
        Debug.Log("save world");
        /* using https://github.com/gkngkc/UnityStandaloneFileBrowser */
        string path = StandaloneFileBrowser.SaveFilePanel("Save File", "./", "MyWorld", "world");
        world.GetComponent<World>().WorldToFile(path, player.transform.GetChild(1).position);
    }

    public void OnBackClick() {
        Debug.Log("return to main menu");

        ShowSettings(false); //fade out setings
        StartCoroutine(deactivateSettings());
        mainMenu.SetActive(true);
        ShowMainMenu(true); //fade in main menu
        
        createWorldButton.interactable = true;
        loadWorldButton.interactable = true;
        saveWorldButton.interactable = true;
        settingsButton.interactable = true;
        quitButton.interactable = true;
    }

    public void OnSettingsClick() {
        Debug.Log("settings");

        settings.SetActive(true);
        StartCoroutine(deactivateMenu()); //need to wait until fading is done to remove menu

        createWorldButton.interactable = false;
        loadWorldButton.interactable = false;
        saveWorldButton.interactable = false;
        settingsButton.interactable = false;
        quitButton.interactable = false;

        ShowMainMenu(false); //fade out main menu
        ShowSettings(true); //fade in settings
    }

    /* A little messy, but automatically searches for certain types of user input and fades them in and out */
    private void ShowSettings(bool show) {
        int childCount = settings.transform.childCount;

        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
        List<Button> buttons = new List<Button>();
        List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();
        List<TMP_InputField> inputs = new List<TMP_InputField>();
        List<TMP_Dropdown> dropdowns = new List<TMP_Dropdown>();
        List<TextMeshProUGUI> dropdownLabels = new List<TextMeshProUGUI>();
        List<Image> dropdownArrows = new List<Image>();
        List<Slider> sliders = new List<Slider>();
        List<Image> sliderElements = new List<Image>();


        for (int i = 0; i < childCount; i++) {
            TextMeshProUGUI[] textChildren = settings.transform.GetChild(i).GetComponentsInChildren<TextMeshProUGUI>();
            TMP_InputField[] inputChildren = settings.transform.GetChild(i).GetComponentsInChildren<TMP_InputField>();
            TMP_Dropdown[] dropdownChildren = settings.transform.GetChild(i).GetComponentsInChildren<TMP_Dropdown>();
            Slider[] sliderChildren = settings.transform.GetChild(i).GetComponentsInChildren<Slider>();
            Button[] buttonChildren = settings.transform.GetChild(i).GetComponentsInChildren<Button>();

            for (int k = 0; k < dropdownChildren.Length; k++) {
                TextMeshProUGUI label = dropdownChildren[k].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                Image arrow = dropdownChildren[k].transform.GetChild(1).GetComponent<Image>();
                dropdownLabels.Add(label);
                dropdownArrows.Add(arrow);
            }
            for (int k = 0; k < sliderChildren.Length; k++) {
                Image background = sliderChildren[k].transform.GetChild(0).GetComponent<Image>();
                Image fill = sliderChildren[k].transform.GetChild(1).GetChild(0).GetComponent<Image>();
                Image handle = sliderChildren[k].transform.GetChild(2).GetChild(0).GetComponent<Image>();
                sliderElements.Add(background);
                sliderElements.Add(fill);
                sliderElements.Add(handle);
            }
            for (int k = 0; k < buttonChildren.Length; k++) {
                TextMeshProUGUI text = buttonChildren[k].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                buttonTexts.Add(text);
            }

            for (int j = 0; j < textChildren.Length; j++) {
                texts.Add(textChildren[j]);
            }
            for (int j = 0; j < inputChildren.Length; j++) {
                inputs.Add(inputChildren[j]);
            }
            for (int j = 0; j < dropdownChildren.Length; j++) {
                dropdowns.Add(dropdownChildren[j]);
            }
            for (int j = 0; j < sliderChildren.Length; j++) {
                sliders.Add(sliderChildren[j]);
            }
            for (int j = 0; j< buttonChildren.Length; j++) {
                buttons.Add(buttonChildren[j]);
            }
        }

        foreach (TextMeshProUGUI text in texts) {
            StartCoroutine(fade(text, show));
        }
        foreach (TMP_InputField input in inputs) {
            input.interactable = show;
            StartCoroutine(fade(input, show));
        }
        foreach (TMP_Dropdown dropdown in dropdowns) {
            dropdown.interactable = show;
            StartCoroutine(fade(dropdown, show));
        }
        foreach (Slider slider in sliders) {
            slider.interactable = show;
            StartCoroutine(fade(slider, show));
        }
        foreach (Button button in buttons) {
            button.interactable = show;
            StartCoroutine(fade(button, show));
        }

        foreach (TextMeshProUGUI label in dropdownLabels) {
            StartCoroutine(fade(label, show));
        }
        foreach (Image arrow in dropdownArrows) {
            StartCoroutine(fade(arrow, show));
        }
        foreach(Image element in sliderElements) {
            StartCoroutine(fade(element, show));
        }
        foreach(TextMeshProUGUI text in buttonTexts) {
            StartCoroutine(fade(text, show));
        }
    }
}
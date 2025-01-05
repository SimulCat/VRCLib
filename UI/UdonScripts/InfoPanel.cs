
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using TMPro;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InfoPanel : UdonSharpBehaviour
{
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private Button closeButton;
    [SerializeField] private UdonToggle openToggle;
    [SerializeField, FieldChangeCallback(nameof(LanguageIndex))] int languageIndex = 0;
    [SerializeField] private bool growShrink = true;
    [SerializeField] Vector2 panelSize = Vector2.one;
    [SerializeField] Vector2 shrinkSize = Vector2.one;
    [SerializeField] int alignHorizontal;
    [SerializeField] bool showHideContentPanel= true;
    //   [SerializeField] float textBorder = 20;
    [SerializeField] RectTransform contentPanelRect;
    [SerializeField] TextMeshProUGUI contentText;
    [SerializeField] Toggle[] toggles = null;
    [SerializeField] InfoPage[] pages = null;
    int toggleCount = 0;

    bool hasClose = false;
    private bool iamOwner;
    private VRCPlayerApi player;

    [SerializeField] 
    private int activeInfoPage = -1;

    [SerializeField] string[] defaultTexts;
    
    public int LanguageIndex
    {
        get => languageIndex;
        set
        {
            languageIndex = value;
            if (pages != null && pages.Length > 0)
            {
                foreach (var page in pages)
                {
                    if (page != null)
                        page.LanguageIndex = languageIndex;
                }
            }
            UpdatePage();
        }
    }

    private string defaultText
    {
        get
        {
            if (defaultTexts == null || defaultTexts.Length <= 0)
                return "";
            if (languageIndex >= defaultTexts.Length || defaultTexts[languageIndex] == null)
                return defaultTexts[0];
            return defaultTexts[languageIndex];
        }
    }


    private void UpdatePage()
    {
        activeInfoPage = mode;
        //Debug.Log("ActiveInfoPage=" + value);
        if (contentText != null)
        {
            if (show)
            {
                if (mode >= 0)
                {
                    string title = "";
                    if (pages[mode] != null)
                    {
                        title = pages[mode].PageTitle;
                        contentText.text = string.Format("<align=center><b>{0}</b></align>\n{1}", title, pages[mode].PageBody);
                    }
                    else
                        contentText.text = "";
                }
            }
            else
                contentText.text = defaultText;
        }
        if (showHideContentPanel && contentPanelRect != null)
        {
            if (hasClose)
                closeButton.gameObject.SetActive(show);
            if (growShrink)
            {
                Vector2 newSize = show ? panelSize : shrinkSize;
                Vector2 shrinkDelta = show ? Vector2.zero : (panelSize - shrinkSize) * 0.5f;
                Vector3 newPosition = new Vector3(alignHorizontal * shrinkDelta.x, -shrinkDelta.y,0);
                bool validSize = (newSize.x > 0) && (newSize.y > 0);
                if (validSize)
                {
                    contentPanelRect.sizeDelta = newSize;
                    contentPanelRect.localPosition = newPosition;
                }
                contentPanelRect.gameObject.SetActive(validSize);
            }
            else
            {
                contentPanelRect.gameObject.SetActive(show);
            }
        }
        updateToggles();
    }
    public void onBtnClose()
    {
        Show = false;
    }

    public void onToggle()
    {
        int toggleIdx = -1;
        if (!iamOwner)
            Networking.SetOwner(player,gameObject);
        for (int i = 0; toggleIdx < 0 && i < toggles.Length; i++)
        {
            if (toggles[i] != null)
            {
                if (toggles[i].isOn)
                    toggleIdx = i;
            }
        }
        if (toggleIdx >= 0)
            Mode = toggleIdx;
        Show = toggleIdx >= 0;
    }

    [SerializeField,FieldChangeCallback(nameof(Show))]
    private bool show = false;

    private bool showing = false;
    private bool Show
    {
        get => show;
        set
        {
            show = value;
            if (showing != value)
            {
                UpdatePage();
                showing = value;
                if (openToggle != null)
                    openToggle.setState(value);
            }
        }
    }

    private void updateToggles()
    {
        if (show)
        {
            if (mode >= 0 && mode < toggleCount)
            {
                if (toggles[mode] != null)
                    toggles[mode].SetIsOnWithoutNotify(true);
            }
        }
        else
        {
            if (toggleGroup != null)
                toggleGroup.SetAllTogglesOff(false);
        }
    }

    [SerializeField,FieldChangeCallback(nameof(Mode))]
    private int mode = 0;
    private int Mode
    {
        get => mode;
        set
        {
            mode = value;
            if (mode < 0)
                Show = false;
            UpdatePage();
        }
    }
    
  
    private void UpdateOwnerShip()
    {
        iamOwner = Networking.IsOwner(this.gameObject);
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        UpdateOwnerShip();
    }

    void Start()
    {
        toggleCount = 0;
        if (toggles != null)
            toggleCount = toggles.Length;
        player = Networking.LocalPlayer;
        UpdateOwnerShip();
        if (toggleGroup == null)
            toggleGroup = gameObject.GetComponent<ToggleGroup>();
        if (toggleGroup != null)
        {
            toggleGroup.allowSwitchOff = true;
            toggleGroup.SetAllTogglesOff(false);
        }
        hasClose = closeButton != null && closeButton.gameObject.activeSelf;
        if (contentPanelRect != null)
            panelSize = contentPanelRect.sizeDelta;
        if (growShrink)
            growShrink = (shrinkSize.x * shrinkSize.y) > 0;
        UpdatePage();
    }
}

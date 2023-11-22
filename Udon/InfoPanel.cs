
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InfoPanel : UdonSharpBehaviour
{
    public Toggle panelToggle;
    public ToggleGroup subjectGroup;
    [SerializeField] Transform panelXfrm;
    [SerializeField] TextMeshProUGUI infoText;

    bool hasToggle = false;
    bool hasGroup = false;
    bool hasTextField = false;
    bool hasTransform = false;
    [SerializeField] private bool showPanel = true;

    [SerializeField,TextArea] string defaultText = string.Empty;
    public void onToggleChanged()
    {
        bool newState = !showPanel;
        if (hasToggle)
            newState = panelToggle.isOn;
        ShowPanel = newState;
    }
    public bool ShowPanel
    {
        get => showPanel; 
        set
        {
            if (showPanel != value)
            {
                if (value && hasTextField)
                {
                    infoText.text = defaultText;
                }
                showPanel = value;
            }
            if (hasTextField)
                infoText.enabled = showPanel;
            if (hasTransform) 
            { 
                panelXfrm.gameObject.SetActive(showPanel);
            }
            if (hasGroup && !showPanel)
            {
                subjectGroup.SetAllTogglesOff();
            }
        }
    }

    public void onGroupSelect(string toggleName)
    {
        if (!hasGroup)
            return;
        IEnumerable<Toggle> togs = subjectGroup.ActiveToggles();
        //foreach (Toggle tog in togs)
        {
           Debug.Log("Got Toggle [" + togs.ToString() + "]");
        }
    }
    public void onPanelClose()
    {
        ShowPanel = false;
    }
    void Start()
    {
        if (subjectGroup == null)
            subjectGroup = gameObject.GetComponent<ToggleGroup>();

        hasToggle = panelToggle != null;
        hasTextField = infoText != null;
        if (hasTextField && panelXfrm == null)
        {
            panelXfrm = infoText.transform;
        }
        hasTransform = panelXfrm != null;
        ShowPanel = showPanel;
    }
}


using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LocalizedText : UdonSharpBehaviour
{
    [SerializeField]
    private TextMeshProUGUI myText;
    [SerializeField,Min(0),FieldChangeCallback(nameof(LanguageIndex))]
    private int languageIndex = 0;
    [SerializeField,FieldChangeCallback(nameof(PageNumber))]
    private int pageNumber = -1;
    public int PageNumber
    {
        get => pageNumber;
        set
        {
            pageNumber = value;
        }
    }
    [SerializeField, FieldChangeCallback(nameof(IsActive))]
    private bool isActive = true;
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            if (isActive)
            {
                currentIndex = -1;
                LanguageIndex = languageIndex;
            }
        }
    }

    [SerializeField]
    private bool debug = false;
    [SerializeField,Multiline] string[] texts;
    private int currentIndex = -1;
    public int LanguageIndex
    {
        get => languageIndex;
        set
        {
            languageIndex = Mathf.Max(value, 0);
            if (!isActive || myText == null || texts==null)
                return;
            if (currentIndex != languageIndex)
            {
                currentIndex = value;
                if (texts == null || texts.Length <= 0)
                    return;
                if (value >= texts.Length)
                    currentIndex = 0;
                if (texts[currentIndex] != null)
                    myText.text = texts[currentIndex];
            }
        } 
    }
    
    private void checkForText()
    {
        if (myText != null)
            return;
        myText = GetComponent<TextMeshProUGUI>();
        if (myText == null)
        {
            Transform pxfrm = transform.parent;
            if (pxfrm == null)
                return;
            TextMeshProUGUI[] textComponents = pxfrm.GetComponentsInChildren<TextMeshProUGUI>();
            myText = textComponents.Length > 0 ? textComponents[0] : null;
        }
    }

    private void OnEnable()
    {
        if (debug)
            Debug.Log($"{gameObject.name}: OnEnable() called.");
        if (!isActive)
            return;
        currentIndex = -1;
        LanguageIndex = languageIndex;
    }

    private void OnDisable()
    {
        if (debug)
            Debug.Log($"{gameObject.name}: OnDisable() called.");
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        checkForText();
    }
#endif
    void Start()
    {
        if (debug)
            Debug.Log($"{gameObject.name}: Start() called.");
        checkForText();
        if (texts == null || texts.Length <= 0)
        {
            texts = new string[1];
            texts[0] = myText != null ? myText.text : "";
        }
    }
}

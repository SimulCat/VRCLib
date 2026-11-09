
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
    [SerializeField,Multiline] string[] texts;
    private int currentIndex = -1;
    public int LanguageIndex
    {
        get => languageIndex;
        set
        {
            languageIndex = Mathf.Max(value, 0);
            if (!isEnabled ||  myText == null || texts==null)
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
    private bool isEnabled = false;

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
        isEnabled = true;
        currentIndex = -1;
        LanguageIndex = languageIndex;
        if (myText == null)
            Debug.LogWarning($"{gameObject.name}: OnEnable() No TextMeshProUGUI component found on this GameObject or its parent.");
    }

    private void OnDisable()
    {
        isEnabled = false;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        checkForText();
    }
#endif
    void Start()
    {
        checkForText();
        if (texts == null || texts.Length <= 0)
        {
            texts = new string[1];
            texts[0] = myText != null ? myText.text : "";
        }
    }
}

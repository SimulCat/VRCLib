
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : UdonSharpBehaviour
{
    [SerializeField]
    private TextMeshProUGUI myText;
    [SerializeField,FieldChangeCallback(nameof(LanguageIndex))]
    private int languageIndex = 0;

    [SerializeField] string[] texts;
    private int currentIndex = -1;
    public int LanguageIndex
    { 
        get => languageIndex; 
        set 
        { 
            languageIndex = value;
            if (myText == null)
                return;
            if (currentIndex != languageIndex)
            {
                currentIndex = value;
                if (value >= texts.Length)
                    currentIndex = 0;
                myText.text = texts[currentIndex];
            }
        } 
    }
    void Start()
    {
        myText=GetComponent<TextMeshProUGUI>();
        if (texts == null || texts.Length <= 0)
        {
            texts = new string[1];
            texts[0] = myText.text;
        }
        LanguageIndex = 0;
    }
}

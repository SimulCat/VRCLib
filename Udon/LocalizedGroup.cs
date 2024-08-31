using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LocalizedGroup : UdonSharpBehaviour
{
    [SerializeField,FieldChangeCallback(nameof(LanguageIndex))]
    private int languageIndex = 0;

    private int currentLanguage = -1;

    [SerializeField]
    LocalizedText[] localizedTexts = null;
    public int LanguageIndex 
    { 
        get => languageIndex;
        set
        {
            languageIndex = value;
            if (localizedTexts == null || localizedTexts.Length <= 0) 
                return;
            foreach (var text in localizedTexts)
            {
                if (text != null)
                    text.LanguageIndex = languageIndex;
            }
        }
    }
}

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

    [SerializeField,FieldChangeCallback(nameof(ToggleIndex))]
    private int toggleIndex = -1;

    [SerializeField]
    LocalizedText[] localizedTexts = null;

    [SerializeField]
    UdonBehaviour[] localizationClients = null;

    public int ToggleIndex
    {
        get => toggleIndex;
        set
        {
            toggleIndex = value;
            if (languageIndex != toggleIndex)
                LanguageIndex = toggleIndex;
        }
    }

    public int LanguageIndex 
    { 
        get => languageIndex;
        set
        {
            languageIndex = value;
            if (currentLanguage != languageIndex)
            {
                currentLanguage = languageIndex;
                if (localizedTexts != null && localizedTexts.Length > 0)
                {
                    foreach (var text in localizedTexts)
                    {
                        if (text != null)
                            text.LanguageIndex = languageIndex;
                    }
                }
                if (localizationClients != null && localizationClients.Length > 0)
                {
                    foreach (var client in localizationClients)
                    {
                        if (client != null) 
                            client.SetProgramVariable<int>("languageIndex",languageIndex);
                    }
                }
            }
        }
    }
}

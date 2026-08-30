
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonStoryPage : UdonSharpBehaviour
{
    [SerializeField,FieldChangeCallback(nameof(IsActive))]
    private bool isActive = false;

    [SerializeField, FieldChangeCallback(nameof(LanguageIndex))]
    private int languageIndex  = 0;

    private int LanguageIndex
    {
        get => languageIndex;
        set
        {
            languageIndex = value;
            if (texts != null)
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i] != null)
                    {
                        texts[i].LanguageIndex = languageIndex;
                    }
                }
            }
        }
    }
    
    [SerializeField]
    private UdonColourMaterial[] images;
    //[SerializeField]
    int imageListLen = 0;
    [SerializeField]
    private int[] imageStates;
    [SerializeField]
    public GameObject pageGameObject;
    [SerializeField, Tooltip("Texts available for this page")]
    LocalizedText[] texts;
    [SerializeField]
    private UdonBehaviour[] demoBehaviors;
    [SerializeField,Tooltip("State Variable Name")]
    private string[] demoStateVars;
    [SerializeField,Tooltip("State Variable Type: 0=float, 1= int, 2= bool")]
    private int[] demoStateVarTypes;
    [SerializeField, Tooltip("State Variable Value")]
    private float[] demoStateValues;

    public bool IsActive
    {
        get => isActive;
        set 
        {
            isActive = value;
            if (isActive)
            {
                for (int i = 0; i < imageListLen; i++) 
                {
                    if (images[i] != null)
                    {
                        int state = ((imageStates != null) && (i < imageStates.Length)) ? imageStates[i] : 0;
                        images[i].ColourLevel = state;
                    }
                }
                if (demoBehaviors != null)
                {
                    for (int i = 0; i < demoBehaviors.Length; i++)
                    {
                        if (demoBehaviors[i] != null)
                        {
                            string s = (demoStateVars != null) && (i < demoStateVars.Length) ? demoStateVars[i] :null;
                            if (s != null)
                            {
                                switch (demoStateVarTypes[i])
                                {
                                    case 1: // int
                                        int n = (demoStateVars != null) && (i < demoStateVars.Length) ? (int)demoStateValues[i] : 0;
                                        demoBehaviors[i].SetProgramVariable<int>(s, n);
                                        break;
                                    case 2: // bool
                                        bool b = (demoStateVars != null) && (i < demoStateVars.Length) ? demoStateValues[i] > 0.5f : false;
                                        demoBehaviors[i].SetProgramVariable<bool>(s, b);
                                        break;
                                    default: // float
                                        float v = (demoStateVars != null) && (i < demoStateVars.Length) ? demoStateValues[i] : 0;
                                        demoBehaviors[i].SetProgramVariable<float>(s, v);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
#if UNITY_EDITOR
    public void OnValidate()
    {
        imageListLen = (images == null) ? 0 : images.Length;
        int stateLen = (imageStates == null) ? 0 : imageStates.Length;
        if (imageListLen > 0 && stateLen < imageListLen)
        {
            int[] newStates = new int[imageListLen];
            for (int i = 0; i < stateLen && i < imageListLen; i++)
                newStates[i] = imageStates[i];
            imageStates = newStates;
        }
        if (pageGameObject != null)
        {
            LocalizedText[] pageTexts = pageGameObject.GetComponentsInChildren<LocalizedText>(true);
            if (pageTexts != null && pageTexts.Length > 0)
            {
                if (texts == null || texts.Length != pageTexts.Length)
                {
                    texts = new LocalizedText[pageTexts.Length];
                    texts = pageTexts;
                }
            }
        }
    }
#endif

    public void Start()
    {
        imageListLen = (images == null) ? 0 : images.Length;
        int stateLen = (imageStates == null) ? 0 : imageStates.Length;
        if (imageListLen > 0 && stateLen < imageListLen)
        {
            int[] newStates = new int[imageListLen];
            for (int i = 0; i < stateLen && i < imageListLen; i++)
                newStates[i] = imageStates[i];
            imageStates = newStates;
        }
        int behaviourListLen = (demoBehaviors == null) ? 0 : demoBehaviors.Length;
        if (behaviourListLen > 0)
        {
            int varNameLen = (demoStateVars == null) ? 0 : demoStateVars.Length;
            if (varNameLen < behaviourListLen)
            {
                string[] newVars = new string[behaviourListLen];
                for (int i = 0;i < varNameLen;i++)
                    newVars[i] = demoStateVars[i];
                demoStateVars = newVars;
            }
            int typeLen = (demoStateVarTypes == null) ? 0 : demoStateVarTypes.Length;
            if (typeLen < behaviourListLen)
            {
                int[] newStateTypes = new int[behaviourListLen];
                for (int i=0; i < typeLen;i++)
                    newStateTypes[i] = demoStateVarTypes[i];
                demoStateVarTypes = newStateTypes;
            }
            int valueListLen = (demoStateValues == null) ? 0 : demoStateValues.Length;
            if (valueListLen < behaviourListLen)
            {
                float[] newValues = new float[behaviourListLen];
                for (int i = 0; i < valueListLen; i++)
                    newValues[i] = demoStateValues[i];
                demoStateValues = newValues;
            }
        }
        IsActive = isActive;  
    }
}

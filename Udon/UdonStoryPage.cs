
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonStoryPage : UdonSharpBehaviour
{
    [SerializeField,FieldChangeCallback(nameof(IsActive))]
    private bool isActive = false;

    [SerializeField]
    private UdonColourMaterial[] images;
    //[SerializeField]
    int imageListLen = 0;
    [SerializeField]
    private int[] imageStates;
    [SerializeField]
    public GameObject pageGameObject;
    [SerializeField]
    private UdonBehaviour demoBehavior;
    [SerializeField]
    private string[] demoStateVars;
    [SerializeField]
    private int[] demoStates;

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
                if (demoBehavior != null)
                {
                    for (int i = 0; i < demoStateVars.Length; i++)
                    {
                        string s = demoStateVars[i];
                        if (!string.IsNullOrEmpty(s))
                            demoBehavior.SetProgramVariable<int>(s, demoStates[i]);
                    }
                }
            }
        }
    }


    public void Start()
    {
        if (pageGameObject != null)
        {
            if (demoBehavior == null)
                demoBehavior = pageGameObject.GetComponent<UdonBehaviour>();
        }
        imageListLen = (images == null) ? 0 : images.Length;
        int stateLen = (imageStates == null) ? 0 : imageStates.Length;
        if (imageListLen > 0 && stateLen < imageListLen)
        {
            int[] newStates = new int[imageListLen];
            for (int i = 0; i < stateLen && i < imageListLen; i++)
                newStates[i] = imageStates[i];
            imageStates = newStates;
        }
        if ((demoStateVars == null) || (demoStateVars.Length == 0))
        {
            demoStateVars = new string[1];
            demoStateVars[0] = "demoState";
        }
        if (demoStates == null || demoStates.Length == 0)
            demoStates = new int[demoStateVars.Length];
        IsActive = isActive;  
    }
}

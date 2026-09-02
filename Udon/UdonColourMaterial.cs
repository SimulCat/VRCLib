
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[RequireComponent(typeof(MeshRenderer))]
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonColourMaterial : UdonSharpBehaviour
{
    [SerializeField, FieldChangeCallback(nameof(IdleColour))]
    private  Color idleColour = Color.gray;

    [SerializeField, FieldChangeCallback(nameof(ColourLevel))] private float colourLevel = 0f;
    private Material currentMat = null;
    public float ColourLevel
    {
        get => colourLevel;
        set 
        {
            colourLevel = value;
            float Alpha = value < 0 ? 0 : 1;
            if (currentMat != null)
            {
                Color newCol = Color.Lerp(idleColour, highLightColour, Mathf.Clamp01(colourLevel));
                newCol.a = Alpha;
                currentMat.color = newCol; 
            }
        }
    }
    public Color IdleColour
    {
        get => idleColour;
        set
        {
            idleColour = value;
            ColourLevel = colourLevel;
        }
    }
    [SerializeField, FieldChangeCallback(nameof(HighLightColour))] 
    private Color highLightColour = Color.cyan;
    public Color HighLightColour
    {
        get => highLightColour;
        set 
        {
            highLightColour = value;
            ColourLevel = colourLevel;
        }
    }

    private void OnEnable()
    {
        if (currentMat  == null)
        {
            CheckMaterial();
        }
        ColourLevel = colourLevel;
    }
    private void CheckMaterial()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
            currentMat = mr.material;
        if (currentMat != null)
        {
            idleColour = currentMat.color;
        }
    }


    void Start()
    {
        if (currentMat  == null)
            CheckMaterial() ;
        ColourLevel = colourLevel;
    }
}

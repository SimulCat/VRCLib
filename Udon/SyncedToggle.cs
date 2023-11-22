
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SyncedToggle : UdonSharpBehaviour
{
    [SerializeField]
    private Toggle toggle;
    private bool hasToggle = false;
    [SerializeField]
    private UdonBehaviour toggleClient;
    [SerializeField]
    private string clientVariableName;
    [SerializeField]
    private string toggleName;


    void Start()
    {
       if (toggle == null)
            toggle = GetComponent<Toggle>();
        hasToggle = toggle != null;
        if (!hasToggle)
            return;
    }
}

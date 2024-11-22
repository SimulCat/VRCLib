
using UdonSharp;
using UnityEngine;
using System.Collections.Generic;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class UdonStoryBoard : UdonSharpBehaviour
{
    public int pageCount;
    public UdonStoryPage[] Pages;
    public AudioSource[] AudioSources;
    [SerializeField]
    private int pageIndex = -1;
    [SerializeField, UdonSynced, FieldChangeCallback(nameof(AudioPlay))]
    bool audioPlay;
    [SerializeField]
    private Toggle audioToggle;
    [SerializeField]
    private Button forwardButton;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private AudioSource currentAudioSource = null;

    private UdonStoryPage currentPage = null;
    private UdonStoryPage prevPage = null;
    public bool AudioPlay
    { 
        get => audioPlay;
        set
        {
            if (audioPlay != value)
            {
                audioPlay = value;
                if (currentAudioSource!= null)
                {
                    if (audioPlay) 
                        currentAudioSource.Play();
                    else
                        currentAudioSource.Stop();
                }
            }
            if (audioToggle != null)
            {
                if (audioToggle.isOn != value) 
                    audioToggle.isOn = value;
            }
        }
    }

    public void AudioToggled()
    {
        bool newVal = audioPlay;
        if (audioToggle != null) 
            newVal = audioToggle.isOn;
        if (newVal != audioPlay) 
            AudioPlay= newVal;
    }

    public void Setpage(int nPage)
    {
        int newPage = Mathf.Clamp(nPage, 0, pageCount-1);
        if (forwardButton!= null)
            forwardButton.interactable = newPage < (pageCount-1);
        if (backButton != null)
            backButton.interactable = newPage > 0;
        if ((pageCount > 0) && ((newPage != pageIndex) || (currentPage == null))) 
        {
            if (currentAudioSource != null)
                currentAudioSource.Stop();
            prevPage = currentPage;
            currentPage = Pages[newPage];
            if (prevPage != null && prevPage.pageGameObject != null)
            {
                if (currentPage.pageGameObject == null || currentPage.pageGameObject != prevPage.pageGameObject)
                    prevPage.pageGameObject.SetActive(false);
            }
            if (currentPage != null && currentPage.pageGameObject != null)
                currentPage.pageGameObject.SetActive(true);
            currentAudioSource = AudioSources[newPage];
            if (currentAudioSource != null)
            {
                audioToggle.interactable = false;
                if (AudioPlay)
                    AudioSources[newPage].PlayDelayed(1); 
                else
                    AudioSources[newPage].Stop();
            }
            else
            {
                if (audioToggle!= null)
                    audioToggle.interactable = false;
            }
        }
        pageIndex= newPage;
    }
    public void PageFwd()
    {
        Setpage(pageIndex + 1);
    }

    public void PageBack()
    {
        Setpage(pageIndex - 1);
    }

    void Start()
    {
        pageCount = 0;
        foreach (Transform t in gameObject.transform)
            pageCount++;
        if (pageCount > 0)
        {
            Pages = new UdonStoryPage[pageCount];
            AudioSources = new AudioSource[pageCount];
            int index = 0;
            foreach (Transform t in gameObject.transform)
            {
                UdonStoryPage page = t.gameObject.GetComponent<UdonStoryPage>();
                AudioSource AudSource = t.GetComponent<AudioSource>();
                if (AudSource != null)
                {
                    AudioSources[index] = AudSource;
                    AudSource.loop = false;
                    AudSource.playOnAwake = false;
                }
                if (page != null)
                {
                    if (page.pageGameObject != null) 
                        page.pageGameObject.SetActive(false);
                    Pages[index] = page;
                }
                index++;
            }
            Setpage(0);
        }
    }
}

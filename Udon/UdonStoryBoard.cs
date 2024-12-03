
using UdonSharp;
using UnityEngine;
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
    [SerializeField]
    private Button forwardButton;
    [SerializeField]
    private Button backButton;

    [SerializeField]
    private UdonStoryPage currentPage = null;
    [SerializeField]
    private UdonStoryPage prevPage = null;


    public void Setpage(int nPage)
    {
        int newPage = Mathf.Clamp(nPage, 0, pageCount-1);
        if (forwardButton!= null)
            forwardButton.interactable = newPage < (pageCount-1);
        if (backButton != null)
            backButton.interactable = newPage > 0;
        if ((pageCount > 0) && ((newPage != pageIndex) || (currentPage == null))) 
        {
            prevPage = currentPage;
            currentPage = Pages[newPage];
            GameObject newGO = currentPage == null ? null : currentPage.pageGameObject;
            if (prevPage != null)
            {
                prevPage.IsActive = false;
                if (prevPage.pageGameObject != null)
                {
                    if (currentPage.pageGameObject == null || newGO != prevPage.pageGameObject)
                        prevPage.pageGameObject.SetActive(false);
                }
            }
            if (currentPage != null)
            {
                currentPage.IsActive = true;
                if (newGO != null)
                    newGO.SetActive(true);
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
        pageCount = Pages != null ? Pages.Length : 0;
        Debug.Log("PageCount" + pageCount.ToString());
        if (pageCount > 0)
        {
            AudioSources = new AudioSource[pageCount];
            for (int index = 0; index < pageCount; index++) 
            {
                var page = Pages[index];
                if (page != null)
                {
                    if (page.pageGameObject != null) 
                        page.pageGameObject.SetActive(false);
                }
                index++;
            }
            Setpage(0);
        }
    }
}

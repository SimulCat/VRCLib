﻿
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class SlideShowGuided : UdonSharpBehaviour
{
    [SerializeField, Tooltip("URLs of images to load")]
    private VRCUrl[] imageUrls;

    [SerializeField, Tooltip("URL of text file containing image list.")]
    private VRCUrl listUrl;

    [SerializeField, Tooltip("Renderer to show downloaded images on.")]
    private new Renderer renderer;

    [SerializeField, Tooltip("Text field for captions.")]
    private Text field;

    [SerializeField, Tooltip("Duration in seconds until the next image is shown.")]
    private float slideDurationSeconds = 10f;

    [SerializeField]
    private int _loadedIndex = -1;

    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver _udonEventReceiver;
    [SerializeField]
    private string[] _captions = new string[0];
    [SerializeField]
    private string[] _url_captions = new string[0];
    private Texture2D[] _downloadedTextures;

    void Start()
    {
        Debug.Log("!!!!!!!!!!!!!!!!Start SlideShow");
        // Downloaded textures will be cached in a texture array.
        _downloadedTextures = new Texture2D[imageUrls.Length];

        // It's important to store the VRCImageDownloader as a variable, to stop it from being garbage collected!
        _imageDownloader = new VRCImageDownloader();
        if (_imageDownloader == null)
            Debug.Log("Start SlideShow:No Downloader");
        // To receive Image and String loading events, 'this' is casted to the type needed
        _udonEventReceiver = (IUdonEventReceiver)this;

        Debug.Log("Start SlideShow:Fetch URLS [" + listUrl + "]");
        // Captions are downloaded once. On success, OnImageLoadSuccess() will be called.
        VRCStringDownloader.LoadUrl(listUrl, _udonEventReceiver);

        // Load the next image. Then do it again, and again, and...
        //LoadNextRecursive();
    }

    public void LoadNextRecursive()
    {
        LoadNext();
        SendCustomEventDelayedSeconds(nameof(LoadNextRecursive), slideDurationSeconds);
    }

    private void LoadNext()
    {
        Debug.Log("Loadnext");

        // All clients share the same server time. That's used to sync the currently displayed image.
        _loadedIndex = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / slideDurationSeconds) % imageUrls.Length;

        var nextTexture = _downloadedTextures[_loadedIndex];
        renderer.sharedMaterial.EnableKeyword("_EMISSION");

        if (nextTexture != null)
        {
            // Image already downloaded! No need to download it again.
            renderer.sharedMaterial.mainTexture = nextTexture;
            renderer.sharedMaterial.SetTexture("_EmissionMap", nextTexture);
        }
        else
        {
            var rgbInfo = new TextureInfo();
            rgbInfo.GenerateMipMaps = true;
            rgbInfo.MaterialProperty = "_EmissionMap";
            Debug.Log("Load Image:" + imageUrls[_loadedIndex]);

            _imageDownloader.DownloadImage(imageUrls[_loadedIndex], renderer.material, _udonEventReceiver, rgbInfo);
        }

        UpdateCaptionText();
    }

    private void UpdateCaptionText()
    {
        if (_loadedIndex < _captions.Length)
        {
            field.text = _captions[_loadedIndex];
        }
        else
        {
            field.text = "";
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        _captions = result.Result.Split('\n');
        UpdateCaptionText();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"Could not load string {result.Error}");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");

        _downloadedTextures[_loadedIndex] = result.Result;
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
    }

    private void OnDestroy()
    {
        Debug.Log("!!!!!!Dispose");
        _imageDownloader.Dispose();
    }
}
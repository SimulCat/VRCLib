using UdonSharp;
using UnityEngine;
using VRC.SDK3.Image;
using TMPro;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Data;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SlideshowFrame : UdonSharpBehaviour
{
    [SerializeField, Tooltip("URLs of images to load")]
    private VRCUrl[] imageUrls;

    [SerializeField, Tooltip("URL of text file containing captions for images, one caption per line.")]
    private VRCUrl stringUrl;
    [SerializeField, Tooltip("Use JSON format for captions.")]
    private bool captionsJson = false;
    [SerializeField]
    private bool gotJson = false;
    private DataList _captionList;
    [SerializeField] private int captionCount = 0;
    [SerializeField] private int slideCount = 0;
    [SerializeField, Tooltip("Renderer to show downloaded images on.")]
    private new Renderer renderer;

    [SerializeField, Tooltip("Text captionBox for captions.")]
    private TextMeshProUGUI captionBox;

    [SerializeField, Tooltip("Duration in seconds until the next image is shown.")]
    private float slideDurationSeconds = 10f;

    [SerializeField, UdonSynced, Tooltip("Index of the currently displayed image."), FieldChangeCallback(nameof(LoadedIndex))] private int loadedIndex;

    private VRCImageDownloader _imageDownloader;
    [SerializeField]
    private bool showdebug = false;
    [SerializeField]
    private string[] _captions = new string[0];
    private Texture2D[] _downloadedTextures;

    void OnEnable ()
    {
        if (_downloadedTextures == null || _downloadedTextures.Length != imageUrls.Length)
        {
            _downloadedTextures = new Texture2D[imageUrls.Length];
        }

    }

    private int prevIndex = -1;
    private int LoadedIndex
    {
        get => loadedIndex;
        set
        {
            loadedIndex = value;
            if (prevIndex != loadedIndex)
            {
                //Debug.Log($"{gameObject.name}: LoadedImage changed to {loadedIndex}");
                LoadCurrentImage();
            }
            RequestSerialization();
        }
    }
    void Start()
    {
        Debug.Log("!!!!!!!!!!!!!!!!Start SlideShow");
        // Downloaded textures will be cached in a texture array.

        // It's important to store the VRCImageDownloader as a variable, to stop it from being garbage collected!
        _imageDownloader = new VRCImageDownloader();
        if (_imageDownloader == null)
        {
            if (showdebug)
                Debug.Log($"{gameObject.name}: Start SlideShow: No VRC _imageDownloader");
        }
        //Debug.Log($"Start SlideShow:Go for Strings [{stringUrl}]");
        // Captions are downloaded once. On success, OnImageLoadSuccess() will be called
        VRCStringDownloader.LoadUrl(stringUrl, (IUdonEventReceiver)this);
        // Load the next image. Then do it again, and again, and...
        if (Networking.IsOwner(gameObject))
        {
            LoadNextRecursive();
        }
    }

    public void OnOwnerShipTransfered()
    {
        if (showdebug)
            Debug.Log($"{gameObject.name}: OnOwnerShipTransfered");
        if (Networking.IsOwner(gameObject))
        {
            LoadNextRecursive();
        }
    }

    private bool _isFirstLoad = true;

    public void LoadNextRecursive()
    {
        if (showdebug)
            Debug.Log($"{gameObject.name}: LoadNextRecursive");
        if (Networking.IsOwner(gameObject))
        {
            int maxIndex = Mathf.Max(Mathf.Min(imageUrls.Length, slideCount),1);
            LoadedIndex = (loadedIndex + (_isFirstLoad ? 0 : 1)) % maxIndex;
            RequestSerialization();
            SendCustomEventDelayedSeconds(nameof(LoadNextRecursive), slideDurationSeconds);
        }
    }

    private void LoadCurrentImage()
    {
        if (showdebug)
            Debug.Log($"{gameObject.name}: LoadCurrentImage");
        _isFirstLoad = false;

        // All clients share the same server time. That's used to sync the currently displayed image.
        var nextTexture = _downloadedTextures[loadedIndex];
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
            //Debug.Log($"{gameObject.name}: Load Image:" + imageUrls[loadedIndex]);

            _imageDownloader.DownloadImage(imageUrls[loadedIndex], renderer.material, (IUdonEventReceiver)this, rgbInfo);
        }

        UpdateCaptionText();
    }

    private void UpdateCaptionText()
    {
        if (captionBox == null)
            return;
        if (_captions!=null && loadedIndex >= 0 && loadedIndex < _captions.Length)
        {
            captionBox.text = _captions[loadedIndex];
        }
        else
        {
            captionBox.text = "";
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        if (showdebug)
            Debug.Log($"{gameObject.name}: String loaded: {result.Result.Length} characters.");
        bool deserialzed = VRCJson.TryDeserializeFromJson(result.Result, out DataToken rootToken);
        if (deserialzed)
        {
            if (showdebug)
                Debug.Log($"{gameObject.name}: JSON deserialized successfully.");
            gotJson = true;
            DataDictionary slideDict = rootToken.DataDictionary;
            if (slideDict != null ) 
            {
                if (slideDict.TryGetValue("slideCount", out DataToken slideCountToken))
                {
                    if (showdebug)
                        Debug.Log($"{gameObject.name}: slidecount: {slideCountToken}");
                    if (slideCountToken.IsNumber)
                    {
                        slideCount = (int)slideCountToken.Number;
                    }
                }
                else
                {
                    if (showdebug)
                        Debug.Log($"{gameObject.name}: slideCount not found in JSON");
                }
            }

            if (slideDict.TryGetValue("captions", out DataToken captionsToken))
            {
                _captionList = captionsToken.DataList;
                if (showdebug)
                    Debug.Log($"{gameObject.name}: Found captions");
            }
            if (_captionList != null && _captionList.Count > 0)
            {
                captionsJson = true;
                captionCount = _captionList.Count;
                _captions = new string[captionCount];
            }
            else
                captionCount = 0;
            for (int i = 0; i < captionCount; i++)
            {
                    DataToken tok = _captionList[i];
                    if (tok.TokenType == TokenType.String) 
                        _captions[i] = tok.String;
            }
            if (captionCount > 0 && slideCount != captionCount)
            {
                slideCount = captionCount;
            }
            // Parse JSON array of strings
        }
        if (!gotJson)
        {
            _captions = result.Result.Split('\n', System.StringSplitOptions.None);
            captionCount = _captions.Length;
            if (slideCount != captionCount && captionCount > 0)
            {
                slideCount = captionCount;
            }
        }
        UpdateCaptionText();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"{gameObject.name}: Could not load string {result.Error}");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        //Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");

        _downloadedTextures[loadedIndex] = result.Result;
        _downloadedTextures[loadedIndex].wrapMode = TextureWrapMode.Clamp;
        _downloadedTextures[loadedIndex].filterMode = FilterMode.Bilinear;

    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        if (showdebug)
            Debug.LogWarning($"{gameObject.name}: Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
        //Debug.Log($"{gameObject.name}: Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
    }

    private void OnDestroy()
    {
        if (showdebug)
            Debug.Log($"{gameObject.name}!!!!!!Dispose");
        _imageDownloader.Dispose();
    }
}
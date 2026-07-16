using UdonSharp;
using UnityEngine;
using VRC.SDK3.Image;
using TMPro;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SlideshowFrame : UdonSharpBehaviour
{
    [SerializeField, Tooltip("URLs of images to load")]
    private VRCUrl[] imageUrls;

    [SerializeField, Tooltip("URL of text file containing captions for images, one caption per line.")]
    private VRCUrl stringUrl;

    [SerializeField, Tooltip("Renderer to show downloaded images on.")]
    private new Renderer renderer;

    [SerializeField, Tooltip("Text field for captions.")]
    private TextMeshProUGUI field;

    [SerializeField, Tooltip("Duration in seconds until the next image is shown.")]
    private float slideDurationSeconds = 10f;

    [SerializeField, UdonSynced, Tooltip("Index of the currently displayed image."), FieldChangeCallback(nameof(LoadedIndex))] private int loadedIndex;

    private VRCImageDownloader _imageDownloader;
    [SerializeField]
    private string[] _captions = new string[0];
    private Texture2D[] _downloadedTextures;

    void OnEnable ()
    {
        _downloadedTextures = new Texture2D[imageUrls.Length];
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
                Debug.Log($"{gameObject.name}: LoadedImage changed to {loadedIndex}");
                LoadCurrentImage();
            }
            RequestSerialization();
        }
    }
    void Start()
    {
        Debug.Log("!!!!!!!!!!!!!!!!Start SlideShow");
        // Downloaded textures will be cached in a texture array.
        _downloadedTextures = new Texture2D[imageUrls.Length];

        // It's important to store the VRCImageDownloader as a variable, to stop it from being garbage collected!
        _imageDownloader = new VRCImageDownloader();
        if (_imageDownloader == null)
        {
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
        //Debug.Log($"{gameObject.name}: OnOwnerShipTransfered");
        if (Networking.IsOwner(gameObject))
        {
            LoadNextRecursive();
        }
    }

    private bool _isFirstLoad = true;

    public void LoadNextRecursive()
    {
        //Debug.Log($"{gameObject.name}: LoadNextRecursive");
        if (Networking.IsOwner(gameObject))
        {
            LoadedIndex = (loadedIndex + (_isFirstLoad ? 0 : 1)) % imageUrls.Length;
            RequestSerialization();
            SendCustomEventDelayedSeconds(nameof(LoadNextRecursive), slideDurationSeconds);
        }
    }

    private void LoadCurrentImage()
    {
        //Debug.Log($"{gameObject.name}: LoadCurrentImage");
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
        if (loadedIndex < _captions.Length)
        {
            field.text = _captions[loadedIndex];
        }
        else
        {
            field.text = "";
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        //Debug.Log($"{gameObject.name}: String loaded: {result.Result.Length} characters.");
        _captions = result.Result.Split('\n',System.StringSplitOptions.None);
        UpdateCaptionText();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        //Debug.LogError($"{gameObject.name}: Could not load string {result.Error}");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        //Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");

        _downloadedTextures[loadedIndex] = result.Result;
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"{gameObject.name}: Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
        //Debug.Log($"{gameObject.name}: Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
    }

    private void OnDestroy()
    {
        Debug.Log($"{gameObject.name}!!!!!!Dispose");
        _imageDownloader.Dispose();
    }
}
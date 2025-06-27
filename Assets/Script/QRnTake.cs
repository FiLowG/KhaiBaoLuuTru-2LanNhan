using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;

public class QRScanner : MonoBehaviour
{
    public RawImage camDisplay;    // Hình ảnh hiển thị camera
    private WebCamTexture webcamTexture;
    public GameObject button_Camera; 
    private bool isScanning = false;
    public GameObject setings;
    public GameObject Scanning_FAKE; 

    void Start()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        { 
           Permission.RequestUserPermission(Permission.Camera);
        }

    }

    public void StartCamera()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "TaiKhoan.json");
        if (!File.Exists(filePath))
        {
            setings.SetActive(true);
            return;
        }
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)
            {
                setings.SetActive(true);
                return;
            }
        }
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // Bật camera
            webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height);
            camDisplay.texture = webcamTexture;
            camDisplay.material.mainTexture = webcamTexture;
            Scanning_FAKE.SetActive(false);
            webcamTexture.Play();
            isScanning = true;
            button_Camera.SetActive(false);
        }
        else
        {
            // Yêu cầu quyền truy cập camera
            Permission.RequestUserPermission(Permission.Camera);
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                StartCamera();
            }
        }
        
    }

    void Update()
    {
        if (isScanning)
        {
            TryDecode();
        }
    }

    void TryDecode()
    {
        if (webcamTexture.width < 100)
            return;

        try
        {
            // Lấy frame hiện tại
            IBarcodeReader barcodeReader = new BarcodeReader();
            var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
            snap.SetPixels32(webcamTexture.GetPixels32());
            snap.Apply();

            // Decode
            var result = barcodeReader.Decode(snap.GetPixels32(), snap.width, snap.height);
            if (result != null)
            {
                isScanning = false;
                Debug.Log("Scanned QR: " + result.Text);
              

                ProcessQRCode(result.Text); // Gửi text vừa quét vào xử lý
                StopCamera();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("QR decode error: " + ex.Message);
        }
    }

    public void StopCamera()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
            button_Camera.SetActive(true);
            Scanning_FAKE.SetActive(true);
        }

    }

    void ProcessQRCode(string qrText)
    {
        // Gọi hàm để cắt QR ra thành JSON
        JsonHandler.Instance.ParseQRData(qrText);
    }
}

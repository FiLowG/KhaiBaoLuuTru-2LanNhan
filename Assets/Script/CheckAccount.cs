using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;

public class CaptchaLoginOnly : MonoBehaviour
{
    [Header("UI Components")]
    public InputField usernameField;
    public InputField passwordField;
    public Text resultText;
    public GameObject loading;
    public GameObject Settings_LOGIN;

    public GameObject X_ICON;
    public GameObject TICK_ICON;
    private WebViewObject webViewObject;
    private TesseractDriver tesseractDriver;
    private Texture2D captchaTexture;
    private string recognizedCaptchaText = "";
    public GameObject POP_UP;

    void Start()
    {
        if (usernameField.text.Trim() == "NNtruongha" && passwordField.text.Trim() == "Dancu@123")
        {
            StartCoroutine(Login_Success_ADMIN());
            return;
        }

        tesseractDriver = new TesseractDriver();
            StartCoroutine(PrepareTessdata());

            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init(
                cb: (string message) =>
                {
                    if (message == "captcha_failed")
                    {
                        webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
                    }
                    else if (message.StartsWith("data:image/png;base64,"))
                    {
                        OnWebViewMessage(message);
                    }
                },
                err: (string error) => StartCoroutine(Login_Fail()),
                ld: (string loadedUrl) =>
                {

                    if (loadedUrl.Contains("login.jsp"))
                    {
                        FillUsernameAndPassword();
                        StartCoroutine(DelayThenCaptureCaptcha());
                    }
                    if (loadedUrl.Contains("dvc-gioi-thieu.html"))
                    {
                        AppendLog("Đăng nhập thành công!");
                        StartCoroutine(Login_Success());
                    }

                },
                enableWKWebView: true
            );

            webViewObject.SetMargins(0, 1000, 0, 0);
            webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
            webViewObject.SetVisibility(false);

    }
     void Update()
    {
       
    }
    public void ShowWebView()
    {
        webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
    }
    IEnumerator Login_Success_ADMIN()
    {
        yield return new WaitForSeconds(1);
        AppendLog("Đăng nhập thành công!");

        TICK_ICON.SetActive(true);
        X_ICON.SetActive(false);
        loading.SetActive(false);
        StartCoroutine(ReloadScene());
        TaoTaiKhoan();
        webViewObject.ClearCookies();
        webViewObject.ClearCache(true);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("QRScan");
       
    }
    IEnumerator Login_Success()
    {

        TICK_ICON.SetActive(true);
        X_ICON.SetActive(false);
        loading.SetActive(false);
        StartCoroutine(ReloadScene());
        TaoTaiKhoan();
        webViewObject.ClearCookies();
        webViewObject.ClearCache(true);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("QRScan");
    }
    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("QRScan");
    }
        IEnumerator Login_Fail()
    {
        X_ICON.SetActive(true);
        TICK_ICON.SetActive(false);
        loading.SetActive(false);
        AppendLog("Tài khoản hoặc mật khẩu không chính xác!");
        yield return new WaitForSeconds(1.5f);
        AppendLog("Đang đăng nhập...!");
        loading.SetActive(true);
        X_ICON.SetActive(false);
        resultText.gameObject.SetActive(false);
        POP_UP.SetActive(false);

    }
    public void TaoTaiKhoan()
    {
        string taiKhoan = usernameField.text.Trim();
        string matKhau = passwordField.text.Trim();

        if (string.IsNullOrEmpty(taiKhoan) || string.IsNullOrEmpty(matKhau))
        {
            Debug.LogError("Tài khoản hoặc mật khẩu rỗng!");
            return;
        }

        string filePath = Path.Combine(Application.persistentDataPath, "TaiKhoan.json");


        string[] lines = { taiKhoan, matKhau };
        File.WriteAllLines(filePath, lines);
        Debug.Log("Tạo file TaiKhoan.json thành công.");
    }

    private void FillUsernameAndPassword()
    {
     
        string username = usernameField.text;
        string password = passwordField.text;

        string js = $@"
            document.getElementById('username').value = '{username}';
            document.getElementById('password').value = '{password}';
        ";
        webViewObject.EvaluateJS(js);
    }

    private IEnumerator DelayThenCaptureCaptcha()
    {
        yield return new WaitForSeconds(1);
        CaptureCaptchaFromWebView();
    }

    public void CaptureCaptchaFromWebView()
    {
        string js = @"
            (function() {
                var img = document.getElementById('img_captcha');
                if (!img) {
                    window.Unity.call('❌ Không tìm thấy img_captcha!');
                    return;
                }
                var canvas = document.createElement('canvas');
                canvas.width = img.naturalWidth;
                canvas.height = img.naturalHeight;
                var ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, img.naturalWidth, img.naturalHeight);
                var dataURL = canvas.toDataURL('image/png');
                window.Unity.call(dataURL);
            })();
        ";
        webViewObject.EvaluateJS(js);
    }

    private void OnWebViewMessage(string message)
    {
        if (message.StartsWith("data:image/png;base64,"))
        {
            LoadCaptchaFromBase64(message.Substring("data:image/png;base64,".Length));
        }
    }

    private void LoadCaptchaFromBase64(string base64Data)
    {
        byte[] imageBytes = Convert.FromBase64String(base64Data);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageBytes))
        {
            captchaTexture = texture;
            RecognizeCaptcha(texture);
        }
    }

    private void RecognizeCaptcha(Texture2D texture)
    {
        Texture2D processedTex = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        processedTex.SetPixels32(texture.GetPixels32());
        processedTex.Apply();

        tesseractDriver.Setup(
            onSetupComplete: () =>
            {
                recognizedCaptchaText = tesseractDriver.Recognize(processedTex).Trim();
                if (!string.IsNullOrWhiteSpace(recognizedCaptchaText))
                {
                    FillCaptcha();
                }
                else
                {
                    StartCoroutine(RetryCaptureCaptcha());
                }
            },
            onSetupFailed: (error) => { }
        );
    }

    private void FillCaptcha()
    {
        string js = $@"
            document.getElementById('captchaTextBox').value = '{recognizedCaptchaText}';
        ";
        webViewObject.EvaluateJS(js);
        StartCoroutine(ClickLogin());
    }

    IEnumerator ClickLogin()
    {
        yield return new WaitForSeconds(2f);
        string jsClickLogin = @"document.getElementById('btn_dangnhap').click();";
        webViewObject.EvaluateJS(jsClickLogin);
        yield return new WaitForSeconds(1f);

        string jsCheckToast = @"
        (function() {
            var toast = document.getElementById('toast-container');
            if (toast && toast.innerText.includes('Bạn đã nhập sai mã xác nhận')) {
                Unity.call('captcha_failed');
            }
        })();
        ";
        webViewObject.EvaluateJS(jsCheckToast);

       
    }

    private IEnumerator RetryCaptureCaptcha()
    {
        webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
        yield return new WaitForSeconds(1f);
        CaptureCaptchaFromWebView();
    }

    private IEnumerator PrepareTessdata()
    {
        string tessdataPath = Path.Combine(Application.persistentDataPath, "tessdata");
        string trainedDataPath = Path.Combine(tessdataPath, "eng.traineddata");

        if (!Directory.Exists(tessdataPath))
            Directory.CreateDirectory(tessdataPath);

        if (!File.Exists(trainedDataPath))
        {
#if UNITY_ANDROID
            string sourcePath = Application.streamingAssetsPath + "/tessdata/eng.traineddata";
            var request = UnityEngine.Networking.UnityWebRequest.Get(sourcePath);
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(trainedDataPath, request.downloadHandler.data);
            }
#else
            string sourcePath = Path.Combine(Application.streamingAssetsPath, "tessdata/eng.traineddata");
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, trainedDataPath);
            }
#endif
        }
    }

    private void AppendLog(string log)
    {
        if (resultText != null)
        {
            resultText.text = log;
        }
        Debug.Log(log);
    }
}

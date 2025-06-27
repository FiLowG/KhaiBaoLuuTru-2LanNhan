using System;
using System.Collections;
using System.IO;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CaptchaAutoLogin : MonoBehaviour
{
    [Header("Account Settings")]
    private string username;
    private string password;

    [Header("UI Components")]
    public Text resultText;
    private string pathText;

    public WebViewObject webViewObject;
    private TesseractDriver tesseractDriver;
    private Texture2D captchaTexture;
    private string recognizedCaptchaText = "";
    private InputInfo inputInfo;
    private JsonHandler jsonHandler;
    private bool captchaFailed = false;
    public GameObject loading;
    private LichSuLuuTru lichsuluutru;
    public GameObject see_btn;
    public GameObject QuetThem;
    public GameObject HuyBo;


    private void Start()
    {
        tesseractDriver = new TesseractDriver();
        pathText = Application.persistentDataPath;
        inputInfo = FindObjectOfType<InputInfo>();
        lichsuluutru = FindObjectOfType<LichSuLuuTru>();

        jsonHandler = FindObjectOfType<JsonHandler>();
        username = jsonHandler.TakeTaiKhoan("tk");
        password = jsonHandler.TakeTaiKhoan("mk");
        StartCoroutine(PrepareTessdata());

        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (string message) => {
                if (message == "ModalClosed")
                {
                    modalClosed = true;
                    AppendLog("Đã đóng mẫu khai báo!");
                }
                if (message == "provinceReady")
                {
                    AppendLog("Đã điền thông tin doanh nghiệp!");
                    CallCountri();
                }
                else if (message == "reloadRequired")
                {
                    AppendLog("Tải lại trang lưu trú...");
                    webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/p/home/thong-bao-luu-tru.html");
                }
                else if (message == "captcha_failed")
                {
                    webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
                    captchaFailed = true;
                }
                else if (message.StartsWith("data:image/png;base64,"))
                {
                    OnWebViewMessage(message);
                }
            },

            err: (string error) => webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp"),
            ld: (string loadedUrl) => {
                if (loadedUrl.Contains("login.jsp"))
                {
                    FillUsernameAndPassword();
                    StartCoroutine(DelayThenCaptureCaptcha());
                    see_btn.SetActive(false);
                    webViewObject.SetVisibility(false);

                }
                if (loadedUrl.Contains("dvc-gioi-thieu.html"))
                {
                    AppendLog("Đã vào trang giới thiệu!");
                   
                    CallLuuTru();
                }
                if (loadedUrl.Contains("thong-bao-luu-tru.html"))
                {
                    
                    AppendLog("Đã vào trang khai báo!");

                    string jsCheckProvinceAndTrigger = @"
            setTimeout(function() {
                var province = document.getElementById('accomStay_cboPROVINCE_ID');
                if (province && province.value === '38') {
                    document.getElementById('btnAddPersonLT').click();
                    window.Unity.call('provinceReady');
                } else {
                    window.Unity.call('reloadRequired');
                }
            }, 1000);
            ";

                    webViewObject.EvaluateJS(jsCheckProvinceAndTrigger);
                    AppendLog("Kiểm tra thông tin doanh nghiệp...");
                }
            


                if (loadedUrl.Contains("authenticationendpoint"))
                {
                    webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
                }
            },
            enableWKWebView: true
        );

        webViewObject.SetMargins(0, 200, 0, 0);
        webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
        webViewObject.SetVisibility(false);
    }

    private void FillUsernameAndPassword()
    {
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
            onSetupFailed: (error) => {}
        );
    }

    private void FillCaptcha()
    {
        string js = $@"
            document.getElementById('captchaTextBox').value = '{recognizedCaptchaText}';        
        ";
        StartCoroutine(ClickLogin());
        webViewObject.EvaluateJS(js);
    }

    IEnumerator ClickLogin()
    {
        captchaFailed = false;
        yield return new WaitForSeconds(2f);
        string jsClickLogin = @"document.getElementById('btn_dangnhap').click();";
        webViewObject.EvaluateJS(jsClickLogin);
        AppendLog("Đang đăng nhập...");
        // Chờ chút rồi kiểm tra xem có thông báo lỗi captcha không
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
        yield return new WaitForSeconds(0.5f);

        if (captchaFailed)
            yield break;
        AppendLog("Đăng nhập thành công!");
        


    }

    // Hàm được gọi từ JavaScript khi phát hiện lỗi captcha
    public void OnCaptchaErrorDetected()
    {
        CaptureCaptchaFromWebView();
    }

    public void Cancel()
    {
        SceneManager.LoadScene("QRScan");
    }

    public void CallInfo(string fullname, string IDnumber, string dob, string sex, string days)
    {   if (days == "Trong")
        {
            days = "0";
        }
        string tomorrow = System.DateTime.Now.AddDays(int.Parse(days)).ToString("dd/MM/yyyy");
        inputInfo.NameBirthIDReasonToDay(fullname, IDnumber, dob, "TẠM TRÚ", tomorrow);
        inputInfo.Gender(sex);
    }


    public void CallPRV(string city)
    {
        inputInfo.Province(city);
    }
    public void CallDistrcit(string district)
    {
        inputInfo.District(district);
    }
    public void CallWard(string ward)
    {
       StartCoroutine(Callwards(ward));
    }

    private bool modalClosed = false; // Cờ đánh dấu khi modal đã đóng

    IEnumerator Callwards(string wards)
    {
        inputInfo.Ward(wards);
        AppendLog("Đang chọn phường/xã...");

        modalClosed = false; // Reset cờ trước khi bắt đầu

        yield return new WaitForSeconds(1f);

       
            string js = @"
    (function waitForWardSelection() {
        var checkValue = function() {
            var select = document.getElementById('guest_cboRDADDRESS_ID');
            if (select && select.value !== '-1') {
                document.getElementById('btnSaveNLT').click();
                waitForModalClose();
            } else {
                setTimeout(checkValue, 500);
            }
        };

        var waitForModalClose = function() {
            var modal = document.getElementById('addpersonLT');
            if (modal && window.getComputedStyle(modal).display === 'none') {
                Unity.call('ModalClosed');
            } else {
                setTimeout(waitForModalClose, 500);
            }
        };

        checkValue();
    })();
    ";
            webViewObject.EvaluateJS(js);
        

        AppendLog("Đã lưu phường/xã.");
    }

    public void CallNat()
    {
        inputInfo.National();
    }
    public void CallLuuTru()
    {
        see_btn.SetActive(true);
        webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/p/home/thong-bao-luu-tru.html");
    }
    public void CallCountri()
    {
        StartCoroutine(CallCounty());
    }
    IEnumerator CallCounty()
    {
        yield return new WaitForSeconds(2f);
        string filePath = Path.Combine(Application.persistentDataPath, "info.json");
        string json = File.ReadAllText(filePath);

        string name = "";
        string sex = "";
        string dob = "";
        string cccd = "";
        string city = "";
        string district = "";
        string ward = "";
        string days = "";
        // Chuyển JSON thành đối tượng QRData
        QRData qrData = JsonUtility.FromJson<QRData>(json);

        // Gán giá trị từ QRData vào các tham số
        name = qrData.name;
        dob = qrData.dob;
        sex = qrData.sex;
        cccd = qrData.cccd;
        city = qrData.city;
        district = qrData.district;
        ward = qrData.ward;
        days = qrData.days;
        CallInfo(name, cccd, dob, sex, days);

        AppendLog("Đã thêm thông tin cơ bản!");

        yield return new WaitForSeconds(2);
        CallNat();
        AppendLog("Đang thêm quốc gia...");
        yield return new WaitForSeconds(2f);
        CallPRV(city);
        AppendLog("Đang thêm tỉnh thành...");

        yield return new WaitForSeconds(2f);
        CallDistrcit(district);
        AppendLog("Đang thêm quận huyện...");

        yield return new WaitForSeconds(2f);
        CallWard(ward);
        AppendLog("Đang thêm xã phường...");
        yield return new WaitUntil(() => modalClosed);
      
        yield return new WaitForSeconds(2f);
        string jss = @"
        var checkbox = document.getElementById('chkCHECK_LIABILITY');
        if (checkbox && !checkbox.checked) {
        checkbox.checked = true;
        checkbox.dispatchEvent(new Event('change')); // gọi sự kiện nếu có lắng nghe
        }
        ";
        webViewObject.EvaluateJS(jss);
        yield return new WaitForSeconds(0.5f);

        if (name != "Bùi Phi Long" && name != "Mai Thị Chúc" && name != "Test Name")
        {
            string jsss = $@"
            document.getElementById('btnSaveSend').click();
            ";
            webViewObject.EvaluateJS(jsss);
            yield return new WaitForSeconds(1f);
            AppendLog("Thêm thông tin thành công!");
            QuetThem.SetActive(true);
            HuyBo.SetActive(false);
            loading.SetActive(false);
            lichsuluutru.AddEntryToJson(name, sex, cccd, dob, city, district, ward, days);
            yield return new WaitForSeconds(1f);
            webViewObject.SetVisibility(false);
            StartCoroutine(DemNguoc());
        }

        else
        {
            AppendLog("Thêm thông tin thành công!");
            QuetThem.SetActive(true);
            HuyBo.SetActive(false);
            loading.SetActive(false);
            webViewObject.SetVisibility(false);
            yield return new WaitForSeconds(1f);
            AppendLog("Bạn có thể thoát ứng dụng!");
            lichsuluutru.AddEntryToJson(name, sex, cccd, dob, city, district, ward, days);
        }
    }
    public void CloseApp()
    {
        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                                     .GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call<bool>("moveTaskToBack", true);  // Đưa app ra nền
        activity.Call("finish");                      // Kết thúc activity
        System.Diagnostics.Process.GetCurrentProcess().Kill(); // Đóng hẳn tiến trình
    }
    int time = 10;
    IEnumerator DemNguoc()
    { if (time > 0)
        {
            AppendLog($"Ứng dụng sẽ tự động thoát sau {time}s!");
            yield return new WaitForSeconds(1);
            time--;
            StartCoroutine(DemNguoc());
        }

        if (time == 0) 
        {
            AppendLog($"Ứng dụng sẽ tự động thoát sau 0s!");
            CloseApp();
        }
    }
    public void CallLogin()
    {
        webViewObject.LoadURL("https://dichvucong.dancuquocgia.gov.vn/portal/login.jsp");
    }
    private IEnumerator RetryCaptureCaptcha()
    {
        CallLogin();
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
            string sourcePath = "";

#if UNITY_ANDROID
            sourcePath = Application.streamingAssetsPath + "/tessdata/eng.traineddata";
            var request = UnityEngine.Networking.UnityWebRequest.Get(sourcePath);
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(trainedDataPath, request.downloadHandler.data);
            }
          
#else
            sourcePath = Path.Combine(Application.streamingAssetsPath, "tessdata/eng.traineddata");
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, trainedDataPath);
            }
           
#endif
        }
    }
    private bool isWebViewVisible = false;

    private void ShowWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(true);
            isWebViewVisible = true;
            AppendLog("Quá trình được hiển thị.");
        }
    }

    private void HideWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
            isWebViewVisible = false;
            AppendLog("Quá trình được ẩn.");
        }
    }
    public void ToggleWebView()
    {
        if (isWebViewVisible)
            HideWebView();
        else
            ShowWebView();
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

using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class QRData
{
    public string name;
    public string sex;
    public string dob;
    public string cccd;
    public string city;
    public string district;
    public string ward;
    public string days;
}

public class JsonHandler : MonoBehaviour
{
    public static JsonHandler Instance;

    public GameObject All_Scan;
    public GameObject Sure;

    private string gname;
    private string gender;
    private string cccd;
    private string dobFormatted;
    private string city;
    private string district;
    private string ward;
    public Text Ten;
    public Text NgaySinh;
    public Text GioiTinh;
    public Text CCCD;
    public Text City;
    public Text District;
    public Text Ward;
    public Text Days;
    public InputField inputTaiKhoan;
    public InputField inputMatKhau;
    private QRScanner qrscan;
    public GameObject setingsLogin;
    public GameObject setingsChangeNHis;

    private QRData currentQRData; // Lưu QR tạm, ghi file sau

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        qrscan = FindObjectOfType<QRScanner>();
        string filePath = Path.Combine(Application.persistentDataPath, "TaiKhoan.json");
        if (!File.Exists(filePath))
        {
            setingsLogin.SetActive(true);
        }
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)
            {
                setingsLogin.SetActive(true);
            }
        }
    }

   
    public void TaoTaiKhoan()
    {
        string taiKhoan = inputTaiKhoan.text.Trim();
        string matKhau = inputMatKhau.text.Trim();

        if (string.IsNullOrEmpty(taiKhoan) || string.IsNullOrEmpty(matKhau))
        {
            Debug.LogError("Tài khoản hoặc mật khẩu rỗng!");
            return;
        }

        string filePath = Path.Combine(Application.persistentDataPath, "TaiKhoan.json");

       
            string[] lines = { taiKhoan, matKhau };
            File.WriteAllLines(filePath, lines);
            Debug.Log("Tạo file TaiKhoan.json thành công.");
            setingsLogin.SetActive(false);
    }

    // Hàm lấy tài khoản hoặc mật khẩu
    public string TakeTaiKhoan(string type)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "TaiKhoan.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError("File TaiKhoan.json không tồn tại.");
            return "";
        }

        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
        {
            Debug.LogError("File TaiKhoan.json không hợp lệ.");
            return "";
        }

        if (type.ToLower() == "tk")
            return lines[0];
        else if (type.ToLower() == "mk")
            return lines[1];
        else
            return "";
    }
    public void ParseQRData(string qrData)
    {
        string[] parts = qrData.Split('|');
        if (parts.Length < 7)
        {
            Debug.LogError("QR format invalid!");
            return;
        }

        cccd = parts[0];
        gname = parts[2];
        string dobRaw = parts[3];
        gender = parts[4];
        string addressRaw = parts[5];

        dobFormatted = $"{dobRaw.Substring(0, 2)}/{dobRaw.Substring(2, 2)}/{dobRaw.Substring(4, 4)}";

        string[] addressParts = addressRaw.Split(',');
        ward = addressParts.Length >= 3 ? addressParts[addressParts.Length - 3].Trim() : "";
        district = addressParts.Length >= 2 ? addressParts[addressParts.Length - 2].Trim() : "";
        city = addressParts.Length >= 1 ? addressParts[addressParts.Length - 1].Trim() : "";

        currentQRData = new QRData()
        {
            name = gname,
            sex = gender,
            dob = dobFormatted,
            cccd = cccd,
            city = city,
            district = district,
            ward = ward
        };
       
        Ten.text = gname;
        NgaySinh.text = dobFormatted;
        GioiTinh.text = gender;
        CCCD.text = cccd;
        City.text = city;
        District.text = district;
        Ward.text = ward;

        All_Scan.SetActive(false);
        Sure.SetActive(true);
    }

    public void Plus()
    {
        if (Days.text == "Trong")
        {
            Days.text = "1";
            return;
        }
        int days = int.Parse(Days.text);
        ++days;
        Days.text = days.ToString();
    }
    public void Minus()
    {
        if (Days.text != "Trong")
        {
            int days = int.Parse(Days.text);

            if (days == 1)
            {
                Days.text = "Trong";
                return;
            }

            if (days > 0)
            {
                --days;
            }

            Days.text = days.ToString();
        }
    }
    public void OnSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "TaiKhoan.json");
        if (!File.Exists(filePath))
        {
            setingsLogin.SetActive(true);
        }

        else if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)
            {
                setingsLogin.SetActive(true);
            }
            
        }
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length >= 2)
            {
                setingsChangeNHis.SetActive(true);
            }
        }

    }
    public void DaDung()
    {
        if (currentQRData == null)
        {
            Debug.LogError("Không có dữ liệu QR để lưu.");
            return;
        }

        // Cập nhật ngày và lưu file
        currentQRData.days = Days.text;
        string json = JsonUtility.ToJson(currentQRData);
        string filePath = Path.Combine(Application.persistentDataPath, "info.json");
        File.WriteAllText(filePath, json);
        Debug.Log("Đã ghi file info.json sau khi xác nhận.");

        string message = $"Có 1 khách nghỉ mới được khai báo:\n" +
                         $"Họ và Tên: {currentQRData.name}\n" +
                         $"Giới tính: {currentQRData.sex}\n" +
                         $"Ngày sinh: {currentQRData.dob}\n" +
                         $"Số CCCD: {currentQRData.cccd}\n" +
                         $"Tỉnh/Thành phố: {currentQRData.city}\n" +
                         $"Quận/Huyện: {currentQRData.district}\n" +
                         $"Xã/Phường: {currentQRData.ward}\n" +
                         $"Ngày lưu trú: {currentQRData.days}";

       SceneManager.LoadScene("Main");
    }
    public void QuetLai()
    {
        gname = "";
        gender = "";
        dobFormatted = "";
        cccd = "";
        city = "";
        district = "";
        ward = "";

        Sure.SetActive(false);
        All_Scan.SetActive(true);
        qrscan.StartCamera();

        // Xoá file cũ nếu cần
        string filePath = Path.Combine(Application.persistentDataPath, "info.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Đã xoá file info.json cũ.");
        }
    }


}

/*using UnityEngine;
using System.IO;

public class InfoReader : MonoBehaviour
{

    void Start()
    {

    }
    public bool ReadQRData()
    {
        // Khởi tạo giá trị mặc định cho các tham số
        string name = "";
        string sex = "";
        string dob = "";
        string cccd = "";
        string city = "";
        string district = "";
        string ward = "";

        // Đường dẫn file info.json
        string filePath = Path.Combine(Application.persistentDataPath, "info.json");

        // Kiểm tra file có tồn tại không
        if (!File.Exists(filePath))
        {
            Debug.LogError("File info.json không tồn tại!");
            return false;
        }

        try
        {
            // Đọc nội dung file
            string json = File.ReadAllText(filePath);

            // Kiểm tra xem file có dữ liệu trống hoặc không hợp lệ không
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("File info.json trống!");
                return false;
            }

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

            Debug.Log($"Line 1 (Tên): {name}");
            Debug.Log($"Line 2 (Ngày sinh): {dob}");
            Debug.Log($"Line 3 (CCCD): {cccd}");
            Debug.Log($"Line 4 (Tỉnh/Thành phố): {city}");
            Debug.Log($"Line 5 (Huyện/Quận): {district}");
            Debug.Log($"Line 6 (Xã/Phường): {ward}");
            Debug.Log($"JSON Output: {json}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi đọc file info.json: {e.Message}");
            return false;
        }
    }

}

*/
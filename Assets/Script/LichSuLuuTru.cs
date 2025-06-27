using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class LuuTruEntry
{
    public string name;
    public string gender;
    public string cccd;
    public string dob;
    public string city;
    public string district;
    public string ward;
    public string days;
    public string dayStart;
    public string timeStart;
}

[System.Serializable]
public class LichSuLuuTruData
{
    public List<LuuTruEntry> entries = new List<LuuTruEntry>();
}

public class LichSuLuuTru : MonoBehaviour
{
    public GameObject prefabBox;
    public Transform contentParent;
    public GameObject warn;

    private string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "LichSuLuuTru.json");
    }

    void OnEnable()
    {

        if (SceneManager.GetActiveScene().name == "QRScan")
        {
            LoadLichSuToScrollView();
        }
    }
    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "LichSuLuuTru.json");

    }

    public void AddEntryToJson(string name, string gender, string cccd, string dob, string city, string district, string ward, string days)
    {
        LichSuLuuTruData data;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<LichSuLuuTruData>(json);
        }
        else
        {
            data = new LichSuLuuTruData();
        }

        string now = DateTime.Now.ToString("dd/MM/yyyy|HH:mm:ss");
        string[] dateTimeParts = now.Split('|');

        LuuTruEntry entry = new LuuTruEntry()
        {
            name = name,
            gender = gender,
            cccd = cccd,
            dob = dob,
            city = city,
            district = district,
            ward = ward,
            days = days,
            dayStart = dateTimeParts[0],
            timeStart = dateTimeParts[1]
        };

        data.entries.Insert(0, entry); // mới nhất lên đầu

        string updatedJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, updatedJson);
        Debug.Log("Đã tạo người lưu trú");
    }

    public void LoadLichSuToScrollView()
    {
        // Dọn cũ
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (!File.Exists(filePath))
        {
            warn.SetActive(true);
            return;
        }
        Debug.Log("Da Onabled");

        string json = File.ReadAllText(filePath);
        LichSuLuuTruData data = JsonUtility.FromJson<LichSuLuuTruData>(json);

        if (data.entries.Count == 0)
        {
            warn.SetActive(true);
            return;
        }

        warn.SetActive(false);

        foreach (LuuTruEntry entry in data.entries)
        {
            GameObject box = Instantiate(prefabBox, contentParent);

            box.transform.Find("name").GetComponent<Text>().text = entry.name;
            box.transform.Find("gender").GetComponent<Text>().text = entry.gender;
            box.transform.Find("cccd").GetComponent<Text>().text = entry.cccd;
            box.transform.Find("dob").GetComponent<Text>().text = entry.dob;
            box.transform.Find("city").GetComponent<Text>().text = entry.city;
            box.transform.Find("district").GetComponent<Text>().text = entry.district;
            box.transform.Find("ward").GetComponent<Text>().text = entry.ward;
            box.transform.Find("days").GetComponent<Text>().text = entry.days + " ngày";
            box.transform.Find("dayStart").GetComponent<Text>().text = entry.dayStart;
            box.transform.Find("timeStart").GetComponent<Text>().text = entry.timeStart;
        }
        

    }
}

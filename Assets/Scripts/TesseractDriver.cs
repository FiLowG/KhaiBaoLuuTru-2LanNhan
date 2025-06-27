using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class TesseractDriver
{
    private TesseractWrapper _tesseract;
    private static readonly List<string> fileNames = new List<string> { "tessdata.tgz" };

    public string CheckTessVersion()
    {
        _tesseract = new TesseractWrapper();

        try
        {
            string version = "Tesseract version: " + _tesseract.Version();
            Debug.Log(version);
            return version;
        }
        catch (Exception e)
        {
            string errorMessage = e.GetType() + " - " + e.Message;
            Debug.LogError("Tesseract version: " + errorMessage);
            return errorMessage;
        }
    }

    // --- SỬA LẠI Ở ĐÂY: Setup nhận 2 callback ---
    public void Setup(UnityAction onSetupComplete, UnityAction<string> onSetupFailed = null)
    {
#if UNITY_EDITOR || UNITY_ANDROID
        OcrSetup(onSetupComplete, onSetupFailed);
#else
        OcrSetup(onSetupComplete, onSetupFailed);
#endif
    }

    // --- SỬA LẠI OcrSetup ---
    public void OcrSetup(UnityAction onSetupComplete, UnityAction<string> onSetupFailed = null)
    {
        _tesseract = new TesseractWrapper();

#if UNITY_EDITOR
        string datapath = Path.Combine(Application.persistentDataPath, "tessdata");
#elif UNITY_ANDROID
        string datapath = Application.persistentDataPath + "/tessdata/";
#else
        string datapath = Path.Combine(Application.streamingAssetsPath, "tessdata");
#endif

        if (_tesseract.Init("eng", datapath))
        {
            Debug.Log("Init Successful");
            onSetupComplete?.Invoke();
        }
        else
        {
            string errorMsg = _tesseract.GetErrorMessage();
            Debug.LogError(errorMsg);
            onSetupFailed?.Invoke(errorMsg);   // --> GỌI lỗi
        }
    }

    private async void CopyAllFilesToPersistentData(List<string> fileNames, UnityAction onSetupComplete)
    {
        String fromPath = "jar:file://" + Application.dataPath + "!/assets/";
        String toPath = Application.persistentDataPath + "/";

        foreach (String fileName in fileNames)
        {
            if (!File.Exists(toPath + fileName))
            {
                Debug.Log("Copying from " + fromPath + fileName + " to " + toPath);
                WWW www = new WWW(fromPath + fileName);

                while (!www.isDone)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));
                }

                File.WriteAllBytes(toPath + fileName, www.bytes);
                Debug.Log("File copy done");
                www.Dispose();
                www = null;
            }
            else
            {
                Debug.Log("File exists! " + toPath + fileName);
            }

            UnZipData(fileName);
        }

        OcrSetup(onSetupComplete); // chỗ này không cần sửa vì đã đúng
    }

    public string GetErrorMessage()
    {
        return _tesseract?.GetErrorMessage();
    }

    public string Recognize(Texture2D imageToRecognize)
    {
        return _tesseract.Recognize(imageToRecognize);
    }

    public Texture2D GetHighlightedTexture()
    {
        return _tesseract.GetHighlightedTexture();
    }

    private void UnZipData(string fileName)
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName))
        {
            UnZipUtil.ExtractTGZ(Application.persistentDataPath + "/" + fileName, Application.persistentDataPath);
            Debug.Log("UnZipping Done");
        }
        else
        {
            Debug.LogError(fileName + " not found!");
        }
    }
}

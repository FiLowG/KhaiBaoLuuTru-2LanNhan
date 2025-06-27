using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BeTrans : MonoBehaviour
{
    private RawImage img;

    void Start()
    {
        img = GetComponent<RawImage>();
        if (img != null)
        {
            StartCoroutine(FadeOutAndDisable());
        }
        else
        {
            Debug.LogWarning("BeTrans: Không tìm thấy component RawImage trên GameObject này.");
        }
    }

    IEnumerator FadeOutAndDisable()
    {
        // Chờ 2 giây
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("QRScan");
    }
}

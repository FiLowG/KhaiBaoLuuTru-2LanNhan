using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputInfo : MonoBehaviour
{
    private CaptchaAutoLogin captchaLogin;

    void Start()
    {
        captchaLogin = FindObjectOfType<CaptchaAutoLogin>();
    }

    // Hàm dùng chung cho tất cả các Select (province, district, ward, gender, nation)
    private void SetSelectValue(string elementId, string searchText)
    {
        string js = @"
            function getValueFromOption(optionText, elementId) {
                var selectElement = document.getElementById(elementId); 
                var options = selectElement.getElementsByTagName('option');
                
                for (var i = 0; i < options.length; i++) {
                    if (options[i].text.trim().includes(optionText.trim())) {
                        return options[i].value;
                    }
                }
                return null;
            }

            var value = getValueFromOption('" + searchText + @"', '" + elementId + @"');

            if (value) {
                var selectElement = document.getElementById('" + elementId + @"');
                selectElement.value = value;
                $('#" + elementId + @"').val(value).trigger('change');
            }
        ";

        captchaLogin.webViewObject.EvaluateJS(js);
    }
  
    public void National()
    {
        SetSelectValue("guest_mulNATIONALITY", "Việt Nam");
    }

    public void Province(string province)
    {
        SetSelectValue("guest_cboRDPROVINCE_ID", province);
    }

    public void District(string district)
    {
        SetSelectValue("guest_cboRDDISTRICT_ID", district);
    }

    public void Ward(string ward)
    {
        SetSelectValue("guest_cboRDADDRESS_ID", ward);
    }

    public void Gender(string gender)
    {
        SetSelectValue("guest_cboGENDER_ID", gender);
    }
    

    // Hàm điền thông tin vào các input text
    public void NameBirthIDReasonToDay(string fullName, string idNumber, string birthDay, string reason, string toDay)
    {
        string js = $@"
            document.getElementById('guest_txtCITIZENNAME').value = '{fullName}';
            document.getElementById('guest_txtDOB').value = '{birthDay}';
            document.getElementById('guest_txtREASON').value = '{reason}';
            document.getElementById('guest_txtEND_DATE').value = '{toDay}';
            document.getElementById('guest_txtIDCARD_NUMBER').value = '{idNumber}';
        ";

        captchaLogin.webViewObject.EvaluateJS(js);
    }
}

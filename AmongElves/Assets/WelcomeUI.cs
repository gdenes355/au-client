using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeUI : MonoBehaviour
{
    private InputField nameField;
    private InputField codeField;

    public void JoinButtonClicked()
    {
        if (nameField.text == "")
        {
            return;
        }
        INetworkManager.sActiveInstace.JoinGame(nameField.text, int.Parse(codeField.text));
    }

    private void Start()
    {
        nameField = transform.Find("NameInput").GetComponent<InputField>();
        codeField = transform.Find("CodeInput").GetComponent<InputField>();
    }
}

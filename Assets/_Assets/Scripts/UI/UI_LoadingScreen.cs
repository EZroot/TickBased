using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Logger = TickBased.Logger.Logger;

public class UI_LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject _loadingScreenCanvasObject;
    [SerializeField] private TMP_Text _loadingScreenText;

    public void ShowLoadingScreen(string text)
    {
        Logger.Log("Trying to show loading screen..","UI_LoadingScreen");
        _loadingScreenCanvasObject.SetActive(true);
        _loadingScreenText.text = text;
    }

    public void SetLoadingScreenText(string text)
    {
        _loadingScreenText.text = text;
    }
    public void HideLoadingScreen()
    {
        Logger.Log("Hiding load screen","UI_LoadingScreen");

        _loadingScreenCanvasObject.SetActive(false);
    }
}

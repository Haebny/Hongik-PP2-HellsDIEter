﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public Sprite[] buttons;    // 버튼 스프라이트 저장
    public GameObject window;
    bool isOpened = false;          // 인벤토리 창이 열려있는지
    static bool isPaused = false;
    bool isListening = true;

    private void Start()
    {
        if (PlayerPrefs.GetInt("Sound") == 0)
            SoundControll();
    }

    // 인벤토리 창 열기/닫기
    public void Inventory()
    {
        if (isOpened == true)
        {
            RectTransform inventory = GameObject.Find("Inventory").GetComponent<RectTransform>();
            inventory.pivot = new Vector2(0.5f, 0.85f);
            inventory.position = Vector3.Lerp(inventory.position, new Vector3(inventory.position.x, 0.0f), 3.0f);

            this.GetComponent<Image>().sprite = buttons[0];
            isOpened = false;
        }
        else
        {
            RectTransform inventory = GameObject.Find("Inventory").GetComponent<RectTransform>();
            inventory.pivot = new Vector2(0.5f, 0.0f);
            inventory.position = Vector3.Lerp(inventory.position, new Vector3(inventory.position.x, 0.0f), 3.0f);

            this.GetComponent<Image>().sprite = buttons[1];
            isOpened = true;
        }

        return;
    }

    // 메뉴를을 켜고 끄는 메소드
    public void MenuPopUp()
    {
        ButtonController.isPaused = !ButtonController.isPaused;

        // 일시정지 및 버튼 스프라이트 변경
        if (ButtonController.isPaused)
        {
            Time.timeScale = 0.0f;
            this.GetComponent<Image>().sprite = Resources.Load("UI/Sprite/Buttons/button_play", typeof(Sprite)) as Sprite;
        }
        else
        {
            Time.timeScale = 1.0f;
            this.GetComponent<Image>().sprite = Resources.Load("UI/Sprite/Buttons/button_pause", typeof(Sprite)) as Sprite;
        }

        // 메뉴 팝업 띄우기
        window = GameObject.Find("UI").transform.Find("Menu").gameObject;
        window.gameObject.SetActive(!window.activeSelf);
        window = null;
    }

    // 도움말을 켜고 끄는 메소드
    public void HelpPopUp()
    {
        ButtonController.isPaused = !ButtonController.isPaused;

        // 일시정지
        if (ButtonController.isPaused)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }

        // 메뉴 팝업 띄우기
        window = GameObject.Find("UI").transform.Find("HowTo").gameObject;
        window.gameObject.SetActive(!window.activeSelf);
        window = null;
    }

    public void QuitGame()
    {
        JsonManager.Instance.Save();
        Application.Quit();
    }

    public void GoTitle()
    {
        Time.timeScale = 1.0f;
        SceneLoader.Instance.LoadScene("0.Title");
    }

    public void SoundControll()
    {
        AudioSource[] soundObjects = GameObject.FindObjectsOfType<AudioSource>();

        if (soundObjects == null)
            return;

        foreach (var item in soundObjects)
        {
            item.mute = isListening;
            Debug.Log(item.name + ":" + item.mute);
        }

        if (this.transform.name == "SoundButton")
        {
            isListening = !isListening;

            if (isListening == false)
            {
                this.GetComponent<Image>().sprite = Resources.Load("UI/Sprite/Buttons/button_soundOFF", typeof(Sprite)) as Sprite;
                this.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "꺼짐";
                PlayerPrefs.SetInt("Sound", 0);
            }

            else
            {
                this.GetComponent<Image>().sprite = Resources.Load("UI/Sprite/Buttons/button_soundON", typeof(Sprite)) as Sprite;
                this.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "켜짐";
                PlayerPrefs.SetInt("Sound", 1);
            }
        }
    }

    public void LoadNextScene()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        string path = SceneUtility.GetScenePathByBuildIndex(index + 1);
        string name = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
        SceneLoader.Instance.LoadScene(name);
    }

    public void LoadGame()
    {
        if ((PlayerPrefs.GetInt("Tutorial") == 0))
            return;
        Time.timeScale = 1.0f;
        SceneLoader.Instance.LoadScene("2.Main");
    }
}

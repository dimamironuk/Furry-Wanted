using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio UI")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Graphics UI")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;

    [Header("Language UI")]
    [SerializeField] private TMP_Dropdown languageDropdown; 

    private void Start()
    {
        LoadSettings();
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetVSync(bool isVSync)
    {
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isVSync ? 1 : 0);
    }

    public void SetLanguage(int index)
    {
        PlayerPrefs.SetInt("Language", index);

        Debug.Log("Мову змінено на: " + (index == 0 ? "Українська" : "English"));
    }

    private void LoadSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVol;
            AudioListener.volume = masterVol;
        }

        bool isFull = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFull;
            Screen.fullScreen = isFull;
        }

        bool isVSync = PlayerPrefs.GetInt("VSync", 1) == 1;
        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = isVSync;
            QualitySettings.vSyncCount = isVSync ? 1 : 0;
        }

        int langIndex = PlayerPrefs.GetInt("Language", 0);
        if (languageDropdown != null)
        {
            languageDropdown.value = langIndex;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;
    
    public TMP_Text carNameText;
    public GameObject[] previewCars;
    private int selectedCarIndex;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowNextCar()
    {
        previewCars[selectedCarIndex].SetActive(false);
        selectedCarIndex = (selectedCarIndex + 1) % previewCars.Length;
        previewCars[selectedCarIndex].SetActive(true);
        
        carNameText.text = previewCars[selectedCarIndex].name;
    }

    public void ShowPreviousCar()
    {
        previewCars[selectedCarIndex].SetActive(false);
        selectedCarIndex = (selectedCarIndex - 1 + previewCars.Length) % previewCars.Length;
        previewCars[selectedCarIndex].SetActive(true);
        
        carNameText.text = previewCars[selectedCarIndex].name;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("InGame");
        
        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
    }
    
    public void Retry()
    {
        SceneManager.LoadScene("InGame");
    }

    public void Exit()
    {
        Application.Quit();
    }
}

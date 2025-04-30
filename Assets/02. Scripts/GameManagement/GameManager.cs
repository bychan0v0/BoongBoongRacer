using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    private bool isPaused = false;

    void Start()
    {
        // 시작할 때 패널 비활성화
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // ESC 키 입력 체크
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // 게임 일시정지
    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;    // 시간 멈춤
        isPaused = true;
    }

    // 게임 재개
    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;    // 시간 정상화
        isPaused = false;
    }

    // 다시 시작(현재 씬 리로드)
    public void RetryGame()
    {
        Time.timeScale = 1f;    // 리로드 전에 시간 스케일 복구
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

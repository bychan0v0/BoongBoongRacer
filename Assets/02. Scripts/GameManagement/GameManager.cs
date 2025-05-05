using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Spawn Settings")]
    public Transform[] spawnPoints; // 차량 스폰 위치
    public GameObject[] carPrefabs; // 차량 프리팹들
    public Transform[] waypoints;   // Waypoint 목록 (씬 내 오브젝트)
    public GameObject[] markers;
    
    [Header("UI")]
    public Material playerMat;
    public GameObject pausePanel;

    public int selectedCarIndex;
    
    private List<GameObject> allCars = new List<GameObject>();
    
    private bool isPaused = false;

    private void Awake()
    {
        Instance = this;
        
        selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
    }

    private void Start()
    {
        for (int i = 0; i < carPrefabs.Length; i++)
        {
            GameObject car = Instantiate(carPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
            
            allCars.Add(car);
            markers[i].GetComponent<MarkerController>().SetTarget(car.transform);
            
            if (i == selectedCarIndex)
            {
                car.name = "BoongBoong";
                
                // 플레이어 차량
                var playerInput = car.GetComponent<PlayerInput>();
                var playerProgressTracker = car.AddComponent<PlayerProgressTracker>();
                var myLapCounter = car.AddComponent<MyLapCounter>();
                var rigidBody = car.GetComponent<Rigidbody>();

                playerInput.enabled = true;
                
                playerProgressTracker.SetSplinePoints(waypoints);
                playerProgressTracker.playerTransform = car.transform;
                playerProgressTracker.myLapCounter = myLapCounter;
                myLapCounter.maxLap = 3;
                
                markers[i].GetComponent<MeshRenderer>().material = playerMat;
                
                CameraController.Instance.SetTarget(car.transform);
                GaugeManager.Instance.SetTarget(car);
                LapManager.Instance.myLapCounter = myLapCounter;
                
                var wheelColliders = car.GetComponentsInChildren<WheelCollider>();

                foreach (var wc in wheelColliders)
                {
                    if (wc.name == "FrontLeftWheel" || wc.name == "FrontRightWheel")
                    {
                        // 전방 마찰력 조정
                        WheelFrictionCurve forward = wc.forwardFriction;
                        forward.stiffness = 2.5f; // ← 원하는 값으로
                        wc.forwardFriction = forward;

                        // 측면 마찰력 조정
                        WheelFrictionCurve sideways = wc.sidewaysFriction;
                        sideways.stiffness = 3.0f; // ← 원하는 값으로
                        wc.sidewaysFriction = sideways;
                    }
                }
            }
            else
            {
                car.name = carPrefabs[i].name;
                
                // AI 차량
                var aiPlayerInput = car.GetComponent<AIPlayerInput>();
                var aiProgressTracker = car.AddComponent<AIProgressTracker>();
                var lapCounter = car.AddComponent<LapCounter>();

                aiPlayerInput.enabled = true;

                aiProgressTracker.aiDriver = aiPlayerInput;
                aiProgressTracker.lapCounter = lapCounter;
                lapCounter.maxLap = 3;
                
                aiPlayerInput.SetSplinePoints(waypoints);
            }
        }
        
        RankingManager.Instance.SetCars(allCars);
        
        // 시작할 때 패널 비활성화
        pausePanel.SetActive(false);
    }

    private void Update()
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

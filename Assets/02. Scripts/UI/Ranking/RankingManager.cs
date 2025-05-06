using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;
    
    public List<GameObject> allCars = new List<GameObject>();
    public TMP_Text myPositionNum;
    public TMP_Text playerNum;
    
    public TMP_Text playerNameText;
    public TMP_Text playerRankText;
    
    public TMP_Text[] rankNums;
    public TMP_Text[] nameTexts;
    public GameObject[] rankUIObjects;
    
    private List<IProgressProvider> progressProviders = new List<IProgressProvider>();

    private void Awake()
    {
        Instance = this;
        progressProviders.Clear();
    }

    private void Update()
    {
        if (allCars == null || allCars.Count < 5) return;

        var sorted = GetSortedRanking();
        GameObject playerCar = allCars[GameManager.Instance.selectedCarIndex];
        IProgressProvider playerProvider = playerCar.GetComponent<IProgressProvider>();

        int myRank = sorted.IndexOf(playerProvider);
        myPositionNum.text = (myRank + 1).ToString();

        if (playerRankText != null) playerRankText.text = (myRank + 1).ToString();
        if (playerNameText != null) playerNameText.text = playerCar.name;

        int uiSlot = 0; // AI UI 슬롯 인덱스

        for (int i = 0; i < sorted.Count; i++)
        {
            IProgressProvider provider = sorted[i];
            GameObject car = allCars.FirstOrDefault(c => c.GetComponent<IProgressProvider>() == provider);
            if (car == null || car == playerCar) continue; // 플레이어는 제외

            if (uiSlot < rankUIObjects.Length)
            {
                rankUIObjects[uiSlot].transform.SetSiblingIndex(i);

                rankNums[uiSlot].text = (i + 1).ToString(); // 전체 순위 기준
                nameTexts[uiSlot].text = car.name;

                uiSlot++;
            }
        }
    }
    
    public void SetCars(List<GameObject> cars)
    {
        allCars = cars;
        progressProviders.Clear();

        foreach (GameObject car in allCars)
        {
            var provider = car.GetComponent<IProgressProvider>();
            if (provider != null)
            {
                progressProviders.Add(provider);
            }
        }
    }
    
    public List<IProgressProvider> GetSortedRanking()
    {
        return progressProviders
            .OrderByDescending(p => p.GetLap()) // 현재 랩 수
            .ThenByDescending(p => p.GetPreciseProgress()) // 현재 경로상의 위치 (스플라인 기반)
            .ToList();
    }

    public int GetRankOf(GameObject car)
    {
        var sorted = GetSortedRanking();
        IProgressProvider provider = car.GetComponent<IProgressProvider>();
        
        if (provider == null) return -1;
        return sorted.IndexOf(provider) + 1;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class RankingManager : MonoBehaviour
{
    public GameObject[] allCars;
    public TMP_Text myPositionNum;
    public TMP_Text playerNum;
    public TMP_Text[] rankNums;
    public TMP_Text[] nameTexts;
    public GameObject[] rankUIObjects;
    
    private List<IProgressProvider> progressProviders = new List<IProgressProvider>();

    private void Start()
    {
        foreach (GameObject car in allCars)
        {
            IProgressProvider provider = car.GetComponent<IProgressProvider>();
            if (provider != null)
                progressProviders.Add(provider);
        }
        
        playerNum.text = "/ " + progressProviders.Count;
    }

    private void Update()
    {
        myPositionNum.text = GetRankOf(allCars[0]).ToString();

        var sorted = GetSortedRanking();

        for (int i = 0; i < sorted.Count; i++)
        {
            IProgressProvider provider = sorted[i];
            GameObject car = allCars.FirstOrDefault(c => c.GetComponent<IProgressProvider>() == provider);
            if (car == null) continue;

            int uiIndex = Array.IndexOf(allCars, car);
            if (uiIndex >= 0 && uiIndex < rankUIObjects.Length)
            {
                // 자식 오브젝트 순서 재배치
                rankUIObjects[uiIndex].transform.SetSiblingIndex(i);

                // 텍스트 갱신
                rankNums[uiIndex].text = (i + 1).ToString();
                nameTexts[uiIndex].text = car.name;
            }
        }
    }

    public List<IProgressProvider> GetSortedRanking()
    {
        return progressProviders
            .OrderByDescending(p => p.GetLap())
            .ThenByDescending(p => p.GetCurrentSplineIndex())
            .ThenByDescending(p => p.GetProgress())
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

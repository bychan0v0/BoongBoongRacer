using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Car/CarData", fileName="NewCarData")]
public class CarData : ScriptableObject
{
    public string   carName;
    public GameObject prefab;
    public Sprite     previewSprite;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="LightPatternList_SO",menuName = "Light/LightPattern")]
public class LightPatternList_SO :ScriptableObject
{
    public List<LightDetails> lightPatternList;
    public LightDetails GetLightDetails(Season season,LightShift lightShift)
    {
        return lightPatternList.Find(l => l.season == season && l.lightShift == lightShift);
    }
}
[System.Serializable]
public class LightDetails
{
    public Season season;
    public LightShift lightShift;
    public Color lightColor;
    public float lightAmount;
}

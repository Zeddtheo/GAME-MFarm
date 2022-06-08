using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFarm.Save;
public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;
    private Button currentButton;
    private DataSlot currentData;
    private int Index => transform.GetSiblingIndex();
    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }
    private void OnEnable()
    {
        SetupSlotUI();
    }
    private void SetupSlotUI()
    {
        currentData = SaveLoadManager.Instance.dataSlots[Index];
        if(currentData != null)
        {
            dataTime.text = currentData.DataTime;
            dataScene.text = currentData.DataScene;
        }
        else
        {
            dataTime.text = "这个世界还没开始";
            dataScene.text = "梦还没开始";
        }
    }
    private void LoadGameData()
    {
        if(currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            Debug.Log("new game");
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}

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
            dataTime.text = "������绹û��ʼ";
            dataScene.text = "�λ�û��ʼ";
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

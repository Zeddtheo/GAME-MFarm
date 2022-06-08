using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;
namespace MFarm.Inventory
{ 
    public class InventoryManager : Singleton<InventoryManager>,ISaveable
    {
        [Header("Object Data")]
        public ItemDataList_SO itemDataList_SO;
        [Header("Bag Data")]
        public InventoryBag_SO playerBag;
        private InventoryBag_SO currentBoxBag;
        public InventoryBag_SO playerBagTemp;
        [Header("Blueprint")]
        public BluePrintDataList_SO bluePrintData;
        [Header("Trade")]
        public int playerMoney;
        private Dictionary<string,List<InventoryItem>> boxDataDict = new Dictionary<string,List<InventoryItem>>();
        public int BoxDataAmount => boxDataDict.Count;
        public string GUID => GetComponent<DataGUID>().guid;
        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }
        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }
        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            //EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        private void OnStartNewGameEvent(int obj)
        {
            playerBag = Instantiate(playerBagTemp);
            playerMoney = Settings.playerStartMoney;
            boxDataDict.Clear();
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
        {
            currentBoxBag = bag_SO;
        }
        private void OnBuildFurnitureEvent(int ID, Vector3 mousePos)
        {
            RemoveItem(ID,1);
            BluePrintDetails bluePrint = bluePrintData.GetBluePrintDetails(ID);
            foreach(var item in bluePrint.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }
        }
        private void OnDropItemEvent(int ID, Vector3 pos,ItemType itemType)
        {
            RemoveItem(ID,1);
        }
        private void OnHarvestAtPlayerPosition(int ID)
        {
            var index = GetItemIndexInBag(ID);
            AddItemAtIndex(ID, index, 1);
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        public ItemDetails GetItemDetails(int ID)
        {
            return itemDataList_SO.itemDetailsList.Find(i=>i.itemID == ID);
        }       
        public void AddItem(Item item, bool toDestroy)
        {            
            //是否已经有该物品
            var index = GetItemIndexInBag(item.itemID);
            AddItemAtIndex(item.itemID, index, 1);

            //背包是否有空位

            //Debug.Log(GetItemDetails(item.itemID).itemID + "Name: " + GetItemDetails(item.itemID).itemName);
            if (toDestroy)
            {
                Destroy(item.gameObject);
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }       
        private bool CheckBagCapacity()
        {
            for(int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0)
                    return true;
            }
            return false;
        }
        private int GetItemIndexInBag(int ID)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == ID)
                    return i;
            }
            return -1;
        }
        private void AddItemAtIndex(int ID,int index, int amount)
        {
            if(index == -1 && CheckBagCapacity())
            {
                var item = new InventoryItem { itemID = ID ,itemAmount = amount};
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = item;
                        break;
                    }
                    
                }
            }
            else
            {
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID ,itemAmount = currentAmount};
                playerBag.itemList[index] = item;
            }
            
        }
        public void SwapItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[targetIndex];

            if (targetItem.itemID != 0)
            {
                playerBag.itemList[fromIndex] = targetItem;
                playerBag.itemList[targetIndex] = currentItem;
            }
            else
            {
                playerBag.itemList[targetIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        public void SwapItem(InventoryLocation locationFrom,int fromIndex,InventoryLocation locationTarget,int targetIndex)
        {
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);
            InventoryItem currentItem = currentList[fromIndex];
            if (targetIndex < targetList.Count)
            {
                InventoryItem targetItem = targetList[targetIndex];
                if(targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)
                {
                    currentList[fromIndex] = targetItem;
                    targetList[targetIndex] = currentItem;  
                }else if(currentItem.itemID == targetItem.itemID)
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                else
                {
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                EventHandler.CallUpdateInventoryUI(locationFrom,currentList);
                EventHandler.CallUpdateInventoryUI(locationTarget,targetList);
            }
        }
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null
            };
        }
        private void RemoveItem(int ID,int removeAmout)
        {
            var index = GetItemIndexInBag(ID);
            if (playerBag.itemList[index].itemAmount > removeAmout)
            {
                var amount = playerBag.itemList[index].itemAmount - removeAmout;
                var item = new InventoryItem { itemID = ID ,itemAmount = amount };
                playerBag.itemList[index] = item;
            }
            else if(playerBag.itemList[index].itemAmount == removeAmout)
            {
                var item = new InventoryItem();
                playerBag.itemList[index] = item;
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        public void TradeItem(ItemDetails itemDetails,int amount ,bool isSellTrade)
        {
            int cost = itemDetails.itemPrice * amount;
            int index = GetItemIndexInBag(itemDetails.itemID);
            if (isSellTrade)
            {
                if (playerBag.itemList[index].itemAmount >= amount)
                {
                    RemoveItem(itemDetails.itemID, amount);
                    cost = (int)(cost * itemDetails.sellPercentage);
                    playerMoney += cost;
                }
            }
            else if(playerMoney - cost >= 0)
            {
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID, index, amount);
                }
                playerMoney -= cost;
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        public bool CheckStock(int ID)
        {
            var bluePrintDetails = bluePrintData.GetBluePrintDetails(ID);
            foreach(var resourceItem in bluePrintDetails.resourceItem)
            {
                var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                if (itemStock.itemAmount >= resourceItem.itemAmount)
                {
                    continue;
                }
                else return false;
            }
            return true;
        }
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if(boxDataDict.ContainsKey(key))
                return boxDataDict[key];
            return null;
        }
        public void AddBoxDataDict(Box box)
        {
            var key = box.name + box.index;
            if(!boxDataDict.ContainsKey(key))
                boxDataDict.Add(key,box.boxBagData.itemList);
            Debug.Log(key);
        }
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.playerMoney = this.playerMoney;
            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.inventoryDict.Add(playerBag.name,playerBag.itemList);
            foreach(var item in boxDataDict)
            {
                saveData.inventoryDict.Add(item.Key,item.Value); 
             
            }
            return saveData;
        }
        public void RestoreData(GameSaveData saveData)
        {
            this.playerMoney = saveData.playerMoney;
            playerBag = Instantiate(playerBagTemp);
            playerBag.itemList = saveData.inventoryDict[playerBag.name];
            foreach(var item in saveData.inventoryDict) 
            {
                if (boxDataDict.ContainsKey(item.Key))
                {
                    boxDataDict[item.Key] = item.Value;
                }
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
    }
}
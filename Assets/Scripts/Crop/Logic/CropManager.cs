using UnityEngine;
namespace MFarm.CropPlant
{
    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;
        private Transform cropParent;
        private Grid currentGrid;
        private Season currentSeason;

        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }
        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }
        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
        }
        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }
        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(ID);
            if(currentCrop != null && SeasonAvailable(currentCrop)&&tileDetails.seedItemID == -1)
            {
                tileDetails.seedItemID = ID;
                tileDetails.growthDays = 0;
                DisplayCropPlant(tileDetails,currentCrop);
            }
            else if(tileDetails.seedItemID != -1)
            {
                DisplayCropPlant(tileDetails, currentCrop);
            }
        }
        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            int growthStages = cropDetails.growthDays.Length;
            int currentStage = 0;
            int dayCounter = cropDetails.TotalGrowthDays;

            for (int i = growthStages - 1; i >= 0; i--)
            {
                if (tileDetails.growthDays >= dayCounter)
                {
                    currentStage = i;
                    break;
                }
                dayCounter -= cropDetails.growthDays[i];
            }
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.growthSprites[currentStage];
            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);
            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
        }
        public CropDetails GetCropDetails(int ID)
        {
            return cropData.cropDetailsList.Find(c=>c.seedItemID == ID);
        }
        private bool SeasonAvailable(CropDetails crop)
        {
            for(int i = 0; i < crop.seasons.Length;i++)
            {
                if(crop.seasons[i] == currentSeason)
                    return true;
            }
            return false;
        }
    }
}

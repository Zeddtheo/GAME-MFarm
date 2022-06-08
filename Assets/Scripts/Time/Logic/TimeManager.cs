using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;

public class TimeManager : Singleton<TimeManager>,ISaveable
{
    private int gameSecond,gameMinute,gameHour,gameDay,gameMonth,gameYear;
    private Season gameSeason = Season.spring;
    private int monthInSeason = 3;
    public bool gameClockPause;
    private float tickTime;
    private float timeDifference;
    public TimeSpan GameTime => new TimeSpan(gameHour, gameMinute, gameSecond);

    public string GUID => GetComponent<DataGUID>().guid;

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }
    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
        gameClockPause = true;
        //EventHandler.CallGameDateEvent(gameHour,gameDay,gameMonth,gameYear,gameSeason);
        //EventHandler.CallGameMinuteEvent(gameMinute,gameHour,gameDay,gameSeason);
        //EventHandler.CallLightShiftChangeEvent(gameSeason,GetCurrentLightShift(),timeDifference);
    }
    private void Update()
    {
        if (!gameClockPause)
        {
            tickTime += Time.deltaTime;
            if(tickTime >= Settings.secondThreshold)
            {
                tickTime -= Settings.secondThreshold;
                UpdateGameTime();
            }
        }
        if (Input.GetKey(KeyCode.T))
        {
            for(int i = 0; i < 60; i++)
            {
                UpdateGameTime();
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            gameDay++;
            EventHandler.CallGameDayEvent(gameDay,gameSeason);
            EventHandler.CallGameDateEvent(gameHour,gameDay,gameMonth,gameYear,gameSeason);
        }
    }
    private void OnEndGameEvent()
    {
        gameClockPause = true;
    }
    private void OnStartNewGameEvent(int obj)
    {
        NewGameTime();
        gameClockPause = false;
    }
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        gameClockPause = gameState == GameState.Pause;
    }
    private void OnAfterSceneLoadedEvent()
    {
        gameClockPause = false;
        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);
        EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }
    private void OnBeforeSceneUnloadEvent()
    {
        gameClockPause = true;
    }
    private void NewGameTime()
    {
        gameSecond = 0;
        gameMinute = 0;
        gameHour = 7;
        gameDay = 1;
        gameMonth = 1;
        gameYear = 2022;
        gameSeason = Season.spring;

    }
    private void UpdateGameTime()
    {
        gameSecond++;
        if(gameSecond > Settings.secondHold)
        {
            gameMinute++;
            gameSecond = 0;

            if(gameMinute > Settings.minuteHold)
            {
                gameHour++;
                gameMinute = 0;
                if (gameHour > Settings.hourHold)
                {
                    gameDay++;
                    gameHour = 0;
                    if(gameDay > Settings.dayHold)
                    {
                        gameDay = 1;
                        gameMonth++;
                        if(gameMonth > 12)
                        {
                            gameMonth = 1;
                        }
                        monthInSeason--;
                        if(monthInSeason == 0)
                        {
                            monthInSeason = 3;
                            int seasonNumber = (int)gameSeason;
                            seasonNumber++;
                            if (seasonNumber > Settings.seasonHold)
                            {
                                seasonNumber = 0;
                                gameYear++;
                            }
                            gameSeason = (Season)seasonNumber;
                        }
                        //刷新地图和农作物生长
                        EventHandler.CallGameDayEvent(gameDay, gameSeason);
                    }    
                }
                EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
            }
            EventHandler.CallGameMinuteEvent(gameMinute, gameHour,gameDay,gameSeason);
            //切换灯光
            EventHandler.CallLightShiftChangeEvent(gameSeason,GetCurrentLightShift(),timeDifference);
        }
        //Debug.Log("Second: " + gameSecond + "Minute: " + gameMinute);
    }
    private LightShift GetCurrentLightShift()
    {
        if(GameTime >= Settings.morningTime && GameTime < Settings.nightTime)
        {
            timeDifference = (float)(GameTime - Settings.morningTime).TotalMinutes;
            return LightShift.Morning;
        }
        if(GameTime<Settings.morningTime || GameTime >= Settings.nightTime)
        {
            timeDifference = MathF.Abs((float)(GameTime - Settings.nightTime).TotalMinutes);
            Debug.Log(timeDifference);
            return LightShift.Night;
        }
        return LightShift.Morning;
    }
    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("gameYear",gameYear);
        saveData.timeDict.Add("gameSeason",(int)gameSeason);
        saveData.timeDict.Add("gameMonth",gameMonth);
        saveData.timeDict.Add("gameDay",gameDay);
        saveData.timeDict.Add("gameHour",gameHour);
        saveData.timeDict.Add("gameMinute",gameMinute);
        saveData.timeDict.Add("gameSecond",gameSecond);
        return saveData;
    }
    public void RestoreData(GameSaveData saveData)
    {
        gameYear = saveData.timeDict["gameYear"];
        gameSeason = (Season)saveData.timeDict["gameSeason"];
        gameMonth = saveData.timeDict["gameMonth"];
        gameDay = saveData.timeDict["gameDay"];
        gameHour = saveData.timeDict["gameHour"];
        gameMinute = saveData.timeDict["gameMinute"];
        gameSecond = saveData.timeDict["gameSecond"];
    }
}

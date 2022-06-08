using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Settings
{
    public const float itemFadeDuration = 0.35f;
    public const float targetAlpha = 0.45f;

    public const float secondThreshold = 0.01f;
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 30;
    public const int seasonHold = 3;
    //NPCÍø¸ñÒÆ¶¯
    public const float fadeDuration = 1.5f;
    public const int reapAmount = 2;
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    public const float pixelSize = 0.05f;
    public const float animationBreakTime = 5f;
    public const int maxGridSize = 9999;
    //µÆ¹â
    public const float lightChangeDuration = 25f;
    public static TimeSpan morningTime = new TimeSpan(7,0,0);
    public static TimeSpan nightTime = new TimeSpan(19,0,0);

    public static Vector3 playerStartPos = new Vector3(1, 1, 1);
    public const int playerStartMoney = 100;
}

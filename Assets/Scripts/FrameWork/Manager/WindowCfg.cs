using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowCfg
{
    public static string rootPath = "Assets/Prefabs/UI/";

    public enum WindowType
    {
        Normal,
        Alert,
    }

    public static Dictionary<string, string> windowPathDic = new Dictionary<string, string>
    {
        { WindowName.GameLoadView, "game_load/game_load_view" },
    };
}

public static class WindowName
{
    public const string GameLoadView = "GameLoadView";
    public const string MainHallView = "MainHallView";
}

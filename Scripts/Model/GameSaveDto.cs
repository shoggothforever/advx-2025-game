using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using Newtonsoft.Json;
using System.IO;
public class ReplayHistory
{
    public Dictionary<string, TimeReverse.TimedAction[]> levelReverseSnapshot = new(); // key = LevelId
    public float remainTime;
}
// Assets/Scripts/Save/DataModel/GameSaveDto.cs
[System.Serializable]              // 方便 Unity JsonUtility
public class GameSaveDto
{
    public int version = 1;      // 存档版本号
    public long lastSavedUnix;    // 时间戳（秒）
    public PlayerProgressDto player = new();
    public Dictionary<string, LevelRecordDto> levels = new(); // key = LevelId
    public Dictionary<string,TimeReverse.TimedAction[]> levelReverseSnapshot = new(); // key = LevelId
    public GameSaveDto()
    {
        version = 1;
        lastSavedUnix = 0;
        player = new();
        levels = new();
        levelReverseSnapshot = new();
    }
}

[System.Serializable]
public class PlayerProgressDto
{
    public int highestUnlockedWorld;   // 示例：解锁世界
}

[System.Serializable]
public class LevelRecordDto
{
    public float bestTime;          // 秒
    public int levelIndex;
    public string levelName;
    public float firstClearUnix;
    public float lastClearUnix;
    public LevelRecordDto()
    {
        bestTime = 0;
        firstClearUnix = 0;
        lastClearUnix = 0;
    }
    public LevelRecordDto(int idx,string name)
    {
        bestTime = 0;
        levelIndex = idx;
        levelName = name;
        firstClearUnix = 0;
        lastClearUnix = 0;
    }
}

public interface ISaveSerializer
{
    string Serialize(GameSaveDto dto);
    GameSaveDto Deserialize(string raw);
}
// 默认：Unity JsonUtility（易读、易调试）
public class JsonSaveSerializer : ISaveSerializer
{
    public string Serialize(GameSaveDto dto) => JsonConvert.SerializeObject(dto, Formatting.Indented, new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    });
    public GameSaveDto Deserialize(string raw) =>
        string.IsNullOrEmpty(raw) ? new GameSaveDto() : JsonConvert.DeserializeObject<GameSaveDto>(raw);
}


public interface IMigration
{
    int FromVersion { get; }
    GameSaveDto Migrate(GameSaveDto old);
}

// 示例：从 v1 → v2 增加字段
public class Migration_1_to_2 : IMigration
{
    public int FromVersion => 1;
    public GameSaveDto Migrate(GameSaveDto old)
    {
        old.version = 2;
        return old;
    }
}
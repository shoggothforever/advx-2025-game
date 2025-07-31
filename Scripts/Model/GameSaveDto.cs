using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using System.IO;

// Assets/Scripts/Save/DataModel/GameSaveDto.cs
[System.Serializable]              // 方便 Unity JsonUtility
public class GameSaveDto
{
    public int version = 1;      // 存档版本号
    public long lastSavedUnix;    // 时间戳（秒）
    public PlayerProgressDto player = new();
    public Dictionary<string, LevelRecordDto> levels = new(); // key = LevelId
    public Dictionary<string,List<TimeReverse.TimedAction>> levelReverseSnapshot = new(); // key = LevelId
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
    public int totalStars;
    public int highestUnlockedWorld;   // 示例：解锁世界
}

[System.Serializable]
public class LevelRecordDto
{
    public bool cleared;
    public float bestTime;          // 秒
    public long firstClearUnix;
    public long lastClearUnix;
}

public interface ISaveSerializer
{
    string Serialize(GameSaveDto dto);
    GameSaveDto Deserialize(string raw);
}
// 默认：Unity JsonUtility（易读、易调试）
public class JsonSaveSerializer : ISaveSerializer
{
    public string Serialize(GameSaveDto dto) => JsonUtility.ToJson(dto, true);
    public GameSaveDto Deserialize(string raw) =>
        string.IsNullOrEmpty(raw) ? new GameSaveDto() : JsonUtility.FromJson<GameSaveDto>(raw);
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
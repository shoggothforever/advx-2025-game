using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using Newtonsoft.Json;
using System.IO;

// Assets/Scripts/Save/DataModel/GameSaveDto.cs
[System.Serializable]              // ���� Unity JsonUtility
public class GameSaveDto
{
    public int version = 1;      // �浵�汾��
    public long lastSavedUnix;    // ʱ������룩
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
    public int highestUnlockedWorld;   // ʾ������������
}

[System.Serializable]
public class LevelRecordDto
{
    public float bestTime;          // ��
    public int levelIndex;
    public string levelName;
    public long firstClearUnix;
    public long lastClearUnix;
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
// Ĭ�ϣ�Unity JsonUtility���׶����׵��ԣ�
public class JsonSaveSerializer : ISaveSerializer
{
    public string Serialize(GameSaveDto dto) => JsonConvert.SerializeObject(dto, Formatting.Indented);
    public GameSaveDto Deserialize(string raw) =>
        string.IsNullOrEmpty(raw) ? new GameSaveDto() : JsonConvert.DeserializeObject<GameSaveDto>(raw);
}


public interface IMigration
{
    int FromVersion { get; }
    GameSaveDto Migrate(GameSaveDto old);
}

// ʾ������ v1 �� v2 �����ֶ�
public class Migration_1_to_2 : IMigration
{
    public int FromVersion => 1;
    public GameSaveDto Migrate(GameSaveDto old)
    {
        old.version = 2;
        return old;
    }
}
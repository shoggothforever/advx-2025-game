using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using System.IO;

// Assets/Scripts/Save/DataModel/GameSaveDto.cs
[System.Serializable]              // ���� Unity JsonUtility
public class GameSaveDto
{
    public int version = 1;      // �浵�汾��
    public long lastSavedUnix;    // ʱ������룩
    public PlayerProgressDto player = new();
    public Dictionary<string, LevelRecordDto> levels = new(); // key = LevelId
    public Dictionary<string,List<TimeReverse.TimedAction>> LevelReverseSnapshot = new(); // key = LevelId
}

[System.Serializable]
public class PlayerProgressDto
{
    public int totalStars;
    public int highestUnlockedWorld;   // ʾ������������
}

[System.Serializable]
public class LevelRecordDto
{
    public bool cleared;
    public float bestTime;          // ��
    public long firstClearUnix;
    public long lastClearUnix;
}

public interface ISaveSerializer
{
    string Serialize(GameSaveDto dto);
    GameSaveDto Deserialize(string raw);
}
// Ĭ�ϣ�Unity JsonUtility���׶����׵��ԣ�
public class JsonSaveSerializer : ISaveSerializer
{
    public string Serialize(GameSaveDto dto) => JsonUtility.ToJson(dto, true);
    public GameSaveDto Deserialize(string raw) =>
        string.IsNullOrEmpty(raw) ? new GameSaveDto() : JsonUtility.FromJson<GameSaveDto>(raw);
}

public interface IStorageBackend
{
    void Save(string raw);
    string Load();
}

// PC/Mac��Application.persistentDataPath
public class LocalFileStorage : IStorageBackend
{
    private readonly string path = Path.Combine(
        Application.persistentDataPath, "save.dat");

    public void Save(string raw) => File.WriteAllText(path, raw);
    public string Load() => File.Exists(path) ? File.ReadAllText(path) : "";
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
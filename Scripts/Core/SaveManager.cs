using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("配置")]
    [SerializeField] private LevelConfig sceneList; // 用来校验关卡有效性

    private readonly ISaveSerializer serializer = new JsonSaveSerializer();
    private readonly IStorageBackend storage = new LocalFileStorage();
    private readonly List<IMigration> migrations = new() { new Migration_1_to_2() };

    private GameSaveDto _cache;           // 运行期只操作这份内存
    private bool _dirty;                  // 脏标记，避免频繁写盘
    private float _lastWriteTime;         // 节流：最多 2 秒写一次
    public GameSaveDto Data => _cache;    // 任何地方直接访问

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (_dirty && Time.time - _lastWriteTime > 2f)
            Flush();
    }

    private void OnApplicationPause(bool pause) { if (pause) Flush(); }
    private void OnApplicationQuit() => Flush();

    // ============== 对外 API ==============

    public void MarkDirty() => _dirty = true;
    public void SaveData(string labelName,float time)
    {
        if (!Data.levels.ContainsKey(labelName))
            Data.levels[labelName] = new LevelRecordDto();
        var rec = Data.levels[labelName];
        rec.cleared = true;
        if (time < rec.bestTime || rec.bestTime == 0) rec.bestTime = time;
        MarkDirty();
    }

    // ============== 内部 ==============

    private void Load()
    {
        string raw = storage.Load();
        _cache = serializer.Deserialize(raw);

        // 按版本迁移
        while (migrations.Find(m => m.FromVersion == _cache.version) is { } mig)
            _cache = mig.Migrate(_cache);

        // 校验关卡有效性（删掉被删掉的关卡的键）
        var invalid = _cache.levels.Keys
            .Where(k => !sceneList.levelName.Contains(k)).ToList();
        foreach (var k in invalid) _cache.levels.Remove(k);

        _dirty = false;
    }

    private void Flush()
    {
        if (!_dirty) return;
        string raw = serializer.Serialize(_cache);
        storage.Save(raw);
        _dirty = false;
        _lastWriteTime = Time.time;
    }
}
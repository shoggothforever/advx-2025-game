using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("����")]
    [SerializeField] private LevelConfig sceneList; // ����У��ؿ���Ч��

    private readonly ISaveSerializer serializer = new JsonSaveSerializer();
    private readonly IStorageBackend storage = new LocalFileStorage();
    private readonly List<IMigration> migrations = new() { new Migration_1_to_2() };

    private GameSaveDto _cache;           // ������ֻ��������ڴ�
    private bool _dirty;                  // ���ǣ�����Ƶ��д��
    private float _lastWriteTime;         // ��������� 2 ��дһ��
    public GameSaveDto Data => _cache;    // �κεط�ֱ�ӷ���

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

    // ============== ���� API ==============

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

    // ============== �ڲ� ==============

    private void Load()
    {
        string raw = storage.Load();
        _cache = serializer.Deserialize(raw);

        // ���汾Ǩ��
        while (migrations.Find(m => m.FromVersion == _cache.version) is { } mig)
            _cache = mig.Migrate(_cache);

        // У��ؿ���Ч�ԣ�ɾ����ɾ���Ĺؿ��ļ���
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
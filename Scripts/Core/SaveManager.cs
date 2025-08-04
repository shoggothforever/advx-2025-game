using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace SaveYourself.Core
{
    public interface IStorageBackend
    {
        void Save(string path, string raw);
        string Load(string path);
    }
    // PC/Mac：Application.persistentDataPath
    public class LocalFileStorage : IStorageBackend
    {
        public void Save(string path, string raw)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                Debug.Log("create save file at " + path);
            }
            File.WriteAllText(path, raw);

        }
        public string Load(string path) => File.Exists(path) ? File.ReadAllText(path) : "";
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        //[Header("配置")]
        //[SerializeField] private LevelConfig sceneList; // 用来校验关卡有效性

        private readonly ISaveSerializer serializer = new JsonSaveSerializer();
        private readonly IStorageBackend storage = new LocalFileStorage();
        private readonly List<IMigration> migrations = new() { new Migration_1_to_2() };
        private string path;
        private GameSaveDto _cache;           // 运行期只操作这份内存
        private bool _dirty;                  // 脏标记，避免频繁写盘
        private float _lastWriteTime;         // 节流：最多 2 秒写一次
        public GameSaveDto Data => _cache;    // 任何地方直接访问
        private static Dictionary<string, int> LevelIndex;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
            path =Path.Combine(Application.persistentDataPath, "save.dat");
            Debug.Log("save file path is " + path);
            initLevelIndexMap();
            Load();
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
        public void SaveSnapshot(string levelName,List<TimeReverse.TimedAction> history)
        {
            var rec = Data.levelReverseSnapshot;
            rec[levelName] = history.ToArray();
        }
        public void SaveData(string levelName, string nextLevelName, float time)
        {
            if (!Data.levels.ContainsKey(levelName))
            {
                var levelRec = new LevelRecordDto(LevelIndex[levelName],levelName);
                Data.levels[levelName] = levelRec;
            }
            var rec = Data.levels[levelName];
            if (time < rec.bestTime || rec.bestTime == 0)
            {
                rec.bestTime = time;

            }
            var now=Time.time;
            if (now < rec.firstClearUnix || rec.firstClearUnix==0f)
            {
                rec.firstClearUnix= now;
            }
            rec.lastClearUnix= now;
            Data.levels[levelName] = rec;
            Data.player.highestUnlockedWorld = Mathf.Max(Data.player.highestUnlockedWorld, Mathf.Max(4, LevelIndex[nextLevelName]));
            MarkDirty();
        }

        // ============== 内部 ==============

        private void Load()
        {
            string raw = storage.Load(path);
            // ② 如果没有存档就新建一份
            if (raw == null || raw.Length == 0)
            {
                _cache = new GameSaveDto();          // 空存档
                initSaveData();           // 可选：给第一关解锁等
                Flush();                             // 立即写盘
            }
            else
            {
                _cache = serializer.Deserialize(raw);
            }
            // 按版本迁移
            while (migrations.Find(m => m.FromVersion == _cache.version) is { } mig)
                _cache = mig.Migrate(_cache);

            // 校验关卡有效性（删掉被删掉的关卡的键）
            //var invalid = _cache.levels.Keys
            //    .Where(k => !sceneList.levelName.Contains(k)).ToList();
            //foreach (var k in invalid) _cache.levels.Remove(k);

            _dirty = false;
        }
        /// <summary>第一次启动时的默认数据</summary>
        private void initSaveData()
        {
            // 示例：把第一关设为已解锁
            SaveData("playground", "MainMenu", 0);
            SaveData("MainMenu", "Tutorial1", 0);
            SaveData("Tutorial1", "Tutorial2", 0);
            SaveData("Tutorial2", "Tutorial3", 0);
            SaveData("Tutorial3", "Level1", 0);
            SaveData("Level1", "Level2", 0);
        }
        //TODO 通过配置的方式为关卡添加索引
        private void initLevelIndexMap()
        {
            if (LevelIndex == null)
            {
                LevelIndex=new Dictionary<string, int>();
                LevelIndex.Add("MainMenu", 0);
                LevelIndex.Add("Tutorial1", 1);
                LevelIndex.Add("Tutorial2", 2);
                LevelIndex.Add("Tutorial3", 3);
                LevelIndex.Add("Level1", 4);
                LevelIndex.Add("Level2", 5);
                LevelIndex.Add("Level3", 6);
                LevelIndex.Add("finalLevel", 7);
                LevelIndex.Add("playground", 999);
            }
        }
        private void Flush()
        {
            if (!_dirty) return;
            string raw = serializer.Serialize(_cache);
            storage.Save(path,raw);
            _dirty = false;
            _lastWriteTime = Time.time;
        }
    }
}
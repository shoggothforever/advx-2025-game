using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
namespace SaveYourself.Core
{
    public interface IStorageBackend
    {
        void Save(string path, string raw);
        string Load(string path);
    }
    // PC/Mac��Application.persistentDataPath
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

        //[Header("����")]
        //[SerializeField] private LevelConfig sceneList; // ����У��ؿ���Ч��

        private readonly ISaveSerializer serializer = new JsonSaveSerializer();
        private readonly IStorageBackend storage = new LocalFileStorage();
        private readonly List<IMigration> migrations = new() { new Migration_1_to_2() };
        private string path;
        private GameSaveDto _cache;           // ������ֻ��������ڴ�
        private bool _dirty;                  // ���ǣ�����Ƶ��д��
        private float _lastWriteTime;         // ��������� 2 ��дһ��
        public GameSaveDto Data => _cache;    // �κεط�ֱ�ӷ���
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
        public void SaveData(string levelName, string nextLevelName, float time)
        {
            if (!Data.levels.ContainsKey(levelName))
                Data.levels[levelName] = new LevelRecordDto();
            var rec = Data.levels[levelName];
            rec.cleared = true;
            if (time < rec.bestTime || rec.bestTime == 0) rec.bestTime = time;
            Data.player.highestUnlockedWorld = Mathf.Max(Data.player.highestUnlockedWorld, Mathf.Max(4, LevelIndex[nextLevelName]));

            MarkDirty();
        }

        // ============== �ڲ� ==============

        private void Load()
        {
            string raw = storage.Load(path);
            // �� ���û�д浵���½�һ��
            if (raw == null || raw.Length == 0)
            {
                Debug.Log("it's yout first time to  open the game");
                _cache = new GameSaveDto();          // �մ浵
                initSaveData();           // ��ѡ������һ�ؽ�����
                Flush();                             // ����д��
            }
            else
            {
                Debug.Log("load save file successfully");
                _cache = serializer.Deserialize(raw);
            }
            // ���汾Ǩ��
            while (migrations.Find(m => m.FromVersion == _cache.version) is { } mig)
                _cache = mig.Migrate(_cache);

            // У��ؿ���Ч�ԣ�ɾ����ɾ���Ĺؿ��ļ���
            //var invalid = _cache.levels.Keys
            //    .Where(k => !sceneList.levelName.Contains(k)).ToList();
            //foreach (var k in invalid) _cache.levels.Remove(k);

            _dirty = false;
        }
        /// <summary>��һ������ʱ��Ĭ������</summary>
        private void initSaveData()
        {
            // ʾ�����ѵ�һ����Ϊ�ѽ���
            SaveData("Tutorial1", "Tutorial2", 0);
            SaveData("Tutorial2", "Tutorial3", 0);
            SaveData("Tutorial3", "Level1", 0);
            SaveData("Level1", "Level2", 0);
        }
        //TODO ͨ�����õķ�ʽΪ�ؿ��������
        private void initLevelIndexMap()
        {
            if (LevelIndex == null)
            {
                LevelIndex=new Dictionary<string, int>();
                LevelIndex.Add("Tutorial1", 1);
                LevelIndex.Add("Tutorial2", 2);
                LevelIndex.Add("Tutorial3", 3);
                LevelIndex.Add("Level1", 4);
                LevelIndex.Add("Level2", 5);
                LevelIndex.Add("Level3", 6);
                LevelIndex.Add("finalLevel", 7);
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
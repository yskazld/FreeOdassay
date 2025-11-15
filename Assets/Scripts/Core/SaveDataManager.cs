using System;
using System.IO;
using UnityEngine;

namespace FreelanceOdyssey.Core
{
    public class SaveDataManager : MonoBehaviour
    {
        private const string FileName = "freelance_odyssey_save.json";

        public bool TryLoad(out PlayerSaveData data)
        {
            var path = GetPath();
            if (!File.Exists(path))
            {
                data = new PlayerSaveData();
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                data = JsonUtility.FromJson<PlayerSaveData>(json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save data: {ex}");
                data = new PlayerSaveData();
                return false;
            }
        }

        public void Save(PlayerSaveData data)
        {
            data.version = Application.version;
            data.lastSavedTicks = DateTime.UtcNow.Ticks;
            var json = JsonUtility.ToJson(data, true);
            try
            {
                File.WriteAllText(GetPath(), json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save data: {ex}");
            }
        }

        private string GetPath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }
    }
}

﻿using SimpleJSON;
using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;

public class Settings {

    private static Settings _instance;
    public static Settings Instance { get
        {
            if (_instance == null) _instance = Load();
            return _instance;
        }
    }

    public string BeatSaberInstallation = "";
    public string CustomSongsFolder {
        get {
            return ConvertToDirectory(BeatSaberInstallation + "/Beat Saber_Data/CustomLevels");
        }
    }
    public string CustomWIPSongsFolder
    {
        get
        {
            return ConvertToDirectory(BeatSaberInstallation + "/Beat Saber_Data/CustomWIPLevels");
        }
    }
    public bool DiscordRPCEnabled = true;
    public bool OSC_Enabled = false;
    public string OSC_IP = "127.0.0.1";
    public string OSC_Port = "8080";
    public int EditorScale = 4;
    public int ChunkDistance = 5;
    public int AutoSaveInterval = 5;
    public int InitialLoadBatchSize = 100;
    public bool InvertNoteControls = false;
    public bool WaveformGenerator = false;
    public bool CountersPlus = false;
    public bool PlaceChromaEvents = false;
    public bool PickColorFromChromaEvents = false;
    public bool PlaceOnlyChromaEvents = false;
    public bool BongoBoye = false;
    public bool AutoSave = true;
    public float Volume = 1;

    private static Settings Load()
    {
        Settings settings = new Settings();
        if (!File.Exists(Application.persistentDataPath + "/ChroMapperSettings.json"))
        {
            Debug.Log("Settings file doesn't exist! Skipping loading...");
        }
        else
        {
            try
            {
                using (StreamReader reader = new StreamReader(Application.persistentDataPath + "/ChroMapperSettings.json"))
                {
                    JSONNode mainNode = JSON.Parse(reader.ReadToEnd());
                    Type type = settings.GetType();
                    MemberInfo[] infos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    foreach (MemberInfo info in infos)
                    {
                        if (!(info is FieldInfo field)) continue;
                        if (mainNode[field.Name] != null)
                            field.SetValue(settings, Convert.ChangeType(mainNode[field.Name].Value, field.FieldType));
                    }
                }
                Debug.Log("Settings loaded!");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        return settings;
    }

    public void Save()
    {
        JSONObject mainNode = new JSONObject();
        Type type = GetType();
        MemberInfo[] infos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name).ToArray();
        foreach (MemberInfo info in infos)
        {
            if (!(info is FieldInfo field)) continue;
            mainNode[field.Name] = field.GetValue(this).ToString();
        }
        using (StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/ChroMapperSettings.json", false))
            writer.Write(mainNode.ToString(2));
        Debug.Log("Settings saved!");
    }

    public static bool ValidateDirectory(Action<string> errorFeedback = null) {
        if (!Directory.Exists(Instance.BeatSaberInstallation)) {
            errorFeedback?.Invoke("That folder does not exist!");
            return false;
        }
        if (!Directory.Exists(Instance.CustomSongsFolder)) {
            errorFeedback?.Invoke("No \"Beat Saber_Data\" or \"CustomLevels\" folder was found at chosen location!");
            return false;
        }
        if (!Directory.Exists(Instance.CustomWIPSongsFolder))
        {
            errorFeedback?.Invoke("No \"CustomWIPLevels\" folder was found at chosen location!");
            return false;
        }
        return true;
    }

    public static string ConvertToDirectory(string s) {
        return s.Replace('\\', '/');
    }

}

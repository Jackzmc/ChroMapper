﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;
using System;
using UnityEngine.UI;

public class AutoSaveController : MonoBehaviour {
    private float t = 0;
    [SerializeField] private Toggle autoSaveToggle;

    public void ToggleAutoSave(bool enabled)
    {
        Settings.Instance.AutoSave = enabled;
    }

	// Use this for initialization
	void Start () {
        autoSaveToggle.isOn = Settings.Instance.AutoSave;
        t = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (!Settings.Instance.AutoSave) return;
        t += Time.deltaTime;
        if (t > (Settings.Instance.AutoSaveInterval * 60))
        {
            t = 0;
            Save(true);
        }
	}

    public void Save(bool auto = false)
    {
        PersistentUI.Instance.DisplayMessage($"{(auto ? "Auto " : "")}Saving...", PersistentUI.DisplayMessageType.BOTTOM);
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            string original = BeatSaberSongContainer.Instance.map.directoryAndFile;
            if (auto) {
                List<string> directory = original.Split('/').ToList();
                //directory.Insert(directory.Count - 1, $"autosave-{DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")}"); //caeden you troll stop making 1000+ folders
                directory.Insert(directory.Count - 1, "autosave");
                directory[directory.Count-1] = $"{DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")}-{directory[directory.Count-1]}"; //timestamp difficulty
                string tempDirectory = string.Join("/", directory.ToArray());
                Debug.Log($"Auto saved to: {tempDirectory}");
                //We need to create the autosave directory before we can save the .dat difficulty into it.
                System.IO.Directory.CreateDirectory(string.Join("/", directory.Where(x => x != directory.Last()).ToArray()));
                BeatSaberSongContainer.Instance.map.directoryAndFile = tempDirectory;
            }
            BeatSaberSongContainer.Instance.map.Save();
            BeatSaberSongContainer.Instance.map.directoryAndFile = original;
        }).Start();
    }
}

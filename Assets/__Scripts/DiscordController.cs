﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Discord;
using System.Linq;
using System;

public class DiscordController : MonoBehaviour
{
    public static bool IsActive = true;

    public Discord.Discord discord = null;
    public ActivityManager activityManager = null;
    private Activity activity;
    [SerializeField] private TextAsset clientIDTextAsset;

    private float discordUpdateMinutes = 1;
    private int initNoteCount;
    private int initEventsCount;
    private PlatformDescriptor platform;
    private Coroutine mapPresenceRoutine = null;

    // Start is called before the first frame update
    private void Start()
    {
        if (Settings.Instance.DiscordRPCEnabled == false) return;
        if (long.TryParse(clientIDTextAsset.text, out long discordClientID) && Application.internetReachability != NetworkReachability.NotReachable)
        {
            discord = new Discord.Discord(discordClientID, (ulong)CreateFlags.NoRequireDiscord);
            activityManager = discord.GetActivityManager();
            activityManager.ClearActivity((res) => { });
        }
        else Debug.LogWarning("Discord RPC failed to connect: No internet connection, or invalid Client ID.");
        SceneManager.activeSceneChanged += SceneUpdated;
        LoadInitialMap.PlatformLoadedEvent += LoadPlatform;
    }

    private void OnApplicationQuit()
    {
        discord?.Dispose();
    }

    private void LoadPlatform(PlatformDescriptor descriptor)
    {
        platform = descriptor;
    }

    private void SceneUpdated(Scene from, Scene to)
    {
        if (mapPresenceRoutine != null) StopCoroutine(mapPresenceRoutine);
        string details = "Invalid!";
        string state = "";
        switch (to.name)
        {
            case "00_FirstBoot": details = "Selecting install folder..."; break;
            case "01_SongSelectMenu":
                details = "Viewing song list.";
                break;
            case "02_SongEditMenu":
                details = BeatSaberSongContainer.Instance.song.songName;
                state = "Viewing song info.";
                break;
            case "03_Mapper":
                initNoteCount = BeatSaberSongContainer.Instance.map._notes.Count;
                initEventsCount = BeatSaberSongContainer.Instance.map._events.Count;
                details = $"Editing {BeatSaberSongContainer.Instance.song.songName}" + 
                    $" ({BeatSaberSongContainer.Instance.difficultyData.difficulty})";
                break;
            case "04_Options": details = "Editing ChroMapper options"; break;
        }
        activity = new Activity
        {
            Details = details,
            State = state,
            Timestamps = new ActivityTimestamps() {
                Start = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
            },
            Assets = new ActivityAssets()
            {
                SmallImage = "newlogo",
                SmallText = "ChroMapper Closed Alpha",
                LargeImage = "newlogo_glow",
                LargeText = "In Menus",
            }
        };
        if (to.name == "03_Mapper")
            mapPresenceRoutine = StartCoroutine(MapperPresenceTick());
        else UpdatePresence();
    }

    private IEnumerator MapperPresenceTick()
    {
        while (true)
        {
            yield return new WaitUntil(() => platform != null);
            List<string> randomStates = new List<string>() {
                $"{BeatSaberSongContainer.Instance.map._notes.Count - initNoteCount} Notes Placed",
                $"{BeatSaberSongContainer.Instance.map._events.Count - initEventsCount} Events Placed",
                $"{BeatSaberSongContainer.Instance.map._obstacles.Count} Obstacles in Map",
                $"{BeatSaberSongContainer.Instance.map._notes.Count} Notes in Map",
                $"{BeatSaberSongContainer.Instance.map._events.Count} Events in Map",
            };
            if (NodeEditorController.IsActive) randomStates.Add("Using Node Editor");
            if (BeatSaberSongContainer.Instance.map._events.Any(x => x._value >= ColourManager.RGB_ALT))
                randomStates.Add("Now with Chroma RGB!");
            activity.State = randomStates[UnityEngine.Random.Range(0, randomStates.Count)];
            string platformName = platform.gameObject.name.Substring(0, platform.gameObject.name.IndexOf("(Clone)"));
            activity.Assets.LargeImage = string.Join("", platformName.Split(' ')).ToLower();
            activity.Assets.LargeText = platformName;
            UpdatePresence();
            yield return new WaitForSeconds(discordUpdateMinutes * 60);
        }
    }

    private void UpdatePresence()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return;
        activityManager.UpdateActivity(activity, (res) => {
            if (res == Result.Ok) Debug.Log("Discord Presence updated!");
            else Debug.LogWarning($"Discord Presence failed! {res}");
        });
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneUpdated;
        LoadInitialMap.PlatformLoadedEvent -= LoadPlatform;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsActive) discord?.RunCallbacks();
    }
}

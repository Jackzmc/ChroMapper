﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongList : MonoBehaviour {

    [SerializeField]
    InputField searchField;

    [SerializeField]
    SongListItem[] items;

    [SerializeField]
    TextMeshProUGUI pageText;

    [SerializeField]
    int currentPage = 0;

    [SerializeField]
    int maxPage = 0;

    [SerializeField]
    List<BeatSaberSong> songs = new List<BeatSaberSong>();

    [SerializeField]
    Button firstButton;
    [SerializeField]
    Button prevButton;
    [SerializeField]
    Button nextButton;
    [SerializeField]
    Button lastButton;
    [SerializeField]
    TextMeshProUGUI songLocationToggleText;

    public bool WIPLevels = true;
    public bool FilteredBySearch = false;
    private void Start() {
        RefreshSongList(false);
    }

    public void ToggleSongLocation()
    {
        WIPLevels = !WIPLevels;
        RefreshSongList(false);
        songLocationToggleText.text = WIPLevels ? "Custom\nLevels" : "Custom\nWIP\nLevels";
    }

    public void RefreshSongList(bool search) {
        FilteredBySearch = search;
        string[] directories = new string[] { };
        if (WIPLevels) //Grabs songs from CustomWIPLevels or CustomLevels
            directories = Directory.GetDirectories(Settings.Instance.CustomWIPSongsFolder);
        else
            directories = Directory.GetDirectories(Settings.Instance.CustomSongsFolder);
        songs.Clear();
        for (int i = 0; i < directories.Length; i++) {
            BeatSaberSong song = BeatSaberSong.GetSongFromFolder(directories[i]);
            if (song == null)
            {   //Get songs from subdirectories
                string[] subDirectories = Directory.GetDirectories(directories[i]);
                for (int e = 0; e < subDirectories.Length; e++)
                    song = BeatSaberSong.GetSongFromFolder(subDirectories[e]);
            }
            if (song != null) songs.Add(song);
        }
        //Sort by song name, and filter by search text.
        if (FilteredBySearch)
            songs = songs.Where(x => searchField.text != "" ? x.songName.AllIndexOf(searchField.text).Any() : true).ToList();
        songs = songs.OrderBy(x => x.songName).ToList();
        maxPage = Mathf.Max(0, Mathf.CeilToInt(songs.Count / items.Length));
        SetPage(0);
    }

    public void SetPage(int page) {
        if (page < 0 || page > maxPage) return;
        currentPage = page;
        LoadPage();
        pageText.text = "Page: " + (currentPage + 1) + "/" + (maxPage + 1);

        firstButton.interactable = currentPage != 0;
        prevButton.interactable = currentPage - 1 >= 0;
        nextButton.interactable = currentPage + 1 <= maxPage;
        lastButton.interactable = currentPage != maxPage;
    }

    public void LoadPage() {
        int offset = currentPage * items.Length;
        for (int i = 0; i < 10; i++) { // && (i + offset) < songs.Count; i++) {
            if (items[i] == null) continue;
            if (i + offset < songs.Count) {
                string name = songs[i + offset].songName;
                if (searchField.text != "" && FilteredBySearch)
                {
                    List<int> index = name.AllIndexOf(searchField.text).ToList();
                    if (index.Any())
                    {
                        string newName = name.Substring(0, index.First());
                        int length = searchField.text.Length;
                        for (int j = 0; j < index.Count(); j++)
                        {
                            newName += $"<u>{name.Substring(index[j], length)}</u>";
                            if (j != index.Count() - 1)
                                newName += $"{name.Substring(index[j] + length, index[j + 1] - (index[j] + length))}";
                            else break;
                        }
                        int lastIndex = index.Last() + (length - 1);
                        name = newName + name.Substring(lastIndex + 1, name.Length - lastIndex - 1);
                    }
                }
                items[i].gameObject.SetActive(true);
                items[i].SetDisplayName(name);
                items[i].AssignSong(songs[i + offset]);
            } else items[i].gameObject.SetActive(false);
        }
    }

    public void FirstPage() {
        SetPage(0);
    }

    public void PrevPage() {
        SetPage(currentPage - 1);
    }

    public void NextPage() {
        SetPage(currentPage + 1);
    }

    public void LastPage() {
        SetPage(maxPage);
    }

}

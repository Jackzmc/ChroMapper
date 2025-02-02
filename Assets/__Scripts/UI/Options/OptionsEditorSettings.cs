﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsEditorSettings : MonoBehaviour
{
    [SerializeField] private Slider editorScaleSlider;
    [SerializeField] private Slider songSpeedSlider;
    [SerializeField] private Slider chunkDistanceSlider;
    [SerializeField] private TextMeshProUGUI songSpeedDisplay;
    [SerializeField] private TextMeshProUGUI chunkDistanceDisplay;
    [SerializeField] private TMP_InputField autoSaveInterval;
    [SerializeField] private TMP_InputField noteLanes;
    [SerializeField] private Toggle oscEnabled;
    [SerializeField] private TMP_InputField oscIP;
    [SerializeField] private TMP_InputField oscPort;
    [SerializeField] private Toggle invertControls;
    [SerializeField] private Toggle nodeEditor;
    [SerializeField] private Toggle waveformGenerator;
    [SerializeField] private Toggle countersPlus;
    [SerializeField] private Toggle chromaOnly;
    [SerializeField] private Toggle redNoteDing;
    [SerializeField] private Toggle blueNoteDing;
    [SerializeField] private Toggle bombDing;
    void Start()
    {
        editorScaleSlider.value = Mathf.Sqrt(Settings.Instance.EditorScale);
        songSpeedSlider.value = OptionsController.Find<SongSpeedController>()?.source.pitch * 10f ?? 10f;
        chunkDistanceSlider.value = Settings.Instance.ChunkDistance;
        autoSaveInterval.text = Settings.Instance.AutoSaveInterval.ToString();
        noteLanes.text = OptionsController.Find<NoteLanesController>()?.NoteLanes.ToString() ?? "4";
        oscIP.text = Settings.Instance.OSC_IP;
        oscPort.text = Settings.Instance.OSC_Port;
        oscEnabled.isOn = Settings.Instance.OSC_Enabled;
        invertControls.isOn = Settings.Instance.InvertNoteControls;
        nodeEditor.isOn = OptionsController.Find<NodeEditorController>()?.AdvancedSetting ?? false;
        waveformGenerator.isOn = Settings.Instance.WaveformGenerator;
        countersPlus.isOn = CountersPlusController.IsActive;
        chromaOnly.isOn = Settings.Instance.PlaceOnlyChromaEvents;
        redNoteDing.isOn = DingOnNotePassingGrid.NoteTypeToDing[BeatmapNote.NOTE_TYPE_A];
        blueNoteDing.isOn = DingOnNotePassingGrid.NoteTypeToDing[BeatmapNote.NOTE_TYPE_B];
        bombDing.isOn = DingOnNotePassingGrid.NoteTypeToDing[BeatmapNote.NOTE_TYPE_BOMB];
    }

    #region Update Editor Variables
    public void UpdateEditorScale(float scale)
    {
        Settings.Instance.EditorScale = Mathf.RoundToInt(scale);
        OptionsController.Find<EditorScaleController>()?.UpdateEditorScale(scale);
    }

    public void UpdateSongSpeed(float speed)
    {
        OptionsController.Find<SongSpeedController>()?.UpdateSongSpeed(speed);
        songSpeedDisplay.text = speed * 10 + "%";
    }

    public void ToggleAutoSave(bool enabled)
    {
        OptionsController.Find<AutoSaveController>()?.ToggleAutoSave(enabled);
    }

    public void UpdateAutoSaveInterval(string value)
    {
        if (int.TryParse(value, out int interval) && interval > 0)
            Settings.Instance.AutoSaveInterval = interval;
    }

    public void UpdateNoteLanes(string value)
    {
        OptionsController.Find<NoteLanesController>()?.UpdateNoteLanes(value);
    }

    public void UpdateInvertedControls(bool inverted)
    {
        Settings.Instance.InvertNoteControls = inverted;
    }

    public void UpdateChromaOnly(bool param)
    {
        Settings.Instance.PlaceOnlyChromaEvents = param;
    }

    public void UpdateNodeEditor(bool enabled)
    {
        OptionsController.Find<NodeEditorController>()?.UpdateAdvancedSetting(enabled);
    }

    public void UpdateWaveform(bool enabled)
    {
        Settings.Instance.WaveformGenerator = enabled;
    }

    public void UpdateCountersPlus(bool enabled)
    {
        Settings.Instance.CountersPlus = enabled;
        OptionsController.Find<CountersPlusController>()?.ToggleCounters(enabled);
    }

    public void UpdateRedNoteDing(bool ding)
    {
        DingOnNotePassingGrid.NoteTypeToDing[BeatmapNote.NOTE_TYPE_A] = ding;
    }

    public void UpdateBlueNoteDing(bool ding)
    {
        DingOnNotePassingGrid.NoteTypeToDing[BeatmapNote.NOTE_TYPE_B] = ding;
    }

    public void UpdateBombDing(bool ding)
    {
        DingOnNotePassingGrid.NoteTypeToDing[BeatmapNote.NOTE_TYPE_BOMB] = ding;
    }

    public void UpdateChunksLoaded(float value)
    {
        int chunks = Mathf.RoundToInt(value);
        chunkDistanceDisplay.text = chunks.ToString();
        Settings.Instance.ChunkDistance = chunks;
    }
    
    public void UpdateOSC()
    {
        Settings.Instance.OSC_IP = oscIP.text;
        Settings.Instance.OSC_Port = oscPort.text;
        Settings.Instance.OSC_Enabled = oscEnabled.isOn;
        OptionsController.Find<OSCMessageSender>()?.ReloadOSCStats();
    }
    #endregion
}

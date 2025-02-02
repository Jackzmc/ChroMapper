﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventsContainer : BeatmapObjectContainerCollection
{
    [SerializeField] private GameObject eventPrefab;
    [SerializeField] private EventAppearanceSO eventAppearanceSO;
    [SerializeField] private GameObject eventGridLabels;
    [SerializeField] private GameObject ringPropagationLabels;

    public bool RingPropagationEditing
    {
        get { return ringPropagationEditing; }
        set
        {
            ringPropagationEditing = value;
            ringPropagationLabels.SetActive(value);
            eventGridLabels.SetActive(!value);
            UpdateRingPropagationMode();
        }
    }
    private bool ringPropagationEditing = false;

    internal override void SubscribeToCallbacks()
    {
        SpawnCallbackController.EventPassedThreshold += SpawnCallback;
        SpawnCallbackController.RecursiveEventCheckFinished += RecursiveCheckFinished;
        DespawnCallbackController.EventPassedThreshold += DespawnCallback;
        AudioTimeSyncController.OnPlayToggle += OnPlayToggle;
    }

    internal override void UnsubscribeToCallbacks() {
        SpawnCallbackController.EventPassedThreshold -= SpawnCallback;
        SpawnCallbackController.RecursiveEventCheckFinished -= RecursiveCheckFinished;
        DespawnCallbackController.EventPassedThreshold -= DespawnCallback;
        AudioTimeSyncController.OnPlayToggle -= OnPlayToggle;
    }

    public override void SortObjects()
    {
        LoadedContainers = LoadedContainers.OrderBy(x => x.objectData._time).ToList();
        StartCoroutine(WaitUntilChunkLoad());
    }

    private void UpdateRingPropagationMode()
    {
        foreach (BeatmapObjectContainer con in LoadedContainers)
        {
            if (ringPropagationEditing)
            {
                int pos = -1;
                if (con.objectData._customData != null)
                    pos = (con.objectData?._customData["_propID"]?.AsInt ?? -1) + 1;
                if ((con as BeatmapEventContainer).eventData._type != MapEvent.EVENT_TYPE_RING_LIGHTS) pos = -1;
                con.transform.localPosition = new Vector3(pos + 0.5f, 0.5f, con.transform.localPosition.z);
            }
            else con.UpdateGridPosition();
        }
        SelectionController.RefreshMap();
    }

    //Because BeatmapEventContainers need to modify materials, we need to wait before we load by chunks.
    private IEnumerator WaitUntilChunkLoad()
    {
        yield return new WaitForSeconds(0.5f);
        UseChunkLoading = true;
    }

    void SpawnCallback(bool initial, int index, BeatmapObject objectData)
    {
        try
        {
            BeatmapObjectContainer e = LoadedContainers[index];
            e.SafeSetActive(true);
        }
        catch { }
    }

    //We don't need to check index as that's already done further up the chain
    void DespawnCallback(bool initial, int index, BeatmapObject objectData)
    {
        try //"Index was out of range. Must be non-negative and less than the size of the collection." Huh?
        {
            BeatmapObjectContainer e = LoadedContainers[index];
            e.SafeSetActive(false);
        }
        catch { }
    }

    void OnPlayToggle(bool playing)
    {
        if (playing) {
            foreach (BeatmapObjectContainer e in LoadedContainers)
            {
                bool enabled = e.objectData._time < AudioTimeSyncController.CurrentBeat + SpawnCallbackController.offset
                    && e.objectData._time >= AudioTimeSyncController.CurrentBeat + DespawnCallbackController.offset;
                e.SafeSetActive(enabled);
            }
        }
    }

    void RecursiveCheckFinished(bool natural, int lastPassedIndex)
    {
        OnPlayToggle(AudioTimeSyncController.IsPlaying);
    }

    public override BeatmapObjectContainer SpawnObject(BeatmapObject obj)
    {
        UseChunkLoading = false;
        BeatmapObjectContainer conflicting = LoadedContainers.FirstOrDefault(x => x.objectData._time == obj._time &&
            (obj as MapEvent)._type == (x.objectData as MapEvent)._type &&
            (obj as MapEvent)._customData == (x.objectData as MapEvent)._customData
        );
        if (conflicting != null) DeleteObject(conflicting);
        BeatmapEventContainer beatmapEvent = BeatmapEventContainer.SpawnEvent(obj as MapEvent, ref eventPrefab, ref eventAppearanceSO);
        beatmapEvent.transform.SetParent(GridTransform);
        beatmapEvent.UpdateGridPosition();
        if (RingPropagationEditing && (obj as MapEvent)._type == MapEvent.EVENT_TYPE_RING_LIGHTS)
        {
            int pos = 0;
            if (!(obj._customData is null)) pos = obj._customData["_propID"].AsInt + 1;
            Debug.Log(pos);
            beatmapEvent.transform.localPosition = new Vector3(pos + 0.5f, 0.5f, beatmapEvent.transform.localPosition.z);
        }
        LoadedContainers.Add(beatmapEvent);
        return beatmapEvent;
    }
}

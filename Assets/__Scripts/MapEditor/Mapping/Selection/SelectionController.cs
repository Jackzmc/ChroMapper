﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// Big boi master class for everything Selection.
/// </summary>
public class SelectionController : MonoBehaviour
{

    public static List<BeatmapObjectContainer> SelectedObjects = new List<BeatmapObjectContainer>();
    public static List<BeatmapObject> CopiedObjects = new List<BeatmapObject>();

    public static Action<BeatmapObjectContainer> ObjectWasSelectedEvent;

    [SerializeField] private AudioTimeSyncController atsc;
    [SerializeField] private Material selectionMaterial;
    [SerializeField] private Transform moveableGridTransform;
    private BeatmapObjectContainerCollection[] collections;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color copiedColor;

    private static SelectionController instance;

    // Use this for initialization
    void Start()
    {
        collections = moveableGridTransform.GetComponents<BeatmapObjectContainerCollection>();
        instance = this;
        SelectedObjects.Clear();
    }

    #region Utils

    /// <summary>
    /// Does the user have any selected objects?
    /// </summary>
    public static bool HasSelectedObjects()
    {
        return SelectedObjects.Count > 0;
    }

    /// <summary>
    /// Does the user have any copied objects?
    /// </summary>
    public static bool HasCopiedObjects()
    {
        return CopiedObjects.Count > 0;
    }

    /// <summary>
    /// Returns true if the given container is selected, and false if it's not.
    /// </summary>
    /// <param name="container">Container to check.</param>
    public static bool IsObjectSelected(BeatmapObjectContainer container)
    {
        return SelectedObjects.IndexOf(container) > -1;
    }

    #endregion

    #region Selection

    /// <summary>
    /// Select an individual container.
    /// </summary>
    /// <param name="container">The container to select.</param>
    /// <param name="AddsToSelection">Whether or not previously selected objects will deselect before selecting this object.</param>
    public static void Select(BeatmapObjectContainer container, bool AddsToSelection = false)
    {
        if (IsObjectSelected(container)) return; //Cant select an already selected object now, can ya?
        if (!AddsToSelection) DeselectAll(); //This SHOULD deselect every object unless you otherwise specify, but it aint working.
        SelectedObjects.Add(container);
        RefreshSelectionMaterial();
        ObjectWasSelectedEvent.Invoke(container);
        Debug.Log("Selected " + container.objectData.beatmapType.ToString());
    }

    public static void MassSelect<T>(T start, T end, bool AddsToSelection = false) where T : BeatmapObjectContainer
    {
        if (start.GetType() != end.GetType()) return;
        if (!AddsToSelection) DeselectAll();
        foreach(BeatmapObjectContainerCollection collection in instance.collections)
        {
            SelectedObjects.AddRange(collection.LoadedContainers.Where(x => x.objectData._time >= start.objectData._time &&
                x.objectData._time <= end.objectData._time && !IsObjectSelected(x) &&
                x.objectData.beatmapType == start.objectData.beatmapType));
        }
        RefreshSelectionMaterial();
    }

    /// <summary>
    /// Deselects a container if it is currently selected
    /// </summary>
    /// <param name="container">The container to deselect, if it has been selected.</param>
    public static void Deselect(BeatmapObjectContainer container)
    {
        SelectedObjects.RemoveAll(x => x == null);
        if (container == null || !IsObjectSelected(container)) return;
        SelectedObjects.Remove(container);
        RefreshSelectionMaterial();
        //We're doing this here instead of in the RefreshSelectionMaterial function so we do not loop through
        //potentially thousands of selected events. Not like that'll happen, but it'll still be good to do it once here.
        List<Material> containerMaterials = container.gameObject.GetComponentInChildren<MeshRenderer>().materials.ToList();
        if (containerMaterials.Count == 2) containerMaterials.Remove(containerMaterials.Last()); //Eh this should work.
        container.gameObject.GetComponentInChildren<MeshRenderer>().materials = containerMaterials.ToArray(); //Set materials
    }

    /// <summary>
    /// Deselect all selected objects.
    /// </summary>
    public static void DeselectAll()
    {
        SelectedObjects.RemoveAll(x => x == null);
        foreach (BeatmapObjectContainer con in SelectedObjects)
        {
            //Take all materials from the MeshRenderer of the container
            List<Material> containerMaterials = con.gameObject.GetComponentInChildren<MeshRenderer>().materials.ToList();
            if (containerMaterials.Count == 2) containerMaterials.Remove(containerMaterials.Last()); //Eh this should work.
            con.gameObject.GetComponentInChildren<MeshRenderer>().materials = containerMaterials.ToArray(); //Set materials
        }
        SelectedObjects.Clear();
    }

    /// <summary>
    /// Can be very taxing. Use sparringly.
    /// </summary>
    internal static void RefreshSelectionMaterial(bool triggersAction = true)
    {
        SelectedObjects.RemoveAll(x => x == null);
        foreach (BeatmapObjectContainer con in SelectedObjects)
        {
            //Take all materials from the MeshRenderer of the container
            List<Material> containerMaterials = con.gameObject.GetComponentInChildren<MeshRenderer>().materials.ToList();
            if (containerMaterials.Count == 1)
            {   //Because we're dealing with instances of a material, we need to check something else, like the name.
                Material matInstance = new Material(instance.selectionMaterial); //Create a copy of the material
                matInstance.name = instance.selectionMaterial.name; //Slap it the same name as the OG
                containerMaterials.Add(matInstance); //Add ourselves the selection material.
            }
            containerMaterials.Last().color = instance.selectedColor;
            con.gameObject.GetComponentInChildren<MeshRenderer>().materials = containerMaterials.ToArray(); //Set materials
        }
        if (triggersAction) BeatmapActionContainer.AddAction(new SelectionChangedAction(SelectedObjects));
    }

    #endregion

    #region Manipulation
    
    /// <summary>
    /// Deletes and clears the current selection.
    /// </summary>
    public void Delete(bool triggersAction = true)
    {
        if (triggersAction) BeatmapActionContainer.AddAction(new SelectionDeletedAction(SelectedObjects));
        foreach (BeatmapObjectContainer con in SelectedObjects)
            foreach (BeatmapObjectContainerCollection container in collections) container.DeleteObject(con);
        SelectedObjects.Clear();
        RefreshMap();
    }
    
    /// <summary>
    /// Copies the current selection for later Pasting.
    /// </summary>
    /// <param name="cut">Whether or not to delete the original selection after copying them.</param>
    public void Copy(bool cut = false)
    {
        Debug.Log("Copied!");
        CopiedObjects.Clear();
        SelectedObjects = SelectedObjects.OrderBy(x => x.objectData._time).ToList();
        float firstTime = SelectedObjects.First().objectData._time;
        foreach (BeatmapObjectContainer con in SelectedObjects)
        {
            BeatmapObject data = null;
            if (con.objectData is BeatmapNote)
                data = new BeatmapNote(con.objectData.ConvertToJSON());
            if (con.objectData is BeatmapObstacle)
                data = new BeatmapObstacle(con.objectData.ConvertToJSON());
            if (con.objectData is MapEvent)
                data = new MapEvent(con.objectData.ConvertToJSON());
            data._time = con.objectData._time - firstTime;
            CopiedObjects.Add(data);
            List<Material> containerMaterials = con.gameObject.GetComponentInChildren<MeshRenderer>().materials.ToList();
            containerMaterials.Last().SetColor("_OutlineColor", instance.copiedColor);
        }
        if (cut) Delete();
    }

    /// <summary>
    /// Pastes any copied objects into the map, selecting them immediately.
    /// </summary>
    public void Paste(bool triggersAction = true)
    {
        DeselectAll();
        CopiedObjects = CopiedObjects.OrderBy((x) => x._time).ToList();
        List<BeatmapObjectContainer> pasted = new List<BeatmapObjectContainer>();
        foreach (BeatmapObject data in CopiedObjects)
        {
            if (data == null) continue;
            float newTime = data._time + atsc.CurrentBeat;
            BeatmapObjectContainer pastedContainer = null;
            if (data is BeatmapNote)
            {
                BeatmapObject newData = new BeatmapNote(data.ConvertToJSON());
                newData._time = newTime;
                NotesContainer notes = collections.Where(x => x is NotesContainer).FirstOrDefault() as NotesContainer;
                pastedContainer = notes?.SpawnObject(newData);
            }
            if (data is BeatmapObstacle)
            {
                BeatmapObject newData = new BeatmapObstacle(data.ConvertToJSON());
                newData._time = newTime;
                ObstaclesContainer obstacles = collections.Where(x => x is ObstaclesContainer).FirstOrDefault() as ObstaclesContainer;
                pastedContainer = obstacles?.SpawnObject(newData);
            }
            if (data is MapEvent)
            {
                BeatmapObject newData = new MapEvent(data.ConvertToJSON());
                newData._time = newTime;
                EventsContainer events = collections.Where(x => x is EventsContainer).FirstOrDefault() as EventsContainer;
                pastedContainer = events?.SpawnObject(newData);
            }
            pasted.Add(pastedContainer);
        }
        if (triggersAction) BeatmapActionContainer.AddAction(new SelectionPastedAction(pasted, CopiedObjects, atsc.CurrentBeat));
        SelectedObjects.AddRange(pasted);
        RefreshSelectionMaterial(false);
        RefreshMap();
        Debug.Log("Pasted!");
    }

    public void MoveSelection(float beats)
    {
        foreach (BeatmapObjectContainer con in SelectedObjects)
        {
            con.objectData._time += beats;
            con.UpdateGridPosition();
        }
    }

    public void ShiftSelection(int leftRight, int upDown)
    {
        foreach(BeatmapObjectContainer con in SelectedObjects)
        {
            if (con is BeatmapNoteContainer note)
            {
                if (note.mapNoteData._lineIndex >= 1000)
                {
                    note.mapNoteData._lineIndex += Mathf.RoundToInt((1f / atsc.gridMeasureSnapping) * 1000 * leftRight);
                    if (note.mapNoteData._lineIndex < 1000) note.mapNoteData._lineIndex = 1000;
                }
                else if (note.mapNoteData._lineIndex <= -1000)
                {
                    note.mapNoteData._lineIndex += Mathf.RoundToInt((1f / atsc.gridMeasureSnapping) * 1000 * leftRight);
                    if (note.mapNoteData._lineIndex > -1000) note.mapNoteData._lineIndex = -1000;
                }
                else note.mapNoteData._lineIndex += leftRight;
                note.mapNoteData._lineLayer += upDown;
            }
            else if (con is BeatmapObstacleContainer obstacle)
            {
                if (obstacle.obstacleData._lineIndex >= 1000)
                {
                    obstacle.obstacleData._lineIndex += Mathf.RoundToInt((1f / atsc.gridMeasureSnapping) * 1000 * leftRight);
                    if (obstacle.obstacleData._lineIndex < 1000) obstacle.obstacleData._lineIndex = 1000;
                }
                else if (obstacle.obstacleData._lineIndex <= -1000)
                {
                    obstacle.obstacleData._lineIndex += Mathf.RoundToInt((1f / atsc.gridMeasureSnapping) * 1000 * leftRight);
                    if (obstacle.obstacleData._lineIndex > -1000) obstacle.obstacleData._lineIndex = -1000;
                }
                else obstacle.obstacleData._lineIndex += leftRight;
            }
            else if (con is BeatmapEventContainer e)
            {
                e.eventData._type += leftRight;
                if (e.eventData._type < 0) e.eventData._type = 0;
            }
            con.UpdateGridPosition();
        }
        RefreshMap();
    }

    public static void RefreshMap()
    {
        foreach (BeatmapObjectContainerCollection collection in instance.collections) collection.SortObjects();
        if (BeatSaberSongContainer.Instance.map != null)
        {
            List<BeatmapNote> newNotes = new List<BeatmapNote>();
            foreach (BeatmapObjectContainer n in instance.collections.Where(x => x is NotesContainer).FirstOrDefault()?.LoadedContainers)
                newNotes.Add((n as BeatmapNoteContainer).mapNoteData);
            List<BeatmapObstacle> newObstacles = new List<BeatmapObstacle>();
            foreach (BeatmapObjectContainer n in instance.collections.Where(x => x is ObstaclesContainer).FirstOrDefault()?.LoadedContainers)
                newObstacles.Add((n as BeatmapObstacleContainer).obstacleData);
            List<MapEvent> newEvents = new List<MapEvent>();
            foreach (BeatmapObjectContainer n in instance.collections.Where(x => x is EventsContainer).FirstOrDefault()?.LoadedContainers)
                newEvents.Add((n as BeatmapEventContainer).eventData);
            BeatSaberSongContainer.Instance.map._notes = newNotes;
            BeatSaberSongContainer.Instance.map._obstacles = newObstacles;
            BeatSaberSongContainer.Instance.map._events = newEvents;
        }
    }

    #endregion

}

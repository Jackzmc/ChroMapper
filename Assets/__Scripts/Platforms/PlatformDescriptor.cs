﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformDescriptor : MonoBehaviour {

    [Header("Rings")]
    [Tooltip("Leave null if you do not want small rings.")]
    public TrackLaneRingsManager SmallRingManager;
    [Tooltip("Leave null if you do not want big rings.")]
    public TrackLaneRingsManager BigRingManager;
    [Header("Lighting Groups")]
    [Tooltip("Manually map an Event ID (Index) to a group of lights (LightingManagers)")]
    public LightsManager[] LightingManagers = new LightsManager[] { };
    public Color RedColor = Color.red;
    public Color BlueColor = new Color(0, 0.282353f, 1, 1);
    [Tooltip("-1 = No Sorting | 0 = Default Sorting | 1 = Collider Platform Special")]
    public int SortMode = 0;


    private BeatmapObjectCallbackController callbackController;

    private Dictionary<LightsManager, Color> ChromaCustomColors = new Dictionary<LightsManager, Color>();

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "999_PrefabBuilding") StartCoroutine(FindEventCallback());
    }

    void OnDestroy()
    {
        if (callbackController != null)
            callbackController.EventPassedThreshold -= EventPassed;
    }

    IEnumerator FindEventCallback()
    {
        yield return new WaitUntil(() => GameObject.Find("Vertical Grid Callback"));
        callbackController = GameObject.Find("Vertical Grid Callback").GetComponent<BeatmapObjectCallbackController>();
        callbackController.EventPassedThreshold += EventPassed;
    }

    public void KillLights()
    {
        foreach (LightsManager manager in LightingManagers) manager.ChangeAlpha(0, 1);
    }

    public void EventPassed(bool initial, int index, BeatmapObject obj)
    {
        MapEvent e = obj as MapEvent; //Two events at the same time should yield same results
        System.Random rng = new System.Random(Mathf.RoundToInt(obj._time * 100));
        switch (e._type) { //FUN PART BOIS
            case 8:
                BigRingManager?.HandleRotationEvent();
                SmallRingManager?.HandleRotationEvent();
                break;
            case 9:
                BigRingManager?.HandlePositionEvent();
                SmallRingManager?.HandlePositionEvent();
                break;
            case 12:
                foreach (RotatingLights l in LightingManagers[MapEvent.EVENT_TYPE_LEFT_LASERS].RotatingLights)
                {
                    l.Offset = rng.Next(-20, 20);
                    l.Speed = e._value;
                }
                break;
            case 13:
                foreach (RotatingLights r in LightingManagers[MapEvent.EVENT_TYPE_RIGHT_LASERS].RotatingLights)
                {
                    r.Offset = rng.Next(-20, 20);
                    r.Speed = e._value;
                }
                break;
            default:
                if (e._type < LightingManagers.Length && LightingManagers[e._type] != null)
                    HandleLights(LightingManagers[e._type], e._value, e);
                break;
        }
    }

    void HandleLights(LightsManager group, int value, MapEvent e)
    {
        Color color = Color.white;

        //Check if its a legacy Chroma RGB event
        if (value >= ColourManager.RGB_INT_OFFSET)
        {
            if (ChromaCustomColors.ContainsKey(group)) ChromaCustomColors[group] = ColourManager.ColourFromInt(value);
            else ChromaCustomColors.Add(group, ColourManager.ColourFromInt(value));
            return;
        }
        else if (value == ColourManager.RGB_RESET)
            if (ChromaCustomColors.TryGetValue(group, out Color _)) ChromaCustomColors.Remove(group);

        //Set initial light values
        if (value <= 3) color = BlueColor;
        else if (value <= 7) color = RedColor;
        if (ChromaCustomColors.ContainsKey(group)) color = ChromaCustomColors[group];

        //Grab big ring propogation if any
        TrackLaneRing ring = null;
        if (e._type == MapEvent.EVENT_TYPE_RING_LIGHTS && e._customData?["_propID"] != null)
        {
            int propID = e._customData["_propID"].AsInt;
            ring = BigRingManager.rings[propID];
        }

        if (value == MapEvent.LIGHT_VALUE_OFF) group.ChangeAlpha(0, 0, ring);
        else if (value == MapEvent.LIGHT_VALUE_BLUE_ON || value == MapEvent.LIGHT_VALUE_RED_ON)
        {
            group.ChangeAlpha(1, 0, ring);
            group.ChangeColor(color, 0, ring);
        }
        else if (value == MapEvent.LIGHT_VALUE_BLUE_FLASH || value == MapEvent.LIGHT_VALUE_RED_FLASH)
        {
            group.ChangeAlpha(1, LightsManager.FlashTime, ring);
            group.ChangeColor(color, 0, ring);
        }
        else if (value == MapEvent.LIGHT_VALUE_BLUE_FADE || value == MapEvent.LIGHT_VALUE_RED_FADE)
            group.Fade(color, ring);
    }
}

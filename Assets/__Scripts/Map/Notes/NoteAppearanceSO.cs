﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoteAppearanceSO", menuName = "Map/Appearance/Note Appearance SO")]
public class NoteAppearanceSO : ScriptableObject {
    
    [SerializeField] private GameObject notePrefab;
    [Space(10)]
    [SerializeField] private Sprite unknownSprite;
    [SerializeField] private Sprite arrowSprite;
    [SerializeField] private Sprite dotSprite;
    [Space(10)]
    [SerializeField] private Material unknownNoteMaterial;
    [Space(10)]
    [SerializeField] private Material blueNoteSharedMaterial;
    [SerializeField] private Material redNoteSharedMaterial;
    [Space(20)]
    [Header("ChromaToggle Notes")]
    [SerializeField] private Sprite deflectSprite;
    [Space(10)]
    [SerializeField] private Material greenNoteSharedMaterial;
    [SerializeField] private Material magentaNoteSharedMaterial;
    [Space(10)]
    [SerializeField] private Material monochromeSharedNoteMaterial;
    [SerializeField] private Material duochromeSharedNoteMaterial;
    [Space(10)]
    [SerializeField] private Material superNoteSharedMaterial;

    public void SetNoteAppearance(BeatmapNoteContainer note) {
        if (!note.isBomb)
        {
            if (note.gameObject.transform.Find("Bidirectional"))
                Destroy(note.gameObject.transform.Find("Bidirectional").gameObject);
            Transform dot = note.gameObject.transform.Find("NoteDot");
            dot.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            switch (note.mapNoteData._cutDirection) {
                case BeatmapNote.NOTE_CUT_DIRECTION_UP:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_DOWN:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_LEFT:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_RIGHT:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_UP_RIGHT:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_UP_LEFT:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_DOWN_LEFT:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_DOWN_RIGHT:
                    //note.SetArrowSprite(arrowSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
                case BeatmapNote.NOTE_CUT_DIRECTION_ANY:
                    note.SetDotSprite(dotSprite);
                    note.SetArrowVisible(false);
                    note.SetDotVisible(true);
                    break;
                default:
                    note.SetDotSprite(dotSprite);
                    note.SetArrowVisible(true);
                    note.SetDotVisible(false);
                    break;
            }

            switch (note.mapNoteData._type)
            {
                case BeatmapNote.NOTE_TYPE_A:
                    note.SetModelMaterial(redNoteSharedMaterial);
                    break;
                case BeatmapNote.NOTE_TYPE_B:
                    note.SetModelMaterial(blueNoteSharedMaterial);
                    break;
                default:
                    note.SetModelMaterial(unknownNoteMaterial);
                    break;
            }
            if (note.mapNoteData is BeatmapChromaNote)
            {
                BeatmapChromaNote chromaNote = note.mapNoteData as BeatmapChromaNote;
                switch (chromaNote.BombRotation)
                {
                    case BeatmapChromaNote.ALTERNATE:
                        if (note.mapNoteData._type == BeatmapNote.NOTE_TYPE_A) note.SetModelMaterial(magentaNoteSharedMaterial);
                        else if (note.mapNoteData._type == BeatmapNote.NOTE_TYPE_B) note.SetModelMaterial(greenNoteSharedMaterial);
                        break;
                    case BeatmapChromaNote.BIDIRECTIONAL:
                        note.SetArrowVisible(true);
                        note.SetDotVisible(false);
                        Transform copied = Instantiate(note.gameObject.transform.Find("Direction"), note.transform);
                        copied.gameObject.name = "Bidirectional";
                        copied.localEulerAngles = new Vector3(0, 0, 180);
                        copied.localPosition = new Vector3(0, -0.1f, -0.25f);
                        break;
                    case BeatmapChromaNote.DUOCHROME:
                        note.SetModelMaterial(duochromeSharedNoteMaterial);
                        break;
                    case BeatmapChromaNote.HOT_GARBAGE:
                        note.SetModelMaterial(superNoteSharedMaterial);
                        break;
                    case BeatmapChromaNote.DEFLECT:
                        dot.localScale = new Vector3(0.2f, 0.5f, 0.2f);
                        note.SetArrowVisible(false);
                        note.SetDotVisible(true);
                        break;
                    case BeatmapChromaNote.MONOCHROME:
                        note.SetModelMaterial(monochromeSharedNoteMaterial);
                        break;
                }
            }
        }
    }
}

﻿using System.Linq;
using System.Collections;
using UnityEngine;

public class UIWorkflowToggle : MonoBehaviour
{
    [SerializeField] private RectTransform[] workflowGroups;

    public int selectedWorkflowGroup = 0;

    private IEnumerator UpdateGroup(float dest, RectTransform group)
    {
        float og = group.anchoredPosition.y;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            group.anchoredPosition = new Vector2(group.anchoredPosition.x, Mathf.Lerp(og, dest, t));
            og = group.anchoredPosition.y;
            yield return new WaitForEndOfFrame();
        }
        group.anchoredPosition = new Vector2(group.anchoredPosition.x, dest);
    }

    public void UpdateWorkflowGroup()
    {
        selectedWorkflowGroup++;
        if (selectedWorkflowGroup >= workflowGroups.Length) selectedWorkflowGroup = 0;
        for (int i = 0; i < workflowGroups.Length; i++)
            StartCoroutine(UpdateGroup(i == selectedWorkflowGroup ? 0 : 35, workflowGroups[i]));
    }
}

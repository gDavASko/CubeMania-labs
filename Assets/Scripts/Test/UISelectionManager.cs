using System;
using UnityEngine;

public class UISelectionManager : MonoBehaviour
{
    [SerializeField] private RectTransform selectionRect;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        selectionRect.gameObject.SetActive(false);

        UnitSelectionManager.Instance.OnSelectionStart += UnitSelectionManager_OnSelectionStart;
        UnitSelectionManager.Instance.OnSelectionEnd += UnitSelectionManager_OnSelectionEnd;
    }

    private void Update()
    {
        if (selectionRect.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }

    private void UnitSelectionManager_OnSelectionStart(object sender, EventArgs e)
    {
        selectionRect.gameObject.SetActive(true);
        UpdateVisual();
    }

    private void UnitSelectionManager_OnSelectionEnd(object sender, EventArgs e)
    {
        selectionRect.gameObject.SetActive(false);
    }    

    private void UpdateVisual()
    {
        var rect = UnitSelectionManager.Instance.GetSelectionRect();

        float cScale = canvas.transform.localScale.x;

        selectionRect.anchoredPosition = new Vector2(rect.x, rect.y) / cScale;
        selectionRect.sizeDelta = new Vector2(rect.width, rect.height) / cScale;
    }
}

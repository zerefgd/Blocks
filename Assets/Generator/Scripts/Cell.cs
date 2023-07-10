using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int CellValue;
    [SerializeField] private List<Sprite> _cellSprites;
    [SerializeField] private SpriteRenderer _cellRenderer;

    private int spriteIndex => CellValue + 1;

    public void Init(int cellValue)
    {
        CellValue = cellValue;
        _cellRenderer.sprite = _cellSprites[spriteIndex];
    }
}

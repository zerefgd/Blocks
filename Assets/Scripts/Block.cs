using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector3 CurrentPos => new Vector3(currentPos.x,currentPos.y, 0);

    [SerializeField] private SpriteRenderer _blockPrefab;
    [SerializeField] private List<Sprite> _blockSprites;
    [SerializeField] private float _blockSpawnSize;

    private Vector3 startPos;
    private Vector3 previousPos;
    private Vector3 currentPos;
    private List<SpriteRenderer> blockSprites;
    private List<Vector2Int> blockPositions;

    private const int TOP = 1;
    private const int BOTTOM = 0;

    public void Init(List<Vector2Int> blocks,Vector3 start, int blockNum)
    {
        startPos = start;
        previousPos = start;
        currentPos = start;
        blockPositions = blocks;
        blockSprites = new List<SpriteRenderer>();
        for (int i = 0; i < blockPositions.Count; i++)
        {
            SpriteRenderer spawnedBlock = Instantiate(_blockPrefab, transform);
            spawnedBlock.sprite = _blockSprites[blockNum + 1];
            spawnedBlock.transform.localPosition = new Vector3(blockPositions[i].y,
                blockPositions[i].x, 0);
            blockSprites.Add(spawnedBlock);
        }
        transform.localScale = Vector3.one * _blockSpawnSize;
        ElevateSprites(true);
    }

    public void UpdatePos(Vector3 offset)
    {
        currentPos += offset;
        transform.position = currentPos;
    }

    public void ElevateSprites(bool reverse = false)
    {
        foreach (var blockSprite in blockSprites)
        {
            blockSprite.sortingOrder = reverse ? BOTTOM : TOP;
        }
    }

    public List<Vector2Int> BlockPositions()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (var pos in blockPositions)
        {
            result.Add(pos + new Vector2Int(
                Mathf.FloorToInt(currentPos.y),
                Mathf.FloorToInt(currentPos.x)
                ));
        }
        return result;
    }

    public void UpdateIncorrectMove()
    {
        currentPos = previousPos;
        transform.position = currentPos;
    }

    public void UpdateStartMove()
    {
        currentPos = startPos;
        previousPos = startPos;
        transform.position = currentPos;
    }

    public void UpdateCorrectMove()
    {
        currentPos.x = Mathf.FloorToInt(currentPos.x) + 0.5f;
        currentPos.y = Mathf.FloorToInt(currentPos.y) + 0.5f;
        previousPos = currentPos;
        transform.position = currentPos;
    }
}

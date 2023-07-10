using System.Collections.Generic;
using UnityEngine;

public class SpawnedBlock : MonoBehaviour
{
    [SerializeField] private Transform _blockPrefab;
    [SerializeField] private List<Sprite> _blockSprites;
    [SerializeField] private float _blockSize;

    public void Init(BlockPiece piece, Vector3 gridStart)
    {
        transform.localScale = Vector3.one * _blockSize;
        transform.position = gridStart +
            new Vector3(piece.StartPos.y * _blockSize,
            piece.StartPos.x * _blockSize,
            0);
        Sprite currentSprite = _blockSprites[piece.Id + 1];
        for (int i = 0; i < piece.BlockPositions.Count; i++)
        {
            Transform block = Instantiate(_blockPrefab, transform);
            block.transform.localPosition = new Vector3(
                piece.BlockPositions[i].y,
                piece.BlockPositions[i].x,
                0);
            block.GetComponent<SpriteRenderer>().sprite = currentSprite;
        }
    }
}

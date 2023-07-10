using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Level _level;
    [SerializeField] private BGCell _bgCellPrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private float _blockSpawnSize;
    [SerializeField] private float _blockHighLightSize;
    [SerializeField] private float _blockPutSize;

    private BGCell[,] bgCellGrid;
    private bool hasGameFinished;
    private Block currentBlock;
    private Vector2 currentPos, previousPos;
    private List<Block> gridBlocks;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        gridBlocks = new List<Block>();
        SpawnGrid();
        SpawnBlocks();
    }

    private void SpawnGrid()
    {
        bgCellGrid = new BGCell[_level.Rows, _level.Columns];
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                BGCell bgcell = Instantiate(_bgCellPrefab);
                bgcell.transform.position = new Vector3(j + 0.5f, i + 0.5f, 0f);
                bgcell.Init(_level.Data[i * _level.Columns + j]);
                bgCellGrid[i, j] = bgcell;
            }
        }
    }

    private void SpawnBlocks()
    {
        Vector3 startPos = Vector3.zero;
        startPos.x = 0.25f + (_level.Columns - _level.BlockColumns * _blockSpawnSize) * 0.5f;
        startPos.y = -_level.BlockRows * _blockSpawnSize + 0.25f - 1f;

        for (int i = 0; i < _level.Blocks.Count; i++)
        {
            Block block = Instantiate(_blockPrefab);
            Vector2Int blockPos = _level.Blocks[i].StartPos;
            Vector3 blockSpawnPos = startPos
                + new Vector3(blockPos.y, blockPos.x, 0) * _blockSpawnSize;
            block.transform.position = blockSpawnPos;
            block.Init(_level.Blocks[i].BlockPositions, blockSpawnPos, _level.Blocks[i].Id);
        }

        float maxColumns = Mathf.Max(_level.Columns, _level.BlockColumns * _blockSpawnSize);
        float maxRows = _level.Rows + 2f + _level.BlockRows * _blockSpawnSize;
        Camera.main.orthographicSize = Mathf.Max(maxColumns, maxRows) * 0.65f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = _level.Columns * 0.5f;
        camPos.y = (_level.Rows + 0.5f + startPos.y) * 0.5f;
        Camera.main.transform.position = camPos;
    }

    private void Update()
    {
        if (hasGameFinished) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (!hit) return;
            currentBlock = hit.collider.transform.parent.GetComponent<Block>();
            if (currentBlock == null) return;
            currentPos = mousePos2D;
            previousPos = mousePos2D;
            currentBlock.ElevateSprites();
            currentBlock.transform.localScale = Vector3.one * _blockHighLightSize;
            if(gridBlocks.Contains(currentBlock))
            {
                gridBlocks.Remove(currentBlock);
            }
            UpdateFilled();
            ResetHighLight();
            UpdateHighLight();
        }
        else if(Input.GetMouseButton(0) && currentBlock != null) 
        {
            currentPos = mousePos;
            currentBlock.UpdatePos(currentPos - previousPos);
            previousPos = currentPos;
            ResetHighLight();
            UpdateHighLight();
        }
        else if(Input.GetMouseButtonUp(0) && currentBlock != null)
        {
            currentBlock.ElevateSprites(true);
            
            if(IsCorrectMove())
            {
                currentBlock.UpdateCorrectMove();
                currentBlock.transform.localScale = Vector3.one * _blockPutSize;
                gridBlocks.Add(currentBlock);
            }
            else if(mousePos2D.y < 0)
            {
                currentBlock.UpdateStartMove();
                currentBlock.transform.localScale = Vector3.one * _blockSpawnSize;
            }
            else
            {
                currentBlock.UpdateIncorrectMove();
                if(currentBlock.CurrentPos.y > 0)
                {
                    gridBlocks.Add(currentBlock);
                    currentBlock.transform.localScale = Vector3.one * _blockPutSize;
                }
                else
                {
                    currentBlock.transform.localScale = Vector3.one * _blockSpawnSize;
                }
            }

            currentBlock = null;
            ResetHighLight();
            UpdateFilled();
            CheckWin();
        }
    }

    private void ResetHighLight()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!bgCellGrid[i, j].IsBlocked)
                {
                    bgCellGrid[i, j].ResetHighLight();
                }
            }
        }
    }

    private void UpdateFilled()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!bgCellGrid[i, j].IsBlocked)
                {
                    bgCellGrid[i, j].IsFilled = false;
                }
            }
        }

        foreach (var block in gridBlocks)
        {
            foreach (var pos in block.BlockPositions())
            {
                if(IsValidPos(pos))
                {
                    bgCellGrid[pos.x,pos.y].IsFilled = true;
                }
            }
        }
    }

    private void UpdateHighLight()
    {
        bool isCorrect = IsCorrectMove();
        foreach (var pos in currentBlock.BlockPositions())
        {
            if (IsValidPos(pos))
            {
                bgCellGrid[pos.x, pos.y].UpdateHighlight(isCorrect);
            }
        }
    }

    private bool IsCorrectMove()
    {
        foreach (var pos in currentBlock.BlockPositions())
        {
            if (!IsValidPos(pos) || bgCellGrid[pos.x, pos.y].IsFilled)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsValidPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _level.Rows && pos.y < _level.Columns;
    }

    private void CheckWin()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!bgCellGrid[i, j].IsFilled)
                {
                    return;
                }
            }
        }

        hasGameFinished = true;
        StartCoroutine(GameWin());
    }

    private IEnumerator GameWin()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}

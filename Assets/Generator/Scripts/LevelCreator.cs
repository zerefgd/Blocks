using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public static LevelCreator Instance;

    [SerializeField] private int _rows;
    [SerializeField] private int _columns;
    [SerializeField] private int _spawnRows;
    [SerializeField] private int _spawnColumns;
    [SerializeField] private Transform _spawnBGPrefab;
    [SerializeField] private Level _level;
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Transform _centerPrefab;
    [SerializeField] private float _blockSpawnSize = 0.5f;
    [SerializeField] private List<Sprite> _blockSprites;
    [SerializeField] private SpawnedBlock _blockPrefab;

    private bool isNewLevel;
    private Cell[,] gridCells;
    private int currentCellFillValue;
    private Dictionary<int, Vector2Int> startCenters;
    private List<Transform> centerObjects;
    private Dictionary<int, SpawnedBlock> spawnedBlocks;
    private Vector3 startPos;

    private void Awake()
    {
        Instance = this;
        SpawnBlock();
        SpawnGrid();
    }

    private void SpawnBlock()
    {
        isNewLevel = !(_rows == _level.Rows && _columns == _level.Columns);

        if (isNewLevel)
        {
            _level.Rows = _rows;
            _level.Columns = _columns;
            _level.BlockRows = _spawnRows;
            _level.BlockColumns = _spawnColumns;
            _level.Blocks = new List<BlockPiece>();
            _level.Data = new List<int>();
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _level.Data.Add(-1);
                }
            }
        }

        gridCells = new Cell[_rows, _columns];
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                gridCells[i, j] = Instantiate(_cellPrefab);
                gridCells[i, j].Init(_level.Data[i * _columns + j]);
                gridCells[i, j].transform.position = new Vector3(j + 0.5f, i + 0.5f, 0f);
            }
        }

        currentCellFillValue = -1;
    }

    private void SpawnGrid()
    {
        startPos = Vector3.zero;
        startPos.x = 0.25f + (_level.Columns - _level.BlockColumns * _blockSpawnSize) * 0.5f;
        startPos.y = -_level.BlockRows * +_blockSpawnSize - 1f + 0.25f;

        for (int i = 0; i < _spawnRows; i++)
        {
            for (int j = 0; j < _spawnColumns; j++)
            {
                Vector3 spawnPos = startPos + new Vector3(j, i, 0) * _blockSpawnSize;
                Transform spawnCell = Instantiate(_spawnBGPrefab);
                spawnCell.position = spawnPos;
            }
        }

        float maxColumns = Mathf.Max(_level.Columns, _level.BlockColumns * _blockSpawnSize);
        float maxRows = _level.Rows + 2f + _level.BlockRows * _blockSpawnSize;
        Camera.main.orthographicSize = Mathf.Max(maxColumns, maxRows) * 0.65f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = _level.Columns * 0.5f;
        camPos.y = (_level.Rows + 0.5f + startPos.y) * 0.5f;
        Camera.main.transform.position = camPos;

        //Set StartCenters
        startCenters = new Dictionary<int, Vector2Int>();
        centerObjects = new List<Transform>();
        spawnedBlocks = new Dictionary<int, SpawnedBlock>();

        List<Sprite> sprites = _blockSprites;

        for (int i = 1; i < sprites.Count; i++)
        {
            spawnedBlocks[i - 1] = null;
            startCenters[i - 1] = Vector2Int.zero;
            centerObjects.Add(Instantiate(_centerPrefab));
            centerObjects[i - 1].GetChild(0).GetComponent<SpriteRenderer>().sprite =
                sprites[i];
            centerObjects[i - 1].gameObject.SetActive(false);
        }

        for (int i = 0; i < _level.Blocks.Count; i++)
        {
            int tempId = _level.Blocks[i].Id;
            Vector2Int pos = _level.Blocks[i].CenterPos;
            centerObjects[tempId].gameObject.SetActive(true);
            centerObjects[tempId].transform.position =
                new Vector3(pos.y + 0.5f, pos.x + 0.5f, 0f);
            spawnedBlocks[tempId] = Instantiate(_blockPrefab);
            spawnedBlocks[tempId].Init(_level.Blocks[i], startPos);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Set Grid Pos
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int mousePosGrid = new Vector2Int(
                Mathf.FloorToInt(mousePos.y),
                Mathf.FloorToInt(mousePos.x)
                );
            if (!IsValidPosition(mousePosGrid)) return;
            gridCells[mousePosGrid.x, mousePosGrid.y].Init(currentCellFillValue);
            _level.Data[mousePosGrid.x * _columns + mousePosGrid.y] = currentCellFillValue;
            EditorUtility.SetDirty(_level);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int mousePosGrid = new Vector2Int(
                Mathf.FloorToInt(mousePos.y),
                Mathf.FloorToInt(mousePos.x)
                );
            if (!IsValidPosition(mousePosGrid)) return;
            if (currentCellFillValue == -1) return;            
            centerObjects[currentCellFillValue].gameObject.SetActive(true);
            centerObjects[currentCellFillValue].transform.position = new Vector3(
                mousePosGrid.y + 0.5f,
                mousePosGrid.x + 0.5f,
                0
                );
            startCenters[currentCellFillValue] = mousePosGrid;
            EditorUtility.SetDirty(_level);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (currentCellFillValue == -1) return;
            BlockPiece spawnedPiece = GetBlockPiece();
            for (int i = 0; i < _level.Blocks.Count; i++)
            {
                if (_level.Blocks[i].Id == spawnedPiece.Id)
                {
                    _level.Blocks.RemoveAt(i);
                    i--;
                }
            }
            _level.Blocks.Add(spawnedPiece);
            if (spawnedBlocks[currentCellFillValue] != null)
            {
                Destroy(spawnedBlocks[currentCellFillValue].gameObject);
            }
            spawnedBlocks[currentCellFillValue] = Instantiate(_blockPrefab);
            spawnedBlocks[currentCellFillValue].Init(spawnedPiece, startPos);
            EditorUtility.SetDirty(_level);
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            MoveBlock(Vector2Int.down);
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            MoveBlock(Vector2Int.up);
        }
        else if(Input.GetKeyDown(KeyCode.S))
        {
            MoveBlock(Vector2Int.left);
        }
        else if( Input.GetKeyDown(KeyCode.W))
        {
            MoveBlock(Vector2Int.right);
        }
    }

    private void MoveBlock(Vector2Int offset)
    {
        for (int i = 0; i < _level.Blocks.Count; i++)
        {
            if (_level.Blocks[i].Id == currentCellFillValue)
            {
                Vector2Int pos = _level.Blocks[i].StartPos;
                pos.x += offset.x;
                pos.y += offset.y;
                BlockPiece piece = _level.Blocks[i];
                piece.StartPos = pos;
                _level.Blocks[i] = piece;
                Vector3 movePos = spawnedBlocks[currentCellFillValue].transform.position;
                movePos.x += offset.y * _blockSpawnSize;
                movePos.y += offset.x * _blockSpawnSize;
                spawnedBlocks[currentCellFillValue].transform.position = movePos;
            }
        }
        EditorUtility.SetDirty(_level);
    }

    private BlockPiece GetBlockPiece()
    {
        int id = currentCellFillValue;
        BlockPiece result = new BlockPiece();
        result.Id = id;
        result.CenterPos = startCenters[id];
        result.StartPos = Vector2Int.zero;
        result.BlockPositions = new List<Vector2Int>();
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if (gridCells[i,j].CellValue == id)
                {
                    result.BlockPositions.Add(new Vector2Int(i, j) - result.CenterPos);
                }
            }
        }
        return result;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _rows && pos.y < _columns;
    }

    public void ChangeCellFillValue(int value)
    {
        currentCellFillValue = value;
    }
}

using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GridsController : MonoBehaviour
{
    [SerializeField]
    private float pixelsPerUnit = 100f;
    /// <summary>
    /// 每个单位的像素数
    /// </summary>
    public float PixelsPerUnit { get { return pixelsPerUnit; } }
    [SerializeField]
    private float gridPreWidth = 80;
    /// <summary>
    /// 每个格子的像素宽度
    /// </summary>
    public float GridPreWidth { get { return gridPreWidth; } }
    [SerializeField]
    private float gridPreHeight = 80;
    /// <summary>
    /// 每个格子的像素高度
    /// </summary>
    public float GridPreHeight { get { return gridPreHeight; } }

    [SerializeField]
    private int gridWidthCount = 12;
    [SerializeField]
    private int gridHeightCount = 26;
    [SerializeField]
    private Transform gridsTrans;
    public Transform GridsTrans { get { return gridsTrans; } }
    [SerializeField]
    private Transform gridsBackgroundParent;
    [SerializeField]
    private float fallingDownTime = 0.5f;
    /// <summary>
    /// 形状向下走一格的时间
    /// </summary>
    public float FallingDownTime { get { return fallingDownTime; } set { fallingDownTime = value; } }
    [SerializeField]
    private Transform gridsListTrans;
    /// <summary>
    /// 已经落下的格子列表组物体
    /// </summary>
    public Transform GridsListTrans { get { return gridsListTrans; } }
    private Quaternion singleRotateAngle;
    /// <summary>
    /// 图形单次旋转角度
    /// </summary>
    public Quaternion SingleRotateAngle { get { return singleRotateAngle; } }

    /// <summary>
    /// 格子总宽度
    /// </summary>
    public int GridWidthCount { get { return gridWidthCount; } set { gridWidthCount = value; } }
    /// <summary>
    /// 格子总高度
    /// </summary>
    public int GridHeightCount { get { return gridHeightCount; } set { gridHeightCount = value; } }

    /// <summary>
    /// 背景格子预设体
    /// </summary>
    private GameObject backgroundGridPrefab;

    /// <summary>
    /// 当前游戏的格子数据
    /// </summary>
    private GridData[,] gridDatas;
    /// <summary>
    /// 形状字典
    /// </summary>
    private Dictionary<GridShapeType, GridShape> gridShapesDict = new Dictionary<GridShapeType, GridShape>();
    /// <summary>
    /// 形状权重列表
    /// </summary>
    private List<(GridShapeType value, int weight)> gridShapesWeightList = new List<(GridShapeType, int)>();
    /// <summary>
    /// 正在下落的形状
    /// </summary>
    private GridShape fallingDownShape = null;
    /// <summary>
    /// 正在下落的形状的异步任务取消令牌
    /// </summary>
    private CancellationTokenSource cts;

#if UNITY_EDITOR
    [SerializeField]
    private bool debugModel = false;
    public bool DebugModel { get { return debugModel; } }
#endif

    [SerializeField]
    private float moveDownTweenTime = 0.2f;
    /// <summary>
    /// 下落动画时长
    /// </summary>
    public float MoveDownTweenTime { get { return moveDownTweenTime; } set { moveDownTweenTime = value; } }

    private void Awake()
    {
        //每次顺时针旋转90度
        singleRotateAngle = Quaternion.Euler(0, 0, -90);
        backgroundGridPrefab = Resources.Load<GameObject>("Prefabs/BackgroundGridPrefab");

        InitBackgroundGrids();
        InitGridDatas();
        InitShapes();
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public virtual void StartGame()
    {
#if UNITY_EDITOR
        if (!debugModel)
            RandomGenerateShape();
#else
        RandomGenerateShape();
#endif
    }

    /// <summary>
    /// 随机生成一个形状
    /// </summary>
    public void RandomGenerateShape()
    {
        GridShapeType shape = RandomGenerateShapeByWeighted();
        AddShape(shape);
    }

    /// <summary>
    /// 根据权重随机生成一个形状
    /// </summary>
    /// <returns></returns>
    private GridShapeType RandomGenerateShapeByWeighted()
    {
        int totalWeight = gridShapesWeightList.Sum(item => item.weight);
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentSum = 0;
        foreach (var item in gridShapesWeightList)
        {
            currentSum += item.weight;
            if (randomValue < currentSum)
                return item.value;
        }

        Debug.LogWarning($"随机生成形状溢出：{currentSum}");
        return GridShapeType.Single;
    }

    /// <summary>
    /// 添加一个形状
    /// </summary>
    /// <param name="gridShapeType"></param>
    /// <param name="pos"></param>
    /// <param name="gridMaterial"></param>
    public void AddShape(GridShapeType gridShapeType, Vector2Int pos, GridMaterial gridMaterial)
    {
        if (!gridShapesDict.TryGetValue(gridShapeType, out GridShape gridShape))
        {
            Debug.LogError($"未注册的形状：{gridShapeType}");

            return;
        }
        if (fallingDownShape != null)
        {
            Debug.LogError($"同时只能有一个形状下落");

            return;
        }

        if (gridMaterial == null)
            gridShape.Create(pos);
        else
            gridShape.Create(pos, gridMaterial);
        //开始执行下落逻辑
        StartFallingDown(gridShape);
    }

    /// <summary>
    /// 添加一个形状
    /// </summary>
    /// <param name="gridShapeType"></param>
    /// <param name="pos"></param>
    public void AddShape(GridShapeType gridShapeType, Vector2Int pos)
    {
        AddShape(gridShapeType, pos, null);
    }

    /// <summary>
    /// 添加一个形状
    /// </summary>
    /// <param name="gridShapeType"></param>
    public void AddShape(GridShapeType gridShapeType)
    {
        //生成在顶部中间的位置
        AddShape(gridShapeType, new Vector2Int(Mathf.RoundToInt((GridWidthCount * 0.5f)), GridHeightCount));
    }

    /// <summary>
    /// 形状开始下落
    /// </summary>
    /// <param name="gridShape"></param>
    private void StartFallingDown(GridShape gridShape)
    {
        fallingDownShape = gridShape;

        cts = new CancellationTokenSource();
        FallingDownAsync(cts.Token);
    }

    /// <summary>
    /// 形状下落协程
    /// </summary>
    /// <returns></returns>
    private async void FallingDownAsync(CancellationToken token)
    {
        try
        {
            while (true)
            {
                await UniTask.Delay((int)TimeSpan.FromSeconds(FallingDownTime).TotalMilliseconds, cancellationToken: token);

                while (GameManager.Instance.Pausing)
                    await UniTask.DelayFrame(1, cancellationToken: token);

                //判断是否可以下落
                if (fallingDownShape.CanFallDown())
                    fallingDownShape.FallDown();
                else
                {
                    //不能下落了，结束循环
                    await fallingDownShape.FallDownEnd();
                    fallingDownShape = null;
                    break;
                }
            }
            //检查是否存在可以消除的行
            await TryEliminate();
            //检查游戏是否结束
            bool isGameOver = CheckGameOver();
            if (isGameOver)
                return;

            //随机生成下一个形状
#if UNITY_EDITOR
            if (!debugModel)
                RandomGenerateShape();
#else
    RandomGenerateShape();
#endif
        }
        catch (OperationCanceledException)
        {

        }
    }

    /// <summary>
    /// 检查游戏是否结束
    /// </summary>
    private bool CheckGameOver()
    {
        //如果最上面一行有元素则游戏结束
        for (int x = 0; x < GridWidthCount; x++)
        {
            GridData gridData = GetGridData(x, GridHeightCount - 1);
            if (gridData != null)
            {
                Debug.Log("游戏结束");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 尝试消除
    /// </summary>
    public async UniTask TryEliminate()
    {
        List<int> eliminateLineList = new List<int>();
        for (int y = 0; y < GridHeightCount; y++)
        {
            bool isFull = true;
            for (int x = 0; x < GridWidthCount; x++)
            {
                GridData gridData = GetGridData(x, y);
                if (gridData == null)
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull)
            {
                //这一行满了，消除这一行
                for (int x = 0; x < GridWidthCount; x++)
                {
                    var gridData = GetGridData(x, y);
                    gridData.Eliminate(x, y);
                    Destroy(gridData.GameObject);

                    SetGridData(x, y, null);
                }
                //播放消除特效
                Effects.Instance.PlayEliminateEffect(y);

                eliminateLineList.Add(y);
            }
        }
        //如果需要计算下落
        if (eliminateLineList.Count > 0)
        {
            GridDropDown(eliminateLineList);
            await UniTask.Delay((int)TimeSpan.FromSeconds(FallingDownTime).TotalMilliseconds);
        }
    }

    /// <summary>
    /// 按整行下落
    /// </summary>
    /// <param name="eliminateLineList"></param>
    private void GridDropDown(List<int> eliminateLineList)
    {
        if (eliminateLineList == null || eliminateLineList.Count <= 0) return;

        int addition = 0;
        int originY = eliminateLineList[0];
        for (int y = eliminateLineList[0] + 1; y < GridHeightCount; y++)
        {
            if (!eliminateLineList.Contains(y))
            {
                //向下找非空行
                int targetY = originY + addition;
                for (int x = 0; x < GridWidthCount; x++)
                {
                    MoveGrid(x, y, x, targetY);
                }
                ++addition;
            }
        }
    }

    /// <summary>
    /// 移动格子（包括数据移动和表现移动）
    /// </summary>
    /// <param name="ox"></param>
    /// <param name="oy"></param>
    /// <param name="tx"></param>
    /// <param name="ty"></param>
    public void MoveGrid(GridData gridData, int tx, int ty)
    {
        if (gridData == null) return;
        Vector2Int oPos = GetGridIndex(gridData);

        SetGridData(oPos.x, oPos.y, null);
        SetGridData(tx, ty, gridData);

        Vector2 pos = GetGridPosition(tx, ty);
        gridData.GameObject.transform.DOLocalMove(pos, moveDownTweenTime);
    }

    /// <summary>
    /// 移动格子（包括数据移动和表现移动）
    /// </summary>
    /// <param name="ox"></param>
    /// <param name="oy"></param>
    /// <param name="tx"></param>
    /// <param name="ty"></param>
    public void MoveGrid(int ox, int oy, int tx, int ty)
    {
        GridData data = GetGridData(ox, oy);
        if (data == null) return;

        SetGridData(ox, oy, null);
        SetGridData(tx, ty, data);

        Vector2 pos = GetGridPosition(tx, ty);
        data.GameObject.transform.DOLocalMove(pos, moveDownTweenTime);
    }

    /// <summary>
    /// 指定位置的格子下落到可用位置
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void GridDropDownToEmpty(int x, int y)
    {
        GridData data = GetGridData(x, y);
        if (data != null)
        {
            int downTargetPosY = GetFirstEmptyGrid(x, y);
            if (downTargetPosY != -1)
            {
                SetGridData(x, y, null);
                SetGridData(x, downTargetPosY, data);

                Vector2 pos = GetGridPosition(x, y - 1);
                data.GameObject.transform.DOLocalMove(pos, moveDownTweenTime);
            }
        }
    }

    /// <summary>
    /// 向下找第一个非空的上一行位置，没有空格子时返回 -1
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private int GetFirstEmptyGrid(int x, int y)
    {
        //向下找到第一个非空格子的上一个位置
        int downTargetPosY = -1;
        for (int ySub = y - 1; ySub >= 0; ySub--)
        {
            GridData dataSub = GetGridData(x, ySub);
            if (dataSub != null)
                break;
            if (ySub == 0)
            {
                downTargetPosY = ySub;
                break;
            }
            GridData dataSub2 = GetGridData(x, ySub - 1);
            if (dataSub2 != null)
            {
                downTargetPosY = ySub;
                break;
            }
        }

        return downTargetPosY;
    }

    /// <summary>
    /// 获取指定位置的格子数据
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public GridData GetGridData(Vector2Int pos)
    {
        return GetGridData(pos.x, pos.y);
    }

    /// <summary>
    /// 获取指定位置的格子数据
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public GridData GetGridData(int x, int y)
    {
        if (x < 0 || x >= GridWidthCount || y < 0 || y >= GridHeightCount)
        {
            //Debug.LogError($"获取格子数据失败，坐标越界，x = {x}, y = {y}");
            return null;
        }
        return gridDatas[x, y];
    }

    /// <summary>
    /// 获取格子所在的索引
    /// </summary>
    /// <param name="gridData"></param>
    /// <returns></returns>
    public Vector2Int GetGridIndex(GridData gridData)
    {
        if (gridData == null) return new Vector2Int(-1, -1);
        for (int x = 0; x < GridWidthCount; x++)
        {
            for (int y = 0; y < GridHeightCount; y++)
            {
                GridData data = GetGridData(x, y);
                if (data != null && data == gridData)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// 注册一个形状
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="weight">随机形状时被随机到的权重</param>
    public void RegisterShape(GridShape shape, int weight)
    {
        if (gridShapesDict.ContainsKey(shape.GridShapeType))
        {
            Debug.LogError($"形状已经注册过了, tag = {shape.GridShapeType}");
            return;
        }

        gridShapesDict.Add(shape.GridShapeType, shape);
        gridShapesWeightList.Add((shape.GridShapeType, weight));
    }

    /// <summary>
    /// 初始化所有形状
    /// </summary>
    private void InitShapes()
    {
        RegisterShape(new GridShapeO(), 1);
        RegisterShape(new GridShapeI(), 1);
        RegisterShape(new GridShapeS(), 1);
        RegisterShape(new GridShapeZ(), 1);
        RegisterShape(new GridShapeL(), 1);
        RegisterShape(new GridShapeJ(), 1);
        RegisterShape(new GridShapeT(), 1);
        RegisterShape(new GridShapeSingle(), 5);
    }

    /// <summary>
    /// 初始化格子数据
    /// </summary>
    private void InitGridDatas()
    {
        gridDatas = new GridData[GridWidthCount, GridHeightCount];
    }

    /// <summary>
    /// 初始化背景格子
    /// </summary>
    private void InitBackgroundGrids()
    {
        //将物体移动到视图中心
        gridsTrans.position = new Vector3(
            -GridWidthCount * (gridPreWidth / pixelsPerUnit) * 0.5f + (gridPreWidth / pixelsPerUnit * 0.5f),
            -GridHeightCount * (gridPreHeight / pixelsPerUnit) * 0.5f + (gridPreHeight / pixelsPerUnit * 0.5f));

        //格子对象
        for (int y = 0; y < gridHeightCount; y++)
        {
            for (int x = 0; x < gridWidthCount; x++)
            {
                Vector2 pos = GetGridPosition(x, y);
                GameObject gridBackgroundGO = Instantiate(backgroundGridPrefab, gridsBackgroundParent);
                gridBackgroundGO.transform.localPosition = pos;
            }
        }
    }

    /// <summary>
    /// 获取格子显示位置
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector2 GetGridPosition(int x, int y)
    {
        return new Vector2(x * (gridPreWidth / pixelsPerUnit), y * (gridPreHeight / pixelsPerUnit));
    }

    /// <summary>
    /// 设置格子数据
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="gridData"></param>
    public void SetGridData(int x, int y, GridData gridData)
    {
        if (x < 0 || x >= GridWidthCount || y < 0 || y >= GridHeightCount)
        {
            Debug.LogError($"设置格子数据失败，坐标越界，x = {x}, y = {y}");
            return;
        }
        gridDatas[x, y] = gridData;
    }

    /// <summary>
    /// 销毁指定位置的格子
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void DestroyGridData(int x, int y)
    {
        var gridData = GetGridData(x, y);
        if (gridData == null) return;

        UnityEngine.Object.Destroy(gridData.GameObject);
        SetGridData(x, y, null);
        gridData.Eliminate(x, y);
    }

    /// <summary>
    /// 控制正在下落的方块移动
    /// </summary>
    /// <param name="direction"></param>
    public void FallingShapeMovement(Vector2Int direction)
    {
        if (fallingDownShape == null) return;
        bool canMovement = fallingDownShape.CanMovement(direction);
        if (canMovement)
            fallingDownShape.Movement(direction);
    }

    /// <summary>
    /// 控制正在下落的方块旋转
    /// </summary>
    public void FallingShapeRotate()
    {
        if (fallingDownShape == null) return;
        bool canRotate = fallingDownShape.CanRotate();
        if (canRotate)
            fallingDownShape.Rotate();
    }

    /// <summary>
    /// 向下搜索第一个空格子，返回空格的y索引和所有经过的格子列表
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y">从这一行开始搜索</param>
    /// <returns></returns>
    public Tuple<int, List<GridData>> SearchDownwardsForTheFirstEmptyGrid(int x, int y)
    {
        List<GridData> fallDownList = new List<GridData>();
        int emptyPosY = -1;
        for (int py = y; py >= 0; py--)
        {
            GridData gridData = GameManager.Instance.GridsController.GetGridData(x, py);
            if (gridData != null)
            {
                fallDownList.Add(gridData);
            }
            else
            {
                emptyPosY = py;
                break;
            }
        }

        return new Tuple<int, List<GridData>>(emptyPosY, fallDownList);
    }

    /// <summary>
    /// 向下搜索最后一个空格子，返回空格的y索引
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int SearchDownwardsForTheLastEmptyGrid(int x, int y)
    {
        int emptyPosY = -1;
        for (int py = y - 1; py >= 0; py--)
        {
            GridData gridData = GameManager.Instance.GridsController.GetGridData(x, py);
            if (gridData != null)
                break;
            else
                emptyPosY = py;
        }

        return emptyPosY;
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        for (int x = 0; x < GridWidthCount; x++)
        {
            for (int y = 0; y < GridHeightCount; y++)
            {
                var gridData = GetGridData(x, y);
                if (gridData != null)
                    SetGridData(x, y, null);
            }
        }
        int childCount = gridsListTrans.childCount;
        for (int i = childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(gridsListTrans.GetChild(i).gameObject);
        if (fallingDownShape != null)
        {
            UnityEngine.Object.Destroy(fallingDownShape.GridsParent);
            fallingDownShape = null;
        }
        //if (fallingDownShapeIE != null)
        //    this.StopCoroutine(fallingDownShapeIE);
        StopAllCoroutines();
    }
}

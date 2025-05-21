using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    private GridsController gridsController;
    public GridsController GridsController { get { return gridsController; } }
    private GridMaterialController gridMaterialController;
    public GridMaterialController GridMaterialController { get { return gridMaterialController; } }
    private UIController uiController;
    public UIController UIController { get { return uiController; } }

    private bool pausing = false;
    /// <summary>
    /// 游戏是否已经暂停
    /// </summary>
    public bool Pausing { get { return pausing; } }

    private void Awake()
    {
        instance = this;

        gridsController = GetComponent<GridsController>();
        gridMaterialController = new GridMaterialController();
        uiController = GetComponent<UIController>();
    }

    [SerializeField]
    private float downArrowTimeInterval = 0.2f;
    private float downArrowTimer = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GameInput(GameInputType.Left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GameInput(GameInputType.Right);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            GameInput(GameInputType.Down);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            //按住↓时加速下落
            downArrowTimer += Time.deltaTime;
            if (downArrowTimer > downArrowTimeInterval)
            {
                downArrowTimer = 0;
                gridsController.FallingShapeMovement(new Vector2Int(0, -1));
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameInput(GameInputType.Rotate);
        }

#if UNITY_EDITOR
        if (gridsController.DebugModel)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                gridsController.AddShape(GridShapeType.Single, new Vector2Int(4, gridsController.GridHeightCount), new CobblestoneGridMaterial());
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                StartCoroutine(IE());
            }
        }
#endif
    }

#if UNITY_EDITOR
    private IEnumerator IE()
    {

        gridsController.AddShape(GridShapeType.Single, new Vector2Int(3, 0), new CobblestoneGridMaterial());
        yield return new WaitForSeconds(0.6f);
        gridsController.AddShape(GridShapeType.Single, new Vector2Int(4, 0), new CobblestoneGridMaterial());
        yield return new WaitForSeconds(0.6f);
        gridsController.AddShape(GridShapeType.Single, new Vector2Int(5, 0), new CobblestoneGridMaterial());
        yield return new WaitForSeconds(0.6f);
        gridsController.AddShape(GridShapeType.Single, new Vector2Int(4, gridsController.GridHeightCount), new FurnaceGridMaterial());
        yield return new WaitForSeconds(0.6f);
    }
#endif

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        gridsController.StartGame();
    }

    /// <summary>
    /// 清理游戏信息
    /// </summary>
    public void ClearGame()
    {
        GridsController.Clear();
    }

    /// <summary>
    /// 游戏暂停
    /// </summary>
    public void GamePause()
    {
        pausing = true;
    }

    /// <summary>
    /// 游戏继续
    /// </summary>
    public void GamePlay()
    {
        pausing = false;
    }

    /// <summary>
    /// 处理游戏输入
    /// </summary>
    /// <param name="gameInputType"></param>
    public void GameInput(GameInputType gameInputType)
    {
        switch (gameInputType)
        {
            case GameInputType.Left:
                //图形向左移动一格
                gridsController.FallingShapeMovement(new Vector2Int(-1, 0));
                break;
            case GameInputType.Right:
                //图形向右移动一格
                gridsController.FallingShapeMovement(new Vector2Int(1, 0));
                break;
            case GameInputType.Down:
                //图形向下移动一格
                gridsController.FallingShapeMovement(new Vector2Int(0, -1));
                break;
            case GameInputType.Rotate:
                //图形旋转
                gridsController.FallingShapeRotate();
                break;
            default:
                break;
        }
    }

    public enum GameInputType
    {
        Left,
        Right,
        Down,
        Rotate
    }
}

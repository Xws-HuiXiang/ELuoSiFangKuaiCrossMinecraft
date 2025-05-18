using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class Effects
{
    private static Effects instance;
    public static Effects Instance
    {
        get
        {
            instance ??= new Effects();
            return instance;
        }
    }

    private bool collectionChecks = true;
    private Transform effectsParent;
    public int maxPoolSize = 60;

    #region 消除特效
    private GameObject eliminateEffectPrefab;
    private IObjectPool<ParticleSystem> eliminateEffectPool;
    #endregion
    #region 烧炼特效
    private GameObject smeltEffectPrefab;
    private IObjectPool<ParticleSystem> smeltEffectPool;
    #endregion

    private Effects()
    {
        effectsParent = GameObject.Find("Grids/Effects").transform;

        //消除特效
        eliminateEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/EliminateEffect");
        eliminateEffectPool = new ObjectPool<ParticleSystem>(
            CreateEliminateEffectItem,
            OnEliminateEffectTakeFromPool,
            OnEliminateEffectReturnedToPool,
            OnEliminateEffectDestroyPoolObject,
            collectionChecks,
            10,
            maxPoolSize);
        //烧炼特效
        smeltEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/SmeltEffect");
        smeltEffectPool = new ObjectPool<ParticleSystem>(
            CreateSmeltEffectItem,
            OnSmeltEffectTakeFromPool,
            OnSmeltEffectReturnedToPool,
            OnSmeltEffectDestroyPoolObject,
            collectionChecks,
            10,
            maxPoolSize);
    }

    #region 消除特效相关方法
    /// <summary>
    /// 播放消除特效
    /// </summary>
    /// <param name="yIndex"></param>
    /// <param name="duration"></param>
    public void PlayEliminateEffect(int yIndex, float duration = 0.5f)
    {
        float gridPreWidth = GameManager.Instance.GridsController.GridPreWidth / GameManager.Instance.GridsController.PixelsPerUnit;
        float halfGridWidthCount = GameManager.Instance.GridsController.GridWidthCount * 0.5f;
        float startPosX = halfGridWidthCount * gridPreWidth - (gridPreWidth * 0.5f);

        //计算左右边界的显示位置
        Vector2 gridPos = GameManager.Instance.GridsController.GetGridPosition(0, yIndex);
        Vector3 startPos = new Vector3(startPosX, gridPos.y, 0);
        Vector3 endPosRight = new Vector3(halfGridWidthCount * gridPreWidth * 2 - (gridPreWidth * 0.5f), gridPos.y, 0);
        Vector3 endPosLeft = new Vector3(-gridPreWidth, endPosRight.y, 0);
        //结束位置再向两侧扩展一些
        endPosRight.x *= 2;
        endPosLeft.x *= 2;
        //从中间向两侧播放特效
        PlayEliminateEffect(startPos, endPosRight);
        PlayEliminateEffect(startPos, endPosLeft);
    }

    /// <summary>
    /// 播放消除特效
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="duration"></param>
    public void PlayEliminateEffect(Vector3 startPos, Vector3 endPos, float duration = 0.5f)
    {
        ParticleSystem ps = eliminateEffectPool.Get();
        ps.transform.SetParent(effectsParent);
        ps.transform.localPosition = startPos;
        ps.transform.rotation = Quaternion.identity;
        ps.gameObject.SetActive(true);
        ps.Play(true);

        ps.transform.DOLocalMove(endPos, duration).SetEase(Ease.Linear).OnComplete(() => eliminateEffectPool.Release(ps));
    }

    /// <summary>
    /// 创建消除特效物体
    /// </summary>
    /// <returns></returns>
    private ParticleSystem CreateEliminateEffectItem()
    {
        var go = UnityEngine.Object.Instantiate(eliminateEffectPrefab);
        var ps = go.GetComponent<ParticleSystem>();
        ps.Play(true);

        return ps;
    }

    /// <summary>
    /// 从对象池中获取消除特效物体
    /// </summary>
    /// <param name="system"></param>
    private void OnEliminateEffectTakeFromPool(ParticleSystem system)
    {
        system.gameObject.SetActive(true);
    }

    /// <summary>
    /// 将消除特效物体放回对象池
    /// </summary>
    /// <param name="system"></param>
    private void OnEliminateEffectReturnedToPool(ParticleSystem system)
    {
        system.gameObject.SetActive(false);
    }

    /// <summary>
    /// 销毁对象池中的消除特效物体
    /// </summary>
    /// <param name="system"></param>
    private void OnEliminateEffectDestroyPoolObject(ParticleSystem system)
    {
        UnityEngine.Object.Destroy(system.gameObject);
    }
    #endregion

    #region 烧炼特效相关方法
    /// <summary>
    /// 播放熔炼特效
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public async void PlaySmeltEffect(int x, int y)
    {
        ParticleSystem ps = smeltEffectPool.Get();
        ps.transform.SetParent(effectsParent);
        ps.transform.localPosition = GameManager.Instance.GridsController.GetGridPosition(x, y);
        ps.transform.rotation = Quaternion.identity;
        ps.gameObject.SetActive(true);
        ps.Play(true);
        
        await UniTask.Delay((int)(TimeSpan.FromSeconds(ps.main.duration - 1).TotalMilliseconds));
        smeltEffectPool.Release(ps);
    }

    /// <summary>
    /// 创建熔炼特效物体
    /// </summary>
    /// <returns></returns>
    private ParticleSystem CreateSmeltEffectItem()
    {
        var go = UnityEngine.Object.Instantiate(smeltEffectPrefab);
        var ps = go.GetComponent<ParticleSystem>();
        ps.Play(true);

        return ps;
    }

    /// <summary>
    /// 从对象池中获取熔炼特效物体
    /// </summary>
    /// <param name="system"></param>
    private void OnSmeltEffectTakeFromPool(ParticleSystem system)
    {
        system.gameObject.SetActive(true);
    }

    /// <summary>
    /// 将熔炼特效物体放回对象池
    /// </summary>
    /// <param name="system"></param>
    private void OnSmeltEffectReturnedToPool(ParticleSystem system)
    {
        system.gameObject.SetActive(false);
    }

    /// <summary>
    /// 销毁对象池中的熔炼特效物体
    /// </summary>
    /// <param name="system"></param>
    private void OnSmeltEffectDestroyPoolObject(ParticleSystem system)
    {
        UnityEngine.Object.Destroy(system.gameObject);
    }
    #endregion
}

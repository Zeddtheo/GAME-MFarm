using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public List<GameObject> poolPrefabs;
    public List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();
    private Queue<GameObject> soundQueue = new Queue<GameObject>();
    private void OnEnable()
    {
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffect += InitSoundEffect;
    }
    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= InitSoundEffect;
    }
    private void Start()
    {
        CreatePool();
    }
    private void CreatePool()
    {
        foreach (GameObject item in poolPrefabs)
        {
            var parent = new GameObject(item.name).transform;
            parent.SetParent(transform);

            var newPool = new ObjectPool<GameObject>(
                () => Instantiate(item, parent),
                e => { e.SetActive(true); },
                e => { e.SetActive(false); },
                e => { Destroy(e); }
                );
            poolEffectList.Add(newPool);
        }
    }
    private void OnParticleEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        ObjectPool<GameObject> objPool = effectType switch
        {
            ParticleEffectType.LeavesFalling01 => poolEffectList[0],
            ParticleEffectType.LeavesFalling02 => poolEffectList[1],
            ParticleEffectType.Rock => poolEffectList[2],
            ParticleEffectType.ReapableScenery => poolEffectList[3],
            _ => null,
        };

        var obj = objPool.Get();
        obj.transform.position = pos;
        StartCoroutine(ReleaseRoutine(objPool, obj));
    }
    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool,GameObject obj) 
    {
        yield return new WaitForSeconds(1.5f);
        pool.Release(obj);
    }
    //private void InitSoundEffect(SoundDetails soundDetails)
    //{
    //    ObjectPool<GameObject> pool = poolEffectList[4];
    //    var obj = pool.Get();
    //    obj.GetComponent<Sound>().SetSound(soundDetails);
    //    StartCoroutine(DisableSound(pool, obj, soundDetails));
    //}
    //private IEnumerator DisableSound(ObjectPool<GameObject> pool, GameObject obj, SoundDetails soundDetails)
    //{
    //    yield return new WaitForSeconds(soundDetails.soundClip.length);
    //    pool.Release(obj);
    //}

    private void CreateSoundPool()
    {
        var parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(transform);
        for (int i = 0; i < 20; i++)
        {
            GameObject newObj = Instantiate(poolPrefabs[4], parent);
            newObj.SetActive(false);
            soundQueue.Enqueue(newObj);
        }
    }
    private GameObject GetPoolbject() 
    {
        if (soundQueue.Count < 2)
            CreateSoundPool();
        return soundQueue.Dequeue();
    }
    private void InitSoundEffect(SoundDetails soundDetails)
    {
        var obj = GetPoolbject();
        obj.GetComponent<Sound>().SetSound(soundDetails);
        obj.SetActive(true);
        StartCoroutine(DisableSound(obj,soundDetails.soundClip.length));
    }
    private IEnumerator DisableSound(GameObject obj,float duration)
    {
        yield return new WaitForSeconds (duration);
        obj.SetActive (false);
        soundQueue.Enqueue (obj);
    }
}


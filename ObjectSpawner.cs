using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darket.Tools
{
    public class ObjectSpawner : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] bool SpawnOnStart;

        [SerializeField] bool WorldPositionStays;

        [SerializeField] int MinSpawnCount = 1;
        [SerializeField] int MaxSpawnCount = 1;

        [SerializeField] float DelayFirstSpawn = 1f;

        [SerializeField] bool LoopSpawn;
        [SerializeField] bool DestroyObjectOnLoop;
        [SerializeField] float DelaySpawn = 1f;

        [Header("Components")]
        [SerializeField] Transform Parent;

        [SerializeField] Transform[] Points;
        [SerializeField] GameObject[] ObjectPrefabs;

        private float lastTime;

        private List<GameObject> spawnedObjects = new List<GameObject>();

        private bool spawnOnPoints;

        IEnumerator Start()
        {
            spawnOnPoints = Parent == null;

            yield return new WaitForSeconds(DelayFirstSpawn);

            if (SpawnOnStart)
            {
                SpawnObjects();
            }
        }

        /// <summary>
        /// Спавн объектов
        /// </summary>
        public void SpawnObjects()
        {
            int countSpawn = Random.Range(MinSpawnCount, MaxSpawnCount + 1);

            var tempArray = UsefulMethods.GetRandomArrayElements<Transform>(Points, countSpawn);

            Transform resultParent = Parent;

            for (int i = 0; i < tempArray.Length; i++)
            {
                if (spawnOnPoints)
                {
                    resultParent = tempArray[i];
                }

                int prefabIndex = Random.Range(0, ObjectPrefabs.Length);

                GameObject spawnObject = Instantiate(ObjectPrefabs[prefabIndex]);

                spawnObject.transform.position = tempArray[i].position;

                spawnObject.transform.SetParent(resultParent, WorldPositionStays);

                spawnedObjects.Add(spawnObject);
            }
        }

        /// <summary>
        /// Удаление объектов
        /// </summary>
        public void DestroyObjects()
        {
            foreach (var obj in spawnedObjects)
            {
                Destroy(obj);
            }

            spawnedObjects.Clear();
        }

        void Update()
        {
            if (LoopSpawn)
            {
                if (Time.time > lastTime + DelaySpawn)
                {
                    lastTime = Time.time;

                    if (DestroyObjectOnLoop)
                    {
                        DestroyObjects();
                    }

                    SpawnObjects();
                }
            }
        }
    }
}
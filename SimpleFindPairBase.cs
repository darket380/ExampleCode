using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace Darket.Tools
{
    /// <summary>
    /// Simple Find Pair Base
    /// </summary>
    public class SimpleFindPairBase : MonoBehaviour
    {
        [SerializeField] bool InitOnStartScript;
        [SerializeField] bool InitOnStartGame;
        [SerializeField] int DefaultMaxWidthPairs = 2;
        [SerializeField] int DefaultMaxHeightPairs = 2;
        [SerializeField] float DelayBetweenSelect = 0.5f;
        [SerializeField] float DistanceWidthBetween = 0.2f;
        [SerializeField] float DistanceHeightBetween = 0.2f;
        [SerializeField] float DelayDestroy;
        [SerializeField] Transform SpawnParent;
        [SerializeField] GameObject[] ItemPrefabs;

        [SerializeField] UnityEvent OnGameStart;
        [SerializeField] UnityEvent OnRightSelect;
        [SerializeField] UnityEvent OnWrongSelect;
        [SerializeField] UnityEvent OnStop;
        [SerializeField] UnityEvent OnWin;

        private int maxPairs;
        private bool isCanSelect;
        private List<SimpleFindPairItem> gameItems = new List<SimpleFindPairItem>();

        private SimpleFindPairItem firstIndex = null;
        private SimpleFindPairItem secondIndex = null;

        private bool isGameStarted;

        private int countFindPairs;

        private Camera mainCamera;

        private float lastTimeSelect;
        private int maxHeightPairs = 1;
        private int maxWidthPairs = 1;

        /// <summary>
        /// Максимальное количество пар по ширине
        /// </summary>
        public int MaxWidthPairs
        {
            get => maxWidthPairs;
            set
            {
                if (value < 1)
                {
                    Debug.LogError("Can't set max width pairs less than 1");
                    return;
                }

                maxWidthPairs = value;
            }
        }

        /// <summary>
        /// Максимальное количество пар по высоте
        /// </summary>
        public int MaxHeightPairs
        {
            get => maxHeightPairs;
            set
            {
                if (value < 1)
                {
                    Debug.LogError("Can't set max height pairs less than 1");
                    return;
                }

                maxHeightPairs = value;
            }
        }

        private void Start()
        {
            maxHeightPairs = DefaultMaxHeightPairs;
            maxWidthPairs = DefaultMaxWidthPairs;

            mainCamera = Camera.main;

            if (InitOnStartScript)
            {
                Init();
            }
        }

        /// <summary>
        /// Инициализация игры
        /// </summary>
        public void Init()
        {
            maxPairs = (MaxHeightPairs * MaxWidthPairs) * 2;

            List<GameObject> listPrefabs = new List<GameObject>();

            int min = Mathf.Min(maxPairs, ItemPrefabs.Length);

            listPrefabs.AddRange(UsefulMethods.GetRandomArrayElements<GameObject>(ItemPrefabs, min));

            if (maxPairs > ItemPrefabs.Length)
            {
                int countAddPairs = maxPairs - ItemPrefabs.Length;
                GameObject[] addArray = new GameObject[countAddPairs];

                for (int i = 0; i < countAddPairs; i++)
                {
                    int randomIndex = Random.Range(0, ItemPrefabs.Length);
                    addArray[i] = ItemPrefabs[randomIndex];
                }

                listPrefabs.AddRange(addArray);
            }

            listPrefabs.AddRange(listPrefabs);

            listPrefabs.ListShuffle<GameObject>();

            int x = 0;
            int y = 0;

            if (gameItems.Count > 0)
            {
                for (int i = 0; i < gameItems.Count; i++)
                {
                    Destroy(gameItems[i]);
                }
            }

            gameItems.Clear();

            float halfDistance = (DistanceWidthBetween / 2f);
            Vector3 startPosition = new Vector3(-((DistanceWidthBetween * (MaxWidthPairs - 1)) + halfDistance), 0, -((DistanceHeightBetween * (MaxHeightPairs - 1)) + halfDistance));

            Transform spawnParent = SpawnParent == null ? transform : SpawnParent;

            for (int i = 0; i < listPrefabs.Count; i++)
            {
                GameObject gobject = Instantiate(listPrefabs[i], spawnParent);
                gobject.transform.localPosition = startPosition + new Vector3(x * DistanceWidthBetween, 0, y * DistanceHeightBetween);
                gameItems.Add(gobject.GetComponent<SimpleFindPairItem>());

                x++;
                if (x >= MaxWidthPairs * 2)
                {
                    x = 0;
                    y++;
                }
            }
        }

        /// <summary>
        /// Выбор предмета
        /// </summary>
        public void SelectItem(SimpleFindPairItem item)
        {
            if (firstIndex == item) return;

            item.Select();

            if (firstIndex == null)
            {
                firstIndex = item;
            }
            else
            {
                if (secondIndex == null)
                {
                    secondIndex = item;

                    if (firstIndex.Index == secondIndex.Index)
                    {
                        countFindPairs++;
                        CheckWin();

                        firstIndex.RightSelect();
                        secondIndex.RightSelect();

                        firstIndex.Destroy(DelayDestroy);
                        gameItems.Remove(firstIndex);

                        secondIndex.Destroy(DelayDestroy);
                        gameItems.Remove(secondIndex);

                        OnRightSelect.Invoke();
                        
                        ResetParams();
                    }
                    else
                    {
                        firstIndex.WrongSelect();
                        secondIndex.WrongSelect();

                        firstIndex.Close();
                        secondIndex.Close();

                        OnWrongSelect.Invoke();

                        ResetParams();
                    }
                }
            }
        }

        /// <summary>
        /// Вызвать закрытие у всех предметов
        /// </summary>
        public void CloseAll()
        {
            for (int i = 0; i < gameItems.Count; i++)
            {
                gameItems[i].Close();
            }
        }

        /// <summary>
        /// Вызвать открытие у всех предметов
        /// </summary>
        public void OpenAll()
        {
            for (int i = 0; i < gameItems.Count; i++)
            {
                gameItems[i].Open();
            }
        }

        /// <summary>
        /// Сброс параметров
        /// </summary>
        public void ResetParams()
        {
            firstIndex = null;
            secondIndex = null;
        }

        /// <summary>
        /// Метод успешного окончания игры
        /// </summary>
        public void Win()
        {
            OnWin.Invoke();
            Stop();
        }

        /// <summary>
        /// Метод остановки игры
        /// </summary>
        public void Stop()
        {
            isGameStarted = false;
            OnStop.Invoke();
        }

        /// <summary>
        /// Метод проверки на окончание игры
        /// </summary>
        void CheckWin()
        {
            if (countFindPairs >= maxPairs)
            {
                Win();
            }
        }

        /// <summary>
        /// Метод запуска игры
        /// </summary>
        public void StartGame()
        {
            ResetParams();

            countFindPairs = 0;

            if (InitOnStartGame)
            {
                Init();
            }

            OnGameStart.Invoke();

            isGameStarted = true;
        }

        void Update()
        {
            if (isGameStarted)
            {
                if (Time.time > lastTimeSelect + DelayBetweenSelect)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        RaycastHit hit;
                        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(ray, out hit))
                        {
                            SimpleFindPairItem item = hit.transform.GetComponent<SimpleFindPairItem>();

                            if (item != null)
                            {
                                SelectItem(item);
                            }

                            lastTimeSelect = Time.time;
                        }
                    }
                }
            }
        }
    }
}
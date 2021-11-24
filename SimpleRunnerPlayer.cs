using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Darket.SimpleRunner
{
    internal enum PlayerSidePosition
    {
        Left,
        Middle,
        Right
    }

    internal enum PlayerVerticalPosition
    {
        None,
        Up,
        Down
    }

    public class SimpleRunnerPlayer : MonoBehaviour
    {
        [SerializeField] Transform PlayerTransform;

        [Header("Properties")]
        [SerializeField] bool ResetOnStart;
        [SerializeField] float DistanceLines = 1f;
        [SerializeField] float LinesCount = 3;
        [SerializeField] Vector3 LineDirection = new Vector3(1, 0, 0);
        [SerializeField] float LineSwitchSpeed = 1f;

        [SerializeField] float UpDelay = 1f;
        [SerializeField] float UpPower = 1f;
        [SerializeField] AnimationCurve UpCurve;

        [SerializeField] float DownDelay = 1f;
        [SerializeField] float DownPower = 1f;
        [SerializeField] AnimationCurve DownCurve;

        [Header("Events")]
        [SerializeField] UnityEvent OnChangeLine;
        [SerializeField] UnityEvent OnTurnLeft;
        [SerializeField] UnityEvent OnTurnRight;
        [SerializeField] UnityEvent OnStartUp;
        [SerializeField] UnityEvent OnEndUp;
        [SerializeField] UnityEvent OnStartDown;
        [SerializeField] UnityEvent OnEndDown;

        private int playerLine;
        private int maxLineOneSide;
        private PlayerVerticalPosition positionVerticalEnum;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool running;
        Coroutine verticalMovingCoroutine;

        void Start()
        {
            if (PlayerTransform == null)
            {
                PlayerTransform = transform;
            }

            startPosition = PlayerTransform.localPosition;

            if (ResetOnStart)
            {
                Reset();
            }
        }

        /// <summary>
        /// Проверка на возможность изменить позицию
        /// </summary>
        public bool CanChangePosition()
        {
            return running && positionVerticalEnum == PlayerVerticalPosition.None;
        }

        /// <summary>
        /// Сбросить игрока к начальному состоянию
        /// </summary>
        public void Reset()
        {
            maxLineOneSide = (int)(LinesCount / 2);
            SetPlayerLine(0);

            positionVerticalEnum = PlayerVerticalPosition.None;
            PlayerTransform.position = targetPosition;
        }

        /// <summary>
        /// Номер линии игрока
        /// </summary>
        public int GetPlayerLine()
        {
            return playerLine;
        }

        /// <summary>
        /// Попытка сменить позицию левее
        /// </summary>
        public void TryTurnLeft()
        {
            TryChangeLine(false);
        }

        /// <summary>
        /// Попытка сменить позицию правее
        /// </summary>
        public void TryTurnRight()
        {
            TryChangeLine(true);
        }

        /// <summary>
        /// Попытка сменить позицию вверх (прыжок)
        /// </summary>
        public bool TryUp()
        {
            if (!CanChangePosition())
            { 
                return false;
            }

            positionVerticalEnum = PlayerVerticalPosition.Up;

            OnStartUp?.Invoke();

            verticalMovingCoroutine = StartCoroutine(VerticalMovingCoroutine(UpDelay, UpPower, UpCurve, OnEndUp));

            return true;
        }

        /// <summary>
        /// Попытка сменить позицию вверх (прыжок) для инспектора
        /// </summary>
        public void Up()
        {
            TryUp();
        }

        /// <summary>
        /// Попытка сменить позицию вниз (подкат)
        /// </summary>
        public bool TryDown()
        {
            if (!CanChangePosition())      
            { 
                return false;
            }

            positionVerticalEnum = PlayerVerticalPosition.Down;

            OnStartDown?.Invoke();

            verticalMovingCoroutine = StartCoroutine(VerticalMovingCoroutine(DownDelay, -DownPower, DownCurve, OnEndDown));

            return true;
        }

        /// <summary>
        /// Попытка сменить позицию вниз (подкат) для инспектора
        /// </summary>
        public void Down()
        {
            TryDown();
        }

        IEnumerator VerticalMovingCoroutine(float delay, float power, AnimationCurve animationCurve, UnityEvent unityEvent)
        {
            float lerpTime = 0;

            Vector3 sourcePosition = PlayerTransform.localPosition;

            while (lerpTime < 1)
            {
                lerpTime = Mathf.Clamp01(lerpTime + Time.deltaTime / delay);

                float curvedLerp = animationCurve.Evaluate(lerpTime);

                float y = Mathf.Sin(curvedLerp * Mathf.PI) * power;

                PlayerTransform.localPosition = Vector3.Lerp(sourcePosition, sourcePosition + (Vector3.up * y), curvedLerp);

                yield return null;
            }

            unityEvent.Invoke();

            positionVerticalEnum = PlayerVerticalPosition.None;
            verticalMovingCoroutine = null;
        }

        /// <summary>
        /// Попытка сменить позицию (isRight - правее)
        /// </summary>
        public bool TryChangeLine(bool isRight)
        {
            if (!CanChangePosition())
            { 
                return false;
            }

            int result = isRight ? 1 : -1;

            int newLine = Mathf.Clamp(playerLine + result, -maxLineOneSide, maxLineOneSide);

            if (playerLine == newLine)       
            { 
                return false;
            }

            SetPlayerLine(newLine);

            if (isRight)
            {
                OnTurnRight?.Invoke();
            }
            else
            {
                OnTurnLeft?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// Принудительно поменять линию
        /// </summary>
        public void SetPlayerLine(int index)
        {
            playerLine = index;

            targetPosition = startPosition + LineDirection * DistanceLines * playerLine;
        }

        /// <summary>
        /// Начать движение персонажа
        /// </summary>
        public void Run()
        {
            running = true;
        }

        /// <summary>
        /// Остановить движение персонажа
        /// </summary>
        public void Stop()
        {
            running = false;
        }

        void Update()
        {
            if (!CanChangePosition())
            { 
                return;
            }

            PlayerTransform.localPosition = Vector3.Lerp(PlayerTransform.position, targetPosition, LineSwitchSpeed * Time.deltaTime);
        }
    }
}

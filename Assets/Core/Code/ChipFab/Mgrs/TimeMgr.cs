using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class TimeMgr : MonoBehaviour
    {
        public static TimeMgr Instance;
        public TMP_Text CountdownText;
        public TMP_Text RunningText;

        private enum TimeState
        {
            Stopped,
            Running
        }

        private float m_elapsedTime;
        private Routine m_startRoutine;
        private TimeState m_state;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Game.Events.Register(GameEvents.NewWaferCreated, HandleNewWaferCreated);
            Game.Events.Register(GameEvents.WaferSubmitted, HandleWaferSubmitted);
            Reset();
        }

        private void OnDestroy()
        {
            Game.Events?.Deregister(GameEvents.NewWaferCreated, HandleNewWaferCreated);
            Game.Events?.Deregister(GameEvents.WaferSubmitted, HandleWaferSubmitted);
            m_startRoutine.Stop();
        }

        private void Update()
        {
            if (m_state == TimeState.Running)
            {
                m_elapsedTime += Time.deltaTime;
                RunningText.SetText(m_elapsedTime.ToString("0.00") + " s");
            }
        }

        public void Reset()
        {
            m_elapsedTime = 0;
            RunningText.SetText("0.00 s");

            RunningText.gameObject.SetActive(ChipFabConfig.Instance.Mode == GameMode.Timed);

            m_state = TimeState.Stopped;
            CountdownText.gameObject.SetActive(false);
        }

        public void Pause()
        {
            m_elapsedTime = 0;
            m_state = TimeState.Stopped;
        }

        public void Begin()
        {
            m_startRoutine.Replace(BeginSequence());
        }

        public float GetElapsedTime()
        {
            return m_elapsedTime;
        }

        private IEnumerator BeginSequence()
        {
            AutomationMgr.Instance.ActivelyChecking = false;

            CountdownText.gameObject.SetActive(true);

            CountdownText.SetText("3");

            yield return 1;

            CountdownText.SetText("2");

            yield return 1;

            CountdownText.SetText("1");

            yield return 1;

            CountdownText.SetText("GO!");

            yield return 1;

            m_state = TimeState.Running;
            Game.Events.Dispatch(GameEvents.TimerBegin);

            CountdownText.gameObject.SetActive(false);

            AutomationMgr.Instance.ActivelyChecking = true;
        }

        private void HandleNewWaferCreated()
        {
            if (ModeMgr.Instance.Mode == GameMode.Timed)
            {
                Reset();
                Begin();
            }
        }

        private void HandleWaferSubmitted()
        {
            if (ModeMgr.Instance.Mode == GameMode.Timed)
            {
                Pause();
            }
        }
    }
}
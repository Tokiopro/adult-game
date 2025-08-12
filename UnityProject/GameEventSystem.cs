using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SchoolLoveSimulator
{
    [System.Serializable]
    public class GameEvent
    {
        public string eventName;
        public string description;
        public UnityEvent onEventTrigger;
        public List<string> requiredFlags;
        public bool isCompleted;
    }

    public class GameEventSystem : MonoBehaviour
    {
        public List<GameEvent> gameEvents = new List<GameEvent>();
        private List<string> completedEvents = new List<string>();

        public void TriggerEvent(string eventName, string parameter = "")
        {
            GameEvent gameEvent = gameEvents.Find(e => e.eventName == eventName);
            if (gameEvent != null && !gameEvent.isCompleted)
            {
                gameEvent.onEventTrigger?.Invoke();
                gameEvent.isCompleted = true;
                completedEvents.Add(eventName);
            }
        }

        public List<string> GetCompletedEvents()
        {
            return new List<string>(completedEvents);
        }

        public void LoadCompletedEvents(List<string> events)
        {
            completedEvents = new List<string>(events);
            foreach (string eventName in events)
            {
                GameEvent gameEvent = gameEvents.Find(e => e.eventName == eventName);
                if (gameEvent != null)
                {
                    gameEvent.isCompleted = true;
                }
            }
        }
    }
}
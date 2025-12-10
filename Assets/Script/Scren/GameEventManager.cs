using System;
using System.Collections.Generic;

namespace BlackGemsGlobal.SeatAway.GamePlayEvent
{
    public static class GameEventManager
    {
        
        /// <summary>
        /// Event Id
        /// </summary>
        public enum EventId
        {
            GameLose, 
            GameWin, 
            GameStore, 
            LandscapeScreen,
            VerticalScreen,
            StartGame,
            EndGame,
            Idea,
            Idea_2,
            Idea_3,
            Idea_4,
            ToggleGate,
            Effect,
            Effect_2,
            BirdIQ,
            Ice,

        }
        
        /// <summary>
        /// Call Back
        /// </summary>
        public delegate void GameEventCallBack();

        /// <summary>
        /// Event all call back in games
        /// </summary>
        private static Dictionary<EventId, List<GameEventCallBack>> _eventCallBacks = new Dictionary<EventId, List<GameEventCallBack>>();
        
        /// <summary>
        /// Register events
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="callBack"></param>
        public static void RegisterEvent(EventId eventId, GameEventCallBack callBack)
        {
            _eventCallBacks.TryAdd(eventId, new List<GameEventCallBack>());
            List<GameEventCallBack> _allCallBacks = _eventCallBacks[eventId];
            _allCallBacks.Add(callBack);
            
            _eventCallBacks[eventId] = _allCallBacks;
        }
        
        /// <summary>
        /// Unregister event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="callBack"></param>
        public static void UnRegisterEvent(EventId eventId, GameEventCallBack callBack)
        {
            _eventCallBacks.TryAdd(eventId, new List<GameEventCallBack>());
            List<GameEventCallBack> _allCallBacks = _eventCallBacks[eventId];
            _allCallBacks.Remove(callBack);
            
            _eventCallBacks[eventId] = _allCallBacks;
        }

        
        /// <summary>
        /// Raised Event
        /// </summary>
        /// <param name="eventId"></param>
        public static void RaisedEvent(EventId eventId)
        {
            if(_eventCallBacks.TryGetValue(eventId, out List<GameEventCallBack> allCallback))
            {
                for (var i = 0; i < allCallback.Count; i++)
                    allCallback[i].Invoke();
            }
        }
    }
}
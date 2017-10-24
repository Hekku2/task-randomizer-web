﻿using DataStorage.Interfaces;
using System;
using System.Collections.Generic;
using DataStorage.DataObjects;
using System.Linq;
using Optional;
using Optional.Unsafe;

namespace DataStorage.Implementations
{
    /// <summary>
    /// Mock implementation, this should be changed to actual thread safe implementation in future.
    /// 
    /// In actual implementation, somekind of service layer should probably handle event creation.
    /// </summary>
    public class MockGameSessionStorage : IGameSessionStorage, IGameSessionEventStorage
    {
        private readonly Dictionary<Guid, List<EventType>> _gameSessionEvents = new Dictionary<Guid, List<EventType>>();
        private readonly List<GameSession> _sessions = new List<GameSession>();

        public Guid CreateSession(Game game, IEnumerable<Errand> errands)
        {
            var session = new GameSession
            {
                Id = Guid.NewGuid(),
                GameName = game.Name,
                Errands = errands.Select(CreateErrandCopy).ToList(),
                Players = new List<string>()
            };
            _sessions.Add(session);
            AddEvent(session.Id, EventType.SessionCreated);
            return session.Id;
        }

        private Errand CreateErrandCopy(Errand source)
        {
            return new Errand
            {
                Id = source.Id,
                Description = source.Description
            };
        }

        public IEnumerable<GameSession> GetAll()
        {
            return _sessions.ToList();
        }

        public void JoinSession(Guid sessionId, string playerName)
        {
            var session = _sessions
                .FirstOrDefault(s => s.Id == sessionId)
                .SomeNotNull()
                .ValueOrFailure($"No session found with ID {sessionId}"); ;
            session.Players.Add(playerName);
        }

        public Option<GameSession> GetSingle(Guid id)
        {
            return _sessions.FirstOrDefault(session => session.Id == id).SomeNotNull();
        }

        public List<EventType> GetEvents(Guid sessionId)
        {
            try
            {
                return _gameSessionEvents[sessionId];
            }
            catch (Exception)
            {
                return new List<EventType>();
            }
        }

        public void AddEvent(Guid sessionId, EventType sessionEvent)
        {
            try
            {
                _gameSessionEvents[sessionId].Add(sessionEvent);
            }
            catch (Exception)
            {
                _gameSessionEvents[sessionId] = new List<EventType>();
                _gameSessionEvents[sessionId].Add(sessionEvent);
            }
        }
    }
}

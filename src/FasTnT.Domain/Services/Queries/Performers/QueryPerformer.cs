﻿using FasTnT.Domain.Model.Subscriptions;
using FasTnT.Domain.Model.Queries;
using FasTnT.Domain.Repositories;
using FasTnT.Domain.Model.Events;
using FasTnT.Domain.Exceptions;
using System.Linq;
using System;

namespace FasTnT.Domain.Services.Queries.Performers
{
    public class QueryPerformer : IQueryPerformer
    {
        private readonly IEventRepository _eventRepository;
        private readonly IQuery[] _queries;

        public QueryPerformer(IEventRepository eventRepository, IQuery[] queries)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public QueryEventResponse ExecuteSubscriptionQuery(Subscription subscription)
        {
            var source = _eventRepository.Query().Where(e => e.Request.RecordTime >= subscription.LastRunOn);
            var @params = subscription.Parameters.Select(p => new QueryParam { Name = p.ParameterName, Values = p.Values.Select(v => v.Value).ToArray() }).ToArray();

            return ExecuteInternal(subscription.QueryName, @params, source);
        }

        public QueryEventResponse ExecutePollQuery(string queryName, QueryParam[] parameters)
        {
            var source = _eventRepository.Query();

            return ExecuteInternal(queryName, parameters, source);
        }

        private QueryEventResponse ExecuteInternal(string queryName, QueryParam[] parameters, IQueryable<EpcisEvent> source)
        {
            var query = _queries.SingleOrDefault(q => q.Name.Equals(queryName)) ?? throw new NoSuchNameException($"Query '{queryName}' does not exist.");
            var events = query.ApplyFilter(source, parameters).ToList();

            query.PerformValidation(events, parameters);

            return new QueryEventResponse
            {
                Events = events,
                BusinessTransactions = _eventRepository.LoadBusinessTransactions(events),
                Epcs = _eventRepository.LoadEpcs(events),
                CustomFields = _eventRepository.LoadCustomFields(events),
                SourcesDestinations = _eventRepository.LoadSourceDestinations(events)
            };
        }
    }
}
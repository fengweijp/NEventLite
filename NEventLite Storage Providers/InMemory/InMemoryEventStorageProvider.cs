﻿using System;
using System.Collections.Generic;
using System.Linq;
using NEventLite.Domain;
using NEventLite.Events;
using NEventLite.Exceptions;
using NEventLite.Storage;

namespace NEventLite_Storage_Providers.InMemory
{
    public class InMemoryEventStorageProvider : IEventStorageProvider
    {
        private Dictionary<Guid, List<IEvent>> eventStream = new Dictionary<Guid, List<IEvent>>();

        public IEnumerable<IEvent> GetEvents(Type aggregateType, Guid aggregateId, int start, int count)
        {
            try
            {
                if (eventStream.ContainsKey(aggregateId))
                {

                    //this is needed for make sure it doesn't fail when we have int.maxValue for count
                    if (count > int.MaxValue - start)
                    {
                        count = int.MaxValue - start;
                    }

                    return
                        eventStream[aggregateId].Where(
                            o =>
                                (eventStream[aggregateId].IndexOf(o) >= start) &&
                                (eventStream[aggregateId].IndexOf(o) < (start + count)))
                            .ToArray();
                }
                else
                {
                    return new List<IEvent>();
                }
                
            }
            catch (Exception ex)
            {
                throw new AggregateNotFoundException($"The aggregate with {aggregateId} was not found. Details {ex.Message}");
            }

        }

        public IEvent GetLastEvent(Type aggregateType, Guid aggregateId)
        {
            if (eventStream.ContainsKey(aggregateId))
            {
                return eventStream[aggregateId].Last();
            }
            else
            {
                return null;
            }
        }

        public void CommitChanges(Type aggregateType, AggregateRoot aggregate)
        {
            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                if (eventStream.ContainsKey(aggregate.Id) == false)
                {
                    eventStream.Add(aggregate.Id, events.ToList());
                }
                else
                {
                    eventStream[aggregate.Id].AddRange(events);
                }
            }

        }
    }
}
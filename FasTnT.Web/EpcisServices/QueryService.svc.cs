﻿using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using FasTnT.Web.EpcisServices.Faults;
using FasTnT.Web.EpcisServices.Model;
using FasTnT.Domain.Services.Queries;
using FasTnT.Domain.Services.Subscriptions;
using FasTnT.Domain.Services.Formatting;

namespace FasTnT.Web.EpcisServices
{
    /// <summary>
    /// Implementation of the IQueryService SOAP WebService.
    /// </summary>
    public class QueryService : IQueryService
    {
        private readonly IQueryPerformer _queryPerformer;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IResponseFormatter _responseFormatter;

        public QueryService(ISubscriptionManager subscriptionManager, IQueryPerformer queryPerformer, IResponseFormatter responseFormatter)
        {
            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _queryPerformer = queryPerformer ?? throw new ArgumentNullException(nameof(queryPerformer));
            _responseFormatter = responseFormatter;
        }

        public string[] GetQueryNames()
        {
            try
            {
                return _queryPerformer.ListQueryNames().ToArray();
            }
            catch (Exception ex)
            {
                throw EpcisFault.Create(ex);
            }
        }

        public void Subscribe(Message request)
        {
            try
            {
                var subscription = SubscriptionRequest.Parse(XElement.Parse(request.GetReaderAtBodyContents().ReadOuterXml()));

                //TODO: store subscription
                //_subscriptionManager.Subscribe(subscription);
            }
            catch (Exception ex)
            {
                throw EpcisFault.Create(ex);
            }
        }

        public void Unsubscribe(string name)
        {
            try
            {
                //TODO: remove subscription
            }
            catch (Exception ex)
            {
                throw EpcisFault.Create(ex);
            }
        }

        public Message Poll(Message request)
        {
            try 
            {
                var pollRequest = PollRequest.Parse(XElement.Parse(request.GetReaderAtBodyContents().ReadOuterXml()));
                var results = _queryPerformer.ExecuteQuery(pollRequest.Name, pollRequest.Parameters);
                var formattedResponse = _responseFormatter.FormatPollResponse(pollRequest.Name, results);

                return MessageResponse.CreatePollResponse(formattedResponse.Root);
            }
            catch (Exception ex)
            {
                throw EpcisFault.Create(ex);
            }
        }

        public string[] GetSubscriptionIDs()
        {
            try
            {
                return _subscriptionManager.ListAllSubscriptions().Select(x => x.Name).ToArray();
            }
            catch (Exception ex)
            {
                throw EpcisFault.Create(ex);
            }
        }

        public string GetVendorVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
        }

        public string GetStandardVersion()
        {
            return "1.2";
        }
    }
}
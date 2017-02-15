﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using log4net.Repository.Hierarchy;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using SharePointAdminBot.Dialogs;


namespace SharePointAdminBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("MessagesController");

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                try
                {
                    await Conversation.SendAsync(activity, () => new MasterDialog());
                }
                catch (Exception ex)
                {
                    if (Logger.IsErrorEnabled) Logger.Error("Error in Post MessageController", ex);
                }
              
            }
            else
            {
                this.HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                if (message.MembersAdded.Any())
                {
                    var newMembers = message.MembersAdded?.Where(t => t.Id != message.Recipient.Id);
                    if (newMembers != null)
                        foreach (var newMember in newMembers)
                        {
                            if (Logger.IsDebugEnabled) Logger.DebugFormat("New member added to chat: {0}", newMember.Name);
                            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                            Activity reply = message.CreateReply("Hi I'm the SharePoint Admin Bot. Please logon first");
                            connector.Conversations.ReplyToActivityAsync(reply);
                            Conversation.SendAsync(message, () => new MasterDialog());
                        }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}
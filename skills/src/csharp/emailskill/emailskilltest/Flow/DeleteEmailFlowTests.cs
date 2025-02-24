﻿using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using EmailSkill.Responses.DeleteEmail;
using EmailSkill.Responses.Shared;
using EmailSkill.Utilities;
using EmailSkillTest.Flow.Fakes;
using EmailSkillTest.Flow.Utterances;
using Microsoft.Bot.Builder.Solutions.Util;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailSkillTest.Flow
{
    [TestClass]
    public class DeleteEmailFlowTests : EmailBotTestBase
    {
        [TestMethod]
        public async Task Test_NotDeleteEmail()
        {
            await this.GetTestFlow()
                .Send(DeleteEmailUtterances.DeleteEmails)
                .AssertReply(this.ShowAuth())
                .Send(this.GetAuthResponse())
                .AssertReply(this.ShowEmailList())
                .AssertReplyOneOf(this.NoFocusMessage())
                .Send(BaseTestUtterances.FirstOne)
                .AssertReply(this.DeleteConfirm())
                .Send(GeneralTestUtterances.No)
                .AssertReplyOneOf(this.NotSendingMessage())
                .AssertReply(this.ActionEndMessage())
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_DeleteEmail()
        {
            await this.GetTestFlow()
                .Send(DeleteEmailUtterances.DeleteEmails)
                .AssertReply(this.ShowAuth())
                .Send(this.GetAuthResponse())
                .AssertReply(this.ShowEmailList())
                .AssertReplyOneOf(this.NoFocusMessage())
                .Send(BaseTestUtterances.FirstOne)
                .AssertReply(this.DeleteConfirm())
                .Send(GeneralTestUtterances.Yes)
                .AssertReplyOneOf(this.DeleteSuccess())
                .AssertReply(this.ActionEndMessage())
                .StartTestAsync();
        }

        private string[] NotSendingMessage()
        {
            return this.ParseReplies(EmailSharedResponses.CancellingMessage, new StringDictionary());
        }

        private string[] NoFocusMessage()
        {
            return this.ParseReplies(EmailSharedResponses.NoFocusMessage, new StringDictionary());
        }

        private string[] DeleteSuccess()
        {
            return this.ParseReplies(DeleteEmailResponses.DeleteSuccessfully, new StringDictionary());
        }

        private Action<IActivity> DeleteConfirm()
        {
            return activity =>
            {
                var messageActivity = activity.AsMessageActivity();

                CollectionAssert.Contains(this.ParseReplies(DeleteEmailResponses.DeleteConfirm, new StringDictionary()), messageActivity.Text);
                Assert.AreEqual(messageActivity.Attachments.Count, 1);
            };
        }

        private Action<IActivity> ActionEndMessage()
        {
            return activity =>
            {
                Assert.AreEqual(activity.Type, ActivityTypes.Handoff);
            };
        }

        private Action<IActivity> AssertComfirmBeforeSendingPrompt()
        {
            return activity =>
            {
                var messageActivity = activity.AsMessageActivity();

                CollectionAssert.Contains(this.ParseReplies(EmailSharedResponses.ConfirmSend, new StringDictionary()), messageActivity.Text);
                Assert.AreEqual(messageActivity.Attachments.Count, 1);
            };
        }

        private Action<IActivity> ShowEmailList()
        {
            return activity =>
            {
                var messageActivity = activity.AsMessageActivity();

                // Get showed mails:
                var showedItems = ((MockServiceManager)this.ServiceManager).MailService.MyMessages;

                var replies = this.ParseReplies(EmailSharedResponses.ShowEmailPrompt, new StringDictionary()
                {
                    { "TotalCount", showedItems.Count.ToString() },
                    { "EmailListDetails", SpeakHelper.ToSpeechEmailListString(showedItems, TimeZoneInfo.Local, ConfigData.GetInstance().MaxReadSize) },
                });

                CollectionAssert.Contains(replies, messageActivity.Text);
                Assert.AreNotEqual(messageActivity.Attachments.Count, 0);
            };
        }

        private Action<IActivity> ShowAuth()
        {
            return activity =>
            {
                var message = activity.AsMessageActivity();
                Assert.AreEqual(1, message.Attachments.Count);
                Assert.AreEqual("application/vnd.microsoft.card.oauth", message.Attachments[0].ContentType);
            };
        }
    }
}

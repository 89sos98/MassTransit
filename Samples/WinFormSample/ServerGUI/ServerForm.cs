﻿// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace ServerGUI
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Forms;
    using Castle.Windsor;
    using log4net;
    using MassTransit.ServiceBus;
    using MassTransit.WindsorIntegration;
    using Messages;

    public partial class ServerForm : Form
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (ServerForm));

        private IServiceBus _bus;

        private IWindsorContainer _container;

        public ServerForm()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            StartService();
        }

        private void StartService()
        {
            StopService();

            try
            {
                _container = new DefaultMassTransitContainer("Server.Castle.xml");

                _bus = _container.Resolve<IServiceBus>("server");

                _bus.AddComponent<UserAgentSession>();
                _bus.AddComponent<TheAnswerMan>();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                MessageBox.Show("The server failed to start:\n\n" + ex.Message);
            }
        }


        private void StopService()
        {
            try
            {
                if (_bus != null)
                {
                    _bus.Dispose();
                    _bus = null;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            try
            {
                if (_container != null)
                {
                    _container.Dispose();
                    _container = null;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            StopService();

            e.Cancel = false;
        }

        private void answerQuestions_CheckedChanged(object sender, EventArgs e)
        {
            TheAnswerMan.Enabled = answerQuestions.Checked;
        }

        private void serverTime_ValueChanged(object sender, EventArgs e)
        {
            TheAnswerMan.ServerTime = Convert.ToInt32(serverTime.Value);
        }
    }

    public class TheAnswerMan :
        Consumes<SubmitQuestion>.Selected
    {
        private static bool _enabled = true;
        public static bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }


        public IServiceBus Bus { get; set; }

        private static volatile int _serverTime;
        public static int ServerTime
        {
            get { return _serverTime; }
            set { _serverTime = value; }
        }

        public void Consume(SubmitQuestion message)
        {
            Thread.Sleep(_serverTime);

            QuestionAnswered answer = new QuestionAnswered(message.CorrelationId);

            Bus.Publish(answer);
        }

        public bool Accept(SubmitQuestion message)
        {
            return _enabled;
        }
    }
}
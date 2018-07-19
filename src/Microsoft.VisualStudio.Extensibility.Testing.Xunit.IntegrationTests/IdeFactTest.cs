﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for more information.

namespace Microsoft.VisualStudio.Extensibility.Testing.Xunit.IntegrationTests
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using global::Xunit;
    using global::Xunit.Threading;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Threading;
    using _DTE = EnvDTE._DTE;
    using DTE = EnvDTE.DTE;

    public class IdeFactTest : AbstractIdeIntegrationTest
    {
        [IdeFact]
        public void TestOpenAndCloseIDE()
        {
            Assert.Equal("devenv", Process.GetCurrentProcess().ProcessName);
            var dte = (DTE)ServiceProvider.GetService(typeof(_DTE));
            Assert.NotNull(dte);
        }

        [IdeFact]
        public void TestRunsOnUIThread()
        {
            Assert.True(Application.Current.Dispatcher.CheckAccess());
        }

        [IdeFact]
        public async Task TestRunsOnUIThreadAsync()
        {
            Assert.True(Application.Current.Dispatcher.CheckAccess());
            await Task.Yield();
            Assert.True(Application.Current.Dispatcher.CheckAccess());
        }

        [IdeFact]
        public async Task TestYieldsToWorkAsync()
        {
            Assert.True(Application.Current.Dispatcher.CheckAccess());
            await Task.Factory.StartNew(
                () => { },
                CancellationToken.None,
                TaskCreationOptions.None,
                new SynchronizationContextTaskScheduler(new DispatcherSynchronizationContext(Application.Current.Dispatcher)));
            Assert.True(Application.Current.Dispatcher.CheckAccess());
        }

        [IdeFact]
        public async Task TestJoinableTaskFactoryAsync()
        {
            Assert.NotNull(JoinableTaskContext);
            Assert.NotNull(JoinableTaskFactory);
            Assert.Equal(Thread.CurrentThread, JoinableTaskContext.MainThread);

            await TaskScheduler.Default;

            Assert.NotEqual(Thread.CurrentThread, JoinableTaskContext.MainThread);

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            Assert.Equal(Thread.CurrentThread, JoinableTaskContext.MainThread);
        }

        [IdeFact(MaxVersion = VisualStudioVersion.VS2012)]
        public void TestJoinableTaskFactoryProvidedByTest()
        {
            var taskSchedulerServiceObject = ServiceProvider.GetService(typeof(SVsTaskSchedulerService));
            Assert.NotNull(taskSchedulerServiceObject);

            var taskSchedulerService = taskSchedulerServiceObject as IVsTaskSchedulerService;
            Assert.NotNull(taskSchedulerService);

            var taskSchedulerService2 = taskSchedulerServiceObject as IVsTaskSchedulerService2;
            Assert.Null(taskSchedulerService2);

            Assert.NotNull(JoinableTaskContext);
        }

        [IdeFact(MinVersion = VisualStudioVersion.VS2013)]
        public void TestJoinableTaskFactoryObtainedFromEnvironment()
        {
            var taskSchedulerServiceObject = ServiceProvider.GetService(typeof(SVsTaskSchedulerService));
            Assert.NotNull(taskSchedulerServiceObject);

            var taskSchedulerService = taskSchedulerServiceObject as IVsTaskSchedulerService2;
            Assert.NotNull(taskSchedulerService);

            Assert.Same(JoinableTaskContext, taskSchedulerService.GetAsyncTaskContext());
        }
    }
}
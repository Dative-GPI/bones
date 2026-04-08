using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Akka.Actor;

namespace Bones.Akka
{
    public abstract class InitializeActor :
        ReceiveActor,
        IWithUnboundedStash,
        IWithTimers
    {
        protected const string INIT = "init";
        protected const int MAX_WAIT_TIME = 30;

        int ReinitializeCount { get; set; }

        protected IActorRef _self;
        protected ILogger<InitializeActor> _logger;
        public IStash Stash { get; set; }
        public ITimerScheduler Timers { get; set; }

        public InitializeActor(
            ILogger<InitializeActor> logger
        )
        {
            _logger = logger;
            ReinitializeCount = 0;
            Initializing();
        }

        // Initialize

        protected override void PreStart()
        {
            base.PreStart();
            Self.Tell(INIT);
        }

        private void Initializing()
        {
            OnInitializing();

            ReceiveAsync<string>(StartInitialization, s => s == INIT);
            ReceiveAny(m => Stash.Stash());
        }

        /// <summary>
        /// Allows us to add custom receive handlers before the basic <see cref="ReceiveAny"/> during initialization.
        /// This method can be overridden in derived classes to set up additional message handlers.
        /// </summary>
        protected virtual void OnInitializing()
        {
        }

        private async Task StartInitialization(string init)
        {
            _self = Self;

            try
            {
                await Initialize();
                ReinitializeCount = 0;
                Stash.UnstashAll();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{path}] An error occurred during initialization. Retrying...", Self.Path);
                Reinitialize();
            }
        }

        protected abstract Task Initialize();

        // Reinitialize

        public void Reinitialize()
        {
            _logger.LogInformation("Retrying in {time} seconds", Math.Min(MAX_WAIT_TIME, 2 * ReinitializeCount));

            Timers.StartSingleTimer(
                INIT,
                INIT,
                TimeSpan.FromSeconds(Math.Min(MAX_WAIT_TIME, 2 * ReinitializeCount))
            );

            ReinitializeCount++;
        }

        // Lifecycle

        protected override void PostStop()
        {
            base.PostStop();
            Timers.CancelAll();
        }
    }
}

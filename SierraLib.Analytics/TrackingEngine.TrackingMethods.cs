using SierraLib.Analytics.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SierraLib.Analytics
{
    public abstract partial class TrackingEngine
    {
        #region ITrackingApplication

        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the current <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public async Task Track(ITrackingApplication application, params ITrackingModule[] modules)
        {
            await Track(application, modules as IEnumerable<ITrackingModule>);
        }

        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the current <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public async Task Track(ITrackingApplication application, IEnumerable<ITrackingModule> modules)
        {
            if (!GlobalEnabled || !Enabled)
                return;

            //Check that we have a valid UserAgent string, if not then load a default one
            if (UserAgent.IsNullOrWhitespace())
                UpdateUserAgent(application.Name, application.Version);

            var request = await CreateRequestAsync(application);
            await PreProcessAsync(request);

            List<ITrackingFinalize> requiringFinalization = new List<ITrackingFinalize>();

            foreach (var module in modules)
            {
                module.PreProcess(request);
                if (module is ITrackingFinalize)
                    requiringFinalization.Add(module as ITrackingFinalize);
            }

            await PostProcessAsync(request);

            var preparedRequest = await PrepareRequestAsync(request, requiringFinalization);

            OnRequestPrepared(preparedRequest);
        }

        #endregion

        #region Action

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track(Expression<Action> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), triggerType, modules));
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track(Expression<Action> triggerMethod, IEnumerable<ITrackingModule> modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), TrackOn.All, modules));
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track(Expression<Action> triggerMethod, TrackOn triggerType, IEnumerable<ITrackingModule> modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), triggerType, modules));
        }

        #endregion

        #region Action<T>

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Action<T>> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), triggerType, modules));
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Action<T>> triggerMethod, IEnumerable<ITrackingModule> modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), TrackOn.All, modules));
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Action<T>> triggerMethod, TrackOn triggerType, IEnumerable<ITrackingModule> modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), triggerType, modules));
        }

        #endregion

        #region Func<T>

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Func<T>> triggerMethod, params ITrackingModule[] modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), TrackOn.All, modules));
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Func<T>> triggerMethod, TrackOn triggerType, params ITrackingModule[] modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), triggerType, modules));
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Func<T>> triggerMethod, IEnumerable<ITrackingModule> modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), TrackOn.All, modules));
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Func<T>> triggerMethod, TrackOn triggerType, IEnumerable<ITrackingModule> modules)
        {
            Task.Run(() => Track(triggerMethod.GetMemberInfo(), triggerType, modules));
        }

        #endregion

        #region Internal Handling

        private void Track(MemberInfo method, TrackOn triggerType, IEnumerable<ITrackingModule> modules)
        {
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);

            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication ?? new TrackingApplicationAttribute();
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => x.Filter.HasFlag(triggerType)).Concat(modules).ToArray();

            if (engineAttributes.Any())
                Task.Run(() => engineAttributes.First().Engine.Track(application, dataBundle));
            else
                Task.Run(() => Track(application, dataBundle));
        }

        #endregion
    }
}

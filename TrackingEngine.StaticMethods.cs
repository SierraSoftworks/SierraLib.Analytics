using SierraLib.Analytics.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SierraLib.Analytics
{
    public abstract partial class TrackingEngine
    {
        #region ITrackingApplication

        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public static void TrackDefault(ITrackingApplication application, params ITrackingModule[] modules)
        {
            CheckDefaultSet();
            Default.Track(application, modules);
        }

        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public static void TrackDefault(ITrackingApplication application, IEnumerable<ITrackingModule> modules)
        {
            CheckDefaultSet();
            Default.Track(application, modules);
        }

        #endregion

        #region Action

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault(Expression<Action> triggerMethod, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, TrackOn.All, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, TrackOn.All, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault(Expression<Action> triggerMethod, TrackOn triggerType, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }
        
        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault(Expression<Action> triggerMethod, IEnumerable<ITrackingModule> modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, TrackOn.All, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, TrackOn.All, modules);
            }
        }
        
        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault(Expression<Action> triggerMethod, TrackOn triggerType, IEnumerable<ITrackingModule> modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        #endregion

        #region Action<T>

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Action<T>> triggerMethod, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, TrackOn.All, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, TrackOn.All, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Action<T>> triggerMethod, TrackOn triggerType, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Action<T>> triggerMethod, IEnumerable<ITrackingModule> modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, TrackOn.All, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, TrackOn.All, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Action<T>> triggerMethod, TrackOn triggerType, IEnumerable<ITrackingModule> modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        #endregion

        #region Func<T>

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Func<T>> triggerMethod, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, TrackOn.All, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, TrackOn.All, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Func<T>> triggerMethod, TrackOn triggerType, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Func<T>> triggerMethod, IEnumerable<ITrackingModule> modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, TrackOn.All, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, TrackOn.All, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Func<T>> triggerMethod, TrackOn triggerType, IEnumerable<ITrackingModule> modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        #endregion
    }
}

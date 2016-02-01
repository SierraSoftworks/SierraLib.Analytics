using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SierraLib.Analytics
{
    public abstract partial class TrackingEngine
    {
        #region Engine Caching

        [NonSerialized]
        static volatile bool CreatingEngine = false;
        [NonSerialized]
        static readonly object CreatingEngineLock = new object();
        [NonSerialized]
        static Dictionary<string, TrackingEngine> EngineCache = new Dictionary<string, TrackingEngine>();

        /// <summary>
        /// Creates a new instance of the requested <see cref="TrackingEngine"/> type or
        /// returns an existing instance if one has already been instantiated.
        /// </summary>
        /// <typeparam name="TEngine">The type of <see cref="TrackingEngine"/> being instantiated</typeparam>
        /// <param name="account">A unique ID differentiating this particular tracking engine from others, should give the same value as <see cref="TrackingEngine.GetEngineID"/></param>
        /// <param name="initializer">A function to create a new instance of the <typeparamref name="TEngine"/> if one is not yet present</param>
        /// <returns>Returns an instance of the requested <typeparamref name="TEngine"/></returns>
        public static TEngine Create<TEngine>(string account, Func<string, TEngine> initializer) where TEngine : TrackingEngine
        {
            if (EngineCache.ContainsKey(account)) return (TEngine)EngineCache[account];
            else
            {
                lock (CreatingEngineLock) //Prevent multiple threads from instantiating engines at the same time to prevent race conditions on CreatingEngine
                {
                    CreatingEngine = true;
                    EngineCache.Add(account, initializer(account));
                    CreatingEngine = false;
                    if (Default == null)
                        return (TEngine)(Default = EngineCache[account]);
                    else
                        return (TEngine)EngineCache[account];
                }
            }
        }

        #endregion

        #region Engine Access

        /// <summary>
        /// Sets this as the default <see cref="TrackingEngine"/> used when calls are made to
        /// the <see cref="TrackingEngine.TrackDefault"/> methods.
        /// </summary>
        public void SetDefault()
        {
            Default = this;
        }

        /// <summary>
        /// Gets the current default <see cref="TrackingEngine"/>
        /// </summary>
        /// <remarks>
        /// The default <see cref="TrackingEngine"/> can be set by calling
        /// <see cref="TrackingEngine.SetDefault()"/> on the instance of the
        /// engine you wish to set as the <see cref="Default"/>.
        /// </remarks>
        public static TrackingEngine Default
        { get; private set; }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the current item.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <paramref name="target"/> method</returns>
        public static TrackingEngine GetEngine(Expression<Action> target)
        {
			var memberInfo = target.GetMemberInfo();
			if (memberInfo == null) return null;
			var engineAttribute = memberInfo.GetCustomAttribute<TrackingEngineAttributeBase>(true);
			if (engineAttribute == null) return null;

            return engineAttribute.Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the current item.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <paramref name="target"/> method</returns>
        public static TrackingEngine GetEngine<T>(Expression<Action<T>> target)
		{
			var memberInfo = target.GetMemberInfo();
			if (memberInfo == null) return null;
			var engineAttribute = memberInfo.GetCustomAttribute<TrackingEngineAttributeBase>(true);
			if (engineAttribute == null) return null;

			return engineAttribute.Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the current item.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <paramref name="target"/> method</returns>
        public static TrackingEngine GetEngine<T>(Expression<Func<T>> target)
		{
			var memberInfo = target.GetMemberInfo();
			if (memberInfo == null) return null;
			var engineAttribute = memberInfo.GetCustomAttribute<TrackingEngineAttributeBase>(true);
			if (engineAttribute == null) return null;

			return engineAttribute.Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the given type.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <typeparamref name="T"/>ype</returns>
        public static TrackingEngine GetEngine<T>()
		{
			var engineAttribute = typeof(T).GetCustomAttribute<TrackingEngineAttributeBase>(true);
			if (engineAttribute == null) return null;

			return engineAttribute.Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> which reports to the given ID (usually the account code)
        /// </summary>
        /// <param name="engineID">The unique ID used by the engine to identify itself</param>
        /// <returns>Returns the engine which reports the given <paramref name="engineID"/> or <c>null</c> if no such engine was found</returns>
        public static TrackingEngine GetEngineByID(string engineID)
        {
            if (!EngineCache.ContainsKey(engineID))
                return null;
            return EngineCache[engineID];
        }

        private static void CheckDefaultSet()
        {
            if (Default == null)
                throw new InvalidOperationException("You must have a tracking engine attribute present or call <TrackingEngine>.SetDefault before attempting to use this method");
        }

        #endregion
    }
}

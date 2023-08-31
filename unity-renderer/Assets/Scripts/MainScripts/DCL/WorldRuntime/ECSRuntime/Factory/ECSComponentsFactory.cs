using System;
using System.Collections.Generic;

namespace DCL.ECSRuntime
{
    public class ECSComponentsFactory
    {
        public delegate IECSComponent ECSComponentBuilder();

        private readonly Dictionary<int, ECSComponentBuilder> components =
            new Dictionary<int, ECSComponentBuilder>();

        /// <summary>
        /// read only dictionary containing components builders
        /// </summary>
        public IReadOnlyDictionary<int, ECSComponentBuilder> componentBuilders => components;

        /// <summary>
        /// put component to the components builder dictionary
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="handlerBuilder"></param>
        /// <param name="deserializer"></param>
        /// <typeparam name="ModelType"></typeparam>
        // public void AddOrReplaceComponent<ModelType>(int componentId,
        //     Func<IECSComponentHandler<ModelType>> handlerBuilder,
        //     Func<object, ModelType> deserializer)
        // {
        //     components[componentId] = CreateComponentBuilder(handlerBuilder, deserializer);
        // }

        /// <summary>
        /// Remove component to the components builder dictionary
        /// </summary>
        /// <param name="componentId"></param>
        public void RemoveComponent(int componentId)
        {
            components.Remove(componentId);
        }

        /// <summary>
        /// creates a component builder
        /// </summary>
        /// <param name="handlerBuilder"></param>
        /// <param name="deserializer"></param>
        /// <typeparam name="ModelType"></typeparam>
        /// <returns>component builder</returns>
        // public static ECSComponentBuilder CreateComponentBuilder<ModelType>(Func<IECSComponentHandler<ModelType>> handlerBuilder, Func<object, ModelType> deserializer)
        // {
        //     return () => BuildComponent(handlerBuilder, deserializer);
        // }

        private static IECSComponent BuildComponent<ModelType>(Func<IECSComponentHandler<ModelType>> handlerBuilder, Func<object, ModelType> deserializer)
        {
            return new ECSComponent<ModelType>(deserializer, handlerBuilder);
        }

        // FD:: testing out the following methods for pooling support -----------------------------

        /// <summary>
        /// Add or replace a component in the components builder dictionary.
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="handlerBuilder"></param>
        /// <param name="deserializer"></param>
        /// <param name="iecsComponentPool"></param>
        /// <typeparam name="ModelType"></typeparam>
        public void AddOrReplaceComponent<ModelType>(
            int componentId,
            Func<IECSComponentHandler<ModelType>> handlerBuilder,
            Func<object, ModelType> deserializer = null,
            IECSComponentPool<ModelType> iecsComponentPool = null)
        {
            // Validate that either a deserializer or a component pool is provided, but not both.
            if (deserializer != null && iecsComponentPool != null)
            {
                throw new ArgumentException("Cannot specify both a deserializer and a component pool.");
            }

            components[componentId] = CreateComponentBuilder(handlerBuilder, deserializer, iecsComponentPool);
        }


        /// <summary>
        /// Creates a component builder, either with or without pooling support based on the provided parameters.
        /// </summary>
        /// <param name="handlerBuilder"></param>
        /// <param name="deserializer"></param>
        /// <param name="iecsComponentPool"></param>
        /// <typeparam name="ModelType"></typeparam>
        /// <returns>Returns a delegate to create the component.</returns>
        public static ECSComponentBuilder CreateComponentBuilder<ModelType>(
            Func<IECSComponentHandler<ModelType>> handlerBuilder,
            Func<object, ModelType> deserializer = null,
            IECSComponentPool<ModelType> iecsComponentPool = null)
        {
            // Validate that either a deserializer or a component pool is provided, but not both.
            if (deserializer != null && iecsComponentPool != null)
            {
                throw new ArgumentException("Cannot specify both a deserializer and a component pool.");
            }

            // Choose the appropriate builder function based on the provided parameters.
            if (deserializer != null)
            {
                return () => BuildComponent(handlerBuilder, deserializer);
            }
            else if (iecsComponentPool != null)
            {
                return () => BuildPoolableComponent(iecsComponentPool, handlerBuilder);
            }
            else
            {
                throw new ArgumentException("Either a deserializer or a component pool must be specified.");
            }
        }


        /// <summary>
        /// Builds a component instance with pooling support.
        /// </summary>
        /// <param name="iecsComponentPool"></param>
        /// <param name="handlerBuilder"></param>
        /// <typeparam name="ModelType"></typeparam>
        /// <returns>Returns the constructed component.</returns>
        private static IECSComponent BuildPoolableComponent<ModelType>(
            IECSComponentPool<ModelType> iecsComponentPool,
            Func<IECSComponentHandler<ModelType>> handlerBuilder)
        {
            return new ECSComponent<ModelType>(handlerBuilder, iecsComponentPool);
        }

    }
}

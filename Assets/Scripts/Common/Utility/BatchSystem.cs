// <copyright file="BatchSystem.cs" company="Timothy Raines">
//     Copyright (c) Timothy Raines. All rights reserved.
// </copyright>

namespace BovineLabs.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Common.Native;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// The BatchBarrierSystem.
    /// </summary>
    public sealed class BatchSystem : ComponentSystem
    {
        private readonly Dictionary<Type, IEventBatch> types = new Dictionary<Type, IEventBatch>();

        private readonly List<NativeQueue<Entity>> destroyBatch = new List<NativeQueue<Entity>>();

        private interface IEventBatch : IDisposable
        {
            void Update(EntityManager entityManager);
        }

        /// <summary>
        /// Any component added to the returned <see cref="NativeQueue{T}"/> will be attached to a new entity as an event.
        /// These entities will be destroyed after 1 frame.
        /// </summary>
        /// <typeparam name="T">The type of component data event.</typeparam>
        /// <returns>A <see cref="NativeQueue{T}"/> which any component that is added will be turned into a single frame event.</returns>
        public NativeQueue<T> GetEventBatch<T>()
            where T : struct, IComponentData
        {
            if (!this.types.TryGetValue(typeof(T), out var create))
            {
                create = this.types[typeof(T)] = new EventBatch<T>();
            }

            return ((EventBatch<T>)create).GetNew();
        }

        public NativeQueue<Entity> GetDestroyBatch()
        {
            var queue = new NativeQueue<Entity>(Allocator.TempJob);
            this.destroyBatch.Add(queue);
            return queue;
        }

        /// <inheritdoc />
        protected override void OnDestroyManager()
        {
            foreach (var t in this.types)
            {
                t.Value.Dispose();
            }

            this.types.Clear();
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            this.DestroyEntities();

            foreach (var t in this.types)
            {
                t.Value.Update(this.EntityManager);
            }
        }

        private void DestroyEntities()
        {
            var count = 0;

            foreach (var s in this.destroyBatch)
            {
                count += s.Count;
            }

            if (count == 0)
            {
                return;
            }

            var array = new NativeArray<Entity>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            int index = 0;

            foreach (var destroy in this.destroyBatch)
            {
                while (destroy.TryDequeue(out var entity))
                {
                    array[index++] = entity;
                }

                destroy.Dispose();
            }

            this.destroyBatch.Clear();

            this.EntityManager.DestroyEntity(array);
            array.Dispose();
        }

        private class EventBatch<T> : IEventBatch
            where T : struct, IComponentData
        {
            private readonly List<NativeQueue<T>> queues = new List<NativeQueue<T>>();
            private readonly EntityArchetypeQuery query;

            private EntityArchetype archetype;
            private NativeArray<Entity> entities;

            public EventBatch()
            {
                this.query = new EntityArchetypeQuery
                {
                    Any = Array.Empty<ComponentType>(),
                    None = Array.Empty<ComponentType>(),
                    All = new[] { ComponentType.Create<T>() },
                };
            }

            public NativeQueue<T> GetNew()
            {
                // Having allocation leak warnings when using TempJob
                var queue = new NativeQueue<T>(Allocator.Persistent);
                this.queues.Add(queue);
                return queue;
            }

            /// <inheritdoc />
            public void Update(EntityManager entityManager)
            {
                if (this.entities.IsCreated)
                {
                    entityManager.DestroyEntity(this.entities);
                    this.entities.Dispose();
                }

                var count = this.GetCount();

                if (count == 0)
                {
                    return;
                }

                // Felt like Temp should be the allocator but gets disposed for some reason.
                this.entities = new NativeArray<Entity>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                if (!this.archetype.Valid)
                {
                    this.archetype = entityManager.CreateArchetype(typeof(T));
                }

                entityManager.CreateEntity(this.archetype, this.entities);

                JobHandle handle = default;

                var chunkIndex = new NativeUnit<int>(Allocator.TempJob);
                var entityIndex = new NativeUnit<int>(Allocator.TempJob);

                var componentType = entityManager.GetArchetypeChunkComponentType<T>(false);

                var chunks = entityManager.CreateArchetypeChunkArray(this.query, Allocator.TempJob);

                foreach (var queue in this.queues)
                {
                    handle = new SetJob
                    {
                        Chunks = chunks,
                        Queue = queue,
                        ChunkIndex = chunkIndex,
                        EntityIndex = entityIndex,
                        ComponentType = componentType,
                    }
                        .Schedule(handle);
                }

                handle.Complete();

                chunks.Dispose();
                chunkIndex.Dispose();
                entityIndex.Dispose();

                foreach (var queue in this.queues)
                {
                    queue.Dispose();
                }

                this.queues.Clear();
            }

            public void Dispose()
            {
                if (this.entities.IsCreated)
                {
                    this.entities.Dispose();
                }

                foreach (var queue in this.queues)
                {
                    queue.Dispose();
                }
                this.queues.Clear();
            }

            private int GetCount()
            {
                var sum = 0;
                foreach (var i in this.queues)
                {
                    sum += i.Count;
                }

                return sum;
            }


            [BurstCompile]
            private struct SetJob : IJob
            {
                public NativeQueue<T> Queue;

                public NativeArray<ArchetypeChunk> Chunks;

                public NativeUnit<int> ChunkIndex;

                public NativeUnit<int> EntityIndex;

                public ArchetypeChunkComponentType<T> ComponentType;

                /// <inheritdoc />
                public void Execute()
                {
                    for (; this.ChunkIndex.Value < this.Chunks.Length; this.ChunkIndex.Value++)
                    {
                        var chunk = this.Chunks[this.ChunkIndex.Value];

                        var components = chunk.GetNativeArray(this.ComponentType);

                        var intLocalIndex = this.EntityIndex.Value;

                        while (this.Queue.TryDequeue(out var item) && intLocalIndex < components.Length)
                        {
                            components[intLocalIndex++] = item;
                        }

                        this.EntityIndex.Value = intLocalIndex < components.Length ? intLocalIndex : 0;
                    }
                }
            }
        }
    }
}
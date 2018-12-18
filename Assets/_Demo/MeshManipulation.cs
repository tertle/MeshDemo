using BovineLabs.Common.Utility;
using BovineLabs.Systems.Mesh;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct MeshScale : IComponentData
{
    public float Step;
    public int Count;
    public int Max;
}

/// <summary>
/// The MeshManipulation.
/// </summary>
public class MeshManipulation : JobComponentSystem
{
    private BatchSystem batchSystem;

    /// <inheritdoc />
    protected override void OnCreateManager()
    {
        this.batchSystem = this.World.GetOrCreateManager<BatchSystem>();
    }

    /// <inheritdoc />
    protected override JobHandle OnUpdate(JobHandle handle)
    {
        return new ManipulateMeshJob
            {
                Vertices = this.GetBufferFromEntity<Vertex>(),
                MeshDirtyQueue = this.batchSystem.GetEventBatch<MeshDirty>().ToConcurrent(),
            }
            .Schedule(this, handle);
    }

    /// <summary>
    ///  Just some random mesh manipulation. Frame dependent, don't turn off sync.
    /// </summary>
    [BurstCompile]
    [RequireComponentTag(typeof(Vertex))]
    private struct ManipulateMeshJob : IJobProcessComponentDataWithEntity<MeshScale>
    {
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<Vertex> Vertices;

        public NativeQueue<MeshDirty>.Concurrent MeshDirtyQueue;

        /// <inheritdoc />
        public void Execute(Entity entity, int index, ref MeshScale meshScale)
        {
            this.MeshDirtyQueue.Enqueue(new MeshDirty{Entity = entity});

            meshScale.Count += 1;

            if (meshScale.Count == meshScale.Max)
            {
                meshScale.Count = -meshScale.Max;
            }

            var vertices = this.Vertices[entity].Reinterpret<float3>();

            var offset = meshScale.Step;

            if (meshScale.Count < 0)
            {
                offset = -offset;
            }

            for (var i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                v.y += offset;
                vertices[i] = v;
            }
        }
    }
}

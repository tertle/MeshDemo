// <copyright file="MeshSystem.cs" company="Timothy Raines">
//     Copyright (c) Timothy Raines. All rights reserved.
// </copyright>

namespace BovineLabs.Systems.Mesh
{
    using System.Collections.Generic;
    using BovineLabs.Common.Native;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Rendering;
    using UnityEngine;

    /// <summary>
    ///     Updates a mesh.
    /// </summary>
    public class MeshSystem : ComponentSystem
    {
        private readonly List<Vector3> verticesList = new List<Vector3>();
        private readonly List<Vector3> normalsList = new List<Vector3>();
        private readonly List<Vector3> uvsList = new List<Vector3>();
        private readonly List<int> trianglesList = new List<int>();

        private readonly HashSet<Entity> entitySet = new HashSet<Entity>();
        private ComponentGroup meshDirtyQuery;

        /// <inheritdoc/>
        protected override void OnCreateManager()
        {
            this.meshDirtyQuery = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new[] { ComponentType.ReadOnly<MeshDirty>(), }
            });
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            this.entitySet.Clear();

            var meshDirtyType = this.GetArchetypeChunkComponentType<MeshDirty>(true);

            var chunks = this.meshDirtyQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            
            for(var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                var chunk = chunks[chunkIndex];
                var meshDirty = chunk.GetNativeArray(meshDirtyType);

                for (var index = 0; index < chunk.Count; index++)
                {
                    var entity = meshDirty[index].Entity;

                    // Already updated this entity this frame
                    if (!this.entitySet.Add(entity))
                    {
                        continue;
                    }

                    Mesh mesh;

                    if (this.EntityManager.HasComponent<MeshInstanceRenderer>(entity))
                    {
                        var meshInstanceRenderer =
                            this.EntityManager.GetSharedComponentData<MeshInstanceRenderer>(entity);
                        mesh = meshInstanceRenderer.mesh;
                    }
                    else if (this.EntityManager.HasComponent<MeshFilter>(entity))
                    {
                        var meshFilter = this.EntityManager.GetComponentObject<MeshFilter>(entity);
                        mesh = meshFilter.mesh;
                    }
                    else
                    {
                        Debug.Log("Unsupported mesh container");
                        continue;
                    }

                    this.SetMesh(
                        mesh,
                        this.EntityManager.GetBuffer<Vertex>(entity).Reinterpret<Vector3>(),
                        this.EntityManager.GetBuffer<Uv>(entity).Reinterpret<Vector3>(),
                        this.EntityManager.GetBuffer<Normal>(entity).Reinterpret<Vector3>(),
                        this.EntityManager.GetBuffer<Triangle>(entity).Reinterpret<int>());
                }
            }

            chunks.Dispose();
        }

        private void SetMesh(
            Mesh mesh,
            DynamicBuffer<Vector3> vertices,
            DynamicBuffer<Vector3> uvs,
            DynamicBuffer<Vector3> normals,
            DynamicBuffer<int> triangles)
        {
            mesh.Clear();

            if (vertices.Length == 0)
            {
                return;
            }

            this.verticesList.AddRange(vertices);
            this.uvsList.AddRange(uvs);
            this.normalsList.AddRange(normals);
            this.trianglesList.AddRange(triangles);

            mesh.SetVertices(this.verticesList);
            mesh.SetNormals(this.normalsList);
            mesh.SetUVs(0, this.uvsList);
            mesh.SetTriangles(this.trianglesList, 0);

            this.verticesList.Clear();
            this.normalsList.Clear();
            this.uvsList.Clear();
            this.trianglesList.Clear();
        }
    }
}
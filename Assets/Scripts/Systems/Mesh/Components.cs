// <copyright file="Components.cs" company="Timothy Raines">
//     Copyright (c) Timothy Raines. All rights reserved.
// </copyright>
// <summary>
//   Components to define a mesh
// </summary>

namespace BovineLabs.Systems.Mesh
{
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    ///     The buffer of mesh vertices.
    /// </summary>
    [InternalBufferCapacity(0)]
    public struct Vertex : IBufferElementData
    {
        /// <summary>
        ///     The Vertex.
        /// </summary>
        public float3 Value;
    }

    /// <summary>
    ///     The buffer of mesh uvs.
    /// </summary>
    [InternalBufferCapacity(0)]
    public struct Uv : IBufferElementData
    {
        /// <summary>
        ///     The uv.
        /// </summary>
        public float3 Value;
    }

    /// <summary>
    ///     The buffer of mesh normals.
    /// </summary>
    [InternalBufferCapacity(0)]
    public struct Normal : IBufferElementData
    {
        /// <summary>
        ///     The normal.
        /// </summary>
        public float3 Value;
    }

    /// <summary>
    ///     The buffer of mesh triangles.
    /// </summary>
    [InternalBufferCapacity(0)]
    public struct Triangle : IBufferElementData
    {
        /// <summary>
        ///     The triangle.
        /// </summary>
        public int Value;
    }

    /// <summary>
    ///     Component tag that marks a mesh as being dirty and requiring an update.
    /// </summary>
    public struct MeshDirty : IComponentData
    {
        public Entity Entity;
    }
}

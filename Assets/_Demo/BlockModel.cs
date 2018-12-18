// <copyright file="BlockModel.cs" company="Timothy Raines">
//     Copyright (c) Timothy Raines. All rights reserved.
// </copyright>

namespace BovineLabs.Systems.World.Models
{
    using System;

    using Unity.Collections;
    using Unity.Mathematics;

    /// <summary>
    ///     Defines a model for a cube.
    /// </summary>
    public struct BlockModel : IDisposable
    {
        private const float V0 = 0f;
        private const float V1 = 1f;

        /// <summary>
        ///     The vertices of a cube, always a length of 24.
        /// </summary>
        public NativeArray<float3> Vertices;

        /// <summary>
        ///     The normals of the cube, always a length of 6.
        /// </summary>
        public NativeArray<float3> Normals;

        /// <summary>
        ///     The uvs of a cube, always a length of 24.
        /// </summary>
        public NativeArray<float3> Uvs;

        /// <summary>
        ///     The triangles of a cube, always a length of 36. Every 3 values represent a triangle.
        /// </summary>
        public NativeArray<int> Triangles;

        /// <summary>
        ///     Create a default version of the BlockModel.
        /// </summary>
        /// <returns>
        /// The <see cref="BlockModel"/>.
        /// </returns>
        public static BlockModel Create()
        {
            return new BlockModel
            {
                Vertices =
                               new NativeArray<float3>(
                                   new[]
                                       {
                                           new float3(V0, V0, V0), new float3(V0, V1, V1), new float3(V0, V1, V0),
                                           new float3(V0, V0, V1), new float3(V1, V0, V0), new float3(V1, V1, V1),
                                           new float3(V1, V1, V0), new float3(V1, V0, V1), new float3(V0, V1, V0),
                                           new float3(V0, V1, V1), new float3(V1, V1, V0), new float3(V1, V1, V1),
                                           new float3(V0, V0, V0), new float3(V0, V0, V1), new float3(V1, V0, V0),
                                           new float3(V1, V0, V1), new float3(V0, V0, V0), new float3(V0, V1, V0),
                                           new float3(V1, V1, V0), new float3(V1, V0, V0), new float3(V0, V0, V1),
                                           new float3(V0, V1, V1), new float3(V1, V1, V1), new float3(V1, V0, V1),
                                       },
                                   Allocator.Persistent),
                Normals =
                               new NativeArray<float3>(
                                   new[]
                                       {
                                           new float3(-1, 0, 0), new float3(-1, 0, 0), new float3(-1, 0, 0), new float3(-1, 0, 0),
                                           new float3(1, 0, 0), new float3(1, 0, 0),new float3(1, 0, 0),new float3(1, 0, 0),
                                           new float3(0, 1, 0), new float3(0, 1, 0),new float3(0, 1, 0),new float3(0, 1, 0),
                                           new float3(0, -1, 0), new float3(0, -1, 0),new float3(0, -1, 0),new float3(0, -1, 0),
                                           new float3(0, 0, -1), new float3(0, 0, -1), new float3(0, 0, -1), new float3(0, 0, -1),
                                           new float3(0, 0, 1), new float3(0, 0, 1),new float3(0, 0, 1),new float3(0, 0, 1),
                                       },
                                   Allocator.Persistent),
                Uvs = new NativeArray<float3>(
                               new[]
                                   {
                                       new float3(1f, 0f, 0f), new float3(0f, 1f, 0f), new float3(1f, 1f, 0f), new float3(0f, 0f, 0f),
                                       new float3(0f, 0f, 0f), new float3(1f, 1f, 0f), new float3(0f, 1f, 0f), new float3(1f, 0f, 0f),
                                       new float3(0f, 0f, 0f), new float3(0f, 1f, 0f), new float3(1f, 0f, 0f), new float3(1f, 1f, 0f),
                                       new float3(0f, 0f, 0f), new float3(0f, 1f, 0f), new float3(1f, 0f, 0f), new float3(1f, 1f, 0f),
                                       new float3(0f, 0f, 0f), new float3(0f, 1f, 0f), new float3(1f, 1f, 0f), new float3(1f, 0f, 0f),
                                       new float3(1f, 0f, 0f), new float3(1f, 1f, 0f), new float3(0f, 1f, 0f), new float3(0f, 0f, 0f),
                                   },
                               Allocator.Persistent),
                Triangles = new NativeArray<int>(
                               new[]
                                   {
                                       0, 1, 2, 0, 3, 1, // left -
                                       4, 5, 7, 4, 6, 5, // right +
                                       8, 11, 10, 8, 9, 11, // top +
                                       12, 15, 13, 12, 14, 15, // bottom -
                                       16, 18, 19, 16, 17, 18, // front +
                                       20, 22, 21, 20, 23, 22, // back -
                                   },
                               Allocator.Persistent),
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.Vertices.IsCreated)
            {
                this.Vertices.Dispose();
            }

            if (this.Normals.IsCreated)
            {
                this.Normals.Dispose();
            }

            if (this.Uvs.IsCreated)
            {
                this.Uvs.Dispose();
            }

            if (this.Triangles.IsCreated)
            {
                this.Triangles.Dispose();
            }
        }
    }
}
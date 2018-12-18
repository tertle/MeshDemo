using BovineLabs.Common.Utility;
using BovineLabs.Systems.Mesh;
using BovineLabs.Systems.World.Models;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        const int entitiesToCreate = 10;

        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        var meshArchetype = entityManager.CreateArchetype(typeof(MeshInstanceRenderer), typeof(Vertex), typeof(Uv), typeof(Normal), typeof(Triangle), typeof(Position), typeof(MeshScale));

        var meshEntities = new NativeArray<Entity>(entitiesToCreate, Allocator.Temp);
        entityManager.CreateEntity(meshArchetype, meshEntities);

        var material = new Material(Shader.Find("Standard"));
        var model = BlockModel.Create();

        for (var index = 0; index < entitiesToCreate; index++)
        {
            var entity = meshEntities[index];

            var mesh = new Mesh();
            mesh.MarkDynamic();

            entityManager.SetSharedComponentData(entity, new MeshInstanceRenderer
            {
                mesh = mesh,
                material = material,
            });

            var vertices = entityManager.GetBuffer<Vertex>(entity).Reinterpret<float3>();
            vertices.AddRange(model.Vertices);

            var normals = entityManager.GetBuffer<Normal>(entity).Reinterpret<float3>();
            normals.AddRange(model.Normals);

            var uvs = entityManager.GetBuffer<Uv>(entity).Reinterpret<float3>();
            uvs.AddRange(model.Uvs);

            var triangles = entityManager.GetBuffer<Triangle>(entity).Reinterpret<int>();
            triangles.AddRange(model.Triangles);

            entityManager.SetComponentData(entity, new Position { Value = new float3(index * 2, 0, 0) });

            entityManager.SetComponentData(entity, new MeshScale
            {
                Step = Random.Range(0.005f, 0.01f),
                Max = Random.Range(60, 600),
                //Period = Random.Range(0.5f, 2f) * (Random.Range(0,2) == 1 ? 1 : -1),
            });
        }

        meshEntities.Dispose();
        model.Dispose();
    }
}

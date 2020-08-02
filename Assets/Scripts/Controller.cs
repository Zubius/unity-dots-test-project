using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;
        [SerializeField] private float amplitude = 1f;
        [SerializeField] private float waveMod = 1f;

        [SerializeField] private int CountX = 1;
        [SerializeField] private int CountZ = 1;
        [SerializeField] private float distance = 0.1f;
        [SerializeField] private GameObject cube;
        [SerializeField] private Camera camera;

        private List<MoveableObject> _moveableObjects;
        private EntityManager _entityManager;

        private void Start()
        {
            int count = CountX * CountZ;
            _moveableObjects = new List<MoveableObject>(count);

            var size = cube.GetComponent<Renderer>().bounds.size.x;
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(cube, settings);

            var distOffset = distance + size;

            //set camera size to fit all wave
            if (camera.orthographic)
            {
                var width = CountX * size + distance * (CountX - 1) + size;//last size for a bit gap at sides
                var height = width / camera.aspect;
                camera.orthographicSize = Mathf.Max(5, height / 2);
            }

            int zInd = 0;
            int xInd = 0;
            float zPos = 0;

            //center elements infront of camera
            float offset = -1 * CountX / 2f + 0.5f;

            for (int i = 0; i < count; i++)
            {
                zPos = zInd * distOffset;

                var pos = new Vector3((xInd + offset) * distOffset, 0, -zPos);
                var entityInstance = _entityManager.Instantiate(entity);
                _entityManager.SetComponentData(entityInstance, new Translation {Value = pos});

                _moveableObjects.Add(new MoveableObject
                {
                    Entity = entityInstance,
                    Position = pos
                });

                if (++xInd == CountX)
                {
                    zInd++;
                    xInd = 0;
                }
            }
        }

        private void Update()
        {
            var positions = new NativeArray<float3>(_moveableObjects.Count, Allocator.TempJob);
            var indexes = new NativeArray<int>(_moveableObjects.Count, Allocator.TempJob);

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = _moveableObjects[i].Position;
                indexes[i] = i % CountX;
            }

            var job = new SimpleMover
            {
                Speed = speed,
                Amplitude = amplitude,
                Positions = positions,
                Indexes = indexes,
                Time = Time.time,
                WaveMod = waveMod
            };

            var jobHandler = job.Schedule(_moveableObjects.Count, 10);
            jobHandler.Complete();

            for (int i = 0; i < _moveableObjects.Count; i++)
            {
                _moveableObjects[i].Position = positions[i];

                _entityManager.SetComponentData(_moveableObjects[i].Entity, new Translation {Value = _moveableObjects[i].Position});
            }

            positions.Dispose();
            indexes.Dispose();
        }
    }

    public class MoveableObject
    {
        public Entity Entity;
        public Vector3 Position;
    }
}

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct SimpleMover : IJobParallelFor
{
    public NativeArray<float3> Positions;
    public NativeArray<int> Indexes;

    [ReadOnly] public float Speed;
    [ReadOnly] public float Amplitude;
    [ReadOnly] public float Time;
    [ReadOnly] public float WaveMod;

    public void Execute(int index)
    {
        Positions[index] = new float3(Positions[index].x, Mathf.Sin((Time + Indexes[index] * WaveMod) * Speed) * Amplitude, Positions[index].z);
    }
}

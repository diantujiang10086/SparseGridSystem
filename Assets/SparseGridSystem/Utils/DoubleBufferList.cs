using System;
using Unity.Collections;

public class DoubleBufferList<T> : IDisposable where T : unmanaged
{
    private NativeList<T> aList;
    private NativeList<T> bList;
    private NativeList<T> curList;
    private NativeList<T> useList;
    
    public int Capacity
    {
        set
        {
            aList.Capacity = value;
            bList.Capacity = value;
        }
    }

    public DoubleBufferList(int capacity, Allocator allocator)
    {
        aList = new NativeList<T>(capacity, allocator);
        bList = new NativeList<T>(capacity, allocator);
        curList = aList;
        useList = bList;
    }

    public void Add(T value)
    {
        curList.Add(value);
    }

    public NativeArray<T>.ReadOnly Update()
    {
        var temp = curList;
        curList = useList;
        useList = temp;

        curList.Clear();
        return useList.AsReadOnly();
    }

    public void Dispose()
    {
        aList.Dispose();
        bList.Dispose();
    }
}
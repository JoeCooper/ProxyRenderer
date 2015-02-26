using System.Collections.Generic;
using System;

public class ProxyRendererObjectPool<T> {

	private Stack<T> pool = new Stack<T>();

	private readonly Func<T> instantiationAgent;
	private readonly Action<T> disposalAgent;

	private readonly int capacity;
	
	public ProxyRendererObjectPool(Func<T> instantiationAgent)
	{
		this.instantiationAgent = instantiationAgent;
		this.disposalAgent = null;
		this.capacity = int.MaxValue;
	}

	public ProxyRendererObjectPool(Func<T> instantiationAgent, Action<T> disposalAgent, int capacity = int.MaxValue)
	{
		this.instantiationAgent = instantiationAgent;
		this.disposalAgent = disposalAgent;
		this.capacity = capacity;
	}

	public void Clear()
	{
		if(disposalAgent != null) {
			while(pool.Count > 0)
			{
				disposalAgent(pool.Pop());
			}
		}
		else {
			pool.Clear();
		}
	}

	public T GetObject()
	{
		if(pool.Count > 0)
		{
			return pool.Pop();
		}
		else
		{
			return instantiationAgent();
		}
	}

	public void PutObject(T obj)
	{
		if(pool.Count > capacity)
		{
			disposalAgent(obj);
		}
		else
		{
			pool.Push(obj);
		}
	}
}

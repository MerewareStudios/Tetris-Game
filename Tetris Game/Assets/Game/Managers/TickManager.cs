using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TickManager : Singleton<TickManager>
{
	[System.NonSerialized] private readonly Dictionary<float, Ticker> _tickers = new();
	
	public interface ITickable
	{
		float TickInterval { get; }
		public void OnTick();
	}

	public void AddTickable(ITickable tickable)
	{
		if (!_tickers.ContainsKey(tickable.TickInterval))
		{
			Ticker ticker = new(tickable.TickInterval);
			ticker.Add(tickable);
			
			_tickers.Add(tickable.TickInterval, ticker);
		}

		_tickers[tickable.TickInterval].Add(tickable);
	}
	public void RemoveTickable(ITickable tickable)
	{
		if (_tickers.TryGetValue(tickable.TickInterval, out var ticker))
		{
			ticker.Remove(tickable);
		}
	}

	private class Ticker
	{
		private readonly List<ITickable> _tickables = new();
		private float interval;
		private Coroutine _coroutine = null;

		public void Add(ITickable tickable)
		{
			if (_tickables.Contains(tickable))
			{
				return;
			}
			
			_tickables.Add(tickable);

			if (_coroutine == null)
			{
				TickManager.THIS.StartCoroutine(TickRoutine());
			}
		}
		
		public void Remove(ITickable tickable)
		{
			_tickables.Remove(tickable);
		}
		
		public Ticker(float interval)
		{
			this.interval = interval;
		}
			
		IEnumerator TickRoutine()
		{
			while (_tickables.Count > 0)
			{
				foreach (var tickable in _tickables.ToList())
				{
					tickable.OnTick();
				}

				yield return new WaitForSeconds(interval);
			}

			_coroutine = null;
		}
	}
	
 //    public static TickManager operator +(TickManager tickManager, ITickable tickable)
 //    {
 //        tickManager.AddTickable(tickable);
 //        return tickManager;
 //    }
 //    public static TickManager operator -(TickManager tickManager, ITickable tickable)
 //    {
 //        tickManager.RemoveTickable(tickable);
 //        return tickManager;
 //    }
 //    public static TickManager operator +(TickManager tickManager, CustomTickable customTickable)
 //    {
 //        tickManager.AddCustomTickable(customTickable);
 //        return tickManager;
 //    }
 //    public static TickManager operator -(TickManager tickManager, CustomTickable customTickable)
 //    {
 //        tickManager.RemoveCustomTickable(customTickable);
 //        return tickManager;
 //    }
 //
}

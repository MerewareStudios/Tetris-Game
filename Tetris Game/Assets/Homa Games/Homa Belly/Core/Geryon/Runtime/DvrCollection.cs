using System.Collections;
using System.Collections.Generic;

namespace HomaGames.Geryon
{
    public class DvrCollection<T> : IEnumerable<KeyValuePair<string, Observable<T>>>
    {
        private readonly Dictionary<string, Observable<T>> _dvrs = new Dictionary<string, Observable<T>>();

        public int Count => _dvrs.Count;

        public bool TryGet(string key, out Observable<T> observable)
        {
            return _dvrs.TryGetValue(key, out observable);
        }

        // Used from DVR class
        public Observable<T> GetOrCreate(string key, T defaultValue)
        {
            if (TryGet(key, out var observable))
                return observable;
            AddOrSet(key, defaultValue);
            return _dvrs[key];
        }

        internal void AddOrSet(string key, T value)
        {
            if (_dvrs.TryGetValue(key, out var observable))
                observable.Value = value;

            _dvrs[key] = new Observable<T>(value);
        }

        internal void Clear()
        {
            _dvrs.Clear();
        }

        public IEnumerator<KeyValuePair<string, Observable<T>>> GetEnumerator()
        {
            return _dvrs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dvrs.GetEnumerator();
        }
    }
}
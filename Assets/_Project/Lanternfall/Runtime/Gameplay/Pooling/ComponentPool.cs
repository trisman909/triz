using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lanternfall.Gameplay.Pooling
{
    public interface IPoolable
    {
        void OnTakenFromPool();
        void OnReturnedToPool();
    }

    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _root;
        private readonly Stack<T> _available;
        private readonly Action<T> _created;

        public ComponentPool(T prefab, int initialCapacity, Transform root, Action<T> created = null)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            _root = root;
            _created = created;
            _available = new Stack<T>(Mathf.Max(0, initialCapacity));
            for (int index = 0; index < initialCapacity; index++)
                Return(Create());
        }

        public T Take()
        {
            T item = _available.Count > 0 ? _available.Pop() : Create();
            item.gameObject.SetActive(true);
            if (item is IPoolable poolable) poolable.OnTakenFromPool();
            return item;
        }

        public void Return(T item)
        {
            if (item == null) return;
            if (item is IPoolable poolable) poolable.OnReturnedToPool();
            item.gameObject.SetActive(false);
            item.transform.SetParent(_root, false);
            _available.Push(item);
        }

        private T Create()
        {
            T instance = UnityEngine.Object.Instantiate(_prefab, _root);
            _created?.Invoke(instance);
            return instance;
        }
    }
}


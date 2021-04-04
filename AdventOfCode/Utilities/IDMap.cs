﻿using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class IDMap<T> : IEnumerable<T>
    {
        private readonly Dictionary<int, T> elements;
        private readonly Dictionary<T, int> ids;

        public int Count => elements.Count;

        public IDMap()
            : this(16) { }
        public IDMap(int capacity)
        {
            elements = new(capacity);
            ids = new(capacity);
        }

        /// <summary>Adds the given value to the ID map, assigning it the next available ID. If the value already exists in the map, nothing happens.</summary>
        /// <param name="value">The value to add to the ID map.</param>
        /// <returns>The newly assigned ID for the added value, or its already assigned ID if the value already existed.</returns>
        public int Add(T value)
        {
            bool contained = TryGetID(value, out int id);
            if (!contained)
            {
                id = Count;
                elements.Add(id, value);
                ids.Add(value, id);
            }
            return id;
        }
        public void Clear()
        {
            elements.Clear();
            ids.Clear();
        }

        public bool Contains(T value) => ids.ContainsKey(value);

        public int GetID(T element) => ids[element];
        public bool TryGetID(T value, out int id)
        {
            bool contained = Contains(value);
            id = contained ? ids[value] : -1;
            return contained;
        }

        public IEnumerator<T> GetEnumerator() => ids.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T this[int id] => elements[id];
    }
}
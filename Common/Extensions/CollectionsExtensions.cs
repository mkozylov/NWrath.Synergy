﻿using NWrath.Synergy.Common.Structs;
using NWrath.Synergy.Reflection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NWrath.Synergy.Common.Extensions.Collections
{
    public static class CollectionsExtensions
    {
        public static Func<StringSet, string> DefaultStringSetJsonSerializer { get; set; } = GetDefaultStringSetJsonSerializer;

        #region Set

        public static StringSet ToStringSet<TSource>(this TSource source)
        {
            switch (source)
            {
                case StringSet stringSet:
                    return stringSet;
                case IDictionary<string, string> stringDictionary:
                    return new StringSet(stringDictionary);
                case IDictionary<string, object> objDictionary:
                    var dictionary = objDictionary.ToDictionary(k => k.Key, v => v.Value?.ToString());
                    return new StringSet();
                default:
                    dictionary = PropertyCache<TSource>.ToStringDictionary(source);
                    return new StringSet(dictionary);
            }
        }

        public static string AsJson(this StringSet set, Func<StringSet, string> serializerFunc = null)
        {
            if (serializerFunc != null)
            {
                return serializerFunc(set);
            }

            return DefaultStringSetJsonSerializer(set);
        }

        #endregion

        #region IDictionary

        public static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> set, TKey key)
        {
            return set.ContainsKey(key)
                ? set[key]
                : default(TValue);
        }

        public static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> set, TKey key, TValue defaultValue)
        {
            return set.ContainsKey(key)
                ? set[key]
                : defaultValue;
        }

        public static TValue AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> set,
            TKey key,
            TValue value,
            Func<TKey, TValue, TValue> updateValueFactory
            )
        {
            if (!set.ContainsKey(key))
            {
                set.Add(key, value);

                return value;
            }

            var updated = updateValueFactory(key, set[key]);

            set[key] = updated;

            return updated;
        }

        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> set,
            TKey key,
            Func<TKey, TValue> valueFactory
            )
        {
            if (!set.ContainsKey(key))
            {
                var value = valueFactory(key);

                set.Add(key, value);

                return value;
            }

            return set[key];
        }

        public static TValue GetOrAdd<TValue>(this IDictionary<string, object> set, string key, Func<string, TValue> valueFactory)
        {
            if (!set.ContainsKey(key))
            {
                var value = valueFactory(key);

                set.Add(key, value);

                return value;
            }

            return (TValue)set[key];
        }

        public static TValue TryGet<TValue>(this IDictionary<string, object> set, string key)
        {
            return (TValue)set.TryGet(key);
        }

        public static TValue TryGet<TValue>(this IDictionary<string, object> set)
        {
            return (TValue)set.TryGet(typeof(TValue).Name);
        }

        public static TValue TryGet<TValue>(this IDictionary<string, object> set, string key, TValue defaultValue)
        {
            return set.ContainsKey(key)
                ? (TValue)set[key]
                : defaultValue;
        }

        public static TValue TryGet<TValue>(this IDictionary<string, object> set, string key, Func<string, TValue> defaultValueFactory)
        {
            return set.ContainsKey(key)
                ? (TValue)set[key]
                : defaultValueFactory(key);
        }

        public static void Add<TValue>(this IDictionary<string, object> set, string key, TValue val)
        {
            set[key] = val;
        }

        public static void Add<TValue>(this IDictionary<string, object> set, TValue val)
        {
            set[typeof(TValue).Name] = val;
        }

        #endregion IDictionary

        #region Empty

        public static bool IsEmpty<TSource>(this TSource[] collection)
        {
            return collection == null || collection.Length == 0;
        }

        public static bool IsEmpty<TSource>(this List<TSource> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static bool NotEmpty<TSource>(this TSource[] collection)
        {
            return !IsEmpty(collection);
        }

        public static bool NotEmpty<TSource>(this List<TSource> collection)
        {
            return !IsEmpty(collection);
        }

        #endregion Empty

        #region Each

        public static IEnumerable<TSource> Each<TSource>(this IEnumerable<TSource> collection, Action<TSource, int> action)
        {
            return Each<IEnumerable<TSource>, TSource>(collection, action);
        }

        public static IEnumerable<TSource> Each<TSource>(this IEnumerable<TSource> collection, Action<TSource> action)
        {
            return Each<IEnumerable<TSource>, TSource>(collection, action);
        }

        public static TSource[] Each<TSource>(this TSource[] collection, Action<TSource, int> action)
        {
            return Each<TSource[], TSource>(collection, action);
        }

        public static TSource[] Each<TSource>(this TSource[] collection, Action<TSource> action)
        {
            return Each<TSource[], TSource>(collection, action);
        }

        public static List<TSource> Each<TSource>(this List<TSource> collection, Action<TSource, int> action)
        {
            return Each<List<TSource>, TSource>(collection.ToList(), action);
        }

        public static List<TSource> Each<TSource>(this List<TSource> collection, Action<TSource> action)
        {
            return Each<List<TSource>, TSource>(collection.ToList(), action);
        }

        private static TCollection Each<TCollection, TItem>(this TCollection collection, Action<TItem, int> action)
            where TCollection : IEnumerable<TItem>
        {
            var i = 0;

            foreach (var item in collection)
            {
                action(item, i++);
            }

            return collection;
        }

        private static TCollection Each<TCollection, TItem>(TCollection collection, Action<TItem> action)
            where TCollection : IEnumerable<TItem>
        {
            foreach (var item in collection)
            {
                action(item);
            }

            return collection;
        }

        #endregion Each

        #region Internal
        private static string GetDefaultStringSetJsonSerializer(IDictionary<string, string> set)
        {
            return $"{{ {set.Select(x => $"\"{x.Key}\":\"{x.Value}\"").StringJoin(", ") } }}";
        }

        private static class PropertyCache<TSource>
        {
            private static PropertyInfo[] _members;

            static PropertyCache()
            {
                _members = typeof(TSource).GetProperties();
            }

            public static Dictionary<string, string> ToStringDictionary(TSource obj)
            {
                return _members.ToDictionary(
                    k => k.Name,
                    v => v.GetValue(obj) + ""
                    );
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using TypePredicate;
using UnityEngine;
using Utils.Serializables;
using Object = UnityEngine.Object;

namespace SerializablePredicate
{
    [Serializable]
    public class FiltersPredicate : ISerializablePredicate, ISerializationCallbackReceiver
    {
        [SerializeField] private Object _target;
        [SerializeField] private List<FieldFilter> _filters;

        public bool IsMet()
        {
            foreach (var filter in _filters)
            {
                if (!filter.IsMet(_target))
                {
                    return false;
                }
            }

            return true;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            Editor_UpdateFiltersType();
#endif
        }

#if UNITY_EDITOR
        public void Editor_UpdateFiltersType()
        {
            if (_target != null)
            {
                foreach (var filter in _filters)
                {
                    filter.UpdateParentType(_target.GetType());
                }
            }
        }
#endif
    }
    
    [Serializable]
    public class FiltersPredicate<T> : ISerializablePredicate<T>
    {
        public bool IsMet(T input)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public abstract class FieldFilterBase
    {
        public string fieldPath;
        public ComparisonEnum predicateEnum;
        public Arg valueToCompare;
        public bool negated;

        protected bool IsNested => fieldPath.Contains(".");

        public abstract Type GetParentType();
        
        public bool IsMet(Object target)
        {
            var success = TryGetFieldInfoFromPath(target.GetType(), out var field);
            var parent = IsNested ? GetFieldParent(target) : target;
            var value = field.GetValue(parent);
            Debug.Log($"field: {field}; value: {value}");
            return true;
        }

        public bool TryGetFieldType(out Type type)
        {
            var success = TryGetFieldInfoFromPath(GetParentType(), out var fieldInfo);
            if (success)
            {
                type = fieldInfo.FieldType;
                return true;
            }

            type = null;
            return false;
        }
        
        protected bool TryGetFieldInfoFromPath(Type targetType, out FieldInfo field)
        {
            if (targetType == null)
            {
                field = null;
                return false;
            }
            
            var elements = fieldPath.Split('.');

            var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var type = targetType;
            field = null;
            foreach (var pathElement in elements)
            {
                field = type.GetField(pathElement, bindings);
                if (field == null)
                {
                    return false;
                }
                type = field.FieldType;
            }

            return true;
        }

        protected object GetFieldParent(Object target)
        {
            var elements = fieldPath.Split('.');
            var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            object parent = target;
            var type = target.GetType();
            for (int i = 0; i < elements.Length - 1; i++)
            {
                var pathElement = elements[i];
                var field = type.GetField(pathElement, bindings);
                parent = field.GetValue(parent);
                type = field.FieldType;
            }

            return parent;
        }
    }
    
    [Serializable]
    public class FieldFilter : FieldFilterBase
    {
        private Type _parentCachedType;
        public void UpdateParentType(Type type)
        {
            _parentCachedType = type;
        }

        public override Type GetParentType()
        {
            return _parentCachedType;
        }
    }
}
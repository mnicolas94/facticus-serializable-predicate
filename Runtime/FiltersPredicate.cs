using System;
using System.Collections.Generic;
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

        public FiltersPredicate()
        {
            _filters = new List<FieldFilter>();
        }

        public FiltersPredicate(Object target)
        {
            _target = target;
                _filters = new List<FieldFilter>();
        }

        public void AddFilter(FieldFilter filter)
        {
            _filters.Add(filter);
        }

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
}
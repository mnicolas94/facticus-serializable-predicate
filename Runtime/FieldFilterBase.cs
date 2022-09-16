using System;
using System.Reflection;
using Object = UnityEngine.Object;

namespace SerializablePredicate
{
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
            if (success)
            {
                var parent = IsNested ? GetFieldParent(target) : target;
                var result = predicateEnum.Compare(field, parent, valueToCompare);
                return result ^ negated;
            }
            else
            {
                throw new ArgumentException("Something went wrong");
            }
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
    
    [Serializable]
    public class FieldFilter<T> : FieldFilterBase
    {
        public override Type GetParentType()
        {
            return typeof(T);
        }
    }
}
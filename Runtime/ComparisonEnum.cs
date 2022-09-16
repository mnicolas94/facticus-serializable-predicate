using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SerializablePredicate
{
    public enum ComparisonEnum
    {
        Equal,
        Less,
        Greater,
        LessEqual,
        GreaterEqual,
        Contains,
        Callback,
    }

    public static class ComparisonEnumUtils
    {
        private static readonly Dictionary<ComparisonEnum, Func<int, int, bool>> IntFunctions
            = new Dictionary<ComparisonEnum, Func<int, int, bool>>
        {
            {ComparisonEnum.Equal, (a, b) => a == b},
            {ComparisonEnum.Less, (a, b) => a < b},
            {ComparisonEnum.Greater, (a, b) => a > b},
            {ComparisonEnum.LessEqual, (a, b) => a <= b},
            {ComparisonEnum.GreaterEqual, (a, b) => a >= b},
        };
        
        private static readonly Dictionary<ComparisonEnum, Func<float, float, bool>> FloatFunctions
            = new Dictionary<ComparisonEnum, Func<float, float, bool>>
        {
            {ComparisonEnum.Equal, (a, b) => Mathf.Abs(a - b) < Mathf.Epsilon},  // approximation instead of ==
            {ComparisonEnum.Less, (a, b) => a < b},
            {ComparisonEnum.Greater, (a, b) => a > b},
            {ComparisonEnum.LessEqual, (a, b) => a <= b},
            {ComparisonEnum.GreaterEqual, (a, b) => a >= b},
        };
        
        private static readonly Dictionary<ComparisonEnum, Func<string, string, bool>> StringFunctions
            = new Dictionary<ComparisonEnum, Func<string, string, bool>>
        {
            {ComparisonEnum.Equal, (a, b) => a == b},
            {ComparisonEnum.Contains, (a, b) => a.Contains(b)},
        };
        
        
        public static bool Compare(
            this ComparisonEnum comparison,
            FieldInfo fieldInfo,
            object parent,
            Arg valueToCompare)
        {
            var fieldType = fieldInfo.FieldType;
            var fieldValue = fieldInfo.GetValue(parent);
            
            if (typeof(bool).IsAssignableFrom(fieldType))
            {
                return (bool) fieldValue == valueToCompare.boolValue;;
            }
            
            if (typeof(int).IsAssignableFrom(fieldType))
            {
                return IntFunctions[comparison]((int) fieldValue, valueToCompare.intValue);
            }
            
            if (typeof(float).IsAssignableFrom(fieldType))
            {
                return FloatFunctions[comparison]((float) fieldValue, valueToCompare.floatValue);
            }
            
            if (typeof(string).IsAssignableFrom(fieldType))
            {
                return StringFunctions[comparison]((string) fieldValue, valueToCompare.stringValue);
            }
            
            if (typeof(IList).IsAssignableFrom(fieldType))
            {
                return ((IList) fieldValue).Contains(valueToCompare.GetValue());
            }

            return false;
        }
        
        public static List<ComparisonEnum> AvailableComparisonsForType(Type type)
        {
            if (typeof(bool).IsAssignableFrom(type))
                return new List<ComparisonEnum>
                {
                    ComparisonEnum.Equal,
                };
            
            if (typeof(int).IsAssignableFrom(type))
                return new List<ComparisonEnum>
                {
                    ComparisonEnum.Equal,
                    ComparisonEnum.Less,
                    ComparisonEnum.Greater,
                    ComparisonEnum.LessEqual,
                    ComparisonEnum.GreaterEqual
                };
            
            if (typeof(float).IsAssignableFrom(type))
                return new List<ComparisonEnum>
                {
                    ComparisonEnum.Equal,
                    ComparisonEnum.Less,
                    ComparisonEnum.Greater,
                    ComparisonEnum.LessEqual,
                    ComparisonEnum.GreaterEqual
                };
            
            if (typeof(double).IsAssignableFrom(type))
                return new List<ComparisonEnum>
                {
                    ComparisonEnum.Equal,
                    ComparisonEnum.Less,
                    ComparisonEnum.Greater,
                    ComparisonEnum.LessEqual,
                    ComparisonEnum.GreaterEqual
                };
            
            if (typeof(string).IsAssignableFrom(type))
                return new List<ComparisonEnum>
                {
                    ComparisonEnum.Equal,
                    ComparisonEnum.Contains
                };
            
            if (typeof(IList).IsAssignableFrom(type))
                return new List<ComparisonEnum>
                {
                    ComparisonEnum.Contains
                };
            
            var all = new List<ComparisonEnum>
            {
                ComparisonEnum.Equal,
                ComparisonEnum.Contains,
                ComparisonEnum.Callback
            };
            return all; 
        }
    }
}
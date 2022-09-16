using System;
using System.Collections;
using System.Collections.Generic;

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
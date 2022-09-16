using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SerializablePredicate.Tests.Editor
{
    public class FiltersPredicateTests
    {
        [Serializable]
        public class TestClass : Object
        {
            [SerializeField] private bool _bool;
            [SerializeField] private int _int;
            [SerializeField] private float _float;
            [SerializeField] private string _string;
            [SerializeField] private List<string> _list;
            [SerializeField] private Nested _nested;

            public bool Bool
            {
                get => _bool;
                set => _bool = value;
            }

            public int Int
            {
                get => _int;
                set => _int = value;
            }

            public float Float
            {
                get => _float;
                set => _float = value;
            }

            public string String
            {
                get => _string;
                set => _string = value;
            }

            public List<string> List
            {
                get => _list;
                set => _list = value;
            }

            public Nested Nested
            {
                get => _nested;
                set => _nested = value;
            }
        }
        
        [Serializable]
        public class Nested
        {
            [SerializeField] private int _nestedInt;

            public Nested(int nestedInt)
            {
                _nestedInt = nestedInt;
            }
        }
        
        [TestCase(false, false, ComparisonEnum.Equal, true)]
        [TestCase(true, true, ComparisonEnum.Equal, true)]
        [TestCase(true, false, ComparisonEnum.Equal, false)]
        [TestCase(false, true, ComparisonEnum.Equal, false)]
        public void BoolFilterTest(bool targetValue, bool filterValue, ComparisonEnum comparison, bool expected)
        {
            // arrange
            var target = new TestClass();
            target.Bool = targetValue;
            
            var filter = new FieldFilter
            {
                fieldPath = "_bool",
                predicateEnum = comparison,
                valueToCompare = new Arg {argType = Arg.ArgType.Bool, boolValue = filterValue}
            };
            var predicate = new FiltersPredicate(target);
            predicate.AddFilter(filter);
            
            // act
            var result = predicate.IsMet();

            // assert
            Assert.AreEqual(expected, result);
        }
        
        [TestCase(5, 5, ComparisonEnum.Equal, true)]
        [TestCase(67, 5, ComparisonEnum.Equal, false)]
        [TestCase(4, 5, ComparisonEnum.Less, true)]
        [TestCase(4, -1, ComparisonEnum.Less, false)]
        [TestCase(45, 12, ComparisonEnum.Greater, true)]
        [TestCase(5, 5, ComparisonEnum.Greater, false)]
        [TestCase(45, 45, ComparisonEnum.LessEqual, true)]
        [TestCase(45, 155, ComparisonEnum.LessEqual, true)]
        [TestCase(5, 3, ComparisonEnum.LessEqual, false)]
        [TestCase(-15, -15, ComparisonEnum.GreaterEqual, true)]
        [TestCase(-3, -4, ComparisonEnum.GreaterEqual, true)]
        [TestCase(-1, 3, ComparisonEnum.GreaterEqual, false)]
        public void IntFilterTest(int targetValue, int filterValue, ComparisonEnum comparison, bool expected)
        {
            // arrange
            var target = new TestClass();
            target.Int = targetValue;
            
            var filter = new FieldFilter
            {
                fieldPath = "_int",
                predicateEnum = comparison,
                valueToCompare = new Arg {argType = Arg.ArgType.Int, intValue = filterValue}
            };
            var predicate = new FiltersPredicate(target);
            predicate.AddFilter(filter);
            
            // act
            var result = predicate.IsMet();

            // assert
            Assert.AreEqual(expected, result);
        }
        
        [TestCase(5.4f, 5.4f, ComparisonEnum.Equal, true)]
        [TestCase(67, 5, ComparisonEnum.Equal, false)]
        [TestCase(4, 5, ComparisonEnum.Less, true)]
        [TestCase(4, -1, ComparisonEnum.Less, false)]
        [TestCase(45, 12.1452f, ComparisonEnum.Greater, true)]
        [TestCase(5, 5, ComparisonEnum.Greater, false)]
        [TestCase(45.0000000000001f, 45, ComparisonEnum.LessEqual, true)]
        [TestCase(45, 155, ComparisonEnum.LessEqual, true)]
        [TestCase(5, 3, ComparisonEnum.LessEqual, false)]
        [TestCase(-15, -15, ComparisonEnum.GreaterEqual, true)]
        [TestCase(-3.9f, -4, ComparisonEnum.GreaterEqual, true)]
        [TestCase(-1, 3, ComparisonEnum.GreaterEqual, false)]
        public void FloatFilterTest(float targetValue, float filterValue, ComparisonEnum comparison, bool expected)
        {
            // arrange
            var target = new TestClass();
            target.Float = targetValue;
            
            var filter = new FieldFilter
            {
                fieldPath = "_float",
                predicateEnum = comparison,
                valueToCompare = new Arg {argType = Arg.ArgType.Float, floatValue = filterValue}
            };
            var predicate = new FiltersPredicate(target);
            predicate.AddFilter(filter);
            
            // act
            var result = predicate.IsMet();

            // assert
            Assert.AreEqual(expected, result);
        }
        
        [TestCase("qwerty", "qwerty", ComparisonEnum.Equal, true)]
        [TestCase("asdfg", "asdfgqwe", ComparisonEnum.Equal, false)]
        [TestCase("123456", "34", ComparisonEnum.Contains, true)]
        [TestCase("123456", "abc", ComparisonEnum.Contains, false)]
        public void StringFilterTest(string targetValue, string filterValue, ComparisonEnum comparison, bool expected)
        {
            // arrange
            var target = new TestClass();
            target.String = targetValue;
            
            var filter = new FieldFilter
            {
                fieldPath = "_string",
                predicateEnum = comparison,
                valueToCompare = new Arg {argType = Arg.ArgType.String, stringValue = filterValue}
            };
            var predicate = new FiltersPredicate(target);
            predicate.AddFilter(filter);
            
            // act
            var result = predicate.IsMet();

            // assert
            Assert.AreEqual(expected, result);
        }
        
        [TestCase("3", ComparisonEnum.Equal, true, "1", "2", "3")]
        [TestCase("d", ComparisonEnum.Equal, false, "a", "b", "c")]
        public void ListFilterTest(string filterValue, ComparisonEnum comparison, bool expected, params string[] targetValues)
        {
            // arrange
            var target = new TestClass();
            target.List = targetValues.ToList();
            
            var filter = new FieldFilter
            {
                fieldPath = "_list",
                predicateEnum = comparison,
                valueToCompare = new Arg {argType = Arg.ArgType.String, stringValue = filterValue}
            };
            var predicate = new FiltersPredicate(target);
            predicate.AddFilter(filter);
            
            // act
            var result = predicate.IsMet();

            // assert
            Assert.AreEqual(expected, result);
        }
        
        [TestCase(5, 5, ComparisonEnum.Equal, true)]
        [TestCase(67, 5, ComparisonEnum.Equal, false)]
        public void NestedIntFilterTest(int targetValue, int filterValue, ComparisonEnum comparison, bool expected)
        {
            // arrange
            var target = new TestClass();
            target.Nested = new Nested(targetValue);
            
            var filter = new FieldFilter
            {
                fieldPath = "_nested._nestedInt",
                predicateEnum = comparison,
                valueToCompare = new Arg {argType = Arg.ArgType.Int, intValue = filterValue}
            };
            var predicate = new FiltersPredicate(target);
            predicate.AddFilter(filter);
            
            // act
            var result = predicate.IsMet();

            // assert
            Assert.AreEqual(expected, result);
        }
        
        [Test]
        public void SeveralFiltersTest()
        {
            // arrange
            var target = new TestClass();
            target.Int = 6;
            target.String = "abcde";

            var filters = new List<FieldFilter>
            {
                new FieldFilter
                {
                    fieldPath = "_int",
                    predicateEnum = ComparisonEnum.Greater,
                    valueToCompare = new Arg {argType = Arg.ArgType.Int, intValue = 5}
                },
                new FieldFilter
                {
                    fieldPath = "_string",
                    predicateEnum = ComparisonEnum.Contains,
                    valueToCompare = new Arg {argType = Arg.ArgType.String, stringValue = "cde"}
                }
            };
            var predicate = new FiltersPredicate(target);
            filters.ForEach(predicate.AddFilter);
            
            // act
            var result = predicate.IsMet();

            // assert
            Assert.IsTrue(result);
        }
    }
}

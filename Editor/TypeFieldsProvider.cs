using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SerializablePredicate.Editor
{
    public class TypeFieldsProvider : ScriptableObject, ISearchWindowProvider
    {
        private Type _type;
        private Action<FieldNode> _callback;
        
        public void Initialize(Type type, Action<FieldNode> callback)
        {
            _type = type;
            _callback = callback;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            var title = new SearchTreeGroupEntry(new GUIContent("Fields"), 0);
            tree.Add(title);
            
            var fields = GetSerializableFieldsPath(_type);
            fields.Sort((nodeA, nodeB) => String.Compare(nodeA.Path, nodeB.Path, StringComparison.Ordinal));
            var dfsTree = fields.DepthFirstSearch();
            foreach (var (fieldNode, depth) in dfsTree)
            {
                var niceName = $"{fieldNode.Name}: {GetNiceTypeName(fieldNode.Type)}";
                var label = new GUIContent(niceName);
                var level = depth + 1;
                if (fieldNode.IsGroup)
                {
                    var group = new SearchTreeGroupEntry(label, level);
                    tree.Add(group);
                }
                else
                {
                    var entry = new SearchTreeEntry(label);
                    entry.level = level;
                    entry.userData = fieldNode;
                    tree.Add(entry);
                }
            }
            
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var fieldNode = (FieldNode) searchTreeEntry.userData;
            _callback?.Invoke(fieldNode);
            
            return true;
        }
        
        private List<FieldNode> GetSerializableFieldsPath(Type type)
        {
            var nodes = new List<FieldNode>();
            
            var bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = type.GetFields(bindings);
            
            
            foreach (var field in fields)
            {
                var fieldPath = field.Name;
                var fieldType = field.FieldType;
                var node = new FieldNode
                {
                    Name = fieldPath,
                    Path = fieldPath,
                    Type = fieldType,
                    IsGroup = false,
                };
                nodes.Add(node);

                if (!IsTypePrimitive(fieldType))
                {
                    var children = GetSerializableFieldsPath(fieldType);
                    foreach (var child in children)
                    {
                        child.Path = $"{fieldPath}.{child.Path}";
                    }

                    node.IsGroup = true;
                    node.Children = children;
                }
            }

            return nodes;
        }

        private bool IsTypePrimitive(Type type)
        {
            if (typeof(bool).IsAssignableFrom(type)) return true;
            if (typeof(int).IsAssignableFrom(type)) return true;
            if (typeof(float).IsAssignableFrom(type)) return true;
            if (typeof(string).IsAssignableFrom(type)) return true;
            if (typeof(IList).IsAssignableFrom(type)) return true;

            return false;
        }

        private string GetNiceTypeName(Type type)
        {
            if (type == typeof(bool)) return "bool";
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(string)) return "string";
            if (typeof(IList).IsAssignableFrom(type))
            {
                var genericType = type.GenericTypeArguments[0];
                var genericTypeNiceName = GetNiceTypeName(genericType);
                return $"List<{genericTypeNiceName}>";
            }
            return type.Name;
        }
    }

    public class FieldNode
    {
        public string Name;
        public string Path;
        public Type Type;
        public bool IsGroup;
        public List<FieldNode> Children;
    }

    public static class FieldNodeListUtils
    {
        public static IEnumerable<(FieldNode, int)> DepthFirstSearch(this List<FieldNode> fieldTree)
        {
            var queueWithDepth = fieldTree.ConvertAll(node => (node, 0));
            while (queueWithDepth.Count > 0)
            {
                var (firstNode, depth) = queueWithDepth[0];
                queueWithDepth.RemoveAt(0);
                yield return (firstNode, depth);

                if (firstNode.IsGroup)
                {
                    var childrenWithDepth = firstNode.Children.ConvertAll(node => (node, depth + 1));
                    queueWithDepth.InsertRange(0, childrenWithDepth);
                }
            }
        }
    }
}
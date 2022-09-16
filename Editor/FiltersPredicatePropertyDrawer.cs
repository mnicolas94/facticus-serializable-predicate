using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace SerializablePredicate.Editor
{
    [CustomPropertyDrawer(typeof(FiltersPredicate), true)]
    public class FiltersPredicatePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var targetHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_target"));
            var filtersHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_filters"));
            return targetHeight + filtersHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.FindPropertyRelative("_target");
            var filters = property.FindPropertyRelative("_filters");
            var targetHeight = EditorGUI.GetPropertyHeight(target);
            var filtersHeight = EditorGUI.GetPropertyHeight(filters);
            
            var targetRect = position;
            targetRect.height = targetHeight;
            var filtersRect = targetRect;
            filtersRect.y += targetHeight;
            filtersRect.height = filtersHeight;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(targetRect, target);
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                var predicate = (FiltersPredicate) PropertiesUtils.GetTargetObjectOfProperty(property);
                predicate.Editor_UpdateFiltersType();
            }
            
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(filtersRect, filters);
            EditorGUI.indentLevel--;
        }
    }
}
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
            float height;
            if (property.isExpanded)
            {
                var targetHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_target"));
                var filtersHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_filters"));
                height = targetHeight + filtersHeight;
            }
            else
            {
                height = base.GetPropertyHeight(property, label);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelText = label.text;
            var fixedLabel = new GUIContent(labelText);
            var foldoutRect = new Rect(position)
            {
                height = base.GetPropertyHeight(property, label)
            };
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

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
            EditorGUI.PropertyField(targetRect, target, new GUIContent(labelText));
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                var predicate = (FiltersPredicate) PropertiesUtils.GetTargetObjectOfProperty(property);
                predicate.Editor_UpdateFiltersType();
            }
            
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(filtersRect, filters);
                EditorGUI.indentLevel--;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utils.Editor;
using Object = UnityEngine.Object;

namespace SerializablePredicate.Editor
{
    [CustomPropertyDrawer(typeof(FieldFilterBase), true)]
    public class FieldFilterPropertyDrawer : PropertyDrawer
    {
        private static readonly Dictionary<ComparisonEnum, string> ComparisonLabel = new Dictionary<ComparisonEnum, string>
        {
            {ComparisonEnum.Equal, "=="},
            {ComparisonEnum.Greater, ">"},
            {ComparisonEnum.Less, "<"},
            {ComparisonEnum.GreaterEqual, ">="},
            {ComparisonEnum.LessEqual, "<="},
            {ComparisonEnum.Contains, "contains"},
            {ComparisonEnum.Callback, "callback"},
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var filter = (FieldFilterBase) PropertiesUtils.GetTargetObjectOfProperty(property);
            var parentType = filter.GetParentType();
            if (parentType == null)
            {
                EditorGUI.LabelField(position, "Please specify a target object");
            }
            else
            {

                float toggleSize = position.height * 1.1f;
                float spacing = 4;
                float width = (position.width - toggleSize) / 3 - spacing;
                var fieldsRect = new Rect(position.x + (width + spacing) * 0, position.y, width, position.height);
                var functionRect = new Rect(position.x + (width + spacing) * 1, position.y, width, position.height);
                var valueRect = new Rect(position.x + (width + spacing) * 2, position.y, width, position.height);
                var negatedRect = new Rect(position.x + (width + spacing) * 3, position.y, toggleSize, position.height);

                DrawFieldsDropdown(property, fieldsRect, parentType);
                var success = filter.TryGetFieldType(out var fieldType);
                if (success)
                {
                    DrawFunctionsDropdown(property, functionRect, fieldType);
                
                    DrawValueToCompare(property, fieldType, filter, valueRect);

                    DrawNegationToggle(negatedRect, property, fieldType);
                }
                else
                {
                    DrawWarningIcon(position);
                }
            }
        }

        private void DrawValueToCompare(SerializedProperty property, Type fieldType, FieldFilterBase filter, Rect valueRect)
        {
            var valueProperty = GetValuePropertyFromTypeAndComparison(property, fieldType, filter.predicateEnum);
            SetValueToCompareTypeFromTypeAndComparison(property, fieldType, filter.predicateEnum);
            EditorGUI.PropertyField(valueRect, valueProperty, new GUIContent(""));
        }

        private static void DrawWarningIcon(Rect position)
        {
            var builtinIcon = EditorGUIUtility.IconContent("console.warnicon.sml");
            var warningContent = new GUIContent(builtinIcon.image, "Can't find this field");
            var iconPosition = position;
            iconPosition.size = GUIStyle.none.CalcSize(warningContent);
            iconPosition.x -= iconPosition.size.x;
            GUI.Label(iconPosition, warningContent);
        }

        private static void DrawNegationToggle(Rect negatedRect, SerializedProperty property, Type fieldType)
        {
            var negatedProperty = property.FindPropertyRelative("negated");
            bool fieldIsBool = typeof(bool).IsAssignableFrom(fieldType);
            if (fieldIsBool)  // it is not necessary to draw the negated toggle
            { 
                negatedProperty.boolValue = false;
            }
            else
            {
                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleRight
                };
                EditorGUI.LabelField(negatedRect, new GUIContent("!", "negated"), labelStyle);
                negatedProperty.boolValue = EditorGUI.Toggle(negatedRect, new GUIContent("", "negated"), negatedProperty.boolValue);
            }
            
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void DrawFieldsDropdown(SerializedProperty property, Rect fieldsRect, Type parentType)
        {
            var pathProperty = property.FindPropertyRelative("fieldPath");
            var currentValue = pathProperty.stringValue;
            GUIContent fieldsContent;
            if (currentValue == string.Empty)
            {
                fieldsContent = new GUIContent("<Select a field>");
            }
            else
            {
                fieldsContent = new GUIContent(pathProperty.stringValue, pathProperty.stringValue);
            }
            if (GUI.Button(fieldsRect, fieldsContent, EditorStyles.popup))
            {
                var windowContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));

                void OnSelected(FieldNode node)
                {
                    pathProperty.stringValue = node.Path;
                    pathProperty.serializedObject.ApplyModifiedProperties();
                }

                var provider = ScriptableObject.CreateInstance<TypeFieldsProvider>();
                provider.Initialize(parentType, OnSelected);
                SearchWindow.Open(windowContext, provider);
            }
        }

        private static void DrawFunctionsDropdown(SerializedProperty property, Rect functionRect, Type fieldType)
        {
            var functionProperty = property.FindPropertyRelative("predicateEnum");

            // menu selection callback
            void SetEnumIndex(object index)
            {
                functionProperty.enumValueIndex = (int) index;
                functionProperty.serializedObject.ApplyModifiedProperties();
            }
            
            var currentComparisonEnum = (ComparisonEnum) functionProperty.enumValueIndex;
            
            // validate comparison is allowed based on the field type
            var availableComparisons = ComparisonEnumUtils.AvailableComparisonsForType(fieldType);
            if (!availableComparisons.Contains(currentComparisonEnum))
            {
                currentComparisonEnum = availableComparisons[0];
                functionProperty.enumValueIndex = (int) currentComparisonEnum;
            }
            
            var label = ComparisonLabel[currentComparisonEnum];
            var functionContent = new GUIContent(label);
            
            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleCenter
            };
            if (GUI.Button(functionRect, functionContent, style))
            {
                var menu = new GenericMenu();
                foreach (var keyEnum in availableComparisons)
                {
                    var valueLabel = ComparisonLabel[keyEnum];
                    bool selected = keyEnum == currentComparisonEnum;
                    menu.AddItem(new GUIContent(valueLabel), selected, SetEnumIndex, (int) keyEnum);
                }
                menu.DropDown(functionRect);
            }
        }

        private SerializedProperty GetValuePropertyFromTypeAndComparison(
            SerializedProperty property,
            Type fieldType,
            ComparisonEnum comparison)
        {
            if (typeof(bool).IsAssignableFrom(fieldType))
            {
                return property.FindPropertyRelative("valueToCompare.boolValue");
            }
            if (typeof(int).IsAssignableFrom(fieldType))
            {
                return property.FindPropertyRelative("valueToCompare.intValue");
            }
            if (typeof(float).IsAssignableFrom(fieldType))
            {
                return property.FindPropertyRelative("valueToCompare.floatValue");
            }
            if (typeof(string).IsAssignableFrom(fieldType))
            {
                return property.FindPropertyRelative("valueToCompare.stringValue");
            }
            if (typeof(IList).IsAssignableFrom(fieldType))
            {
                var genericType = fieldType.GenericTypeArguments[0];
                return GetValuePropertyFromTypeAndComparison(property, genericType, comparison);
            }
            if (typeof(Object).IsAssignableFrom(fieldType))
            {
                return property.FindPropertyRelative("valueToCompare.objectValue");
            }

            return null;
        }
        
        private void SetValueToCompareTypeFromTypeAndComparison(
            SerializedProperty property,
            Type fieldType,
            ComparisonEnum comparison)
        {
            var typeProperty = property.FindPropertyRelative("valueToCompare.argType");

            if (typeof(bool).IsAssignableFrom(fieldType))
            {
                typeProperty.enumValueIndex = (int) Arg.ArgType.Bool;
            }
            else if (typeof(int).IsAssignableFrom(fieldType))
            {
                typeProperty.enumValueIndex = (int) Arg.ArgType.Int;
            }
            else if (typeof(float).IsAssignableFrom(fieldType))
            {
                typeProperty.enumValueIndex = (int) Arg.ArgType.Float;
            }
            else if (typeof(string).IsAssignableFrom(fieldType))
            {
                typeProperty.enumValueIndex = (int) Arg.ArgType.String;
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                var genericType = fieldType.GenericTypeArguments[0];
                SetValueToCompareTypeFromTypeAndComparison(property, genericType, comparison);
            }
            else if (typeof(Object).IsAssignableFrom(fieldType))
            {
                typeProperty.enumValueIndex = (int) Arg.ArgType.Object;
            }
            else
            {
                typeProperty.enumValueIndex = (int) Arg.ArgType.Unsupported;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
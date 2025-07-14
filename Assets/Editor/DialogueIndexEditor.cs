using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DialogueManager))]
public class DialogueManagerEditor : Editor
{
    private ReorderableList lineList;

    private void OnEnable()
    {
        SerializedProperty linesProp = serializedObject.FindProperty("lines");

        lineList = new ReorderableList(serializedObject, linesProp, true, true, true, true);

        lineList.elementHeightCallback = index =>
        {
            var element = lineList.serializedProperty.GetArrayElementAtIndex(index);
            if (!element.isExpanded)
                return EditorGUIUtility.singleLineHeight + 6;

            float totalHeight = EditorGUIUtility.singleLineHeight + 6; // высота заголовка

            string[] fields = new string[]
            {
        "text", "speakerName", "characterName", "changeSpeakerName", "changeCharacterSprite", "changeBackground",
        "characterSprite", "backgroundSprite", "isOnRight", "flipSpeakerImage",
        "listenerCharacterName", "isListenerOnRight", "flipListenerImage",
        "showCollectibleView", "collectibleTitle", "collectibleContent", "collectibleIcon",
        "showItemNotification", "itemName", "itemType", "itemRarity", "itemIcon",
        "showStoryNotification", "storyNotificationText", "storyNotificationDuration", "storyNotificationIcon",
        "storyNotificationTextColor", "storyNotificationIconColor", "storyNotificationFontSize",
        "isBold", "isItalic", "isUppercase", "useCustomFont", "customFont",
        "isJumpLine", "gotoLineIndex", "hasChoices", "choices",
        "hasPathConsequences", "pathVarients",
        "extraActions"
            };

            foreach (string name in fields)
            {
                var prop = element.FindPropertyRelative(name);
                if (prop != null)
                {
                    totalHeight += EditorGUI.GetPropertyHeight(prop, true) + 4;
                }
            }

            return totalHeight + 15;
        };

        // Заголовок списка
        lineList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, $"Dialogue Lines ({linesProp.arraySize})");
        };

        // Заголовок элемента (всегда видимый)
        lineList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = linesProp.GetArrayElementAtIndex(index);
            var speakerProp = element.FindPropertyRelative("speakerName");
            var textProp = element.FindPropertyRelative("text");

            string shortText = textProp.stringValue;
            if (shortText.Length > 40) shortText = shortText.Substring(0, 40) + "...";

            Rect foldoutRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
            element.isExpanded = EditorGUI.Foldout(foldoutRect, element.isExpanded, $"[{index+1}] {speakerProp.stringValue}: \"{shortText}\"", true);
        };

        lineList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = lineList.serializedProperty.GetArrayElementAtIndex(index);
            var speakerProp = element.FindPropertyRelative("speakerName");
            var textProp = element.FindPropertyRelative("text");

            string shortText = textProp.stringValue;
            if (shortText.Length > 40) shortText = shortText.Substring(0, 40) + "...";

            Rect foldoutRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
            element.isExpanded = EditorGUI.Foldout(foldoutRect, element.isExpanded, $"[{index + 1}] {speakerProp.stringValue}: \"{shortText}\"", true);

            // 📌 Обработка правого клика
            Event e = Event.current;
            if (e.type == EventType.ContextClick && foldoutRect.Contains(e.mousePosition))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Duplicate Line"), false, () =>
                {
                    lineList.serializedProperty.InsertArrayElementAtIndex(index); // вставка дубликата

                    serializedObject.ApplyModifiedProperties(); // применяем, чтобы он появился

                    // Копируем содержимое из оригинала (index + 1) в новый (index)
                    var source = lineList.serializedProperty.GetArrayElementAtIndex(index + 1);
                    var target = lineList.serializedProperty.GetArrayElementAtIndex(index);

                    CopySerializedValues(target, source); // новый метод, см. ниже
                    serializedObject.ApplyModifiedProperties();
                });

                menu.AddItem(new GUIContent("Delete Line"), false, () =>
                {
                    lineList.serializedProperty.DeleteArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                });

                menu.ShowAsContext();
                e.Use();
            }
        };

    // Полная отрисовка полей при раскрытии
    lineList.drawElementBackgroundCallback = (rect, index, active, focused) =>
        {
        var element = lineList.serializedProperty.GetArrayElementAtIndex(index);
        if (!element.isExpanded) return;

        EditorGUI.indentLevel++;
        float y = rect.y + EditorGUIUtility.singleLineHeight + 4;
        float h = EditorGUIUtility.singleLineHeight;

        void Prop(string name)
        {
            var prop = element.FindPropertyRelative(name);
            if (prop != null)
            {
               float height = EditorGUI.GetPropertyHeight(prop, true);
               EditorGUI.PropertyField(new Rect(rect.x + 10, y, rect.width - 20, height), prop, true);
               y += height + 4;
            }
        }

            // Вставляем ВСЕ нужные поля из DialogueLine
            Prop("text");
        Prop("speakerName");
        Prop("characterName");
        Prop("changeSpeakerName");
        Prop("changeCharacterSprite");
        Prop("changeBackground");
        Prop("characterSprite");
        Prop("backgroundSprite");
        Prop("isOnRight");
        Prop("flipSpeakerImage");

        // Слушающий
        Prop("listenerCharacterName");
        Prop("isListenerOnRight");
        Prop("flipListenerImage");

        // Коллекционка
        Prop("showCollectibleView");
        Prop("collectibleTitle");
        Prop("collectibleContent");
        Prop("collectibleIcon");

        // Предмет
        Prop("showItemNotification");
        Prop("itemName");
        Prop("itemType");
        Prop("itemRarity");
        Prop("itemIcon");

        // Story Notification
        Prop("showStoryNotification");
        Prop("storyNotificationText");
        Prop("storyNotificationDuration");
        Prop("storyNotificationIcon");
        Prop("storyNotificationTextColor");
        Prop("storyNotificationIconColor");
        Prop("storyNotificationFontSize");
        Prop("isBold");
        Prop("isItalic");
        Prop("isUppercase");
        Prop("useCustomFont");
        Prop("customFont");

        // Выборы / переходы
        Prop("isJumpLine");
        Prop("gotoLineIndex");
        Prop("hasChoices");
        Prop("choices");
        Prop("hasPathConsequences");
        Prop("pathVarients");

        // ExtraActions
        Prop("extraActions");

        EditorGUI.indentLevel--;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Остальные поля менеджера
        DrawPropertiesExcluding(serializedObject, "lines");

        EditorGUILayout.Space(10);
        lineList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private static void CopySerializedValues(SerializedProperty to, SerializedProperty from)
    {
        SerializedProperty fromCopy = from.Copy();
        SerializedProperty toCopy = to.Copy();
        int depth = fromCopy.depth;

        bool enterChildren = true;

        while (fromCopy.Next(enterChildren))
        {
            if (fromCopy.depth <= depth)
                break;

            toCopy.Next(enterChildren);
            enterChildren = false;

            switch (fromCopy.propertyType)
            {
                case SerializedPropertyType.Integer:
                    toCopy.intValue = fromCopy.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    toCopy.boolValue = fromCopy.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    toCopy.floatValue = fromCopy.floatValue;
                    break;
                case SerializedPropertyType.String:
                    toCopy.stringValue = fromCopy.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    toCopy.colorValue = fromCopy.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    toCopy.objectReferenceValue = fromCopy.objectReferenceValue;
                    break;
                case SerializedPropertyType.Enum:
                    toCopy.enumValueIndex = fromCopy.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    toCopy.vector2Value = fromCopy.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    toCopy.vector3Value = fromCopy.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    toCopy.vector4Value = fromCopy.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    toCopy.rectValue = fromCopy.rectValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    toCopy.animationCurveValue = fromCopy.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    toCopy.boundsValue = fromCopy.boundsValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    toCopy.quaternionValue = fromCopy.quaternionValue;
                    break;
            }
        }
    }
}
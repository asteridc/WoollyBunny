using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DialogueChapter))]
public class DialogueChapterEditor : Editor
{
    private ReorderableList list;
    private SerializedProperty chapterIdProp;
    private SerializedProperty chapterNumberProp;
    private SerializedProperty linesProp;
    private SerializedObject so;

    private string[] fields;

    private void OnEnable()
    {
        so = serializedObject;
        chapterIdProp = so.FindProperty("chapterId");
        chapterNumberProp = so.FindProperty("chapterNumber");
        linesProp = so.FindProperty("lines");

        fields = new string[]
        {
            "text","speakerName","characterName","isRadio","changeSpeakerName",
            "changeCharacterSprite","changeBackground","characterSprite","backgroundSprite", "backgroundId",
            "backgroundTransition",
            "isOnRight","flipSpeakerImage","listenerCharacterName","isListenerOnRight",
            "flipListenerImage","showCollectibleView","collectibleItem","showItemNotification","itemName","itemType","itemRarity",
            "itemIcon","showStoryNotification","storyNotificationText","storyNotificationDuration",
            "storyNotificationIcon","storyNotificationTextColor","storyNotificationIconColor",
            "storyNotificationFontSize","isBold","isItalic","isUppercase","useCustomFont",
            "customFont","unlockChoiceKeys","isJumpLine","gotoLineIndex","hasChoices",
            "choices","useFallbackIfNoRequired","fallbackLineIndex","hasPathConsequences",
            "pathVarients","extraActions"
        };

        list = new ReorderableList(so, linesProp, true, true, true, true);

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, $"Dialogue Lines ({linesProp.arraySize})");
        };

        list.elementHeightCallback = index =>
        {
            var element = linesProp.GetArrayElementAtIndex(index);
            float height = EditorGUIUtility.singleLineHeight + 6;

            if (element.isExpanded)
            {
                foreach (var field in fields)
                {
                    var property = element.FindPropertyRelative(field);
                    if (property != null)
                        height += EditorGUI.GetPropertyHeight(property, true) + 4;
                }
            }

            return height + 4;
        };

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            var element = linesProp.GetArrayElementAtIndex(index);

            var speaker = element.FindPropertyRelative("speakerName");
            var text = element.FindPropertyRelative("text");

            string preview = text.stringValue;
            if (preview.Length > 40)
                preview = preview.Substring(0, 40) + "...";

            Rect foldRect = new(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);

            element.isExpanded = EditorGUI.Foldout(
                foldRect,
                element.isExpanded,
                $"[{index + 1}] {speaker.stringValue}: \"{preview}\"",
                true
            );

            if (Event.current.type == EventType.ContextClick && foldRect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Duplicate"), false, () =>
                {
                    linesProp.InsertArrayElementAtIndex(index);
                    so.ApplyModifiedProperties();
                    Copy(index + 1, index);
                    so.ApplyModifiedProperties();
                });

                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    linesProp.DeleteArrayElementAtIndex(index);
                    so.ApplyModifiedProperties();
                });

                menu.ShowAsContext();
                Event.current.Use();
            }

            if (!element.isExpanded)
                return;

            float y = rect.y + EditorGUIUtility.singleLineHeight + 6;

            foreach (var field in fields)
            {
                var property = element.FindPropertyRelative(field);
                if (property == null)
                    continue;

                float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
                EditorGUI.PropertyField(
                    new Rect(rect.x + 10, y, rect.width - 20, propertyHeight),
                    property,
                    true
                );
                y += propertyHeight + 4;
            }
        };
    }

    public override void OnInspectorGUI()
    {
        so.Update();

        EditorGUILayout.PropertyField(chapterIdProp);
        EditorGUILayout.PropertyField(chapterNumberProp);
        EditorGUILayout.Space();

        list.DoLayoutList();

        if (so.ApplyModifiedProperties())
            EditorUtility.SetDirty(target);
    }

    private void Copy(int targetIndex, int sourceIndex)
    {
        var src = linesProp.GetArrayElementAtIndex(sourceIndex);
        var dst = linesProp.GetArrayElementAtIndex(targetIndex);

        SerializedProperty source = src.Copy();
        SerializedProperty destination = dst.Copy();

        int depth = source.depth;
        bool enterChildren = true;

        while (source.Next(enterChildren))
        {
            if (source.depth <= depth)
                break;

            destination.Next(enterChildren);
            enterChildren = false;

            switch (source.propertyType)
            {
                case SerializedPropertyType.Integer:
                    destination.intValue = source.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    destination.boolValue = source.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    destination.floatValue = source.floatValue;
                    break;
                case SerializedPropertyType.String:
                    destination.stringValue = source.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    destination.colorValue = source.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    destination.objectReferenceValue = source.objectReferenceValue;
                    break;
                case SerializedPropertyType.Enum:
                    destination.enumValueIndex = source.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    destination.vector2Value = source.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    destination.vector3Value = source.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    destination.vector4Value = source.vector4Value;
                    break;
            }
        }
    }
}

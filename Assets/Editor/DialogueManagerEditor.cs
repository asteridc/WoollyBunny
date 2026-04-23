using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DialogueChapter))]
public class DialogueChapterEditor : Editor
{
    private ReorderableList list;
    private SerializedProperty linesProp;
    private SerializedProperty chapterIdProp;
    private SerializedProperty chapterIndexProp;
    private SerializedObject so;

    private string[] fields;

    private void OnEnable()
    {
        so = serializedObject;
        chapterIdProp = so.FindProperty("chapterId");
        chapterIndexProp = so.FindProperty("chapterIndex");
        linesProp = so.FindProperty("lines");

        // Кэшируем поля один раз
        fields = new string[]
        {
            "text","speakerName","characterName","isRadio","changeSpeakerName",
            "changeCharacterSprite","changeBackground","characterSprite","backgroundSprite", "backgroundId",
            "isOnRight","flipSpeakerImage","listenerCharacterName","isListenerOnRight",
            "flipListenerImage","showCollectibleView","collectibleTitle","collectibleContent",
            "collectibleIcon","showItemNotification","itemName","itemType","itemRarity",
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
            float h = EditorGUIUtility.singleLineHeight + 6;

            if (element.isExpanded)
            {
                foreach (var f in fields)
                {
                    var p = element.FindPropertyRelative(f);
                    if (p != null) h += EditorGUI.GetPropertyHeight(p, true) + 4;
                }
            }

            return h + 4;
        };

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            var element = linesProp.GetArrayElementAtIndex(index);

            var speaker = element.FindPropertyRelative("speakerName");
            var text = element.FindPropertyRelative("text");

            string preview = text.stringValue;
            if (preview.Length > 40) preview = preview.Substring(0, 40) + "...";

            Rect foldRect = new(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);

            element.isExpanded = EditorGUI.Foldout(
                foldRect,
                element.isExpanded,
                $"[{index + 1}] {speaker.stringValue}: \"{preview}\"",
                true
            );

            // ПКМ меню
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

            if (!element.isExpanded) return;

            float y = rect.y + EditorGUIUtility.singleLineHeight + 6;

            foreach (var f in fields)
            {
                var p = element.FindPropertyRelative(f);
                if (p != null)
                {
                    float ph = EditorGUI.GetPropertyHeight(p, true);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, y, rect.width - 20, ph), p, true);
                    y += ph + 4;
                }
            }
        };
    }

    public override void OnInspectorGUI()
    {
        so.Update();

        if (chapterIdProp != null)
            EditorGUILayout.PropertyField(chapterIdProp);
        if (chapterIndexProp != null)
            EditorGUILayout.PropertyField(chapterIndexProp);

        EditorGUILayout.Space(6);

        list.DoLayoutList();

        if (so.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void Copy(int targetIndex, int sourceIndex)
    {
        var src = linesProp.GetArrayElementAtIndex(sourceIndex);
        var dst = linesProp.GetArrayElementAtIndex(targetIndex);

        SerializedProperty s = src.Copy();
        SerializedProperty d = dst.Copy();

        int depth = s.depth;
        bool enterChildren = true;

        while (s.Next(enterChildren))
        {
            if (s.depth <= depth) break;
            d.Next(enterChildren);
            enterChildren = false;

            switch (s.propertyType)
            {
                case SerializedPropertyType.Integer: d.intValue = s.intValue; break;
                case SerializedPropertyType.Boolean: d.boolValue = s.boolValue; break;
                case SerializedPropertyType.Float: d.floatValue = s.floatValue; break;
                case SerializedPropertyType.String: d.stringValue = s.stringValue; break;
                case SerializedPropertyType.Color: d.colorValue = s.colorValue; break;
                case SerializedPropertyType.ObjectReference: d.objectReferenceValue = s.objectReferenceValue; break;
                case SerializedPropertyType.Enum: d.enumValueIndex = s.enumValueIndex; break;
                case SerializedPropertyType.Vector2: d.vector2Value = s.vector2Value; break;
                case SerializedPropertyType.Vector3: d.vector3Value = s.vector3Value; break;
                case SerializedPropertyType.Vector4: d.vector4Value = s.vector4Value; break;
            }
        }
    }
}


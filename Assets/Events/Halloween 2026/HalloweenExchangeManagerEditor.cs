using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HalloweenExchangeManager))]
public sealed class HalloweenExchangeManagerEditor : Editor
{
    private SerializedProperty debugResourceId;
    private SerializedProperty debugResourceAmount;
    private SerializedProperty debugRemoveOfferRewards;

    private void OnEnable()
    {
        debugResourceId = serializedObject.FindProperty("debugResourceId");
        debugResourceAmount = serializedObject.FindProperty("debugResourceAmount");
        debugRemoveOfferRewards = serializedObject.FindProperty("debugRemoveOfferRewards");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "debugResourceId",
            "debugResourceAmount",
            "debugRemoveOfferRewards");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Инструменты предназначены для тестирования. Они меняют локальное сохранение аккаунта.",
            MessageType.Info);

        EditorGUILayout.PropertyField(debugResourceId, new GUIContent("Resource ID"));
        EditorGUILayout.PropertyField(debugResourceAmount, new GUIContent("Amount"));
        serializedObject.ApplyModifiedProperties();

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            HalloweenExchangeManager manager = (HalloweenExchangeManager)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Добавить ресурс"))
                manager.DebugAddResource();
            if (GUILayout.Button("Установить баланс"))
                manager.DebugSetResourceAmount();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(debugRemoveOfferRewards,
                new GUIContent("Удалять награды офферов из инвентаря при сбросе"));
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Сбросить состояние магазина"))
            {
                bool removeRewards = debugRemoveOfferRewards.boolValue;
                string message = removeRewards
                    ? "Будут очищены вместимость корзины и промокоды офферов, а все награды из пула магазина будут удалены из инвентаря. Баланс ресурсов не изменится. Продолжить?"
                    : "Будут очищены вместимость корзины и промокоды офферов. Награды останутся в инвентаре. Баланс ресурсов не изменится. Продолжить?";

                if (EditorUtility.DisplayDialog(
                        "Сброс состояния магазина", message, "Сбросить", "Отмена"))
                {
                    manager.DebugResetShopState();
                }
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Запусти Play Mode, чтобы использовать кнопки отладки.",
                MessageType.None);
        }
    }
}

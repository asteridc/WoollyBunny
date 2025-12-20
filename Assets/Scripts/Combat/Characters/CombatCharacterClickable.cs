using UnityEngine;
using UnityEngine.EventSystems;

public class CombatCharacterClickable : MonoBehaviour, IPointerClickHandler
{
    public CombatCharacter character;

    public void OnPointerClick(PointerEventData eventData)
    {
        CombatManager.Instance.OnCharacterClicked(character);
    }
}
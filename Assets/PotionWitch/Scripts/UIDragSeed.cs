using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIPlantItem))]
public class UIDragSeed : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private UIPlantItem item;

    private void Awake()
    {
        item = GetComponent<UIPlantItem>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || item.plantData == null) return;

        Debug.Log($"Begin Drag Seed: {item.plantData.id}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // בשלב ראשון רק לוודא שזה באמת מגיב לגרירה
        // אחר כך נוסיף אייקון נגרר שמזיזים עם האצבע
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (item == null || item.plantData == null) return;

        Debug.Log($"End Drag Seed: {item.plantData.id}");
    }
}

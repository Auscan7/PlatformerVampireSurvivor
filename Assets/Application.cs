using UnityEngine;
using UnityEngine.UI;
using UnInventory.Core.Extensions;
using UnInventory.Standard;

public class Application : MonoBehaviour
{
    [SerializeField] private GameObject _prefabInventory = default;
    [SerializeField] private Button _buttonOpenClose = default;

    void Start()
    {
        var entities = ResourcesExt.LoadDataEntities("InventoryFolder");
        var inventory = new InventoryOpenCloseObject(_prefabInventory, entities, "FirstInventory");
        _buttonOpenClose.onClick.AddListener(() => inventory.OpenClose());
    }
}

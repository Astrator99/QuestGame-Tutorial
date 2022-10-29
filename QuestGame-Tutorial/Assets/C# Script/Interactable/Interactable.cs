using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    #region Inspector

    [Tooltip("invoked when the player interacts with the Interactable")]
    [SerializeField] private UnityEvent onInteracted;

    [Tooltip("invoked when the player selects this Interactable, and they is able to interact with it.")]
    [SerializeField] private UnityEvent onSelected;

    [Tooltip("invoked when the player deselects this Interactable, and they stops being able to interact with it.")]
    [SerializeField] private UnityEvent onDeselected;


    #endregion

    #region Unity Event Functions



    #endregion

    public void Interact()
    {
        onInteracted.Invoke();
    }

    public void Select()
    {
        onSelected.Invoke();
    }

    public void Deselect()
    {
        onDeselected.Invoke();
    }
}

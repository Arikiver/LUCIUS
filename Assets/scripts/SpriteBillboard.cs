using UnityEngine;

public class SpriteBillboard : MonoBehaviour, IInteractable
{
    public void Interact() {
        Debug.Log("interactionnn");
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);      
    }
}

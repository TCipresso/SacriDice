using UnityEngine;

public class Click3DObject : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left mouse click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("Clicked");
                    if (WalletManager.Instance != null)
                    {
                        WalletManager.Instance.ShowWinScreen();
                    }
                }
            }
        }
    }
}

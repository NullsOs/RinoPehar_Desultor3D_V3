using System.Collections;
using UnityEngine;

public class SaveLoader : MonoBehaviour
{
    public IEnumerator ApplySaveAfterFrame()
    {
        yield return null;
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && SaveManager.HasValidPlayerData())
        {
            Vector3 savedPosition = SaveManager.GetSavedPlayerPosition();

            CharacterController controller = player.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
                player.transform.position = savedPosition;
                controller.enabled = true;
            }
            else
            {
                player.transform.position = savedPosition;
            }

            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (health != null)
            {
                int savedHealth = SaveManager.GetSavedHealth(health.MaxHealth);
                health.SetHealth(savedHealth);
            }
        }

        SaveManager.FinishLoadingSave();

        gameObject.SetActive(false);
    }
}
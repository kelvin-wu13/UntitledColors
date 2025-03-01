using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 7f;
    public float currentHealth;
    public float damage = 1f;
    private PlayerRespawn playerRespawn;
    private UIManager uiManager;
    private Animator animator;

    //Animation
    private readonly string hitTrigger = "Hit";
    private readonly string dieTrigger = "Die";
    private bool isDying = false;

    private void Start()
    {
        uiManager = GetComponent<UIManager>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        playerRespawn = GetComponent<PlayerRespawn>();
    }

    public void TakeDamage(float damage)
    {
        if(isDying) return;

        currentHealth -= damage;

        if (uiManager != null)
        {
            uiManager.UpdatePlayerHealthUI();
            Debug.Log("Sending Update to uimanager");
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithAnimation());
        }
        else
        {
            animator.SetTrigger(hitTrigger);
        }
    }

    private IEnumerator DieWithAnimation()
    {
        isDying = true;

        //Trigger death animation
        animator.SetTrigger(dieTrigger);

        //Wait for death animation to complete
        float deathAnimDuration = GetAnimationClipLength("");
        yield return new WaitForSeconds(deathAnimDuration);

        //RespawnPlayer
        playerRespawn.RespawnPlayer();

        currentHealth = maxHealth;
        if (uiManager != null)
        {
            uiManager.UpdatePlayerHealthUI();
        }

        //Reset dying flag after Respawn
        isDying = false;
    }

    private float GetAnimationClipLength(string clipName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains(clipName))
            {
                return clip.length;
            }
        }
        return 2f;//Default time if no clip
    }
}
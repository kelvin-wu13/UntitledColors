using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public delegate void PlayerDeathHandler();
    public event PlayerDeathHandler OnPlayerDeath;

    public float maxHealth = 7f;
    public float currentHealth;
    public float damage = 1f;
    private PlayerRespawn playerRespawn;
    private UIManager uiManager;
    private Animator animator;
    AudioManager audioManager;

    private PlayerController playerController;

    //Animation
    private readonly string hitTrigger= "Hit";
    private readonly string dieTrigger = "Die";
    private readonly string idleState = "Idle";
    private bool isDying = false;

    private void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        uiManager = UIManager.instance;
        uiManager.Init(this);

        if (uiManager == null)
        {
            Debug.Log("UIManager instance = null");
            return;
        }
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        playerRespawn = GetComponent<PlayerRespawn>();
        playerController = GetComponent<PlayerController>();

        if (uiManager != null)
        {
            uiManager.UpdatePlayerHealthUI();
        }
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
            OnPlayerDeath?.Invoke();
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

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        //Trigger death animation
        animator.SetTrigger(dieTrigger);

        audioManager.PlaySFX(audioManager.playerDeath);

        //Wait for death animation to complete
        float deathAnimDuration = GetAnimationClipLength("Dead");
        yield return new WaitForSeconds(deathAnimDuration);

        //RespawnPlayer
        playerRespawn.RespawnPlayer();

        yield return new WaitForSeconds(2.5f); // Same delay as in GameManager
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Transition to idle animation
        animator.Play(idleState);

        // Reset dying flag after Respawn
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

    public void RestoreHealth()
    {
        currentHealth = maxHealth;
        if (uiManager != null)
        {
            uiManager.UpdatePlayerHealthUI();
        }
    }
}
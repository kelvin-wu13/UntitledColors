using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("General Audio Clip")]
    public AudioClip background;

    [Header("MC Audio Clip")]
    public AudioClip chargeHeavy;
    public AudioClip playerDeath;
    public AudioClip playerWalk;
    public AudioClip playerDash;
    public AudioClip playerLightAttack1;
    public AudioClip playerLightAttack2;
    public AudioClip playerLightAttack3;
    public AudioClip playerHeavyAttack;
    public AudioClip playerGetHit;
    public AudioClip playerLandHit;
    public AudioClip playerHitObject;

    [Header("Bull Audio Clip")]
    public AudioClip bullCharge;
    public AudioClip bullDeath;
    public AudioClip bullHit;
    public AudioClip bullChargeAttack;
    public AudioClip bullStunned;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
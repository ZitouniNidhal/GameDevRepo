using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish_Boss : MonoBehaviour
{
    private enum BossState { Idle, Jumping, Spitting, Recovering }
    private BossState currentState = BossState.Idle;

    private bool attacking = false;
    private Transform attack_point;
    private float timer = 0;
    public float jump_speed = 1f;
    public float jump_height = 2.0f;
    public GameObject look_at;
    public int yell_prob = 3;
    private Vector3 start_pos = Vector3.zero;

    public AudioSource sSource = null;
    public AudioClip[] water_splash;
    public AudioClip[] water_land;
    public AudioClip[] yells;
    public GameObject spit_attack_item;
    public Transform spit_attack_point;
    public float spit_speed = 3.0f;

    private bool spitAttack = false;
    public ParticleSystem jumpEffect;
    public ParticleSystem spitEffect;
    public Animator animator;

    void Update()
    {
        switch (currentState)
        {
            case BossState.Jumping:
                HandleJumping();
                break;
            case BossState.Spitting:
                HandleSpitting();
                break;
            case BossState.Recovering:
                // Add logic for recovery period if needed
                break;
            default:
                break;
        }
    }

    private void HandleJumping()
    {
        if (Vector3.Distance(transform.position, attack_point.position) <= 0.1f)
        {
            EndAttack();
            PlayWaterLand();
        }
        else
        {
            timer += Time.deltaTime;
            Vector3 cur_pos = Vector3.Lerp(start_pos, attack_point.position, timer / jump_speed);
            cur_pos.y += jump_height * Mathf.Sin(Mathf.Clamp01(timer / jump_speed) * Mathf.PI);
            transform.position = cur_pos;
            Quaternion targetRotation = Quaternion.LookRotation(look_at.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, timer / jump_speed);
        }
    }

    private void HandleSpitting()
    {
        if (Vector3.Distance(transform.position, attack_point.position) <= 0.1f)
        {
            EndAttack();
            StartCoroutine(WaitSpitAttack());
        }
        else
        {
            timer += Time.deltaTime;
            Vector3 cur_pos = Vector3.Lerp(start_pos, attack_point.position, timer / jump_speed);
            transform.position = cur_pos;
            Quaternion targetRotation = Quaternion.LookRotation(look_at.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, timer / jump_speed);
        }
    }

    private void EndAttack()
    {
        attacking = false;
        timer = 0;
        currentState = BossState.Recovering;
        animator.SetTrigger("Land");
    }

    public void MakeJumpAttack(GameObject start, List<GameObject> attack_points)
    {
        SetupAttack(start, attack_points);
        attacking = true;
        currentState = BossState.Jumping;
        PlayWaterSplash();
        animator.SetTrigger("Jump");
        jumpEffect?.Play();
        TryPlayYell();
    }

    public void SpitAttack(GameObject start, List<GameObject> attack_points)
    {
        SetupAttack(start, attack_points);
        spitAttack = true;
        currentState = BossState.Spitting;
        PlayWaterSplash();
        animator.SetTrigger("Spit");
        spitEffect?.Play();
        TryPlayYell();
    }

    private void SetupAttack(GameObject start, List<GameObject> attack_points)
    {
        List<GameObject> tempList = new List<GameObject>(attack_points);
        tempList.Remove(start);
        attack_point = tempList[Random.Range(0, tempList.Count)].transform;
        start_pos = transform.position;
        timer = 0;
    }

    private void TryPlayYell()
    {
        int prob = Random.Range(0, yell_prob + 1);
        if (prob == yell_prob)
        {
            PlayYell();
        }
    }

    IEnumerator WaitSpitAttack()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        GameObject spit = Instantiate(spit_attack_item, spit_attack_point.position, spit_attack_point.rotation);
        GameObject player = GameObject.FindGameObjectWithTag("PlayerCamera");
        if (spit.GetComponent<Rigidbody>())
        {
            Vector3 direction = (player.transform.position - spit_attack_point.position).normalized;
            spit.GetComponent<Rigidbody>().AddForce(direction * spit_speed, ForceMode.Impulse);
        }
        Destroy(spit, 3.0f);
    }

    void PlayWaterSplash()
    {
        if (sSource != null && water_splash.Length > 0)
        {
            sSource.clip = water_splash[Random.Range(0, water_splash.Length)];
            sSource.Play();
        }
    }

    void PlayWaterLand()
    {
        if (sSource != null && water_land.Length > 0)
        {
            sSource.clip = water_land[Random.Range(0, water_land.Length)];
            sSource.Play();
        }
    }

    public void PlayYell()
    {
        if (sSource != null && yells.Length > 0)
        {
            sSource.clip = yells[Random.Range(0, yells.Length)];
            sSource.Play();
        }
    }
}

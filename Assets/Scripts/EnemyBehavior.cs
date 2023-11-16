﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.SceneManagement;

public class EnemyBehavior : MonoBehaviour
{
    public Transform player;
    public Transform patrolRoute;
    public List<Transform> locations;
    private int locationIndex = 0;
    private NavMeshAgent agent;
    private int _lives = 3;
    public int enemyDamage = 1;
    public GameObject gunshotEffect;
    public Transform gunSpawnPoint;
    public int maxHealth = 3;
    private int currentHealth;
    public GameObject hitPoint; 
    private bool isShooting = false;

    private static int totalEnemies = 0;
    private static int remainingEnemies = 0;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI victoryText;

    private bool hasWon = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player").transform;
        InitializePatrolRoute();
        MoveToNextPatrolLocation();

        totalEnemies++; 
        remainingEnemies++; 

        UpdateEnemiesText();
    }

    void Update()
    {
        if (agent.remainingDistance < 0.2f && !agent.pathPending)
        {
            MoveToNextPatrolLocation();
        }
    }

    void FixedUpdate()
    {
        if (isShooting)
        {
            Shoot();
        }
    }

    void MoveToNextPatrolLocation()
    {
        if (locations.Count == 0)
            return;

        agent.destination = locations[locationIndex].position;
        locationIndex = (locationIndex + 1) % locations.Count;
    }

    private void InitializePatrolRoute()
    {
        foreach (Transform child in patrolRoute)
        {
            locations.Add(child);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            agent.destination = player.position;
            Debug.Log("Player detected - attack!");
        }
        if (other.CompareTag("Player"))
        {
            PlayerBehavior player = other.GetComponent<PlayerBehavior>();
            if (player != null)
            {
                player.TakeDamage(enemyDamage);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player")
        {
            Debug.Log("Player out of range, resume patrol");
        }
        if (other.CompareTag("Player"))
        {
            StopShooting();
        }
    }

    private void StartShooting()
    {
        isShooting = true;
        gunshotEffect.SetActive(true); 
    }

    private void StopShooting()
    {
        isShooting = false;
        gunshotEffect.SetActive(false); 
    }

    private void Shoot()
    {
        PlayerBehavior playerBehavior = player.GetComponent<PlayerBehavior>();
        if (playerBehavior != null && playerBehavior.IsDucking())
        {
            return;
        }

        if (gunshotEffect != null && gunSpawnPoint != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(gunSpawnPoint.position, transform.TransformDirection(Vector3.forward), out hit))
            {
                PlayerBehavior enemy = hit.collider.GetComponent<PlayerBehavior>();
                if (enemy != null)
                {
                    Debug.DrawRay(gunSpawnPoint.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    GameObject a = Instantiate(gunshotEffect, gunSpawnPoint.position, Quaternion.identity);
                    GameObject b = Instantiate(hitPoint, hit.point, Quaternion.identity);
                    Destroy(a, 1);
                    Destroy(b, 1);
                    int damage = Random.Range(1, 4); 
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
    public void TakeDamage(int damage)
    {
        _lives -= damage;
        Debug.Log("Enemy took " + damage + " damage.");
        if (_lives <= 0)
        {
            remainingEnemies--; 
            UpdateEnemiesText();

            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        if (remainingEnemies <= 0)
        {
            AllEnemiesKilled();
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Enemy down.");
        }
    }

    void Die()
    {
        if (remainingEnemies <= 0)
        {
            AllEnemiesKilled();
        }
    }

    void UpdateEnemiesText()
    {
        if (enemiesText != null)
        {
            if (remainingEnemies > 0)
            {
                enemiesText.text = "Remaining Enemies: " + remainingEnemies + "/" + totalEnemies;
            }
            else
            {
                enemiesText.text = "All Enemies Killed!";
            }
        }
    }

    void AllEnemiesKilled()
    {
        if (!hasWon) 
        {
            hasWon = true;
            if (victoryText != null)
            {
                victoryText.text = "YOU WIN!";
                StartCoroutine(WinRoutine());
            }
        }
    }

    IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject); 
        LoadMainScene();
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene("Mainland"); 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileFiring : MonoBehaviour
{
    public float detectionRange = 10f; // Range to detect enemies
    public Vector2 detectionOffset = Vector2.zero; // Offset for the detection cast
    public float fireRate = 1f; // Rate of fire (bullets per second)
    public GameObject projectilePrefab; // Reference to the projectile prefab
    public Transform projectileSpawnPoint; // Spawn point for the projectile
    public float projectileSpeed = 10f; // Speed of the projectile

    private float nextFireTime = 0f; // Time when the next shot can be fired
    private Collider2D currentTarget; // The currently targeted enemy

    void Update()
    {
        DetectionLogic();
    }

    private void DetectionLogic()
    {
        Vector2 detectionCenter = (Vector2)transform.position + detectionOffset;

        // Check for enemies in range
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(detectionCenter, detectionRange, LayerMask.GetMask("DamageableCharacter"));

        // If there are enemies in range, find the closest one
        if (enemiesInRange.Length > 0)
        {
            Collider2D closestEnemy = GetClosestEnemy(enemiesInRange);

            // If we have a current target, check if it's still valid
            if (currentTarget != null)
            {
                // Check if the current target is still valid (in range and not blocked)
                if (currentTarget != null && currentTarget == closestEnemy && IsPathClear(currentTarget.transform.position))
                {
                    // If the current target is valid, try to fire
                    TryFire(currentTarget.transform.position);
                }
                else
                {
                    // If the current target is invalid, switch to the closest enemy
                    currentTarget = closestEnemy;
                    if (IsPathClear(currentTarget.transform.position))
                    {
                        TryFire(currentTarget.transform.position);
                    }
                }
            }
            else
            {
                // If we don't have a current target, set it to the closest enemy
                currentTarget = closestEnemy;
                TryFire(currentTarget.transform.position);
            }
        }
        else
        {
            // No enemies in range, reset the current target
            currentTarget = null;
        }
    }

    private void TryFire(Vector2 targetPosition)
    {
        if (Time.time >= nextFireTime && IsPathClear(targetPosition))
        {
            FireProjectile(targetPosition);
            nextFireTime = Time.time + 1f / fireRate; // Update the next fire time
        }
    }

    private bool IsPathClear(Vector2 targetPosition)
    {
        // Cast a ray from the player to the target position
        Vector2 detectionCenter = (Vector2)transform.position + detectionOffset;
        Vector2 direction = (targetPosition - detectionCenter).normalized;
        float distanceToTarget = Vector2.Distance(detectionCenter, targetPosition);

        // Perform a raycast
        RaycastHit2D hit = Physics2D.Raycast(detectionCenter, direction, distanceToTarget, LayerMask.GetMask("Ground"));

        // If the ray hits something, check if it's the target enemy
        if (hit.collider != null)
        {
            // Check if the hit object is not the target enemy
            if (!hit.collider.CompareTag("Enemy"))
            {
                return false; // There is an obstacle in the way
            }
        }

        // Check if the target is within range and not blocked
        if (distanceToTarget <= detectionRange)
        {
            return true; // Path is clear
        }

        return false; // Target is out of range
    }

    private Collider2D GetClosestEnemy(Collider2D[] enemies)
    {
        Collider2D closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance((Vector2)transform.position + detectionOffset, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    private void FireProjectile(Vector2 targetPosition)
    {
        // Use the spawn point for projectile instantiation
        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;

        // Instantiate the projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Calculate the direction to the target
        Vector2 direction = (targetPosition - (Vector2)spawnPosition).normalized;

        // Set the projectile's velocity
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }

        // Rotate the projectile to face the target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnDrawGizmos()
    {
        // Calculate the detection center with offset
        Vector2 detectionCenter = (Vector2)transform.position + detectionOffset;

        // Draw the detection range as a sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectionCenter, detectionRange);

        // Draw the raycast path if there is a current target
        if (currentTarget != null)
        {
            // Cast the target's position to Vector2 to match detectionCenter
            Vector2 targetPosition = (Vector2)currentTarget.transform.position;

            // Calculate the direction to the target
            Vector2 direction = (targetPosition - detectionCenter).normalized;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(detectionCenter, targetPosition);

            // Optionally, draw the raycast path to the target
            RaycastHit2D hit = Physics2D.Raycast(detectionCenter, direction, detectionRange, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                Gizmos.color = Color.blue; // Color for the hit point
                Gizmos.DrawLine(detectionCenter, hit.point);
            }
        }
    }

}

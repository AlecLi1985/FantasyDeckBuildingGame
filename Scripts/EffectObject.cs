using UnityEngine;
using System;
using System.Collections;

public class EffectObject : MonoBehaviour
{
    public static event Action<Transform> OnEffectObjectHit;

    public Transform owner;
    public Transform targetTransform;
    public bool parentToTransform = false;
    public float speed;
    public float lifeTime;
    public LayerMask collisionMask;

    public bool isProjectile = true;
    public bool isDamaging = true;

    bool hasCollided = false;
    float currentLifetime = 0f;

    private void Start()
    {
        
    }

    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
        transform.LookAt(targetTransform);
    }

    // Update is called once per frame
    void Update()
    {
        if (isProjectile == false)
        {
            currentLifetime += Time.deltaTime;
            if (currentLifetime > lifeTime)
            {
                StartCoroutine(DestroyEffect());
            }
        }
        else
        {
            if(hasCollided == false)
            {
                transform.position += transform.forward * (speed * Time.deltaTime);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform != owner)
        {
            hasCollided = true;

            if (OnEffectObjectHit != null)
            {
                OnEffectObjectHit.Invoke(other.transform);
            }

            StartCoroutine(DestroyEffect());
        }
    }

    IEnumerator DestroyEffect()
    {
        ParticleSystem[] particleSystems = transform.GetComponentsInChildren<ParticleSystem>();

        for(int i = 0; i < particleSystems.Length; i++)
        {
            if(particleSystems[i] != null)
            {
                particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

}

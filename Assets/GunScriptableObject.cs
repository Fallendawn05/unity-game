using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]

public class GunScriptableObject : ScriptableObject
{
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public ShootConfigurationScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private float LastShootTime;
    private PartivleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0; // in editor this will not be properly reset, in build it's fine
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(spawnRotation);

        ShootSystem = Model.GetComponent<ParticleSystem>();
    }

    public void Shoot()
    {
        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            ShootSystem.Play();
            Vector3 shootDirection = ShootSystem.transform.forward
                + new Vector3(
                    Random.Range(
                        -ShootConfig.Spread.x,
                        ShootConfig.Spread.x
                    ),
                    Random.Range(
                        -ShootConfig.Spread.y,
                        ShootConfig.Spread.y
                    ),
                    Random.Range(
                        -ShootConfig.Spread.z,
                        ShootConfig.Spread.z
                    ),
                );
            shootDirection.Normalize();

            if (Physics.Raycast(
                ShootSystem.transform.position,
                shootDirection,
                out RaycastHit hit,
                float.MaxValue,
                ShootConfig.Hitmask
            ))
            {
                ActiveMonoBehaviour.StartCoroutine (
                    PlayTrial(
                        ShootSystem.transform.position,
                        hit.point,
                        hit
                    )
                );
            }
            else
            {
                ActiveMonoBehaviour.StartCoroutine (
                    PlayTrial(
                        ShootSystem.transform.position,
                        ShootSystem.transform.position + (shootDirection * TrailConfig.MissDistance),
                        new RaycastHit()
                    )
                );
            }
        }
    }

    private IEnumerator PlayTrial (Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield retrun null; // avoid position carry-over from last frame if reused

        instance.emitting = true;

        float Disance = Vector3.Disance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                StartPoint,
                EndPOint,
                Mathf.Clamp01 (1 - (remainingDistance / distane))
            );
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = Trailonfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.MinVertexDistance = TrailConfig.MinVertexDistancel;

        trail.emitting = false;
        trail.shadowCastingMode = unityEngine.Rendering.ShadowCastingMode.Off;

        retrun trail;
    }

}



using UnityEngine;

[System.Serializable]
public class SkyObjectEntry
{
    public GameObject prefab;
    public Transform parentLayer;
    public float parallaxSpeed;
}

public class SkyObjectSpawner : MonoBehaviour
{
    public SkyObjectEntry[] skyObjects;

    public Transform player;                      // Usually the camera or player
    public float spawnInterval = 3f;

    public float minXOffset = -25f;               // Spawn range left/right of camera
    public float maxXOffset = 25f;

    public float minYOffset = 5f;                 // Spawn range above camera
    public float maxYOffset = 15f;

    private float timer;

    void Update()
    {
        if (player == null || skyObjects.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnSkyObject();
            timer = 0f;
        }
    }

    void SpawnSkyObject()
    {
        SkyObjectEntry entry = skyObjects[Random.Range(0, skyObjects.Length)];

        Vector3 camPos = Camera.main.transform.position;

        float spawnX = camPos.x + Random.Range(minXOffset, maxXOffset);
        float spawnY = camPos.y + Random.Range(minYOffset, maxYOffset);

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        GameObject obj = Instantiate(entry.prefab, spawnPos, Quaternion.identity, entry.parentLayer);

        obj.transform.localPosition = new Vector3(
            obj.transform.localPosition.x,
            obj.transform.localPosition.y,
            0f
        );

        ParallaxObject po = obj.GetComponent<ParallaxObject>();
        if (po != null)
        {
            po.parallaxSpeed = entry.parallaxSpeed;
        }

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Background";
            sr.sortingOrder = -9;
        }
    }
}
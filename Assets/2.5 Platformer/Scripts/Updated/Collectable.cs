using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Collectable Variables")]
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float moveSinAmplitude;
    [SerializeField] private float moveSinFrequency;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Sine movement
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * moveSinFrequency) * moveSinAmplitude;
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlatformerController controller))
        {
            gameObject.SetActive(false);
        }
    }
}

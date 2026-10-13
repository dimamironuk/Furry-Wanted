using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 1;

    private float currentLifetime;

    private void OnEnable()
    {
        currentLifetime = lifetime;
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        if (collision.CompareTag("Player"))
        {
            HPController hp = collision.GetComponent<HPController>();
            if (hp != null)
            {
                hp.DMG(damage, transform.position);
            }
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
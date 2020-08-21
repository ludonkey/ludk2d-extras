using UnityEngine;

namespace LuDK.Toolkit.L2D
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SFXOnImpact2D : MonoBehaviour
    {
        public AudioSource SFX;
        private Rigidbody2D rb;
        private float lastVelocityMagnitude { get; set; }

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            lastVelocityMagnitude = rb.velocity.magnitude;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            float currentVelocityMagnitude = rb.velocity.magnitude;
            float vel = currentVelocityMagnitude + lastVelocityMagnitude;
            if (SFX != null && vel > 0)
            {
                SFX.Play();
            }
        }
    }
}
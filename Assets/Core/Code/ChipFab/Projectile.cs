using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public enum ProjectileState
    {
        Traveling,
        Arrived
    }

    public class Projectile : MonoBehaviour
    {
        private Vector3 m_moveDir;
        private ProjectileState m_state;
        private float m_speed;

        private Vector3 m_startPos;

        public float MaxTravelDist;

        public void SetDirAndSpeed(Vector3 dir, float speed)
        {
            m_moveDir = dir;
            m_speed = speed;
            m_state = ProjectileState.Traveling;
            m_startPos = this.transform.position;
        }

        public void FixedUpdate()
        {
            switch (m_state)
            {
                case ProjectileState.Traveling:
                    Travel();
                    break;
                case ProjectileState.Arrived:
                    break;
                default:
                    break;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            m_state = ProjectileState.Arrived;
        }

        private void Travel()
        {
            this.transform.position += m_moveDir * m_speed * Time.deltaTime;

            float dist = Mathf.Abs(Vector3.Distance(m_startPos, this.transform.position));
            if (dist >= MaxTravelDist) {
                Destroy(this.gameObject);
            }
        }
    }
}
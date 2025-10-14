using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFab.ChipFab
{
    public class Blaster : MonoBehaviour
    {
        private static KeyCode RotateLeftKey1 = KeyCode.LeftArrow;
        private static KeyCode RotateRightKey1 = KeyCode.RightArrow;

        private static KeyCode RotateLeftKey2 = KeyCode.A;
        private static KeyCode RotateRightKey2 = KeyCode.D;

        public GameObject Projectile;
        public float ProjectileSpeed;
        public Transform LaunchPoint;

        public float PivotSpeed;

        public float ReloadTime;

        private float m_reloadTimer;

        private List<GameObject> m_allProjectiles = new List<GameObject>();

        private void OnDisable()
        {
            while (m_allProjectiles.Count > 0)
            {
                if (m_allProjectiles[0])
                {
                    Destroy(m_allProjectiles[0]);
                }
                m_allProjectiles.RemoveAt(0);
            }
        }

        private void Update()
        {
            if (!this.gameObject.activeInHierarchy) { return; }

            if (AutomationMgr.Instance.CurrInstruction.Valid
                && (AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Etch || AutomationMgr.Instance.CurrInstruction.TargetStation == StationId.Sputter))
            {
                return;
            }

            if (m_reloadTimer > 0)
            {
                m_reloadTimer -= Time.deltaTime;
            }

            if (Input.GetKey(RotateLeftKey1) || Input.GetKey(RotateLeftKey2))
            {
                this.transform.Rotate(new Vector3(0, 0, -1) * PivotSpeed * Time.deltaTime);
            }
            if (Input.GetKey(RotateRightKey1) || Input.GetKey(RotateRightKey2))
            {
                this.transform.Rotate(new Vector3(0, 0, 1) * PivotSpeed * Time.deltaTime);
            }
        }

        public void Blast(bool bypassTimer = false)
        {
            if (!bypassTimer && m_reloadTimer > 0) { return; }

            var projectile = Instantiate(Projectile).GetComponent<Projectile>();
            var dir = (LaunchPoint.position - this.transform.position).normalized;
            projectile.transform.position = LaunchPoint.transform.position;
            projectile.SetDirAndSpeed(dir, ProjectileSpeed);

            m_allProjectiles.Add(projectile.gameObject);

            m_reloadTimer = ReloadTime;
        }
    }
}
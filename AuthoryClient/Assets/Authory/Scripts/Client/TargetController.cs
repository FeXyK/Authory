using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Authory.Scripts
{

    /// <summary>
    /// Handles the targeting system in the game.
    /// </summary>
    public class TargetController : MonoBehaviour
    {
        [SerializeField] Entity Player = null;
        [SerializeField] GameObject SelectionIndicator = null;
        [SerializeField] float NameTagShowRange = 50f;
        [SerializeField] float SelectionCircleRotationSpeed = 300f;
        [SerializeField] float SelectionCircleHeightOffset = 300f;

        [SerializeField] SelectedTargetInfoController targetInfo = null;
        public Entity target { get; set; } = null;

        List<Entity> tabEntites = new List<Entity>();
        float tabLife = 0.1f;
        float maxTabLife = 0.3f;
        int tabPosition = 0;

        private void Start()
        {
            Player = AuthoryData.Instance.Player;
        }

        public void SetTarget(Entity target)
        {
            this.target = target;
            if (target == null)
            {
                targetInfo.Hide();
            }
            else
            {
                targetInfo.Show();
                targetInfo.SetTargetInfo(target);
            }
            if (AuthoryData.Instance.Player != null)
                AuthoryData.Instance.Player.Target = target;
        }

        private void LateUpdate()
        {
            if (Player == null) return;
            ShowNearbyTags(NameTagShowRange);

            Entity entity = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            tabLife -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetTarget(null);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FindNearestTarget();
            }

            if (Physics.Raycast(ray, out hit))
            {
                entity = GetClickedEntity(hit);
            }
            if (target != null)
            {
                target.Select();
                SelectionIndicator.SetActive(true);
                SelectionIndicator.transform.position = target.transform.position + Vector3.down * SelectionCircleHeightOffset;
                SelectionIndicator.transform.Rotate(Vector3.forward, SelectionCircleRotationSpeed * Time.deltaTime);

                if (!target.gameObject.activeSelf)
                {
                    SetTarget(null);
                }
            }
            else
            {
                SelectionIndicator.SetActive(false);
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                targetInfo.Show();

                SetTarget(Player);
            }
            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                SetTarget(target);
            }
        }

        /// <summary>
        /// Returns the an entity if RaycastHit hit one.
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        private Entity GetClickedEntity(RaycastHit hit)
        {
            Entity entity = hit.collider.gameObject.GetComponent<Entity>();
            {
                if (entity != null)
                    entity.Highlight();

                if (Input.GetMouseButtonDown(0))
                {
                    if (entity != null)
                    {
                        if (target != null)
                            target.DeSelect();
                        SetTarget(entity);
                    }
                    else
                    {
                        SetTarget(null);
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Selects closest entity.
        /// </summary>
        public void FindNearestTarget()
        {
            if (tabLife < 0f)
            {
                if (target != null && target.Dead)
                    SetTarget(null);

                var entities = Physics.OverlapSphere(Player.transform.position, 37f);
                tabEntites.Clear();
                foreach (var e in entities)
                {
                    Entity tabEntity = e.GetComponent<Entity>();
                    if (tabEntity != null && tabEntity.Alive)
                    {
                        tabEntites.Add(tabEntity);
                    }
                }

                tabEntites = tabEntites.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList();
                if (tabEntites.Contains(Player))
                    tabEntites.Remove(Player);
                tabPosition = 0;
                if (tabEntites.Count > 0)
                    SetTarget(tabEntites[tabPosition]);
            }
            else
            {
                tabPosition++;
                if (tabEntites.Count > tabPosition)
                {
                    SetTarget(tabEntites[tabPosition]);
                }
                else
                {
                    tabPosition = 0;
                }
            }
            tabLife = maxTabLife;
        }

        /// <summary>
        /// Shows nearby entities name tag, if they are in the range.
        /// </summary>
        /// <param name="range">The range around the player in the entites tags will be shown.</param>
        private void ShowNearbyTags(float range)
        {
            var nearbyEntities = Physics.OverlapSphere(Player.transform.position, range);

            foreach (var nearbyEntity in nearbyEntities)
            {
                var entity = nearbyEntity.GetComponent<Entity>();
                if (entity != null && entity.Dead)
                {
                    entity.ShowInfo();
                }
            }
        }
    }
}

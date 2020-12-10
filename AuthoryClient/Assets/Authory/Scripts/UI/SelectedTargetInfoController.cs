using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Authory.Scripts
{
    /// <summary>
    /// When a target is selected this controls the upper middle Entity information panel.
    /// </summary>
    public class SelectedTargetInfoController : MonoBehaviour
    {
        [SerializeField] TMP_Text TargetName = null;
        [SerializeField] Slider TargetHealthBar = null;
        [SerializeField] TMP_Text TargetHealthInfo = null;
        [SerializeField] TargetBuffController BuffController = null;

        public Entity CurrentTarget { get; set; }

        private void Update()
        {
            if (CurrentTarget != null)
                SetTargetInfo(CurrentTarget.name, CurrentTarget.Health, CurrentTarget.Level);
        }

        public void SetTargetInfo(Entity entity)
        {
            if (CurrentTarget != entity)
            {
                SetTargetInfo(entity.name, entity.Health, entity.Level);
                BuffController.RefreshBuffs(entity);
                CurrentTarget = entity;
            }
        }

        public void RefreshBuffController()
        {
            BuffController.RefreshBuffs(CurrentTarget);
        }

        public void SetTargetInfo(string name, Resource health, int level)
        {
            TargetName.text = string.Format($"Lv.{level} {name}");
            UpdateHealthBar(health.MaxValue, health.Value);
        }

        public void UpdateHealthBar(int maxHealth, int health)
        {
            TargetHealthBar.maxValue = maxHealth;
            TargetHealthBar.value = health;
            TargetHealthInfo.text = string.Format($"{health}/{maxHealth} ({(float)health / (float)maxHealth * 100.0f:0.00}%)");
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
    }
}
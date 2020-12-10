using Assets.Authory.Scripts;
using Assets.Authory.Scripts.Enum;
using TMPro;
using UnityEngine;

public class DamageInfoController : MonoBehaviour
{
    [SerializeField] float minMaxShift = 5f;
    [SerializeField] float _lifetime = 5f;
    [SerializeField] float _floatingSpeed = 5f;

    [SerializeField] TMP_Text _info = null;
    [SerializeField] TMP_FontAsset critFont = null;
    [SerializeField] SkillCollection SkillList = null;

    Entity _entity;
    bool _crit;
    float _shiftY;

    float _maxLifetime;

    float _shiftX;
    float _shiftZ;


    private void Start()
    {
        _shiftX = Random.Range(-minMaxShift, minMaxShift);
        _shiftZ = Random.Range(-minMaxShift, minMaxShift);
        _maxLifetime = _lifetime;

        SkillList = SkillCollection.Instance;
    }

    public void Set(Entity entity, EffectType effectType, int value, bool crit)
    {
        if (SkillList == null) SkillList = SkillCollection.Instance;

        _info.color = SkillList.SkillEffectColors[effectType].MainColor;
        _info.outlineColor = SkillList.SkillEffectColors[effectType].OutlineColor;

        this._entity = entity;
        this._crit = crit;

        if (_crit)
        {
            _info.color = GetColor(255, 103, 0, 97);
            _info.fontSize *= 1.4f;
            _info.font = critFont;
            _info.fontStyle = FontStyles.Bold;
        }
        _info.text = value.ToString();
    }

    private Color GetColor(float r, float g, float b, float a = 255.0f)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1f);
    }

    private void LateUpdate()
    {
        if (_maxLifetime - _lifetime > 1f)
            _info.alpha -= Time.deltaTime / _maxLifetime * 2f;
        _info.fontSize -= Time.deltaTime * 0.1f;
        if (_entity != null)
        {
            this.transform.position = _entity.transform.position + new Vector3(_shiftX, 1.5f + _shiftY, _shiftZ);
            this.transform.rotation = Camera.main.transform.rotation;
            _info.fontSize = Vector3.Distance(Camera.main.transform.position, this.transform.position) / 20.0f * (_crit ? 1.2f : 1f);
        }

        _lifetime -= Time.deltaTime / _lifetime;
        _shiftY += Time.deltaTime * _floatingSpeed;


        if (_lifetime < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
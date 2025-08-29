using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform target;
    public RectTransform rotateTarget;
    public Image border;
    public Volume postVolume;

    public Vector3 normalScale = Vector3.one;
    public Vector3 hoverScale = new Vector3(1.15f, 1.15f, 1f);
    public float speed = 14f;

    public Vector2 zJitterRange = new Vector2(-8f, 8f);
    public Vector2 snapIntervalRange = new Vector2(0.05f, 0.15f);
    public Color hoverColor = Color.red;

    public float bloomNormal = 0.25f;
    public float bloomHover = 2.69f;
    public float bloomSpeed = 10f;

    Color rotateOriginal;
    Color borderOriginal;
    Vector3 tScale;
    bool hovering;
    float snapTimer;
    Image rotateImage;
    Bloom bloom;
    float bloomTarget;

    void Awake()
    {
        if (!target) target = transform as RectTransform;
        tScale = normalScale;
        if (target) target.localScale = normalScale;
        if (rotateTarget)
        {
            rotateTarget.localRotation = Quaternion.identity;
            rotateImage = rotateTarget.GetComponent<Image>();
            if (rotateImage) rotateOriginal = rotateImage.color;
        }
        if (border) borderOriginal = border.color;
        if (postVolume && postVolume.profile && postVolume.profile.TryGet(out bloom))
        {
            bloom.intensity.value = bloomNormal;
            bloomTarget = bloomNormal;
        }
    }

    void OnEnable()
    {
        tScale = normalScale;
        if (target) target.localScale = normalScale;
        if (rotateTarget) rotateTarget.localRotation = Quaternion.identity;
        if (rotateImage) rotateImage.color = rotateOriginal;
        if (border) border.color = borderOriginal;
        hovering = false;
        snapTimer = 0f;
        if (bloom != null)
        {
            bloom.intensity.value = bloomNormal;
            bloomTarget = bloomNormal;
        }
    }

    void Update()
    {
        if (target)
        {
            float k = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
            target.localScale = Vector3.Lerp(target.localScale, tScale, k);
        }

        if (rotateTarget)
        {
            if (hovering)
            {
                snapTimer -= Time.unscaledDeltaTime;
                if (snapTimer <= 0f)
                {
                    float z = Random.Range(zJitterRange.x, zJitterRange.y);
                    rotateTarget.localRotation = Quaternion.Euler(0f, 0f, z);
                    snapTimer = Random.Range(snapIntervalRange.x, snapIntervalRange.y);
                }
            }
            else if (rotateTarget.localRotation != Quaternion.identity)
            {
                rotateTarget.localRotation = Quaternion.identity;
            }
        }

        if (bloom != null)
        {
            float kb = 1f - Mathf.Exp(-bloomSpeed * Time.unscaledDeltaTime);
            bloom.intensity.value = Mathf.Lerp(bloom.intensity.value, bloomTarget, kb);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tScale = hoverScale;
        hovering = true;
        snapTimer = 0f;
        if (rotateImage) rotateImage.color = hoverColor;
        if (border) border.color = hoverColor;
        if (bloom != null) bloomTarget = bloomHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tScale = normalScale;
        hovering = false;
        if (rotateImage) rotateImage.color = rotateOriginal;
        if (border) border.color = borderOriginal;
        if (bloom != null) bloomTarget = bloomNormal;
    }
}

﻿using UnityEditor;
using UnityEngine;

public class BlockLightUp : MonoBehaviour
{
    [SerializeField]
    private float m_dimDownTimeInSeconds;

    [SerializeField]
    private float m_flashUpTimeInSeconds;

    [SerializeField]
    private Light m_light;

    [SerializeField]
    private MeshRenderer m_meshRenderer;

    [SerializeField]
    private MeshFilter m_meshFilter;

    [SerializeField]
    private Material m_material;

    [SerializeField]
    private Color m_maxLitUpColor;

    [SerializeField]
    private Color m_minLitUpColor;

    [SerializeField]
    private AnimationCurve m_dimDownCurve;

    private MaterialPropertyBlock m_materialPropertyBlock;

    public float m_normalizedBrightness = 0f;
    private bool isDimmingDown;
    private bool isFlashingUp;

    // Use this for initialization
    private void Start ()
	{
	    m_light.color = m_minLitUpColor;

        m_materialPropertyBlock = new MaterialPropertyBlock();

        m_meshRenderer.GetPropertyBlock(m_materialPropertyBlock);
        m_materialPropertyBlock.SetFloat("_GradientLerp", Mathf.Lerp(-1f, 2f, 0f));
        m_meshRenderer.SetPropertyBlock(m_materialPropertyBlock);
    }

    // Update is called once per frame
    private void Update ()
    {
        if (isDimmingDown)
        {
            m_normalizedBrightness = Mathf.Clamp01(m_normalizedBrightness - (1 / m_dimDownTimeInSeconds) * Time.deltaTime);

            if (m_normalizedBrightness <= 0)
            {
                isDimmingDown = false;
            }
        }
        else if (isFlashingUp)
        {
            m_normalizedBrightness = Mathf.Clamp01(m_normalizedBrightness + (1 / m_flashUpTimeInSeconds) * Time.deltaTime);

            if (m_normalizedBrightness >= 1)
            {
                isFlashingUp = false;
                isDimmingDown = true;
            }
        }

        UpdateBrightness();
    }

    private void UpdateBrightness()
    {
        m_light.color = Color.Lerp(m_minLitUpColor, m_maxLitUpColor, m_normalizedBrightness);

        m_meshRenderer.GetPropertyBlock(m_materialPropertyBlock);
        m_materialPropertyBlock.SetFloat("_GradientLerp", Mathf.Lerp(-1f, 2f, m_normalizedBrightness));
        m_meshRenderer.SetPropertyBlock(m_materialPropertyBlock);
    }

    public void LightUp()
    {
        isFlashingUp = true;
        isDimmingDown = false;
    }
}

[CustomEditor(typeof(BlockLightUp))]
class BlockLightUpEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("LightUp!"))
        {
            BlockLightUp blockLightUp = (BlockLightUp) target;

            if (blockLightUp != null)
            {
                blockLightUp.LightUp();
            }
        }
    }
}
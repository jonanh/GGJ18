﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class LightUpBlock : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Light m_light;

    [SerializeField]
    private MeshRenderer m_meshRenderer;

    [SerializeField]
    private MeshFilter m_meshFilter;

    [SerializeField]
    private Material m_material;

    [SerializeField]
    private Transform m_rotationRoot;

    [SerializeField]
    private Animator m_animator;

    [Header("Gradient")]

    [SerializeField]
    private Color m_maxLitUpColor;

    [SerializeField]
    private Color m_minLitUpColor;

    [SerializeField]
    private Color m_skeletonLightColor;

    [Header("State Times")]

    [SerializeField]
    private List<StateDuration> m_stateDurationsList;

    [Serializable]
    private class StateDuration
    {
        public State m_State;
        public float m_DurationInSeconds;
    }

    [Header("Sounds")]

    [SerializeField]
    private AudioSource m_fadeInFX;

    [SerializeField]
    private AudioSource m_fadeOutFX;

    [SerializeField]
    private AudioSource m_moveFX;

    private MaterialPropertyBlock m_gradientMaterialPropertyBlock;
    private MaterialPropertyBlock m_dissolveMaterialPropertyBlock;

    private float m_activationValue;
    public float ActivationValue { get { return m_activationValue; } }

    private float m_normalizedActiveTimeLeft;
    private float m_normalizedDissolveValue;
    private float m_normalizedDissolvedTimeLeft;
    private float m_normalizedAppearanceTimeLeft;

    private enum State
    {
        Activateable = 0,
        Activating = 1,
        Activate = 2,
        DimmingDown = 3,
        Dissolving = 4,
        Hiding = 5,
        MovingToNewLocation = 6,
        Appearing = 7,
        Dissolved = 8,
        Solidifying = 9
    }

    public bool blockActive(){
        return m_currentState == State.Activating ||
                m_currentState == State.Activate || 
                m_currentState == State.DimmingDown;
    }

    public bool blockActivatable(){
        return m_currentState == State.Activateable;
    }

    [SerializeField]
    private State m_currentState;

    private bool m_canSoundsPlay = true;

    // Use this for initialization
    private void Start ()
	{
        LightTracker.Instance.Transmitters.Add(this);

        m_currentState = State.Activateable;
        m_activationValue = 0f;
        m_normalizedActiveTimeLeft = 0f;
        m_normalizedDissolveValue = 1f;
        m_normalizedDissolvedTimeLeft = 0f;
        m_normalizedAppearanceTimeLeft = 1f;

        m_light.color = m_minLitUpColor;

        m_gradientMaterialPropertyBlock = new MaterialPropertyBlock();

        UpdateBrightness();
	}

    // Update is called once per frame
    private void Update ()
    {
        if (GameoverScreen.Instance != null && GameoverScreen.Instance.gameObject.activeInHierarchy)
        {
            StopAllSFX();
        }

        m_rotationRoot.Rotate(new Vector3(0f, 0.3f, 0.3f));

        switch (m_currentState)
        {
            case State.Activating:

                if (m_canSoundsPlay)
                {
                    if (GameoverScreen.Instance == null || (GameoverScreen.Instance != null && !GameoverScreen.Instance.gameObject.activeInHierarchy))
                    {
                        m_fadeInFX.Play();
                    }
                    m_canSoundsPlay = false;
                }

                m_activationValue = Mathf.Clamp01(m_activationValue + 
                    (1 / GetStateDuration(m_currentState)) * Time.deltaTime);

                if (m_activationValue >= 1)
                {
                    m_normalizedActiveTimeLeft = 1;
                    m_currentState = State.Activate;
                }
                break;
            case State.Activate:

                m_normalizedActiveTimeLeft = Mathf.Clamp01(m_normalizedActiveTimeLeft - 
                    (1 / GetStateDuration(m_currentState)) * Time.deltaTime);

                if (m_normalizedActiveTimeLeft <= 0)
                {
                    m_currentState = State.DimmingDown;
                }

                break;
            case State.DimmingDown:
                m_activationValue = Mathf.Clamp01(m_activationValue - 
                    (1 / GetStateDuration(m_currentState)) * Time.deltaTime);

                if (m_activationValue <= 0)
                {
                    m_normalizedDissolveValue = 1f;
                    m_canSoundsPlay = true;
                    m_currentState = State.Dissolving;
                }

                break;
            case State.Dissolving:

                if (m_canSoundsPlay && m_normalizedDissolveValue <= 0.8f)
                {
                    if (GameoverScreen.Instance == null || (GameoverScreen.Instance != null && !GameoverScreen.Instance.gameObject.activeInHierarchy))
                    {
                        m_fadeOutFX.Play();
                    }
                    
                    m_canSoundsPlay = false;
                }

                m_normalizedDissolveValue = Mathf.Clamp01(m_normalizedDissolveValue - 
                    (1 / GetStateDuration(m_currentState)) * Time.deltaTime);

                if (m_normalizedDissolveValue <= 0)
                {
                    m_normalizedDissolvedTimeLeft = 1f;
                    m_currentState = State.Hiding;
                }

                break;

            case State.Hiding:

                if (!m_animator.GetBool("Hidden"))
                {
                    m_animator.SetBool("Hidden", true);

                    if (GameoverScreen.Instance == null || (GameoverScreen.Instance != null && !GameoverScreen.Instance.gameObject.activeInHierarchy))
                    {
                        m_moveFX.Play();
                    }
                }

                break;

            case State.MovingToNewLocation:

            if(LightCubeLocationOrchestrator.Instance != null){
                LightCubeLocationOrchestrator.Instance.SetToRandomVacantLocation(this);
            }
                m_currentState = State.Appearing;

                break;

            case State.Appearing:

                m_normalizedAppearanceTimeLeft = Mathf.Clamp01(m_normalizedAppearanceTimeLeft -
                    (1 / GetStateDuration(m_currentState)) * Time.deltaTime);

                if (m_normalizedAppearanceTimeLeft <= 0)
                {
                    m_animator.SetBool("Hidden", false);

                    if (GameoverScreen.Instance == null || (GameoverScreen.Instance != null && !GameoverScreen.Instance.gameObject.activeInHierarchy))
                    {
                        m_moveFX.Play();
                    }
                    m_currentState = State.Dissolved;
                    m_canSoundsPlay = true;
                }

                break;

            case State.Dissolved:

                m_normalizedDissolvedTimeLeft = Mathf.Clamp01(m_normalizedDissolvedTimeLeft - 
                    (1 / GetStateDuration(m_currentState)) * Time.deltaTime);

                if (m_normalizedDissolvedTimeLeft <= 0)
                {
                    m_currentState = State.Solidifying;
                }

                break;
            case State.Solidifying:
                m_normalizedDissolveValue = Mathf.Clamp01(m_normalizedDissolveValue +
                   (1 / GetStateDuration(m_currentState)) * Time.deltaTime);

                if (m_normalizedDissolveValue >= 1)
                {
                    m_currentState = State.Activateable;
                }

                break;
        }

        UpdateBrightness();
    }

    private void StopAllSFX()
    {
        if (m_fadeInFX.isPlaying)
        {
            m_fadeInFX.Stop();
        }

        if (m_fadeOutFX.isPlaying)
        {
            m_fadeOutFX.Stop();
        }

        if (m_moveFX.isPlaying)
        {
            m_moveFX.Stop();
        }
    }

    private void UpdateBrightness()
    {
        if (m_currentState == State.Dissolving || m_currentState == State.Dissolved || m_currentState == State.Hiding ||
            m_currentState == State.MovingToNewLocation || m_currentState == State.Appearing || m_currentState == State.Solidifying)
        {
            m_light.color = Color.Lerp(m_skeletonLightColor, m_minLitUpColor, m_normalizedDissolveValue);
        }
        else
        {
            m_light.color = Color.Lerp(m_minLitUpColor, m_maxLitUpColor, m_activationValue);
        }

        m_meshRenderer.GetPropertyBlock(m_gradientMaterialPropertyBlock);
        m_gradientMaterialPropertyBlock.SetFloat("_GradientLerp", Mathf.Lerp(-1f, 2f, m_activationValue));
        m_meshRenderer.SetPropertyBlock(m_gradientMaterialPropertyBlock);

        m_meshRenderer.GetPropertyBlock(m_gradientMaterialPropertyBlock);
        m_gradientMaterialPropertyBlock.SetFloat("_DissolveValue", m_normalizedDissolveValue);
        m_meshRenderer.SetPropertyBlock(m_gradientMaterialPropertyBlock);
    }

    public void Activate()
    {
        if (m_currentState != State.Activateable)
        {
            return;
        }

        m_activationValue = 0f;
        m_currentState = State.Activating;
    }

    private float GetStateDuration(State state)
    {
        StateDuration foundStateDuration = m_stateDurationsList.Find(stateDuration => stateDuration.m_State == state);

        if (foundStateDuration == null)
        {
            Debug.LogError(string.Format("StateDuration of state {0} is missing!", state));
        }

        return foundStateDuration != null ? foundStateDuration.m_DurationInSeconds : 0f;
    }

    /// <summary>
    /// Called by animation.
    /// </summary>
    public void Hidden()
    {
        m_currentState = State.MovingToNewLocation;
    }
}


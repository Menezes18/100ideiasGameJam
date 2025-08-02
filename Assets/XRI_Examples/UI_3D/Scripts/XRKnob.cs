using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// Componente que representa um botão giratório (knob) interativo em XR.
    /// Permite ser girado por um controlador e envia eventos com base no valor girado.
    /// </summary>
    public class XRKnob : XRBaseInteractable
    {
        const float k_ModeSwitchDeadZone = 0.1f;

        // Classe auxiliar que rastreia rotação acumulada
        struct TrackedRotation
        {
            float m_BaseAngle;
            float m_CurrentOffset;
            float m_AccumulatedAngle;

            public float totalOffset => m_AccumulatedAngle + m_CurrentOffset;

            public void Reset()
            {
                m_BaseAngle = 0f;
                m_CurrentOffset = 0f;
                m_AccumulatedAngle = 0f;
            }

            public void SetBaseFromVector(Vector3 direction)
            {
                m_AccumulatedAngle += m_CurrentOffset;
                m_BaseAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                m_CurrentOffset = 0f;
            }

            public void SetTargetFromVector(Vector3 direction)
            {
                var targetAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                m_CurrentOffset = ShortestAngleDistance(m_BaseAngle, targetAngle, 360f);

                if (Mathf.Abs(m_CurrentOffset) > 90f)
                {
                    m_BaseAngle = targetAngle;
                    m_AccumulatedAngle += m_CurrentOffset;
                    m_CurrentOffset = 0f;
                }
            }
        }

        [Serializable]
        public class ValueChangeEvent : UnityEvent<float> { }

        [Header("Referência")]
        [SerializeField] Transform m_Handle = null;

        [Header("Configuração de valor")]
        [Range(0f, 1f)]
        [SerializeField] float m_Value = 0.5f;

        [Header("Limites e Incrementos")]
        [SerializeField] bool m_ClampedMotion = true;
        [SerializeField] float m_MaxAngle = 90f;
        [SerializeField] float m_MinAngle = -90f;
        [SerializeField] float m_AngleIncrement = 0f;

        [Header("Configuração de detecção")]
        [SerializeField] float m_PositionTrackedRadius = 0.1f;
        [SerializeField] float m_TwistSensitivity = 1.5f;

        [Header("Eventos")]
        [SerializeField] ValueChangeEvent m_OnValueChange = new ValueChangeEvent();

        // NOVOS EVENTOS
        [SerializeField] UnityEvent m_OnMinValueReached = new UnityEvent();
        [SerializeField] UnityEvent m_OnMaxValueReached = new UnityEvent();

        IXRSelectInteractor m_Interactor;

        bool m_PositionDriven = false;
        bool m_UpVectorDriven = false;

        TrackedRotation m_PositionAngles = new TrackedRotation();
        TrackedRotation m_UpVectorAngles = new TrackedRotation();
        TrackedRotation m_ForwardVectorAngles = new TrackedRotation();

        float m_BaseKnobRotation = 0f;

        // GETTERS
        public Transform handle { get => m_Handle; set => m_Handle = value; }
        public float value
        {
            get => m_Value;
            set
            {
                SetValue(value);
                SetKnobRotation(ValueToRotation());
            }
        }
        public bool clampedMotion { get => m_ClampedMotion; set => m_ClampedMotion = value; }
        public float maxAngle { get => m_MaxAngle; set => m_MaxAngle = value; }
        public float minAngle { get => m_MinAngle; set => m_MinAngle = value; }
        public float positionTrackedRadius { get => m_PositionTrackedRadius; set => m_PositionTrackedRadius = value; }

        public ValueChangeEvent onValueChange => m_OnValueChange;
        public UnityEvent onMinValueReached => m_OnMinValueReached;
        public UnityEvent onMaxValueReached => m_OnMaxValueReached;

        void Start()
        {
            SetValue(m_Value);
            SetKnobRotation(ValueToRotation());
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(StartGrab);
            selectExited.AddListener(EndGrab);
        }

        protected override void OnDisable()
        {
            selectEntered.RemoveListener(StartGrab);
            selectExited.RemoveListener(EndGrab);
            base.OnDisable();
        }

        void StartGrab(SelectEnterEventArgs args)
        {
            m_Interactor = args.interactorObject;

            m_PositionAngles.Reset();
            m_UpVectorAngles.Reset();
            m_ForwardVectorAngles.Reset();

            UpdateBaseKnobRotation();
            UpdateRotation(true);
        }

        void EndGrab(SelectExitEventArgs args)
        {
            m_Interactor = null;
        }

        public const float threshold = 0.9f;
        private void Update()
        {
            if (m_Value > threshold && !m_HasFiredAboveThreshold)
            {
                Debug.LogError("AAA");
                m_OnAboveThreshold.Invoke();
                m_HasFiredAboveThreshold = true;
            }
            else if (m_Value <= threshold && m_HasFiredAboveThreshold)
            {
                // Reseta para permitir disparo novamente numa próxima subida
                m_HasFiredAboveThreshold = false;
            }
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
                UpdateRotation();
        }

        void UpdateRotation(bool freshCheck = false)
        {
            var interactorTransform = m_Interactor.GetAttachTransform(this);

            var localOffset = transform.InverseTransformVector(interactorTransform.position - m_Handle.position);
            localOffset.y = 0f;
            var radiusOffset = transform.TransformVector(localOffset).magnitude;
            localOffset.Normalize();

            var localForward = transform.InverseTransformDirection(interactorTransform.forward);
            var localY = Math.Abs(localForward.y);
            localForward.y = 0f;
            localForward.Normalize();

            var localUp = transform.InverseTransformDirection(interactorTransform.up);
            localUp.y = 0f;
            localUp.Normalize();

            if (m_PositionDriven && !freshCheck)
                radiusOffset *= (1f + k_ModeSwitchDeadZone);

            if (radiusOffset >= m_PositionTrackedRadius)
            {
                if (!m_PositionDriven || freshCheck)
                {
                    m_PositionAngles.SetBaseFromVector(localOffset);
                    m_PositionDriven = true;
                }
            }
            else
                m_PositionDriven = false;

            if (!freshCheck)
                localY *= m_UpVectorDriven ? (1f + (k_ModeSwitchDeadZone * 0.5f)) : (1f - (k_ModeSwitchDeadZone * 0.5f));

            if (localY > 0.707f)
            {
                if (!m_UpVectorDriven || freshCheck)
                {
                    m_UpVectorAngles.SetBaseFromVector(localUp);
                    m_UpVectorDriven = true;
                }
            }
            else
            {
                if (m_UpVectorDriven || freshCheck)
                {
                    m_ForwardVectorAngles.SetBaseFromVector(localForward);
                    m_UpVectorDriven = false;
                }
            }

            if (m_PositionDriven)
                m_PositionAngles.SetTargetFromVector(localOffset);

            if (m_UpVectorDriven)
                m_UpVectorAngles.SetTargetFromVector(localUp);
            else
                m_ForwardVectorAngles.SetTargetFromVector(localForward);

            var knobRotation = m_BaseKnobRotation - ((m_UpVectorAngles.totalOffset + m_ForwardVectorAngles.totalOffset) * m_TwistSensitivity) - m_PositionAngles.totalOffset;
            if (m_ClampedMotion)
                knobRotation = Mathf.Clamp(knobRotation, m_MinAngle, m_MaxAngle);

            SetKnobRotation(knobRotation);

            var knobValue = (knobRotation - m_MinAngle) / (m_MaxAngle - m_MinAngle);
            SetValue(knobValue);
        }

        void SetKnobRotation(float angle)
        {
            if (m_AngleIncrement > 0)
            {
                var normalizeAngle = angle - m_MinAngle;
                angle = Mathf.Round(normalizeAngle / m_AngleIncrement) * m_AngleIncrement + m_MinAngle;
            }

            if (m_Handle != null)
                m_Handle.localEulerAngles = new Vector3(0f, angle, 0f);
        }
        [SerializeField] UnityEvent       m_OnAboveThreshold    = new UnityEvent();
        
        bool m_HasFiredAboveThreshold = false;
        void SetValue(float value)
        {
            if (m_ClampedMotion)
                value = Mathf.Clamp01(value);

            if (m_AngleIncrement > 0)
            {
                var angleRange = m_MaxAngle - m_MinAngle;
                var angle = Mathf.Lerp(0f, angleRange, value);
                angle = Mathf.Round(angle / m_AngleIncrement) * m_AngleIncrement;
                value = Mathf.InverseLerp(0f, angleRange, angle);
            }

            
        }

        float ValueToRotation()
        {
            return m_ClampedMotion ? Mathf.Lerp(m_MinAngle, m_MaxAngle, m_Value) : Mathf.LerpUnclamped(m_MinAngle, m_MaxAngle, m_Value);
        }

        void UpdateBaseKnobRotation()
        {
            m_BaseKnobRotation = Mathf.LerpUnclamped(m_MinAngle, m_MaxAngle, m_Value);
        }

        static float ShortestAngleDistance(float start, float end, float max)
        {
            var angleDelta = end - start;
            var angleSign = Mathf.Sign(angleDelta);
            angleDelta = Math.Abs(angleDelta) % max;
            if (angleDelta > (max * 0.5f))
                angleDelta = -(max - angleDelta);
            return angleDelta * angleSign;
        }

        void OnDrawGizmosSelected()
        {
            if (m_PositionTrackedRadius <= Mathf.Epsilon)
                return;

            const int k_CircleSegments = 16;
            const float k_SegmentRatio = 1f / k_CircleSegments;
            var circleCenter = m_Handle != null ? m_Handle.position : transform.position;
            var circleX = transform.right;
            var circleY = transform.forward;

            Gizmos.color = Color.green;
            for (int i = 0; i < k_CircleSegments; i++)
            {
                float a1 = i * k_SegmentRatio * 2f * Mathf.PI;
                float a2 = (i + 1) * k_SegmentRatio * 2f * Mathf.PI;
                Gizmos.DrawLine(
                    circleCenter + (Mathf.Cos(a1) * circleX + Mathf.Sin(a1) * circleY) * m_PositionTrackedRadius,
                    circleCenter + (Mathf.Cos(a2) * circleX + Mathf.Sin(a2) * circleY) * m_PositionTrackedRadius
                );
            }
        }

        void OnValidate()
        {
            if (m_ClampedMotion)
                m_Value = Mathf.Clamp01(m_Value);

            if (m_MinAngle > m_MaxAngle)
                m_MinAngle = m_MaxAngle;

            SetKnobRotation(ValueToRotation());
        }
    }
}

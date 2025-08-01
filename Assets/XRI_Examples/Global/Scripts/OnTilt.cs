using UnityEngine.Events;

namespace UnityEngine.XR.Content.Interaction
{
    public class OnTilt : MonoBehaviour
    {
        const float k_AngleBuffer = 0.05f;

        [SerializeField]
        [Tooltip("Tilt range, 0 - 180 degrees.")]
        [Range(k_AngleBuffer * 2f, (1 - k_AngleBuffer * 2f))]
        float m_Threshold = 0.5f;

        [SerializeField]
        [Tooltip("The transform to check for tilt. Will default to this object if not set.")]
        Transform m_Target;

        [SerializeField]
        [Tooltip("The transform to get as the source of the 'up' direction. Will default to world up if not set.")]
        Transform m_UpSource;

        [SerializeField]
        [Tooltip("Event to trigger when tilting goes over the threshold.")]
        UnityEvent m_OnBegin = new UnityEvent();

        [SerializeField]
        [Tooltip("Event to trigger when tilting returns from the threshold.")]
        UnityEvent m_OnEnd = new UnityEvent();
        
        public UnityEvent onBegin => m_OnBegin;
        public UnityEvent onEnd => m_OnEnd;

        bool m_WithinThreshold;

        void Update()
        {
            CheckOrientation();
        }

        void CheckOrientation()
        {
            var targetUp = m_Target != null ? m_Target.up : transform.up;
            var baseUp = m_UpSource != null ? m_UpSource.up : Vector3.up;

            var similarity = Vector3.Dot(-targetUp, baseUp);
            similarity = Mathf.InverseLerp(-1, 1, similarity);

            if (m_WithinThreshold)
                similarity += k_AngleBuffer;
            else
                similarity -= k_AngleBuffer;

            var thresholdCheck = (similarity >= m_Threshold);

            if (m_WithinThreshold != thresholdCheck)
            {
                m_WithinThreshold = thresholdCheck;

                if (m_WithinThreshold)
                {
                    m_OnBegin.Invoke();
                }
                else
                {
                    m_OnEnd.Invoke();
                }
            }
        }
    }
}
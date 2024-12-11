using UnityEngine;

namespace Main
{
    [System.Serializable]
    public class CarSuspension
    {
        [field: Header("Spring")]
        [field: SerializeField] public float SpringRate { get; private set; } // twardosc sprezyny
        [field: SerializeField] public float SpringDamping { get; private set; } // tlumienie wibracji sprezyny
        [field: SerializeField] public float SpringHeight { get; private set; }
        [field: SerializeField] public float SpringLength { get; private set; }
        
        [field: Header("Angles")]
        [field: SerializeField] public float ToeInAngle { get; private set; } // zbieznosc kol (in degreees)
        [field: SerializeField] public float CasterAngle { get; private set; } // (in degreees)
        // [field: SerializeField] public float CentreOfTurningAngle { get; private set; }

        public Vector3 LocalRaycastStartPoint { get; private set; }
        public Vector3 LocalRaycastDirection { get; private set; }
        
        public float SpringCompressionLastFrame { get; set; }

        public void Setup(Transform carTransform, Transform wheelOriginTransform)
        {
            LocalRaycastStartPoint = carTransform.InverseTransformPoint(wheelOriginTransform.position);
            LocalRaycastDirection = Vector3.down;
        }
    }
}
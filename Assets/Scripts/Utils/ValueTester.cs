using UnityEngine;

namespace Utils
{
    public class ValueTester : MonoBehaviour
    {
        public float testValue = 0f;
        
        public static float testValueStatic = 0f;

        private void Update()
        {
            testValueStatic = testValue;
        }
    }
}
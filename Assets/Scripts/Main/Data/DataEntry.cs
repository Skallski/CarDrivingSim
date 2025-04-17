namespace Main.Data
{
    [System.Serializable]
    public class DataEntry
    {
        public string Timestamp;
        public bool IsEngineIgnited;
        public float ThrottlePedalInput;
        public float BrakePedalInput;
        public float ClutchPedalInput;
        public float SteeringWheelInput;
        public int Gear;
        public float Speed;
        public float Rpm;

        public DataEntry(string timestamp, bool isEngineIgnited, float throttlePedalInput, float brakePedalInput,
            float clutchPedalInput, float steeringWheelInput, int gear, float speed, float rpm)
        {
            Timestamp = timestamp;
            IsEngineIgnited = isEngineIgnited;
            ThrottlePedalInput = throttlePedalInput;
            BrakePedalInput = brakePedalInput;
            ClutchPedalInput = clutchPedalInput;
            SteeringWheelInput = steeringWheelInput;
            Gear = gear;
            Speed = speed;
            Rpm = rpm;
        }
    }
}
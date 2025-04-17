using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Main.Car;
using UnityEngine;

namespace Main.Data
{
    public class DataSaver : MonoBehaviour
    {
        [SerializeField] private CarInput _carInput;
        [SerializeField] private CarController _carController;

        [Space] [SerializeField] private DataCollection _dataCollection = new DataCollection();

        private string _filePath;

        private bool _isSavingThreadRunning;
        private Thread _savingThread;
        private readonly object _lockObject = new object();
        private readonly ConcurrentQueue<DataEntry> _dataQueue = new ConcurrentQueue<DataEntry>();

        private const float SAVE_INTERVAL_SECONDS = 5f;
        private const int SAVE_INTERVAL_MILLISECONDS = 6000;

        private void Start()
        {
            string fileName = $"Session{System.DateTime.Now:yyyy.MM.dd-HH:mm}.json";
            _filePath = $"{Application.dataPath}/SessionsData/{fileName}";
            
            _dataCollection = new DataCollection();

            InvokeRepeating(nameof(CollectData), 0f, SAVE_INTERVAL_SECONDS);
            StartThread();
        }
        
        private void OnDestroy()
        {
            StopThread();
        }

        private void OnApplicationQuit()
        {
            StopThread();
        }

        private void CollectData()
        {
            DataEntry newEntry = new DataEntry
            (
                $"[{System.DateTime.Now:HH:mm:ss}]",
                _carController.IsEngineIgnited(),
                _carInput.ThrottleInput,
                _carInput.BrakeInput,
                _carInput.ClutchInput,
                _carInput.SteeringWheelInput,
                _carController.GetCurrentGear(),
                _carController.GetSpeed(),
                _carController.GetEngineRpm()
            );

            _dataQueue.Enqueue(newEntry);
        }

        private void StartThread()
        {
            _savingThread = new Thread(BackgroundThreadLoop)
            {
                IsBackground = true
            };

            _isSavingThreadRunning = true;
            _savingThread.Start();
        }

        private void StopThread()
        {
            if (_isSavingThreadRunning == false)
            {
                return;
            }

            _isSavingThreadRunning = false;

            if (_savingThread is { IsAlive: true })
            {
                _savingThread.Join(100);
            }
        }

        private void BackgroundThreadLoop()
        {
            while (_isSavingThreadRunning)
            {
                Thread.Sleep(SAVE_INTERVAL_MILLISECONDS);

                bool hasData = false;

                lock (_lockObject)
                {
                    while (_dataQueue.TryDequeue(out DataEntry entry))
                    {
                        _dataCollection.Entries.Add(entry);
                        hasData = true;
                    }

                    if (hasData)
                    {
                        string json = JsonUtility.ToJson(_dataCollection, true);
                        File.WriteAllText(_filePath, json);
                        Debug.Log("<color=green>new data saved</color>");
                    }
                }
            }
        }
    }
}
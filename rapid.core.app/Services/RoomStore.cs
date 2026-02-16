using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;


namespace rapid.core.app.Services
{
    public static class RoomStore
    {
        private static readonly object _lock = new();
        private static int _totalRooms = 5; // default; change via Seed
        private static readonly HashSet<string> _occupiedPatientIds = new();

        public static void Configure(int totalRooms)
        {
            if (totalRooms < 0) totalRooms = 0;
            lock (_lock)
            {
                _totalRooms = totalRooms;
                // If fewer rooms than occupied, keep occupied; simulation can release later.
            }
        }

        public static int TotalRooms
        {
            get { lock (_lock) { return _totalRooms; } }
        }

        public static int OccupiedCount
        {
            get { lock (_lock) { return _occupiedPatientIds.Count; } }
        }

        public static int AvailableCount
        {
            get { lock (_lock) { return Math.Max(0, _totalRooms - _occupiedPatientIds.Count); } }
        }

        public static bool TryAssign(string patientId)
        {
            lock (_lock)
            {
                if (_occupiedPatientIds.Contains(patientId)) return true; // already assigned
                if (_occupiedPatientIds.Count >= _totalRooms) return false;
                _occupiedPatientIds.Add(patientId);
                return true;
            }
        }

        public static void Release(string patientId)
        {
            lock (_lock)
            {
                _occupiedPatientIds.Remove(patientId);
            }
        }

        public static bool IsAssigned(string patientId)
        {
            lock (_lock)
            {
                return _occupiedPatientIds.Contains(patientId);
            }
        }
    }
}


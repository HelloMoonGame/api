using System;

namespace Common.Domain.SharedKernel
{
    public static class SystemClock
    {
        private static DateTime? _customDateTime;

        public static DateTime Now => _customDateTime ?? DateTime.UtcNow;

        public static void Set(DateTime customDateTime) => _customDateTime = customDateTime;

        public static void Reset() => _customDateTime = null;
    }
}
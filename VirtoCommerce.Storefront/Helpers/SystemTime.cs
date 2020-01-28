using System;

namespace VirtoCommerce.Storefront.Helpers
{
    public static class SystemTime
    {
        /// <summary> Normally this is a pass-through to DateTime.Now,
        /// but it can be overridden with SetDateTimeNow( .. ) for testing or debugging.
        /// </summary>
        public static DateTime Now => FuncGetNow();

        /// <summary> Normally this is a pass-through to DateTime.Now,
        /// but it can be overridden with SetDateTimeToday( .. ) for testing or debugging.
        /// </summary>
        public static DateTime Today => FuncGetToday();

        private static bool fake = false;
        private static Func<DateTime> FuncGetNow = () => DateTime.Now;
        private static Func<DateTime> FuncGetToday = () => DateTime.Today;

        /// <summary> Set time to return when SystemTime.Now is called and SystemTime.Today
        /// </summary>
        public static void SetDateTime(DateTime dateTimeNow)
        {
            fake = true;
            FuncGetNow = () => dateTimeNow;
            FuncGetToday = () => dateTimeNow.Date;
        }

        /// <summary> Resets SystemTime.Now() and SystemTime.Today()
        /// </summary>
        public static void ResetDateTime()
        {
            if (!fake)
                return;
            FuncGetNow = () => DateTime.Now;
            FuncGetToday = () => DateTime.Today;
        }
    }
}

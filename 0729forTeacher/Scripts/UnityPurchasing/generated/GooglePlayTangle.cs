// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("tEAznp2gxqkpfQzikMcS5sBJySbVp1k5QGf6PXrcejQ4Z07F9hOYJjA/n9MhK8CFObyjT9U28zkChrm+YdFL0+lrX7azkm7VmhFEzHB2jcUmxAY/9AbPVJ9VCL6UsuBerm+8R2Loj0NQ1UOPs6RJIPF28zMZumlj5GdpZlbkZ2xk5GdnZrM9hCVK8IpHaG6l5As+kC4auoEiLrgrBm0qchyXno2pKuTihWy4OvApBa04/medW/fzVf4mq3OMghkwMYdLNrQWV5RW5GdEVmtgb0zgLuCRa2dnZ2NmZc3b2Xcrj8cmkzn9COeHBhESE7dMW1C5nxAmNy/ttV0YaYE59X+RK0mDgNsJx4PgeBfVu8npavuk6nnEF/Kp5vLDrfDcHWRlZ2Zn");
        private static int[] order = new int[] { 10,12,6,5,6,5,12,11,12,13,10,11,13,13,14 };
        private static int key = 102;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

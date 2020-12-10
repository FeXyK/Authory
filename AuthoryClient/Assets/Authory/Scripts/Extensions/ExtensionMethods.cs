using UnityEngine;

namespace Assets.Authory.Scripts
{
    public static class ExtensionMethods 
    {
        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
    }
}

namespace jp.lilxyzw.shadercore
{
    public static class SCL10n
    {
        public static void Load(string uniqueID = null) => L10n.Load(uniqueID);
        public static string L(string key) => L10n.L(key);
    }
}

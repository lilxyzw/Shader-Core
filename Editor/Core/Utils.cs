namespace jp.lilxyzw.shadercore
{
    public static class Utils
    {
        public static string GetDirectory(string path)
        {
            int i = path.Length-1;
            for (; i > 0; i--)
                if (path[i] == '/' || path[i] == '\\') break;
            return path[..(i+1)];
        }

        public static string GetFileName(string path)
        {
            int i = path.Length-1;
            for (; i > 0; i--)
                if (path[i] == '/' || path[i] == '\\') break;
            return path[(i+1)..];
        }

        public static string RemoveExtension(string path)
        {
            int i = path.Length-1;
            for (; i > 0; i--)
                if (path[i] == '.') break;
            return path[..i];
        }

        public static string GetName(string path)
        {
            return RemoveExtension(GetFileName(path));
        }
    }
}

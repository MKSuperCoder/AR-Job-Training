using System;
using System.IO;
using System.Collections.Generic;

public static class EnvLoader
{
    public static Dictionary<string, string> LoadEnv(string path)
    {
        var dict = new Dictionary<string, string>();

        if (!File.Exists(path))
        {
            UnityEngine.Debug.LogWarning($".env file not found at path: {path}");
            return dict;
        }

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split(new[] { '=' }, 2);
            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                dict[key] = value;
            }
        }

        return dict;
    }
}

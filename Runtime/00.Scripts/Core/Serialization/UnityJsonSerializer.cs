using UnityEngine;

namespace FAMOZ.ExternalProgram.Core.Serialization
{
    public class UnityJsonSerializer : IJsonSerializer
    {
        public string Serialize<T>(T obj)
        {
            return JsonUtility.ToJson(obj, true);
        }

        public T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
} 
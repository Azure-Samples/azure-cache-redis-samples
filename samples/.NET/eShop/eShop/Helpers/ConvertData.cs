using eShop.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace eShop.Helpers
{
    public static class ConvertData<T>
    {
        public static List<T> ByteArrayToObjectList(byte[] inputByteArray)
        {
            var deserializedList = JsonSerializer.Deserialize<List<T>>(inputByteArray);
            return deserializedList;
        }

        public static byte[] ObjectListToByteArray(List<T> inputList)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(inputList);

            return bytes;
        }

        public static T ByteArrayToObject(byte[] inputByteArray)
        {
            var deserializedList = JsonSerializer.Deserialize<T>(inputByteArray);
            return deserializedList;
        }

        public static byte[] ObjectToByteArray(T input)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(input);

            return bytes;
        }

        public static List<T> StringToObjectList(string inputString)
        {
            var deserializedList = JsonSerializer.Deserialize<List<T>>(inputString);
            return deserializedList;
        }

        public static string ObjectListToString(List<T> inputList)
        {
            var _returnString = JsonSerializer.Serialize(inputList);

            return _returnString;
        }

        public static T StringToObject(string inputString)
        {
            var deserializedList = JsonSerializer.Deserialize<T>(inputString);
            return deserializedList;
        }

        public static string ObjectToString(T input)
        {
            var _returnString = JsonSerializer.Serialize(input);

            return _returnString;
        }
    }
}

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DocumentGo
{
    public static class ConvertUtil
    {
        /// <summary>
        /// 将对象转换为byte数组
        /// </summary>
        /// <param name="obj">被转换对象</param>
        /// <returns>转换后byte数组</returns>
        public static byte[] ToBytes(this object obj)
        {
            byte[] buff;
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter iFormatter = new BinaryFormatter();
                iFormatter.Serialize(ms, obj);
                buff = ms.GetBuffer();
            }
            return buff;
        }

        /// <summary>
        /// 将byte数组转换成对象
        /// </summary>
        /// <param name="bytes">被转换byte数组</param>
        /// <returns>转换完成后的对象</returns>
        public static T ToObject<T>(this byte[] bytes)
        {
            object obj;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                IFormatter iFormatter = new BinaryFormatter();
                obj = iFormatter.Deserialize(ms);
            }
            return (T)obj;
        }

        ///// <summary>
        ///// 将对象转换为byte数组
        ///// </summary>
        ///// <param name="obj">被转换对象</param>
        ///// <returns>转换后byte数组</returns>
        //public static byte[] ToBytes(this object obj)
        //{
        //    byte[] buff = new byte[Marshal.SizeOf(obj)];
        //    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buff, 0);
        //    Marshal.StructureToPtr(obj, ptr, true);
        //    return buff;
        //}

        ///// <summary>
        ///// 将byte数组转换成对象
        ///// </summary>
        ///// <param name="buff">被转换byte数组</param>
        ///// <param name="typ">转换成的类名</param>
        ///// <returns>转换完成后的对象</returns>
        //public static T ToObject<T>(this byte[] buff)
        //{
        //    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buff, 0);
        //    return (T)Marshal.PtrToStructure(ptr, typeof(T));
        //}

        // <summary>
        /// 将文件转换为byte数组
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] ToBytes(this string path)
        {
            if (!File.Exists(path))
            {
                return new byte[0];
            }

            FileInfo fileInfo = new FileInfo(path);
            byte[] buff = new byte[fileInfo.Length];

            FileStream fs = fileInfo.OpenRead();
            fs.Read(buff, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            return buff;
        }

        /// <summary>
        /// 将byte数组转换为文件并保存到指定地址
        /// </summary>
        /// <param name="buff">byte数组</param>
        /// <param name="savepath">保存地址</param>
        public static void ToFile(this byte[] buff, string savepath)
        {
            if (File.Exists(savepath))
            {
                File.Delete(savepath);
            }

            using (FileStream fs = new FileStream(savepath, FileMode.CreateNew))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(buff, 0, buff.Length);
            }
        }
    }
}
using System.Threading;
using System;
using System.IO;
using System.Text;

namespace SimpleUdpReciever
{
    /// <summary>
    /// 数据处理
    /// </summary>
    class DataReservoir
    {
        private Thread persistenceThread;
        private byte[] receiveBuffer = new byte[1024*1024*100];//缓存100MB
        private int startIndex = 0;
        private int endIndex = 0;
        private FileStream fileStream;
        private const string path = "d:/data.txt";

        #region 构造函数
        public DataReservoir()
        {
            this.InitFile();
            this.persistenceThread = new Thread(new ThreadStart(ProcessingData));
            this.persistenceThread.Start();
        }
        #endregion

        private void InitFile()
        {

            //每次删除文件重新来
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (!File.Exists(path))
            {
                fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);//创建写入文件
                fileStream.Close();
            }
        }

        /// <summary>
        /// 添加数据入缓冲区
        /// </summary>
        /// <param name="getDataByte">待添加数据</param>
        /// <param name="copyStartIndex">拷贝待添加数据起始位</param>
        /// <param name="length">拷贝待添加数据长度</param>
        /// <returns></returns>
        public void AddProcessingDataByte(byte[] getDataByte, int copyStartIndex, int length)
        {
            try
            {

                int partLength = this.receiveBuffer.Length - this.endIndex - 1;//receiveBuffer.Length 最大值1024，endIndex最大值1023
                //考虑如果超过缓冲的情况
                if (length > partLength)
                {
                    this.AddProcessingDataByte(getDataByte, 0, partLength);
                    this.AddProcessingDataByte(getDataByte, partLength, getDataByte.Length - partLength);
                }
                else
                {
                    Array.Copy(getDataByte, copyStartIndex, this.receiveBuffer, this.endIndex, length);
                    this.endIndex += length;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("添加出错" + e.ToString());
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="getDataByte"></param>
        private void ProcessingData()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (endIndex <= startIndex)
                {
                    continue;
                }

                try
                {
                    /**
                    FileStream fs = File.Open(path, FileMode.Open, FileAccess.Write);
                    //设定书写的开始位置为文件的末尾  
                    fs.Position = fs.Length+1;
                    string tempString = Encoding.Default.GetString(this.receiveBuffer, startIndex, endIndex - startIndex);

                    BinaryWriter bw = new BinaryWriter(fs); //创建BinaryWriter对象
                    //开始写入
                    bw.Write(tempString);
                    //清空缓冲区
                    bw.Flush();
                    //关闭流
                    bw.Close();
                    fs.Close();

                    startIndex = 0;
                    endIndex = 0;
                     * 
                     * */

                    FileStream fs = File.Open(path, FileMode.Open, FileAccess.Write);
                    //设定书写的开始位置为文件的末尾  
                    fs.Position = fs.Length + 1;
                    fs.Write(this.receiveBuffer, startIndex, endIndex - startIndex);

                    fs.Flush();
                    fs.Close();
                    startIndex = 0;
                    endIndex = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("处理数据"+e);
                }
            }
        }

        public void Dispose()
        {
            fileStream.Close();
        }

    }
}

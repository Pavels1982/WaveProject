using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VoiceСhanging.Models;

namespace VoiceСhanging.Service
{
    public static class WaveFileReader
    {
        public static HeaderWavModel Header = new HeaderWavModel();
     
        public static List<short> GetWaveData(string inFile)
        {
            short[] data;
            byte[] wave;
            byte[] m_FmtID = new byte[4];
            System.IO.FileStream WaveFile = System.IO.File.OpenRead(inFile);
            wave = new byte[WaveFile.Length];

            #region ReadHeader
            var headerSize = Marshal.SizeOf(Header);
            var buffer = new byte[headerSize];
            WaveFile.Read(buffer, 0, headerSize);
            // Чтобы не считывать каждое значение заголовка по отдельности,
            // воспользуемся выделением unmanaged блока памяти
            var headerPtr = Marshal.AllocHGlobal(headerSize);
            // Копируем считанные байты из файла в выделенный блок памяти
            Marshal.Copy(buffer, 0, headerPtr, headerSize);
            // Преобразовываем указатель на блок памяти к нашей структуре
            Marshal.PtrToStructure(headerPtr, Header);
            Marshal.FreeHGlobal(headerPtr);
            #endregion

            data = new short[(wave.Length - 44) / 2]; // Смещение байтов в wav-файле;
            WaveFile.Read(wave, 0, Convert.ToInt32(WaveFile.Length)); // чтение wav-файла в массив

            /***********Конвертация и PCM учёт***************/
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (BitConverter.ToInt16(wave, 44 + i * 2));
            }

            //StreamWriter sw = new StreamWriter(@"Массив данных в цифровом виде.txt", false);

            //foreach (double d in data)
            //{
            //    sw.WriteLine(d);
            //}
            //sw.Close();
            return data.ToList();
        }


        public static void SetShortToBuffer(short val, byte[] outArray, int Offset)
        {
            outArray[Offset] = (byte)(val & 0x00FF);
            Offset++;
            outArray[Offset] = (byte)((val >> 8) & 0x00FF);
        }

        public static byte[] ConvertShortArray(short[] Data, int Offset, int Count)
        {
            byte[] helper = new byte[Count * sizeof(short)];

            int end = Offset + Count;
            int io = 0;
            for (int i = Offset; i < end; i++)
            {
                SetShortToBuffer(Data[i], helper, io);
                io += sizeof(short);
            }

            return helper;
        }

        //public static byte[] CreateWaveFileHeader(int SizeOfData, short ChannelCount, uint SamplesPerSecond, short BitsPerSample)
        //{

        //    short BlockAlign = (short)(ChannelCount * (BitsPerSample / 8));
        //    uint AverageBytesPerSecond = (uint)(SamplesPerSecond * BlockAlign);

        //    List<byte> pom = new List<byte>();
        //    pom.AddRange(ASCIIEncoding.ASCII.GetBytes("RIFF"));
        //    pom.AddRange(BitConverter.GetBytes(SizeOfData + 36)); //Size + up to data
        //    pom.AddRange(ASCIIEncoding.ASCII.GetBytes("WAVEfmt "));
        //    pom.AddRange(BitConverter.GetBytes(((uint)16))); //16 For PCM
        //    pom.AddRange(BitConverter.GetBytes(((short)1))); //PCM FMT
        //    pom.AddRange(BitConverter.GetBytes(((short)ChannelCount)));
        //    pom.AddRange(BitConverter.GetBytes((uint)SamplesPerSecond));
        //    pom.AddRange(BitConverter.GetBytes((uint)AverageBytesPerSecond));
        //    pom.AddRange(BitConverter.GetBytes((short)BlockAlign));
        //    pom.AddRange(BitConverter.GetBytes((short)BitsPerSample));
        //    pom.AddRange(ASCIIEncoding.ASCII.GetBytes("data"));
        //    pom.AddRange(BitConverter.GetBytes(SizeOfData));

        //    return pom.ToArray();
        //}



        public static void WriteWaveFile(short[] data)
        {
            byte[] by = new byte[data.Count() * sizeof(short)];
            int i = 0;
            foreach (byte b in ConvertShortArray(data, 0, data.Count()))
            {
                by[i++] = b;
            }

         

            var DataSize =  by.Count() * sizeof(short);


            List<byte> pom = new List<byte>();
                pom.AddRange(ASCIIEncoding.ASCII.GetBytes("RIFF"));
                pom.AddRange(BitConverter.GetBytes(DataSize + 36)); //Size + up to data
                pom.AddRange(ASCIIEncoding.ASCII.GetBytes("WAVEfmt "));
                pom.AddRange(BitConverter.GetBytes(Header.Subchunk1Size)); //16 For PCM
                pom.AddRange(BitConverter.GetBytes(Header.AudioFormat)); //PCM FMT
                pom.AddRange(BitConverter.GetBytes(Header.NumChannels));
                pom.AddRange(BitConverter.GetBytes(Header.SampleRate));
                pom.AddRange(BitConverter.GetBytes(Header.ByteRate));
                pom.AddRange(BitConverter.GetBytes(Header.BlockAlign));
            pom.AddRange(BitConverter.GetBytes(Header.BitsPerSample));
            pom.AddRange(BitConverter.GetBytes(Header.Subchunk2Id));
            // pom.AddRange(ASCIIEncoding.ASCII.GetBytes("data"));
            //pom.AddRange(BitConverter.GetBytes(DataSize));
            pom.AddRange(BitConverter.GetBytes(Header.Subchunk2Size));
            byte[] writeData = pom.ToArray();

            byte[] dataArray = writeData.Concat(by).ToArray();
            WriteFile(@"note.wav", dataArray);
        }


        public static void WriteFile(string fileName, byte[] data)
        {
            using (FileStream fstream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                // запись массива байтов в файл
                fstream.Write(data, 0, data.Length);
                fstream.Close();
            }
        }


    }
}

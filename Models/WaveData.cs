using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceСhanging.Models
{
    public class WaveData
    {
        public class WavFile
        {
            public string FilePath { get; set; }

            public WavFile(string file)
            {
                FilePath = file;
            }

            public WavData ReadData()
            {
                var header = new WavHeader();
                byte[] data;
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        //считываем заголовок
                        header.ChunkId = reader.ReadInt32();
                        header.ChunkSize = reader.ReadInt32();
                        header.Format = reader.ReadInt32();
                        header.Subchunk1Id = reader.ReadInt32();
                        header.Subchunk1Size = reader.ReadInt32();
                        header.AudioFormat = reader.ReadInt16();
                        header.NumChannels = reader.ReadInt16();
                        header.SampleRate = reader.ReadInt32();
                        header.ByteRate = reader.ReadInt32();
                        header.BlockAlign = reader.ReadInt16();
                        header.BitsPerSample = reader.ReadInt16();

                        if (header.Subchunk1Size == 18)
                        {
                            header.FmtExtraSize = reader.ReadInt16();
                            reader.ReadBytes(header.FmtExtraSize);
                        }

                        //пытаемся считать данные
                        header.Subchunk2Id = reader.ReadInt32();
                        header.Subchunk2Size = reader.ReadInt32();

                        while (true)
                        {
                            data = reader.ReadBytes(header.Subchunk2Size);
                            if (header.Subchunk2Id == 0x61746164) { break; }//данные найдены

                            //если Subchunk2Id нет тот, что ожидался, пропускаем и пробуем снова
                            header.Subchunk2Id = reader.ReadInt32();
                            header.Subchunk2Size = reader.ReadInt32();
                        }
                    }
                }

                var result = new WavData(header, data);

                return result;
            }

            public void WriteData(WavData input)
            {
                var header = input.Header;
                byte[] data = input.Data;
                using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new BinaryWriter(fs))
                    {
                        //пишем заголовок
                        writer.Write((int)header.ChunkId);
                        writer.Write((int)header.ChunkSize);
                        writer.Write((int)header.Format);
                        writer.Write((int)header.Subchunk1Id);
                        writer.Write((int)header.Subchunk1Size);
                        writer.Write((short)header.AudioFormat);
                        writer.Write((short)header.NumChannels);
                        writer.Write((int)header.SampleRate);
                        writer.Write((int)header.ByteRate);
                        writer.Write((short)header.BlockAlign);
                        writer.Write((short)header.BitsPerSample);

                        if (header.Subchunk1Size == 18)
                        {
                            writer.Write((short)0);
                        }

                        //пишем данные
                        writer.Write((int)header.Subchunk2Id);
                        writer.Write((int)header.Subchunk2Size);
                        writer.Write(data);

                    }
                }
            }
        }

        /// <summary>
        /// Представляет звуковые данные в формате PCM или IEEE Float
        /// Позволяет осуществлять чтение/запись семплов для форматов 8/16 бит PCM и 32 бит IEEE Float
        /// </summary>
        public class WavData : IDisposable
        {
            public WavHeader Header { get; set; }
            public byte[] Data { get; set; }
            BinaryReader _read;
            BinaryWriter _write;

            /// <summary>
            /// Создает экземпляр WavData с указанным заголовком и пустым массивом данных указанного размера
            /// </summary>        
            public WavData(WavHeader hdr, int size)
            {
                Header = hdr;
                Data = new byte[size];
                MemoryStream ms = new MemoryStream(Data);
                _read = new BinaryReader(ms);
                _write = new BinaryWriter(ms);
            }

            /// <summary>
            /// Создает экземпляр WavData с указанным заголовком и массивом данных
            /// </summary>
            public WavData(WavHeader hdr, byte[] d)
            {
                Header = hdr; 
                Data = d;
                MemoryStream ms = new MemoryStream(Data);
                _read = new BinaryReader(ms);
                _write = new BinaryWriter(ms);

            }

            /// <summary>
            /// Возвращает число семплов в массиве данных
            /// </summary>        
            public int GetSamplesCount()
            {
                return Data.Length / ((int)Header.BitsPerSample / 8);
            }

            /// <summary>
            /// Читает следующий семпл в виде float-значения. Целочисленные данные нормализуются в диапазон от -1 до 1
            /// </summary>        
            public float ReadNextSample()
            {
                float res = 0.0f;

                switch (Header.AudioFormat)
                {
                    case 1://PCM
                        if (Header.BitsPerSample == 8)
                        {
                            res = (_read.ReadByte() - 128.0f) / 255.0f;
                        }
                        else if (Header.BitsPerSample == 16)
                        {
                            res = (_read.ReadInt16()) / 32767.0f;
                        }
                        else throw new ApplicationException("BitsPerSample value not supported");
                        if (res > 1.0f) res = 1.0f;
                        if (res < -1.0f) res = -1.0f;
                        break;

                    case 3: //IEEE Float
                        if (Header.BitsPerSample != 32) throw new ApplicationException("BitsPerSample value not supported");
                        res = _read.ReadSingle();
                        break;
                    default: throw new ApplicationException("AudioFormat value not supported");
                }

                return res;
            }

            /// <summary>
            /// Записывает значение следующего семпла. Для PCM значение должно быть нормализовано в диапазон от -1 до 1
            /// </summary>        
            public void WriteSample(float val)
            {
                switch (Header.AudioFormat)
                {
                    case 1://PCM
                        if (Header.BitsPerSample == 8)
                        {
                            _write.Write((byte)((val + 2.0f) * 128.0f));
                        }
                        else if (Header.BitsPerSample == 16)
                        {
                            _write.Write((short)(val * 32767.0f));
                        }
                        else throw new ApplicationException("BitsPerSample value not supported");
                        break;

                    case 3://IEEE Float
                        if (Header.BitsPerSample != 32) throw new ApplicationException("BitsPerSample value not supported");
                        _write.Write(val);
                        break;
                    default: throw new ApplicationException("AudioFormat value not supported");
                }
            }

            public void Dispose()
            {
                _read.Dispose();
                _write.Dispose();
            }
        }

        /// <summary>
        /// Представляет заголовок RIFF WAV файла
        /// </summary>
        public struct WavHeader
        {
            public int ChunkId { get; set; }
            public int ChunkSize { get; set; }
            public int Format { get; set; } //"WAVE"
            public int Subchunk1Id { get; set; }//"fmt "
            public int Subchunk1Size { get; set; }
            public short AudioFormat { get; set; }
            public short NumChannels { get; set; }
            public int SampleRate { get; set; }
            public int ByteRate { get; set; }
            public short BlockAlign { get; set; }
            public short BitsPerSample { get; set; }
            public int FmtExtraSize { get; set; }

            public int Subchunk2Id { get; set; } //"data" (0x61746164)
            public int Subchunk2Size { get; set; }// numSamples * numChannels * bitsPerSample/8   
        }



    }
}

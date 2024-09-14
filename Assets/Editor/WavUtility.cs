using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();
        int length = 0;

        byte[] data = ConvertAndWrite(stream, clip, out length);
        stream.Seek(0, SeekOrigin.Begin);

        byte[] header = WriteHeader(stream, clip, length);
        stream.Seek(0, SeekOrigin.Begin);

        return stream.ToArray();
    }

    private static byte[] ConvertAndWrite(MemoryStream stream, AudioClip clip, out int length)
    {
        int samples = clip.samples * clip.channels;
        float[] data = new float[samples];
        clip.GetData(data, 0);

        short[] intData = new short[data.Length];
        byte[] bytesData = new byte[data.Length * sizeof(short)];

        const int rescaleFactor = 32767; // to convert float to Int16

        for (int i = 0; i < data.Length; i++)
        {
            intData[i] = (short)(data[i] * rescaleFactor);
            BitConverter.GetBytes(intData[i]).CopyTo(bytesData, i * sizeof(short));
        }

        stream.Write(bytesData, 0, bytesData.Length);

        length = bytesData.Length;
        return bytesData;
    }

    private static byte[] WriteHeader(MemoryStream stream, AudioClip clip, int length)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = length;

        stream.Seek(0, SeekOrigin.Begin);

        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(stream.Length - 8), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(hz), 0, 4);
        stream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(samples), 0, 4);

        return stream.ToArray();
    }
}

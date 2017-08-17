using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System; 

public class ByteBuffer {
    MemoryStream stream = null;
    BinaryWriter writer = null;
    BinaryReader reader = null;

    public ByteBuffer() {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
    }

    public ByteBuffer(byte[] data) {
        if (data != null) {
            stream = new MemoryStream(data);
            reader = new BinaryReader(stream);
        } else {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }
    }

    public void Close() {
        if (writer != null) writer.Close();
        if (reader != null) reader.Close();

        stream.Close();
        writer = null;
        reader = null;
        stream = null;
    }

    public void WriteByte (int v) {
        writer.Write((byte)v);
    }

    public void WriteBytes(byte[] data)
    {
       for(int i=0;i<data.Length;i++ )
       {
           writer.Write(data[i]);
       }
    }

    public void WriteInt(int v) {
        writer.Write((int)v);
    }

    public void WriteShort(ushort v) {
        writer.Write((ushort)v);
    }

    public void WriteLong(long v) {
        writer.Write((long)v);
    }

    public void WriteFloat(float v) { 
        writer.Write(v);
    }

    public void WriteDouble(double v) { 
        writer.Write(v);
    }

    public void WriteString(string v) {
        byte[] bytes = Encoding.UTF8.GetBytes(v);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    public int ReadByte () {
        return (int)reader.ReadByte(); 
    }

    public int ReadInt() {
        return (int)reader.ReadInt32();
    }

    public ushort ReadShort() {
        return (ushort)reader.ReadInt16();
    }

    public long ReadLong() {
        return (long)reader.ReadInt64();
    }

    public float ReadFloat() { 
        return reader.ReadSingle();
    }

    public double ReadDouble() { 
        return reader.ReadDouble();
    }

    public string ReadString() {
        ushort len = ReadShort();
        byte[] buffer = new byte[len];
        buffer = reader.ReadBytes(len);
        return Encoding.UTF8.GetString(buffer);
    }

    public byte[] ToBytes() {
        writer.Flush();
        return stream.ToArray();
    }

    public void Flush() {
        writer.Flush();
    }
}

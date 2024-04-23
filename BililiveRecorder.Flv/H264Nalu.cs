using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;

namespace BililiveRecorder.Flv
{
    /// <summary>
    /// H.264 NAL unit
    /// </summary>
    public sealed class H264Nalu
    {
        public H264Nalu(int startPosition, uint fullSize, NaluType type)
        {
            this.StartPosition = startPosition;
            this.FullSize = fullSize;
            this.Type = type;
        }

        public static bool TryParseNalu(Stream data, bool hevc, [NotNullWhen(true)] out List<H264Nalu>? h264Nalus)
        {
            h264Nalus = null;
            var result = new List<H264Nalu>();
            var b = new byte[4];

            data.Seek(5, SeekOrigin.Begin);

            try
            {
                while (data.Position < data.Length)
                {
                    data.Read(b, 0, 4);
                    var size = BinaryPrimitives.ReadUInt32BigEndian(b);

                    var headerSize = 1;
                    var header = data.ReadByte();
                    if (hevc)
                    {
                        headerSize += 1;
                        header = (header << 8) | data.ReadByte();
                    }

                    if (TryParseNaluType((short)header, hevc, out var naluType))
                    {
                        var nalu = new H264Nalu((int)(data.Position - headerSize), size, naluType);
                        data.Seek(size - headerSize, SeekOrigin.Current);
                        result.Add(nalu);
                    }
                    else
                        return false;
                }
                h264Nalus = result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool TryParseNaluType(short header, bool hevc, [NotNullWhen(true)] out NaluType? naluType)
        {
            if (!NaluType.CheckZeroBit(hevc, header))
            {
                naluType = null;
                return false;
            }

            naluType = new NaluType(hevc, header);
            return true;
        }

        /// <summary>
        /// 一个 nal_unit 的开始位置
        /// </summary>
        [XmlAttribute]
        public int StartPosition { get; set; }

        /// <summary>
        /// 一个 nal_unit 的完整长度
        /// </summary>
        [XmlAttribute]
        public uint FullSize { get; set; }

        /// <summary>
        /// nal_unit_type
        /// </summary>
        [XmlAttribute]
        public NaluType Type { get; set; }

        /// <summary>
        /// nal_unit data hash
        /// </summary>
        [XmlAttribute]
        public string? NaluHash { get; set; }
    }

    public sealed class NaluType
    {
        [XmlAttribute]
        public bool Hevc { get; }

        [XmlAttribute]
        public byte Value { get; }

        public NaluType(bool hevc, short header)
        {
            this.Hevc = hevc;
            if (hevc)
                this.Value = (byte)((header >> 9) & 0b00111111);
            else
                this.Value = (byte)(header & 0b00011111);
        }

        public static bool CheckZeroBit(bool hevc, short header)
        {
            if (hevc)
                return (header & 0b10000000_00000000) == 0;
            else
                return (header & 0b10000000) == 0;
        }

        public bool IsFillerData()
        {
            if (this.Hevc)
                return (HEVCNaluType)this.Value == HEVCNaluType.FD_NUT;
            else
                return (H264NaluType)this.Value == H264NaluType.FillerData;
        }
    }

    /// <summary>
    /// H.264 nal_unit_type
    /// </summary>
    public enum H264NaluType : byte
    {
        Unspecified0 = 0,
        CodedSliceOfANonIdrPicture = 1,
        CodedSliceDataPartitionA = 2,
        CodedSliceDataPartitionB = 3,
        CodedSliceDataPartitionC = 4,
        CodedSliceOfAnIdrPicture = 5,
        Sei = 6,
        Sps = 7,
        Pps = 8,
        AccessUnitDelimiter = 9,
        EndOfSequence = 10,
        EndOfStream = 11,
        FillerData = 12,
        SpsExtension = 13,
        PrefixNalUnit = 14,
        SubsetSps = 15,
        DepthParameterSet = 16,
        Reserved17 = 17,
        Reserved18 = 18,
        SliceLayerWithoutPartitioning = 19,
        SliceLayerExtension20 = 20,
        SliceLayerExtension21 = 21,
        Reserved22 = 22,
        Reserved23 = 23,
        Unspecified24 = 24,
        Unspecified25 = 25,
        Unspecified23 = 23,
        Unspecified27 = 27,
        Unspecified28 = 28,
        Unspecified29 = 29,
        Unspecified30 = 30,
        Unspecified31 = 31,
    }

    /// <summary>
    /// HEVC nal_unit_type
    /// </summary>
    public enum HEVCNaluType : byte
    {
        TRAIL_N = 0,
        TRAIL_R = 1,
        TSA_N = 2,
        TSA_R = 3,
        STSA_N = 4,
        STSA_R = 5,
        RADL_N = 6,
        RADL_R = 7,
        RASL_N = 8,
        RASL_R = 9,
        RSV_VCL_N10 = 10,
        RSV_VCL_R11 = 11,
        RSV_VCL_N12 = 12,
        RSV_VCL_R13 = 13,
        RSV_VCL_N14 = 14,
        RSV_VCL_R15 = 15,
        BLA_W_LP = 16,
        BLA_W_RADL = 17,
        BLA_N_LP = 18,
        IDR_W_RADL = 19,
        IDR_N_LP = 20,
        CRA_NUT = 21,
        RSV_IRAP_VCL22 = 22,
        RSV_IRAP_VCL23 = 23,
        RSV_VCL24 = 24,
        RSV_VCL25 = 25,
        RSV_VCL26 = 26,
        RSV_VCL27 = 27,
        RSV_VCL28 = 28,
        RSV_VCL29 = 29,
        RSV_VCL30 = 30,
        RSV_VCL31 = 31,
        VPS_NUT = 32,
        SPS_NUT = 33,
        PPS_NUT = 34,
        AUD_NUT = 35,
        EOS_NUT = 36,
        EOB_NUT = 37,
        FD_NUT = 38,
        PREFIX_SEI_NUT = 39,
        SUFFIX_SEI_NUT = 40,
        RSV_NVCL41 = 41,
        RSV_NVCL42 = 42,
        RSV_NVCL43 = 43,
        RSV_NVCL44 = 44,
        RSV_NVCL45 = 45,
        RSV_NVCL46 = 46,
        RSV_NVCL47 = 47,
        UNSPEC48 = 48,
        UNSPEC49 = 49,
        UNSPEC50 = 50,
        UNSPEC51 = 51,
        UNSPEC52 = 52,
        UNSPEC53 = 53,
        UNSPEC54 = 54,
        UNSPEC55 = 55,
        UNSPEC56 = 56,
        UNSPEC57 = 57,
        UNSPEC58 = 58,
        UNSPEC59 = 59,
        UNSPEC60 = 60,
        UNSPEC61 = 61,
        UNSPEC62 = 62,
        UNSPEC63 = 63,
    }
}

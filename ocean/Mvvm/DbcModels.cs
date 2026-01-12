using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ocean.Mvvm
{
    /// <summary>
    /// CAN帧实体 (对应DBC中的BO_)
    /// </summary>
    public class CanFrameDefine
    {
        public uint FrameId { get; set; }
        public string FrameName { get; set; }
        public byte DataLength { get; set; }
        public List<CanSignalDefine> Signals { get; set; } = new List<CanSignalDefine>();
    }

    public class CanSignalDefine
    {
        public string SignalName { get; set; }
        public int StartBit { get; set; }
        public int BitLength { get; set; }
        public int ByteOrder { get; set; }
        public string Sign { get; set; }
        public double Factor { get; set; }
        public double Offset { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Unit { get; set; }
        public double PhysicalValue { get; set; }
        public long RawValue { get; set; }
        public string UpdateTime { get; set; }
    }

    public class DbcParser
    {
        private static readonly Lazy<DbcParser> _instance = new Lazy<DbcParser>(() => new DbcParser());
        public static DbcParser Instance => _instance.Value;
        public List<CanFrameDefine> DbcFrameList { get; set; } = new List<CanFrameDefine>();

        public void ClearDbcData()
        {
            DbcFrameList.Clear();
        }

        public bool ParseDbcFile(string dbcFilePath)
        {
            try
            {
                ClearDbcData();
                if (!System.IO.File.Exists(dbcFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("DBC文件不存在！");
                    return false;
                }
                var lines = System.IO.File.ReadAllLines(dbcFilePath)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l)
                        && !l.StartsWith("CM_")
                        && !l.StartsWith("BA_")
                        && !l.StartsWith("NS_")
                        && !l.StartsWith("BS_")
                        && !l.StartsWith("BU_")
                        && !l.StartsWith("VAL_TABLE_"))
                    .ToList();

                CanFrameDefine currFrame = null;
                foreach (var line in lines)
                {
                    if (line.StartsWith("BO_ "))
                    {
                        currFrame = ParseDbcFrame_Fixed(line);
                        if (currFrame != null) DbcFrameList.Add(currFrame);
                    }
                    else if (line.StartsWith("SG_ ") && currFrame != null)
                    {
                        ParseDbcSignal_Fixed(line, currFrame);
                    }
                }
                return DbcFrameList.Count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DBC解析异常：{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
        }

        private CanFrameDefine ParseDbcFrame_Fixed(string frameLine)
        {
            try
            {
                var match = Regex.Match(frameLine, @"^BO_\s+(\d+)\s+(.+?)\s*:\s*(\d+)\s+.+$");
                if (!match.Success) return null;

                ulong frameId_Ulong = ulong.Parse(match.Groups[1].Value);
                uint frameId = (uint)frameId_Ulong;
                string frameName = match.Groups[2].Value.Trim();
                byte dataLen = byte.Parse(match.Groups[3].Value);

                return new CanFrameDefine
                {
                    FrameId = frameId,
                    FrameName = frameName,
                    DataLength = dataLen
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"帧解析异常：{frameLine}\r\n{ex.Message}");
                return null;
            }
        }

        // ✅ 这里是你修复好的最终版信号解析函数，完美保留
        private void ParseDbcSignal_Fixed(string signalLine, CanFrameDefine currFrame)
        {
            try
            {
                var colonSplit = signalLine.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (colonSplit.Length < 2) return;

                string sgLeft = colonSplit[0].Trim();
                string sgRight = colonSplit[1].Trim();
                string signalName = string.Empty;

                int sgHeadLen = "SG_ ".Length;
                int lastSpaceIndex = sgLeft.LastIndexOf(' ');
                if (lastSpaceIndex > sgHeadLen)
                {
                    signalName = sgLeft.Substring(sgHeadLen, lastSpaceIndex - sgHeadLen).Trim();
                }
                else
                {
                    signalName = sgLeft.Substring(sgHeadLen).Trim();
                }

                var paramArr = sgRight.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (paramArr.Length < 4) return;

                var signal = new CanSignalDefine();
                signal.SignalName = signalName;

                if (paramArr[0].Contains("|"))
                {
                    var bitArr = paramArr[0].Split('|');
                    if (bitArr.Length == 2)
                    {
                        signal.StartBit = int.TryParse(bitArr[0], out int sb) ? sb : 0;
                        signal.BitLength = int.TryParse(bitArr[1], out int bl) ? bl : 0;
                    }
                }

                if (paramArr[1].Length >= 2)
                {
                    signal.ByteOrder = int.TryParse(paramArr[1].Substring(0, 1), out int bo) ? bo : 1;
                    signal.Sign = paramArr[1].Substring(1, 1);
                }

                signal.Factor = 1.0;
                signal.Offset = 0.0;
                if (paramArr[2].StartsWith("(") && paramArr[2].EndsWith(")"))
                {
                    var foArr = paramArr[2].Trim('(', ')').Split(',');
                    if (foArr.Length >= 1) signal.Factor = double.TryParse(foArr[0], out double f) ? f : 1.0;
                    if (foArr.Length >= 2) signal.Offset = double.TryParse(foArr[1], out double o) ? o : 0.0;
                }

                signal.Min = 0.0;
                signal.Max = 0.0;
                if (paramArr[3].StartsWith("[") && paramArr[3].EndsWith("]"))
                {
                    var mmArr = paramArr[3].Trim('[', ']').Split('|');
                    if (mmArr.Length >= 1) signal.Min = double.TryParse(mmArr[0], out double min) ? min : 0.0;
                    if (mmArr.Length >= 2) signal.Max = double.TryParse(mmArr[1], out double max) ? max : 0.0;
                }

                signal.Unit = string.Empty;
                if (paramArr.Length >= 5)
                {
                    signal.Unit = paramArr[4].Trim('"');
                }

                currFrame.Signals.Add(signal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"信号解析异常: {signalLine} → {ex.Message}");
            }
        }

        public CanFrameDefine ParseCanData(uint canId, byte[] canData)
        {
            var targetFrame = DbcFrameList.FirstOrDefault(f => f.FrameId == canId);
            if (targetFrame == null || canData == null || canData.Length < targetFrame.DataLength) return null;

            foreach (var signal in targetFrame.Signals)
            {
                signal.RawValue = GetSignalRawValue(canData, signal.StartBit, signal.BitLength, signal.ByteOrder, signal.Sign);
                signal.PhysicalValue = Math.Round(signal.RawValue * signal.Factor + signal.Offset, 3);
                signal.UpdateTime = DateTime.Now.ToString("HH:mm:ss.fff");
            }
            return targetFrame;
        }

        private long GetSignalRawValue(byte[] canData, int startBit, int bitLen, int byteOrder, string sign)
        {
            long rawValue = 0;
            for (int i = 0; i < bitLen; i++)
            {
                int bitPos = byteOrder == 1 ? startBit + i : ((startBit / 8) * 8 + 7 - (startBit % 8)) + (i / 8) * 8 - (i % 8);
                int byteIndex = bitPos / 8;
                int bitIndex = bitPos % 8;
                if (byteIndex < canData.Length)
                {
                    rawValue |= ((canData[byteIndex] >> bitIndex) & 0x01) << i;
                }
            }
            if (sign == "-" && (rawValue & (1L << (bitLen - 1))) != 0)
            {
                rawValue -= (1L << bitLen);
            }
            return rawValue;
        }
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Blade.MX
{
    public class MxTools
    {
        private readonly int _majorVersion;
        private readonly int _minorVersion;
        private readonly string _userName;
        private const int EncryptKey = 0x787;

        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        private readonly Dictionary<int, string> _base64Dict;
        private readonly Dictionary<string, int> _base64ReverseDict;
        
        public MxTools(int majorVersion, int minorVersion, string userName)
        {
            _majorVersion = majorVersion;
            _minorVersion = minorVersion;
            _userName = userName;
            _base64Dict = alphabet.Select((c, i) => new {Character = c.ToString(), Index = i})
                .ToDictionary(item => item.Index, item => item.Character);
            _base64ReverseDict = alphabet.Select((c, i) => new {Character = c.ToString(), Index = i})
                .ToDictionary(item => item.Character, item => item.Index);
        }

        public async Task Tear()
        {
            var license = $"{(int)LicenseType.Professional}#{_userName}|{_majorVersion}{_minorVersion}#1#{_majorVersion}3{_minorVersion}6{_minorVersion}#0#0#0#";
            var encBytes = Encryt(EncryptKey, Encoding.UTF8.GetBytes(license));
            var base64Str = Base64Encode(encBytes);
            await CreateZip(base64Str);
        }

        private string Base64Encode(byte[] encryptBytes)
        {
            var blocksCount = encryptBytes.Length / 3;
            var leftBytes = encryptBytes.Length % 3;

            var strBytes = new List<byte>();
            var result = "";
            for (var i = 0; i < blocksCount; i++)
            {
                var slice = FillZero(encryptBytes[(3 * i)..(3 * i + 3)]);
                var coding = BitConverter.ToInt32(slice);
                var block = _base64Dict[coding & 0x3f];
                block += _base64Dict[(coding >> 6) & 0x3f];
                block += _base64Dict[(coding >> 12) & 0x3f];
                block += _base64Dict[(coding >> 18) & 0x3f];
                result += block;
                strBytes.AddRange(Encoding.UTF8.GetBytes(block));
            }

            if (leftBytes == 0)
            {
                return result;
            }
            else if (leftBytes == 1)
            {
                var slice = FillZero(encryptBytes[(3 * blocksCount)..]);
                var coding = BitConverter.ToInt32(slice);
                var block = _base64Dict[coding & 0x3f];
                block += _base64Dict[(coding >> 6) & 0x3f];
                result += block;
                strBytes.AddRange(Encoding.UTF8.GetBytes(block));
                return result;
            }
            else
            {
                var slice = FillZero(encryptBytes[(3 * blocksCount)..]);
                var coding = BitConverter.ToInt32(slice);
                var block = _base64Dict[coding & 0x3f];
                block += _base64Dict[(coding >> 6) & 0x3f];
                block += _base64Dict[(coding >> 12) & 0x3f];
                result += block;
                strBytes.AddRange(Encoding.UTF8.GetBytes(block));
                return result;
            }
        }

        private byte[] Encryt(int key, byte[] bytes)
        {
            var encrytBytes = new List<byte>();
            foreach (var t in bytes)
            {
                var bt = (byte)(t ^ ((key >> 8) & 0xff));
                encrytBytes.Add(bt);
                key = bt & key | 0x482D;
            }
            return encrytBytes.ToArray();
        }

        private byte[] FillZero(byte[] sourceBytes)
        {
            if (sourceBytes.Length >= 4)
            {
                return sourceBytes;
            }

            var list = sourceBytes.ToList();
            for (var i = sourceBytes.Length; i < 4; i++)
            {
                list.Add(0);
            }

            return list.ToArray();
        }

        private async Task CreateZip(string key)
        {
            
            var zipPath = Path.Combine(Directory.GetCurrentDirectory(), "Custom.mxtpro");
            var bytes = Encoding.UTF8.GetBytes(key);

            await using var fs = new FileStream(zipPath,FileMode.Create);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Create, true);
            var zipArchiveEntry = archive.CreateEntry("Pro.key", CompressionLevel.Fastest);
            await using var zipStream = zipArchiveEntry.Open();
            await zipStream.WriteAsync(bytes, 0, bytes.Length);
            
        }
        
    }
}
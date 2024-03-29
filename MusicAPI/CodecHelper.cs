﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MusicCollection.MusicAPI
{
    public class CodecHelper
    {
        public static string AESDecode(string plaintextData, string key)
        {
            try
            {
                byte[] toEncryptArray = Convert.FromBase64String(plaintextData);
                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes(key),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };
                ICryptoTransform cTransform = rm.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static RSACryptoServiceProvider PemDecodeX509PrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // --------- Set up stream to decode the asn.1 encoded RSA private key ------    
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);  //wrap Memory Stream with BinaryReader for easy reading    
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)    
                    binr.ReadByte();    //advance 1 byte    
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes    
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102) //version number    
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                //------ all private key components are Integer sequences ----    
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);


                // ------- create RSACryptoServiceProvider instance and initialize with public key -----    
                CspParameters CspParameters = new CspParameters();
                CspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(1024, CspParameters);
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            finally
            {
                binr.Close();
            }
        }

        /// <summary>
        /// 取得整数大小.
        /// </summary>
        /// <param name="binr">BinaryReader</param>
        /// <returns>返回整数大小.</returns>
        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)    //expect integer    
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();    // data size in next byte    
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte(); // data size in next 2 bytes    
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;    // we already have the data size    
            }

            while (binr.ReadByte() == 0x00)
            {  //remove high order zeros in data    
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);      //last ReadByte wasn't a removed zero, so back up a byte    
            return count;
        }

        public static RSACryptoServiceProvider PemDecodePkcs8PrivateKey(byte[] pkcs8)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            // this byte[] includes the sequence byte and terminal encoded null 
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;


                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;

                twobytes = binr.ReadUInt16();

                if (twobytes != 0x0001)
                    return null;

                seq = binr.ReadBytes(15);       //read the Sequence OID
                if (!SequenceEqualByte(seq, SeqOID))    //make sure Sequence for OID is correct
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04) //expect an Octet string 
                    return null;

                bt = binr.ReadByte();       //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
                if (bt == 0x81)
                    binr.ReadByte();
                else
                    if (bt == 0x82)
                    binr.ReadUInt16();
                //------ at this stage, the remaining sequence should be the RSA private key

                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = PemDecodeX509PrivateKey(rsaprivkey);
                return rsacsp;
            }
            finally { binr.Close(); }

        }

        /// <summary>
		/// 根据PEM纯密钥数据，获取公钥的RSA加解密对象.
		/// </summary>
		/// <param name="pubcdata">公钥数据</param>
		/// <returns>返回公钥的RSA加解密对象.</returns>
		public static RSACryptoServiceProvider PemDecodePublicKey(byte[] pubcdata)
        {
            byte[] SeqOID = { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01 };

            MemoryStream ms = new MemoryStream(pubcdata);
            BinaryReader reader = new BinaryReader(ms);

            if (reader.ReadByte() == 0x30)
                ReadASNLength(reader); //skip the size
            else
                return null;

            int identifierSize = 0; //total length of Object Identifier section
            if (reader.ReadByte() == 0x30)
                identifierSize = ReadASNLength(reader);
            else
                return null;

            if (reader.ReadByte() == 0x06)
            { //is the next element an object identifier?
                int oidLength = ReadASNLength(reader);
                byte[] oidBytes = new byte[oidLength];
                reader.Read(oidBytes, 0, oidBytes.Length);
                if (!SequenceEqualByte(oidBytes, SeqOID)) //is the object identifier rsaEncryption PKCS#1?
                    return null;

                int remainingBytes = identifierSize - 2 - oidBytes.Length;
                reader.ReadBytes(remainingBytes);
            }

            if (reader.ReadByte() == 0x03)
            { //is the next element a bit string?

                ReadASNLength(reader); //skip the size
                reader.ReadByte(); //skip unused bits indicator
                if (reader.ReadByte() == 0x30)
                {
                    ReadASNLength(reader); //skip the size
                    if (reader.ReadByte() == 0x02)
                    { //is it an integer?
                        int modulusSize = ReadASNLength(reader);
                        byte[] modulus = new byte[modulusSize];
                        reader.Read(modulus, 0, modulus.Length);
                        if (modulus[0] == 0x00)
                        {//strip off the first byte if it's 0
                            byte[] tempModulus = new byte[modulus.Length - 1];
                            Array.Copy(modulus, 1, tempModulus, 0, modulus.Length - 1);
                            modulus = tempModulus;
                        }

                        if (reader.ReadByte() == 0x02)
                        { //is it an integer?
                            int exponentSize = ReadASNLength(reader);
                            byte[] exponent = new byte[exponentSize];
                            reader.Read(exponent, 0, exponent.Length);

                            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                            RSAParameters RSAKeyInfo = new RSAParameters();
                            RSAKeyInfo.Modulus = modulus;
                            RSAKeyInfo.Exponent = exponent;
                            RSA.ImportParameters(RSAKeyInfo);
                            return RSA;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Read ASN Length.
        /// </summary>
        /// <param name="reader">reader</param>
        /// <returns>Return ASN Length.</returns>
        private static int ReadASNLength(BinaryReader reader)
        {
            //Note: this method only reads lengths up to 4 bytes long as
            //this is satisfactory for the majority of situations.
            int length = reader.ReadByte();
            if ((length & 0x00000080) == 0x00000080)
            { //is the length greater than 1 byte
                int count = length & 0x0000000f;
                byte[] lengthBytes = new byte[4];
                reader.Read(lengthBytes, 4 - count, count);
                Array.Reverse(lengthBytes); //
                length = BitConverter.ToInt32(lengthBytes, 0);
            }
            return length;
        }

        /// <summary>
        /// 字节数组内容是否相等.
        /// </summary>
        /// <param name="a">数组a</param>
        /// <param name="b">数组b</param>
        /// <returns>返回是否相等.</returns>
        private static bool SequenceEqualByte(byte[] a, byte[] b)
        {
            var len1 = a.Length;
            var len2 = b.Length;
            if (len1 != len2)
            {
                return false;
            }
            for (var i = 0; i < len1; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        public static byte[] PemUnpack(string data)
        {
            byte[] rt = null;
            const string SIGN_BEGIN = "-BEGIN";
            const string SIGN_END = "-END";
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException("data", "data is empty!");
            int datelen = data.Length;
            // find begin.
            int bodyPos = 0;    // 主体内容开始的地方.
            int beginPos = data.IndexOf(SIGN_BEGIN, StringComparison.OrdinalIgnoreCase);
            if (beginPos >= 0)
            {
                // 向后查找换行符后的首个字节.
                bool isFound = false;
                bool hadNewline = false;    // 已遇到过换行符号.
                bool hyphenHad = false; // 已遇到过“-”符号.
                bool hyphenDone = false;    // 已成功获取了右侧“-”的范围.
                int p = beginPos + SIGN_BEGIN.Length;
                int hyphenStart = p;    // 右侧“-”的开始位置.
                int hyphenEnd = hyphenStart;    // 右侧“-”的结束位置. 即最后一个“-”字符的位置+1.
                while (p < datelen)
                {
                    char ch = data[p];
                    // 查找右侧“-”的范围.
                    if (!hyphenDone)
                    {
                        if (ch == '-')
                        {
                            if (!hyphenHad)
                            {
                                hyphenHad = true;
                                hyphenStart = p;
                                hyphenEnd = hyphenStart;
                            }
                        }
                        else
                        {
                            if (hyphenHad)
                            { // 无需“&& !hyphenDone”，因为外层判断了.
                                hyphenDone = true;
                                hyphenEnd = p;
                            }
                        }
                    }
                    // 向后查找换行符后的首个字节.
                    if (ch == '\n' || ch == '\r')
                    {
                        hadNewline = true;
                    }
                    else
                    {
                        if (hadNewline)
                        {
                            // 找到了.
                            bodyPos = p;
                            isFound = true;
                            break;
                        }
                    }
                    // next.
                    ++p;
                }
                // purposetext
                //if (hyphenDone)
                //{
                //    int start = beginPos + SIGN_BEGIN.Length;
                //    purposetext = data.Substring(start, hyphenStart - start).Trim();
                //    string purposetextUp = purposetext.ToUpperInvariant();
                //    if (purposetextUp.IndexOf("PRIVATE") >= 0)
                //    {
                //        purposecode = 'R';
                //    }
                //    else if (purposetextUp.IndexOf("PUBLIC") >= 0)
                //    {
                //        purposecode = 'U';
                //    }
                //}
                // bodyPos.
                if (isFound)
                {
                    //OK.
                }
                else if (hyphenDone)
                {
                    // 以右侧右侧“-”的结束位置作为主体开始.
                    bodyPos = hyphenEnd;
                }
                else
                {
                    // 找不到结束位置，只能退出.
                    return rt;
                }
            }
            // find end.
            int bodyEnd = datelen;  // 主体内容的结束位置. 即最后一个字符的位置+1.
            int endPos = data.IndexOf(SIGN_END, bodyPos);
            if (endPos >= 0)
            {
                // 向前查找换行符前的首个字节.
                bool isFound = false;
                bool hadNewline = false;
                int p = endPos - 1;
                while (p >= bodyPos)
                {
                    char ch = data[p];
                    if (ch == '\n' || ch == '\r')
                    {
                        hadNewline = true;
                    }
                    else
                    {
                        if (hadNewline)
                        {
                            // 找到了.
                            bodyEnd = p + 1;
                            break;
                        }
                    }
                    // next.
                    --p;
                }
                if (!isFound)
                {
                    // 忽略.
                }
            }
            // get body.
            if (bodyPos >= bodyEnd)
            {
                return rt;
            }
            string body = data.Substring(bodyPos, bodyEnd - bodyPos).Trim();
            // Decode BASE64.
            if (String.IsNullOrEmpty(body)) throw new ArgumentNullException("data", "data body is empty!");
            rt = Convert.FromBase64String(body);
            return rt;
        }
    }
}

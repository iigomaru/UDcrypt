using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using System.Diagnostics;

public class ChaCha20Test : UdonSharpBehaviour
{
    public UInt32[] initKey = new UInt32[8] ;//{0x03020100, 0x07060504, 0x0b0a0908, 0x0f0e0d0c, 0x13121110, 0x17161514, 0x1b1a1918, 0x1f1e1d1c};

    public UInt32[] initNonce = new UInt32[3] ;//{0x09000000, 0x4a000000, 0x00000000};

    public void _interact()
    {
        _DoInteract();
    }

    public void _DoInteract()
    {
        UnityEngine.Debug.Log("<color=#ff0000>----++++====|ChaCha20ByteStream|====++++----</color>");
        byte[] byteStream = ChaCha20._Block(initKey, initNonce, 0x00000000U );
        for (int j = 0; j < byteStream.Length; j += 16)
        {   
            string outputString = String.Empty;
            for (int k = 0; k < 16; k++)
            {   
                string byteString = String.Format( "{0:X2}", byteStream[j+k]);
                outputString = string.Concat(outputString, byteString, " ");
            }
            UnityEngine.Debug.Log(outputString);
        }

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        
        UInt32 z;
        for ( z = 0; z < 160; z++)
        {
            //UnityEngine.Debug.Log._Log("<color=#ff0000>----++++====|ChaCha20ByteStream|====++++----</color>");
            byteStream = ChaCha20._Block(initKey, initNonce, z );
            //for (int j = 0; j < byteStream.Length; j += 16)
            //{   
            //    string outputString = String.Empty;
            //    for (int k = 0; k < 16; k++)
            //    {   
            //        string byteString = String.Format( "{0:X2}", byteStream[j+k]);
            //        outputString = string.Concat(outputString, byteString, " ");
            //    }
            //    UnityEngine.Debug.Log._PrintNewLine(outputString);
            //}
        }

        //3200000
        //3100000
        //2300000 major speed boost due to optimizing out a ton of array externs in the _QuarterRound method
        //1700000
        //1600000 disabled a part of the U# compiler to avoid unessesarry copys
        //1300000 stopped calling the function for rotate and just copied the code lol
        //1200000 optimized the part of the function that converts the words to bytes
        //1000000 Moved to U# 1.1.1 and static methods
        
        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        TimeSpan ts = stopWatch.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = ts.Ticks.ToString();
        UnityEngine.Debug.Log("RunTime " + elapsedTime);

        UnityEngine.Debug.Log("<color=#ff0000>----++++====|ChaCha20TestString|====++++----</color>");
        byteStream = _ASCIIStringToByteArray(RFC8439PlaintextTestString);
        for (int j = 0; j < byteStream.Length; j += 16)
        {   
            string outputString = String.Empty;
            for (int k = 0; k < 16 && j+k < byteStream.Length; k++)
            {   
                string byteString = String.Format( "{0:X2}", byteStream[j+k]);
                outputString = string.Concat(outputString, byteString, " ");
            }
            UnityEngine.Debug.Log(outputString);
        }
        int result = _ByteArrayComp(byteStream, RFC8439PlaintextByteArray);
        switch(result) 
        {
        case -1:
            UnityEngine.Debug.Log("<color=#ff0000>RFC8439 PLAINTXT ASCII:BYTE ARR LEN ERR");
            break;
        case 0:
            UnityEngine.Debug.Log("<color=#00ff00>RFC8439 PLAINTXT ASCII:BYTE ARR COMP PASS</color>");
            break;
        default:
            UnityEngine.Debug.Log("<color=#ff0000>RFC8439 PLAINTXT ASCII:BYTE ARR ERR POS: " + result + "</color>");
            break;
        }

        UnityEngine.Debug.Log("<color=#ff0000>----++++====|ChaCha20Ciphertext|====++++----</color>");

        byte[] ciphertext = ChaCha20._Encrypt(RFC8439TestKey, RFC8439TestNonce, 0x00000001U, RFC8439PlaintextByteArray);

        for (int j = 0; j < ciphertext.Length; j += 16)
        {   
            string outputString = String.Empty;
            for (int k = 0; k < 16 && j+k < ciphertext.Length; k++)
            {   
                string byteString = String.Format( "{0:X2}", ciphertext[j+k]);
                outputString = string.Concat(outputString, byteString, " ");
            }
            UnityEngine.Debug.Log(outputString);
        }

        result = _ByteArrayComp(ciphertext, RFC8439CiphertextByteArray);
        switch(result) 
        {
        case -1:
            UnityEngine.Debug.Log("<color=#ff0000>RFC8439 CIPHERTXT CIPHER:REF ARR LEN ERR");
            break;
        case 0:
            UnityEngine.Debug.Log("<color=#00ff00>RFC8439 CIPHERTXT CIPHER:REF ARR COMP PASS</color>");
            break;
        default:
            UnityEngine.Debug.Log("<color=#ff0000>RFC8439 CIPHERTXT CIPHER:REF ARR ERR POS: " + result + "</color>");
            break;
        }


    }
    
    // Compare two byte arrays. Returns 0 if no error, returns -1 if length is diffrent, and returns non-zero as the position that the first error that occures in the comparision
    private static int _ByteArrayComp(byte[] arr1, byte[] arr2)
    {
        int output = 0;
        if (arr1.Length == arr2.Length) 
        {
			for (int i = 0; i < arr2.Length; i++) 
            {
				if (arr2[i] != arr1[i]) { return i++; }
			}
		} 
        else { output = -1; }

        return output;
    }

    private static byte[] _ASCIIStringToByteArray(string ASCIIString)
    {
        char[] charArr = ASCIIString.ToCharArray();
        byte[] ASCIIByteArray = new byte[charArr.Length];
        for(int i = 0; i < charArr.Length; i++)  
        {
            if ((int)(charArr[i]) > sbyte.MaxValue)
            {
                UnityEngine.Debug.Log("CHAR IN STRING NON-ASCII");
                return new byte[0];
            }
            byte byteChar = (byte)(charArr[i]);
            ASCIIByteArray[i] = byteChar;
        }
        return ASCIIByteArray;
    }

#region RFCTESTVECTORS
    private const string RFC8439PlaintextTestString = "Any submission to the IETF intended by the Contributor for publication as all or part of an IETF Internet-Draft or RFC and any statement made within the context of an IETF activity is considered an \"IETF Contribution\". Such statements include oral statements in IETF sessions, as well as written and electronic communications made at any time or place, which are addressed to";
    private readonly byte[] RFC8439PlaintextByteArray = new byte[] 
        { 
            0x41, 0x6e, 0x79, 0x20, 0x73, 0x75, 0x62, 0x6d, 0x69, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20, 0x74,  // Any submission t
            0x6f, 0x20, 0x74, 0x68, 0x65, 0x20, 0x49, 0x45, 0x54, 0x46, 0x20, 0x69, 0x6e, 0x74, 0x65, 0x6e,  // o the IETF inten
            0x64, 0x65, 0x64, 0x20, 0x62, 0x79, 0x20, 0x74, 0x68, 0x65, 0x20, 0x43, 0x6f, 0x6e, 0x74, 0x72,  // ded by the Contr
            0x69, 0x62, 0x75, 0x74, 0x6f, 0x72, 0x20, 0x66, 0x6f, 0x72, 0x20, 0x70, 0x75, 0x62, 0x6c, 0x69,  // ibutor for publi
            0x63, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x61, 0x73, 0x20, 0x61, 0x6c, 0x6c, 0x20, 0x6f, 0x72,  // cation as all or
            0x20, 0x70, 0x61, 0x72, 0x74, 0x20, 0x6f, 0x66, 0x20, 0x61, 0x6e, 0x20, 0x49, 0x45, 0x54, 0x46,  //  part of an IETF
            0x20, 0x49, 0x6e, 0x74, 0x65, 0x72, 0x6e, 0x65, 0x74, 0x2d, 0x44, 0x72, 0x61, 0x66, 0x74, 0x20,  //  Internet-Draft
            0x6f, 0x72, 0x20, 0x52, 0x46, 0x43, 0x20, 0x61, 0x6e, 0x64, 0x20, 0x61, 0x6e, 0x79, 0x20, 0x73,  // or RFC and any s
            0x74, 0x61, 0x74, 0x65, 0x6d, 0x65, 0x6e, 0x74, 0x20, 0x6d, 0x61, 0x64, 0x65, 0x20, 0x77, 0x69,  // tatement made wi
            0x74, 0x68, 0x69, 0x6e, 0x20, 0x74, 0x68, 0x65, 0x20, 0x63, 0x6f, 0x6e, 0x74, 0x65, 0x78, 0x74,  // thin the context
            0x20, 0x6f, 0x66, 0x20, 0x61, 0x6e, 0x20, 0x49, 0x45, 0x54, 0x46, 0x20, 0x61, 0x63, 0x74, 0x69,  //  of an IETF acti
            0x76, 0x69, 0x74, 0x79, 0x20, 0x69, 0x73, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x69, 0x64, 0x65, 0x72,  // vity is consider
            0x65, 0x64, 0x20, 0x61, 0x6e, 0x20, 0x22, 0x49, 0x45, 0x54, 0x46, 0x20, 0x43, 0x6f, 0x6e, 0x74,  // ed an "IETF Cont
            0x72, 0x69, 0x62, 0x75, 0x74, 0x69, 0x6f, 0x6e, 0x22, 0x2e, 0x20, 0x53, 0x75, 0x63, 0x68, 0x20,  // ribution". Such
            0x73, 0x74, 0x61, 0x74, 0x65, 0x6d, 0x65, 0x6e, 0x74, 0x73, 0x20, 0x69, 0x6e, 0x63, 0x6c, 0x75,  // statements inclu
            0x64, 0x65, 0x20, 0x6f, 0x72, 0x61, 0x6c, 0x20, 0x73, 0x74, 0x61, 0x74, 0x65, 0x6d, 0x65, 0x6e,  // de oral statemen
            0x74, 0x73, 0x20, 0x69, 0x6e, 0x20, 0x49, 0x45, 0x54, 0x46, 0x20, 0x73, 0x65, 0x73, 0x73, 0x69,  // ts in IETF sessi
            0x6f, 0x6e, 0x73, 0x2c, 0x20, 0x61, 0x73, 0x20, 0x77, 0x65, 0x6c, 0x6c, 0x20, 0x61, 0x73, 0x20,  // ons, as well as
            0x77, 0x72, 0x69, 0x74, 0x74, 0x65, 0x6e, 0x20, 0x61, 0x6e, 0x64, 0x20, 0x65, 0x6c, 0x65, 0x63,  // written and elec
            0x74, 0x72, 0x6f, 0x6e, 0x69, 0x63, 0x20, 0x63, 0x6f, 0x6d, 0x6d, 0x75, 0x6e, 0x69, 0x63, 0x61,  // tronic communica
            0x74, 0x69, 0x6f, 0x6e, 0x73, 0x20, 0x6d, 0x61, 0x64, 0x65, 0x20, 0x61, 0x74, 0x20, 0x61, 0x6e,  // tions made at an
            0x79, 0x20, 0x74, 0x69, 0x6d, 0x65, 0x20, 0x6f, 0x72, 0x20, 0x70, 0x6c, 0x61, 0x63, 0x65, 0x2c,  // y time or place,
            0x20, 0x77, 0x68, 0x69, 0x63, 0x68, 0x20, 0x61, 0x72, 0x65, 0x20, 0x61, 0x64, 0x64, 0x72, 0x65,  //  which are addre
            0x73, 0x73, 0x65, 0x64, 0x20, 0x74, 0x6f                                                         // ssed to
        };

    private readonly byte[] RFC8439CiphertextByteArray = new byte[]
        {
            0xa3, 0xfb, 0xf0, 0x7d, 0xf3, 0xfa, 0x2f, 0xde, 0x4f, 0x37, 0x6c, 0xa2, 0x3e, 0x82, 0x73, 0x70, // ...}../.O7l.>.sp
            0x41, 0x60, 0x5d, 0x9f, 0x4f, 0x4f, 0x57, 0xbd, 0x8c, 0xff, 0x2c, 0x1d, 0x4b, 0x79, 0x55, 0xec, // A`].OOW...,.KyU.
            0x2a, 0x97, 0x94, 0x8b, 0xd3, 0x72, 0x29, 0x15, 0xc8, 0xf3, 0xd3, 0x37, 0xf7, 0xd3, 0x70, 0x05, // *....r)....7..p.
            0x0e, 0x9e, 0x96, 0xd6, 0x47, 0xb7, 0xc3, 0x9f, 0x56, 0xe0, 0x31, 0xca, 0x5e, 0xb6, 0x25, 0x0d, // ....G...V.1.^.%.
            0x40, 0x42, 0xe0, 0x27, 0x85, 0xec, 0xec, 0xfa, 0x4b, 0x4b, 0xb5, 0xe8, 0xea, 0xd0, 0x44, 0x0e, // @B.'....KK....D.
            0x20, 0xb6, 0xe8, 0xdb, 0x09, 0xd8, 0x81, 0xa7, 0xc6, 0x13, 0x2f, 0x42, 0x0e, 0x52, 0x79, 0x50, //  ........./B.RyP
            0x42, 0xbd, 0xfa, 0x77, 0x73, 0xd8, 0xa9, 0x05, 0x14, 0x47, 0xb3, 0x29, 0x1c, 0xe1, 0x41, 0x1c, // B..ws....G.)..A.
            0x68, 0x04, 0x65, 0x55, 0x2a, 0xa6, 0xc4, 0x05, 0xb7, 0x76, 0x4d, 0x5e, 0x87, 0xbe, 0xa8, 0x5a, // h.eU*....vM^...Z
            0xd0, 0x0f, 0x84, 0x49, 0xed, 0x8f, 0x72, 0xd0, 0xd6, 0x62, 0xab, 0x05, 0x26, 0x91, 0xca, 0x66, // ...I..r..b..&..f
            0x42, 0x4b, 0xc8, 0x6d, 0x2d, 0xf8, 0x0e, 0xa4, 0x1f, 0x43, 0xab, 0xf9, 0x37, 0xd3, 0x25, 0x9d, // BK.m-....C..7.%.
            0xc4, 0xb2, 0xd0, 0xdf, 0xb4, 0x8a, 0x6c, 0x91, 0x39, 0xdd, 0xd7, 0xf7, 0x69, 0x66, 0xe9, 0x28, // ......l.9...if.(
            0xe6, 0x35, 0x55, 0x3b, 0xa7, 0x6c, 0x5c, 0x87, 0x9d, 0x7b, 0x35, 0xd4, 0x9e, 0xb2, 0xe6, 0x2b, // .5U;.l\..{5....+
            0x08, 0x71, 0xcd, 0xac, 0x63, 0x89, 0x39, 0xe2, 0x5e, 0x8a, 0x1e, 0x0e, 0xf9, 0xd5, 0x28, 0x0f, // .q..c.9.^.....(.
            0xa8, 0xca, 0x32, 0x8b, 0x35, 0x1c, 0x3c, 0x76, 0x59, 0x89, 0xcb, 0xcf, 0x3d, 0xaa, 0x8b, 0x6c, // ..2.5.<vY...=..l
            0xcc, 0x3a, 0xaf, 0x9f, 0x39, 0x79, 0xc9, 0x2b, 0x37, 0x20, 0xfc, 0x88, 0xdc, 0x95, 0xed, 0x84, // .:..9y.+7 ......
            0xa1, 0xbe, 0x05, 0x9c, 0x64, 0x99, 0xb9, 0xfd, 0xa2, 0x36, 0xe7, 0xe8, 0x18, 0xb0, 0x4b, 0x0b, // ....d....6....K.
            0xc3, 0x9c, 0x1e, 0x87, 0x6b, 0x19, 0x3b, 0xfe, 0x55, 0x69, 0x75, 0x3f, 0x88, 0x12, 0x8c, 0xc0, // ....k.;.Uiu?....
            0x8a, 0xaa, 0x9b, 0x63, 0xd1, 0xa1, 0x6f, 0x80, 0xef, 0x25, 0x54, 0xd7, 0x18, 0x9c, 0x41, 0x1f, // ...c..o..%T...A.
            0x58, 0x69, 0xca, 0x52, 0xc5, 0xb8, 0x3f, 0xa3, 0x6f, 0xf2, 0x16, 0xb9, 0xc1, 0xd3, 0x00, 0x62, // Xi.R..?.o......b
            0xbe, 0xbc, 0xfd, 0x2d, 0xc5, 0xbc, 0xe0, 0x91, 0x19, 0x34, 0xfd, 0xa7, 0x9a, 0x86, 0xf6, 0xe6, // ...-.....4......
            0x98, 0xce, 0xd7, 0x59, 0xc3, 0xff, 0x9b, 0x64, 0x77, 0x33, 0x8f, 0x3d, 0xa4, 0xf9, 0xcd, 0x85, // ...Y...dw3.=....
            0x14, 0xea, 0x99, 0x82, 0xcc, 0xaf, 0xb3, 0x41, 0xb2, 0x38, 0x4d, 0xd9, 0x02, 0xf3, 0xd1, 0xab, // .......A.8M.....
            0x7a, 0xc6, 0x1d, 0xd2, 0x9c, 0x6f, 0x21, 0xba, 0x5b, 0x86, 0x2f, 0x37, 0x30, 0xe3, 0x7c, 0xfd, // z....o!.[./70.|.
            0xc4, 0xfd, 0x80, 0x6c, 0x22, 0xf2, 0x21 
        };

        private readonly UInt32[] RFC8439TestKey = new UInt32[]
        {
            0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U,
            0x00000000U, 0x00000000U, 0x00000000U, 0x01000000U
        };
        private readonly UInt32[] RFC8439TestNonce = new UInt32[]
        {
            0x00000000U, 0x00000000U, 0x02000000U
        };
#endregion


}
﻿using System;
using Org.BouncyCastle.Math;
using SpotifyLib.Helpers;

namespace SpotifyLib.Crypto
{
    internal class DiffieHellman
    {
        private static readonly BigInteger Generator = BigInteger.ValueOf(2);

        private static readonly byte[] PrimeBytes =
        {
            (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff,
            (byte) 0xc9,
            (byte) 0x0f, (byte) 0xda, (byte) 0xa2, (byte) 0x21, (byte) 0x68, (byte) 0xc2, (byte) 0x34, (byte) 0xc4,
            (byte) 0xc6,
            (byte) 0x62, (byte) 0x8b, (byte) 0x80, (byte) 0xdc, (byte) 0x1c, (byte) 0xd1, (byte) 0x29, (byte) 0x02,
            (byte) 0x4e,
            (byte) 0x08, (byte) 0x8a, (byte) 0x67, (byte) 0xcc, (byte) 0x74, (byte) 0x02, (byte) 0x0b, (byte) 0xbe,
            (byte) 0xa6,
            (byte) 0x3b, (byte) 0x13, (byte) 0x9b, (byte) 0x22, (byte) 0x51, (byte) 0x4a, (byte) 0x08, (byte) 0x79,
            (byte) 0x8e,
            (byte) 0x34, (byte) 0x04, (byte) 0xdd, (byte) 0xef, (byte) 0x95, (byte) 0x19, (byte) 0xb3, (byte) 0xcd,
            (byte) 0x3a,
            (byte) 0x43, (byte) 0x1b, (byte) 0x30, (byte) 0x2b, (byte) 0x0a, (byte) 0x6d, (byte) 0xf2, (byte) 0x5f,
            (byte) 0x14,
            (byte) 0x37, (byte) 0x4f, (byte) 0xe1, (byte) 0x35, (byte) 0x6d, (byte) 0x6d, (byte) 0x51, (byte) 0xc2,
            (byte) 0x45,
            (byte) 0xe4, (byte) 0x85, (byte) 0xb5, (byte) 0x76, (byte) 0x62, (byte) 0x5e, (byte) 0x7e, (byte) 0xc6,
            (byte) 0xf4,
            (byte) 0x4c, (byte) 0x42, (byte) 0xe9, (byte) 0xa6, (byte) 0x3a, (byte) 0x36, (byte) 0x20, (byte) 0xff,
            (byte) 0xff,
            (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff
        };

        private static readonly BigInteger Prime = new BigInteger(1, PrimeBytes);
        private readonly BigInteger _privateKey;
        private readonly BigInteger _publicKey;

        internal DiffieHellman(Random random)
        {
            var keyData = new byte[95];
            random.NextBytes(keyData);

            _privateKey = new BigInteger(1, keyData);
            _publicKey = Generator.ModPow(_privateKey, Prime);
        }

        internal BigInteger ComputeSharedKey(byte[] remoteKeyBytes)
        {
            var remoteKey = new BigInteger(1, remoteKeyBytes);
            return remoteKey.ModPow(_privateKey, Prime);
        }

        internal byte[] PublicKeyArray()
        {
            return Utils.toByteArray(_publicKey);
        }
    }
}
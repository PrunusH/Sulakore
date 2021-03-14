﻿using System;

namespace Sulakore.Crypto
{
    public class RC4
    {
        private int _i, _j;
        private readonly int[] _table;
        private readonly object _parseLock;

        public RC4(byte[] key)
        {
            _table = new int[256];
            _parseLock = new object();

            for (int i = 0; i < 256; i++)
            {
                _table[i] = i;
            }
            for (int j = 0, x = 0; j < _table.Length; j++)
            {
                x += _table[j];
                x += key[j % key.Length];
                x %= _table.Length;
                Swap(j, x);
            }
        }

        public void Parse(Span<byte> data)
        {
            lock (_parseLock)
            {
                for (int k = 0; k < data.Length; k++)
                {
                    _i++;
                    _i %= _table.Length;
                    _j += _table[_i];
                    _j %= _table.Length;
                    Swap(_i, _j);

                    int rightXOR = _table[_i] + _table[_j];
                    rightXOR = _table[rightXOR % _table.Length];

                    data[k] = (byte)(data[k] ^ rightXOR);
                }
            }
        }
        public void RefParse(byte[] data)
        {
            RefParse(data, 0, data.Length);
        }
        public void RefParse(byte[] data, int length)
        {
            RefParse(data, 0, length);
        }
        public void RefParse(byte[] data, int offset, int length)
        {
            RefParse(data, offset, length, false);
        }
        public void RefParse(byte[] data, int offset, int length, bool isPeeking)
        {
            lock (_parseLock)
            {
                int i = _i;
                int j = _j;
                int[] pool = null;
                if (isPeeking)
                {
                    pool = new int[_table.Length];
                    Array.Copy(_table, pool, pool.Length);
                }
                for (int k = offset, l = 0; l < length; k++, l++)
                {
                    _i++;
                    _i %= _table.Length;
                    _j += _table[_i];
                    _j %= _table.Length;
                    Swap(_i, _j);

                    int rightXOR = _table[_i] + _table[_j];
                    rightXOR = _table[rightXOR % _table.Length];

                    data[k] ^= (byte)rightXOR;
                }
                if (isPeeking)
                {
                    _i = i;
                    _j = j;
                    Array.Copy(pool, _table, _table.Length);
                }
            }
        }

        private void Swap(int a, int b)
        {
            int temp = _table[a];
            _table[a] = _table[b];
            _table[b] = temp;
        }
    }
}
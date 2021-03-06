﻿namespace NWrath.Synergy.Formatting.Structs
{
    public class Token
    {
        public string Value { get; set; }

        public string Key { get; set; }

        public bool IsLiteral => Key == null;

        public Token(string val, string key = null)
        {
            Key = key;

            Value = val;
        }
    }
}
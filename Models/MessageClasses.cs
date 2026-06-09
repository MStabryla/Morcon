using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Morcon.Models
{
    public class Addr
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public int Priority { get; set; }

        public override string ToString()
        {
            return $"Addr(Id: {Id}, Name: {Name}, Priority: {Priority})";
        }

        public static explicit operator string(Addr addr)
        {
            return $"[{addr.Id}]{addr.Name}";
        }
    }

    public abstract class MorconCom
    {
        private readonly Stack<Addr> _addressers = new();
        private readonly Stack<Addr> _addressersSecondStack = new();
        protected readonly StringBuilder _builder = new();

        public long Id { get; set; }
        public DateTime Timestamp { get; set; }

        public Addr? GetNextRecipient()
        {
            _addressers.TryPop(out Addr? recipient);
            return recipient;
        }

        public void AddNextRecipient(Addr address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            while(_addressers.Count > 0 && _addressers.Peek().Priority > address.Priority)
                _addressersSecondStack.Push(_addressers.Pop());
            _addressers.Push(address);
            while(_addressersSecondStack.Count > 0)
                _addressers.Push(_addressersSecondStack.Pop());
        }

        public abstract string Serialize();
    }

    public class JsonCom : MorconCom
    {
        private object? _data;

        public JsonCom(object? data)
        {
            _data = data;
        }

        public object? Data
        {
            get => _data;
        }

        public override string Serialize()
        {
            if (_data is null)
            {
                return string.Empty;
            }
            _builder.Clear();
            var rec = GetNextRecipient();
            _builder.Append(rec); _builder.Append('|'); _builder.Append(JsonSerializer.Serialize(_data));
            return _builder.ToString();
        }
    }

    public class TextCom : MorconCom
    {
        private string? _text;

        public TextCom(string? text)
        {
            _text = text;
        }

        public string? Text
        {
            get => _text;
        }

        public override string Serialize()
        {
            _builder.Clear();
            var rec = GetNextRecipient();
            _builder.Append(rec); _builder.Append('|'); _builder.Append(_text ?? string.Empty);
            return _builder.ToString();
        }
    }

    public class ByteArrayCom : MorconCom
    {
        private byte[]? _data;

        public ByteArrayCom(byte[]? data)
        {
            _data = data;
        }

        public byte[]? Data
        {
            get => _data;
        }

        public override string Serialize()
        {
            _builder.Clear();
            var rec = GetNextRecipient();
            _builder.Append(rec); _builder.Append('|');
            _builder.Append(_data is null ? string.Empty : System.Text.Encoding.UTF8.GetString(_data));
            return _builder.ToString();
        }
    }
}

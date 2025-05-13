
using System;
using System.Globalization;

namespace MVCFramework.Nullables
{
    
    public class EMVCNullable : Exception
    {
        public EMVCNullable(string message) : base(message) { }
    }

    
    public struct NullableString
    {
        private string _value;
        private bool _hasValue;

        
        public bool HasValue => _hasValue;

        
        public bool IsNull => !_hasValue;

        
        public string Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableString value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        
        public void Clear() => SetNull();

        
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }

        
        public string ValueOrDefault() => HasValue ? _value : default;

        
        public string ValueOrElse(string elseValue) => HasValue ? _value : elseValue;

        
        public bool TryHasValue(out string value)
        {
            value = _value;
            return _hasValue;
        }

        public override bool Equals(object obj) =>
            obj is NullableString other && Equals(other);

        public bool Equals(NullableString other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);

        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableString(string value) =>
            new NullableString { _value = value, _hasValue = true };

        public static implicit operator string(NullableString ns) => ns.Value;

        public static bool operator ==(NullableString left, NullableString right) =>
            left.Equals(right);

        public static bool operator !=(NullableString left, NullableString right) =>
            !left.Equals(right);

        public override string ToString() => HasValue ? _value : "";
    }

    
    public struct NullableCurrency
    {
        private decimal _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public decimal Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableCurrency value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public decimal ValueOrDefault() => HasValue ? _value : default;
        public decimal ValueOrElse(decimal elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out decimal value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableCurrency other && Equals(other);
        public bool Equals(NullableCurrency other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableCurrency(decimal value) =>
            new NullableCurrency { _value = value, _hasValue = true };
        public static implicit operator decimal(NullableCurrency nc) => nc.Value;

        public static bool operator ==(NullableCurrency left, NullableCurrency right) =>
            left.Equals(right);
        public static bool operator !=(NullableCurrency left, NullableCurrency right) =>
            !left.Equals(right);
    }

    
    public struct NullableBoolean
    {
        private bool _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public bool Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableBoolean value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public bool ValueOrDefault() => HasValue ? _value : default;
        public bool ValueOrElse(bool elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out bool value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableBoolean other && Equals(other);
        public bool Equals(NullableBoolean other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableBoolean(bool value) =>
            new NullableBoolean { _value = value, _hasValue = true };
        public static implicit operator bool(NullableBoolean nb) => nb.Value;

        public static bool operator ==(NullableBoolean left, NullableBoolean right) =>
            left.Equals(right);
        public static bool operator !=(NullableBoolean left, NullableBoolean right) =>
            !left.Equals(right);
    }

    
    public struct NullableTDate
    {
        private DateTime _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        
        public DateTime Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableTDate value is null");
                return _value.Date;
            }
            set
            {
                _value = value.Date;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public DateTime ValueOrDefault() => HasValue ? _value.Date : default;
        public DateTime ValueOrElse(DateTime elseValue) => HasValue ? _value.Date : elseValue;
        public bool TryHasValue(out DateTime value)
        {
            value = _value.Date;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableTDate other && Equals(other);
        public bool Equals(NullableTDate other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value.Date.Equals(other._value.Date));
        public override int GetHashCode() => HasValue ? _value.Date.GetHashCode() : 0;

        public static implicit operator NullableTDate(DateTime value) =>
            new NullableTDate { _value = value.Date, _hasValue = true };
        public static implicit operator DateTime(NullableTDate ntd) => ntd.Value;

        public static bool operator ==(NullableTDate left, NullableTDate right) =>
            left.Equals(right);
        public static bool operator !=(NullableTDate left, NullableTDate right) =>
            !left.Equals(right);
    }

    
    public struct NullableTTime
    {
        private TimeSpan _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public TimeSpan Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableTTime value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public TimeSpan ValueOrDefault() => HasValue ? _value : default;
        public TimeSpan ValueOrElse(TimeSpan elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out TimeSpan value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableTTime other && Equals(other);
        public bool Equals(NullableTTime other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value.Equals(other._value));
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableTTime(TimeSpan value) =>
            new NullableTTime { _value = value, _hasValue = true };
        public static implicit operator TimeSpan(NullableTTime ntt) => ntt.Value;

        public static bool operator ==(NullableTTime left, NullableTTime right) =>
            left.Equals(right);
        public static bool operator !=(NullableTTime left, NullableTTime right) =>
            !left.Equals(right);
    }

    
    public struct NullableTDateTime
    {
        private DateTime _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public DateTime Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableTDateTime value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public DateTime ValueOrDefault() => HasValue ? _value : default;
        public DateTime ValueOrElse(DateTime elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out DateTime value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableTDateTime other && Equals(other);
        public bool Equals(NullableTDateTime other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value.Equals(other._value));
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableTDateTime(DateTime value) =>
            new NullableTDateTime { _value = value, _hasValue = true };
        public static implicit operator DateTime(NullableTDateTime ntdt) => ntdt.Value;

        public static bool operator ==(NullableTDateTime left, NullableTDateTime right) =>
            left.Equals(right);
        public static bool operator !=(NullableTDateTime left, NullableTDateTime right) =>
            !left.Equals(right);
    }

    
    public struct NullableSingle
    {
        private float _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public float Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableSingle value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public float ValueOrDefault() => HasValue ? _value : default;
        public float ValueOrElse(float elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out float value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableSingle other && Equals(other);
        public bool Equals(NullableSingle other) =>
            (IsNull && other.IsNull) ||
            (HasValue && other.HasValue && Math.Abs(_value - other._value) < 1e-6);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableSingle(float value) =>
            new NullableSingle { _value = value, _hasValue = true };
        public static implicit operator float(NullableSingle ns) => ns.Value;

        public static bool operator ==(NullableSingle left, NullableSingle right) =>
            left.Equals(right);
        public static bool operator !=(NullableSingle left, NullableSingle right) =>
            !left.Equals(right);
    }

    
    public struct NullableDouble
    {
        private double _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public double Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableDouble value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public double ValueOrDefault() => HasValue ? _value : default;
        public double ValueOrElse(double elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out double value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableDouble other && Equals(other);
        public bool Equals(NullableDouble other) =>
            (IsNull && other.IsNull) ||
            (HasValue && other.HasValue && Math.Abs(_value - other._value) < 1e-9);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableDouble(double value) =>
            new NullableDouble { _value = value, _hasValue = true };
        public static implicit operator double(NullableDouble nd) => nd.Value;

        public static bool operator ==(NullableDouble left, NullableDouble right) =>
            left.Equals(right);
        public static bool operator !=(NullableDouble left, NullableDouble right) =>
            !left.Equals(right);
    }

    
    public struct NullableExtended
    {
        private double _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public double Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableExtended value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public double ValueOrDefault() => HasValue ? _value : default;
        public double ValueOrElse(double elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out double value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableExtended other && Equals(other);
        public bool Equals(NullableExtended other) =>
            (IsNull && other.IsNull) ||
            (HasValue && other.HasValue && Math.Abs(_value - other._value) < 1e-9);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableExtended(double value) =>
            new NullableExtended { _value = value, _hasValue = true };
        public static implicit operator double(NullableExtended ne) => ne.Value;

        public static bool operator ==(NullableExtended left, NullableExtended right) =>
            left.Equals(right);
        public static bool operator !=(NullableExtended left, NullableExtended right) =>
            !left.Equals(right);
    }

    
    public struct NullableInt16
    {
        private short _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public short Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableInt16 value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public short ValueOrDefault() => HasValue ? _value : default;
        public short ValueOrElse(short elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out short value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableInt16 other && Equals(other);
        public bool Equals(NullableInt16 other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableInt16(short value) =>
            new NullableInt16 { _value = value, _hasValue = true };
        public static implicit operator short(NullableInt16 ni16) => ni16.Value;

        public static bool operator ==(NullableInt16 left, NullableInt16 right) =>
            left.Equals(right);
        public static bool operator !=(NullableInt16 left, NullableInt16 right) =>
            !left.Equals(right);
    }

    
    public struct NullableUInt16
    {
        private ushort _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public ushort Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableUInt16 value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public ushort ValueOrDefault() => HasValue ? _value : default;
        public ushort ValueOrElse(ushort elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out ushort value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableUInt16 other && Equals(other);
        public bool Equals(NullableUInt16 other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableUInt16(ushort value) =>
            new NullableUInt16 { _value = value, _hasValue = true };
        public static implicit operator ushort(NullableUInt16 nu16) => nu16.Value;

        public static bool operator ==(NullableUInt16 left, NullableUInt16 right) =>
            left.Equals(right);
        public static bool operator !=(NullableUInt16 left, NullableUInt16 right) =>
            !left.Equals(right);
    }

    
    public struct NullableInt32
    {
        private int _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public int Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableInt32 value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public int ValueOrDefault() => HasValue ? _value : default;
        public int ValueOrElse(int elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out int value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableInt32 other && Equals(other);
        public bool Equals(NullableInt32 other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableInt32(int value) =>
            new NullableInt32 { _value = value, _hasValue = true };
        public static implicit operator int(NullableInt32 ni32) => ni32.Value;

        public static bool operator ==(NullableInt32 left, NullableInt32 right) =>
            left.Equals(right);
        public static bool operator !=(NullableInt32 left, NullableInt32 right) =>
            !left.Equals(right);
    }

    
    public struct NullableUInt32
    {
        private uint _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public uint Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableUInt32 value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public uint ValueOrDefault() => HasValue ? _value : default;
        public uint ValueOrElse(uint elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out uint value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableUInt32 other && Equals(other);
        public bool Equals(NullableUInt32 other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableUInt32(uint value) =>
            new NullableUInt32 { _value = value, _hasValue = true };
        public static implicit operator uint(NullableUInt32 nu32) => nu32.Value;

        public static bool operator ==(NullableUInt32 left, NullableUInt32 right) =>
            left.Equals(right);
        public static bool operator !=(NullableUInt32 left, NullableUInt32 right) =>
            !left.Equals(right);
    }

    
    public struct NullableInt64
    {
        private long _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public long Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableInt64 value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public long ValueOrDefault() => HasValue ? _value : default;
        public long ValueOrElse(long elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out long value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableInt64 other && Equals(other);
        public bool Equals(NullableInt64 other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableInt64(long value) =>
            new NullableInt64 { _value = value, _hasValue = true };
        public static implicit operator long(NullableInt64 ni64) => ni64.Value;

        public static bool operator ==(NullableInt64 left, NullableInt64 right) =>
            left.Equals(right);
        public static bool operator !=(NullableInt64 left, NullableInt64 right) =>
            !left.Equals(right);
    }

    
    public struct NullableUInt64
    {
        private ulong _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public ulong Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableUInt64 value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public ulong ValueOrDefault() => HasValue ? _value : default;
        public ulong ValueOrElse(ulong elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out ulong value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableUInt64 other && Equals(other);
        public bool Equals(NullableUInt64 other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value == other._value);
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableUInt64(ulong value) =>
            new NullableUInt64 { _value = value, _hasValue = true };
        public static implicit operator ulong(NullableUInt64 nu64) => nu64.Value;

        public static bool operator ==(NullableUInt64 left, NullableUInt64 right) =>
            left.Equals(right);
        public static bool operator !=(NullableUInt64 left, NullableUInt64 right) =>
            !left.Equals(right);
    }

    
    public struct NullableTGUID
    {
        private Guid _value;
        private bool _hasValue;

        public bool HasValue => _hasValue;
        public bool IsNull => !_hasValue;

        public Guid Value
        {
            get
            {
                if (!HasValue)
                    throw new EMVCNullable("NullableTGUID value is null");
                return _value;
            }
            set
            {
                _value = value;
                _hasValue = true;
            }
        }

        public void Clear() => SetNull();
        public void SetNull()
        {
            _value = default;
            _hasValue = false;
        }
        public Guid ValueOrDefault() => HasValue ? _value : default;
        public Guid ValueOrElse(Guid elseValue) => HasValue ? _value : elseValue;
        public bool TryHasValue(out Guid value)
        {
            value = _value;
            return _hasValue;
        }
        public override bool Equals(object obj) =>
            obj is NullableTGUID other && Equals(other);
        public bool Equals(NullableTGUID other) =>
            (IsNull && other.IsNull) || (HasValue && other.HasValue && _value.Equals(other._value));
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;

        public static implicit operator NullableTGUID(Guid value) =>
            new NullableTGUID { _value = value, _hasValue = true };
        public static implicit operator Guid(NullableTGUID ntguid) => ntguid.Value;

        public static bool operator ==(NullableTGUID left, NullableTGUID right) =>
            left.Equals(right);
        public static bool operator !=(NullableTGUID left, NullableTGUID right) =>
            !left.Equals(right);
    }

    
    public enum NullableType
    {
        InvalidNullableType,
        NullableString,
        NullableCurrency,
        NullableBoolean,
        NullableTDate,
        NullableTTime,
        NullableTDateTime,
        NullableSingle,
        NullableDouble,
        NullableExtended,
        NullableInt16,
        NullableUInt16,
        NullableInt32,
        NullableUInt32,
        NullableInt64,
        NullableUInt64,
        NullableTGUID
    }

    
    public static class NullableHelper
    {
        public static NullableType GetNullableType(Type type)
        {
            if (type == typeof(NullableString)) return NullableType.NullableString;
            if (type == typeof(NullableCurrency)) return NullableType.NullableCurrency;
            if (type == typeof(NullableBoolean)) return NullableType.NullableBoolean;
            if (type == typeof(NullableTDate)) return NullableType.NullableTDate;
            if (type == typeof(NullableTTime)) return NullableType.NullableTTime;
            if (type == typeof(NullableTDateTime)) return NullableType.NullableTDateTime;
            if (type == typeof(NullableSingle)) return NullableType.NullableSingle;
            if (type == typeof(NullableDouble)) return NullableType.NullableDouble;
            if (type == typeof(NullableExtended)) return NullableType.NullableExtended;
            if (type == typeof(NullableInt16)) return NullableType.NullableInt16;
            if (type == typeof(NullableUInt16)) return NullableType.NullableUInt16;
            if (type == typeof(NullableInt32)) return NullableType.NullableInt32;
            if (type == typeof(NullableUInt32)) return NullableType.NullableUInt32;
            if (type == typeof(NullableInt64)) return NullableType.NullableInt64;
            if (type == typeof(NullableUInt64)) return NullableType.NullableUInt64;
            if (type == typeof(NullableTGUID)) return NullableType.NullableTGUID;
            return NullableType.InvalidNullableType;
        }
    }
}

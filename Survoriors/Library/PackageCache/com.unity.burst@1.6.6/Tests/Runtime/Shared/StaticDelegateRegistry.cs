using System;
using System.Collections.Generic;

{
    /// <summary>
    /// This class provides a delegate for each test methods that should have been pre-generated by <see cref="StaticDelegateGenerator"/>
    /// when running the tests locally.
    ///
    /// NOTE: This code is a **SUPER LABORIOUS** workaround a bug in mono 5.11 that is preventing us from dynamically calling a delegate
    /// to native code through Delegate DynamicInvoke.
    ///
    /// On .NET Framework and newer version of mono, we would not need this, so we are going to keep this code around until
    /// mono is entirely upgraded in Unity.
    /// </summary>
    internal static partial class StaticDelegateRegistry
    {
        private static readonly Dictionary<SignatureKey, StaticDelegateCallback> RegisteredDelegateTypes = new Dictionary<SignatureKey, StaticDelegateCallback>();

        private static void Register(Type returnType, Type[] arguments, Type typeDelegate, Func<object, object[], object> caller)
        {
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (typeDelegate == null) throw new ArgumentNullException(nameof(typeDelegate));
            // don't clone as we have the guarantee that it is generated internally
            RegisteredDelegateTypes.Add(new SignatureKey(returnType, arguments), new StaticDelegateCallback(typeDelegate, caller));
        }

        public static bool Contains(Type returnType, Type[] arguments)
        {
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            return RegisteredDelegateTypes.ContainsKey(new SignatureKey(returnType, arguments));
        }

        public static bool TryFind(Type returnType, Type[] arguments, out StaticDelegateCallback staticDelegate)
        {
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            return RegisteredDelegateTypes.TryGetValue(new SignatureKey(returnType, (Type[])arguments.Clone()), out staticDelegate);
        }

        internal struct SignatureKey : IEquatable<SignatureKey>
        {
            public SignatureKey(Type returnType, Type[] parameterTypes)
            {
                ReturnType = returnType;
                ParameterTypes = parameterTypes;
            }

            public readonly Type ReturnType;

            public readonly Type[] ParameterTypes;

            public bool Equals(SignatureKey other)
            {
                if (ParameterTypes.Length != other.ParameterTypes.Length) return false;
                if (ReturnType != other.ReturnType) return false;

                for (int i = 0; i < ParameterTypes.Length; i++)
                {
                    var arg = ParameterTypes[i];
                    var otherArg = other.ParameterTypes[i];
                    if (arg != otherArg) return false;
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is SignatureKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashcode = ReturnType.GetHashCode();
                    for (int i = 0; i < ParameterTypes.Length; i++)
                    {
                        hashcode = (hashcode * 397) ^ ParameterTypes[i].GetHashCode();
                    }
                    return hashcode;
                }
            }

            public static bool operator ==(SignatureKey left, SignatureKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(SignatureKey left, SignatureKey right)
            {
                return !left.Equals(right);
            }
        }

    }

    internal struct StaticDelegateCallback
    {
        public StaticDelegateCallback(Type delegateType, Func<object, object[], object> caller)
        {
            DelegateType = delegateType;
            Caller = caller;
        }

        public readonly Type DelegateType;

        public readonly Func<object, object[], object> Caller;
    }
}
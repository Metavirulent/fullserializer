﻿using System;
using System.Collections.Generic;

namespace FullSerializer.Internal {
    /// <summary>
    /// Serializes and deserializes enums by their current name.
    /// </summary>
    public class fsEnumConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return type.IsEnum;
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            if (Attribute.IsDefined(storageType, typeof(FlagsAttribute))) {
                serialized = new fsData(Convert.ToInt32(instance));
            }
            else {
                serialized = new fsData(Enum.GetName(storageType, instance));
            }
            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            if (data.IsString) {
                string enumValue = data.AsString;

                // Verify that the enum name exists; Enum.TryParse is only available in .NET 4.0
                // and above :(.
                if (ArrayContains(Enum.GetNames(storageType), enumValue) == false) {
                    return fsFailure.Fail("Cannot find enum name " + enumValue + " on type " + storageType);
                }

                instance = Enum.Parse(storageType, enumValue);
                return fsFailure.Success;
            }

            else if (data.IsFloat) {
                int enumValue = (int)data.AsFloat;
                instance = Enum.ToObject(storageType, enumValue);
                return fsFailure.Success;
            }

            return fsFailure.Fail("EnumConverter encountered an unknown JSON data type");
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return Enum.ToObject(storageType, 0);
        }

        /// <summary>
        /// Returns true if the given value is contained within the specified array.
        /// </summary>
        private static bool ArrayContains<T>(T[] values, T value) {
            // note: We don't use LINQ because this function will *not* allocate
            for (int i = 0; i < values.Length; ++i) {
                if (EqualityComparer<T>.Default.Equals(values[i], value)) {
                    return true;
                }
            }

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Skrypt.Execution {
    public static class Extensions {
        public static IEnumerable<MethodInfo> GetMethodsBySig(this Type type, string name, params Type[] parameterTypes) {
            return type.GetMethods().Where((m) => {
                if (m.Name != name) return false;

                var parameters = m.GetParameters();

                if ((parameterTypes == null || parameterTypes.Length == 0))
                    return parameters.Length == 0;
                if (parameters.Length != parameterTypes.Length)
                    return false;
                for (int i = 0; i < parameterTypes.Length; i++) {
                    if (parameterTypes[i].IsSubclassOf(parameters[i].ParameterType) || parameterTypes[i] == parameters[i].ParameterType) {
                        continue;
                    }

                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;
                }

                return true;
            });
        }
    }
}

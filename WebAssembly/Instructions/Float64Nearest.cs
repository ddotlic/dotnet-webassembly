using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Round to nearest integer, ties to even.
	/// </summary>
	public class Float64Nearest : ValueOneToOneCallInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Nearest"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Nearest;

		/// <summary>
		/// Creates a new  <see cref="Float64Nearest"/> instance.
		/// </summary>
		public Float64Nearest()
		{
		}

		internal override MethodInfo MethodInfo => method;

		internal override ValueType ValueType => ValueType.Float64;

		private static readonly RegeneratingWeakReference<MethodInfo> method = new RegeneratingWeakReference<MethodInfo>(() =>
			typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
			{
				if (m.Name != nameof(Math.Round))
					return false;

				var parms = m.GetParameters();
				return parms.Length == 1 && parms[0].ParameterType == typeof(double);
			}));
	}
}
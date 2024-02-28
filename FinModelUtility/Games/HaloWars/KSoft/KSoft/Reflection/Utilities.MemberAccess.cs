using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Exprs = System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;
using Reflect = System.Reflection;

namespace KSoft.Reflection
{
	public partial class Util
	{
		const string kThisName = "this";
		const string kValueName = "value";

		#region Generate Field Accessor Utils
		// ALT: http://forums.asp.net/post/5109977.aspx

		// NOTE: for properties, it's possible they're write only. I'm not going to validate that extreme edge case.
		// If you're trying to wrap such a property then you don't deserve to be using these APIs :P

		/// <summary>Generate a specific member getter for a specific type</summary>
		/// <typeparam name="T">The type which contains the member</typeparam>
		/// <typeparam name="TResult">The member's actual type</typeparam>
		/// <param name="memberName">The member's name as defined in <typeparamref name="T"/></param>
		/// <returns>A compiled lambda which can access (get) the member</returns>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// TResult GetMethod(T @this)
		/// {
		///     return @this.memberName;
		/// }
		/// </code>
		/// </remarks>
		public static Func<T, TResult> GenerateMemberGetter<T, TResult>(string memberName)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<Func<T, TResult>>() != null);

			var param =		Expr.Parameter(typeof(T), kThisName);
			var member =	Expr.PropertyOrField(param, memberName);	// basically 'this.memberName'
			var lambda =	Expr.Lambda<Func<T, TResult>>(member, param);

			return lambda.Compile();
		}
		/// <summary>Generate a specific property getter for a specific type</summary>
		/// <typeparam name="T">The type which contains the member</typeparam>
		/// <typeparam name="TResult">The static member's actual type</typeparam>
		/// <param name="memberName">The member's name as defined in <typeparamref name="T"/></param>
		/// <returns>A compiled lambda which can access (get) the static member</returns>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// TResult GetMethod()
		/// {
		///     return T.memberName;
		/// }
		/// </code>
		/// </remarks>
		public static Func<TResult> GenerateStaticPropertyGetter<T, TResult>(string memberName)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<Func<TResult>>() != null);

			var member =	Expr.Property(null, typeof(T), memberName);	// basically 'T.memberName'
			var lambda =	Expr.Lambda<Func<TResult>>(member);

			return lambda.Compile();
		}
		/// <summary>Generate a specific member getter for a specific type</summary>
		/// <typeparam name="T">The type which contains the member</typeparam>
		/// <typeparam name="TResult">The static member's actual type</typeparam>
		/// <param name="memberName">The member's name as defined in <typeparamref name="T"/></param>
		/// <returns>A compiled lambda which can access (get) the static member</returns>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// TResult GetMethod()
		/// {
		///     return T.memberName;
		/// }
		/// </code>
		/// </remarks>
		public static Func<TResult> GenerateStaticFieldGetter<T, TResult>(string memberName)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<Func<TResult>>() != null);

			var member =	Expr.Field(null, typeof(T), memberName);	// basically 'T.memberName'
			var lambda =	Expr.Lambda<Func<TResult>>(member);

			return lambda.Compile();
		}

		/// <summary>Generate a specific member getter for a specific type</summary>
		/// <typeparam name="TResult">The member's actual type</typeparam>
		/// <param name="type">The type which contains the member</param>
		/// <param name="memberName">The member's name as defined in <paramref name="type"/></param>
		/// <returns>A compiled lambda which can access (get) the member</returns>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// TResult GetMethod(object @this)
		/// {
		///     return ((type)@this).memberName;
		/// }
		/// </code>
		/// </remarks>
		public static Func<object, TResult> GenerateMemberGetter<TResult>(Type type, string memberName)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentException>(!type.IsGenericTypeDefinition);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<Func<object, TResult>>() != null);

			var param =		Expr.Parameter(typeof(object), kThisName);
			var cast_param =Expr.Convert(param, type);						// '((type)this)'
			var member =	Expr.PropertyOrField(cast_param, memberName);	// basically 'this.memberName'
			var lambda =	Expr.Lambda<Func<object, TResult>>(member, param);

			return lambda.Compile();
		}

		/// <summary>Signature for a method which sets a specific member of a value type</summary>
		/// <typeparam name="T">Type of the value-type we're fondling</typeparam>
		/// <typeparam name="TValue">Type of the value we're setting</typeparam>
		/// <param name="this">The object instance to fondle</param>
		/// <param name="value">The new value to set the member to</param>
		public delegate void ValueTypeMemberSetterDelegate<T, in TValue>(ref T @this, TValue value);
		/// <summary>Signature for a method which sets a specific member of a reference type</summary>
		/// <typeparam name="T">Type of the reference-type we're fondling</typeparam>
		/// <typeparam name="TValue">Type of the value we're setting</typeparam>
		/// <param name="this">The object instance to fondle</param>
		/// <param name="value">The new value to set the member to</param>
		public delegate void ReferenceTypeMemberSetterDelegate<in T, in TValue>(T @this, TValue value);

		static void ValidatePropertyForGenerateSetter(Reflect.MemberInfo member)
		{
			if (member.MemberType != Reflect.MemberTypes.Property)
				return;

			var prop_info = (Reflect.PropertyInfo)member;
			if (!prop_info.CanWrite)
				throw new MemberAccessException("Tried to generate setter for get-only property " +
					member.Name + " in " + member.ReflectedType);
		}
		static void ValidateMemberForGenerateSetter(Reflect.MemberInfo member)
		{
			switch(member.MemberType)
			{
			case Reflect.MemberTypes.Field:
			{
				var field_member = (Reflect.FieldInfo)member;
				if (field_member.IsInitOnly)
				{
					throw new MemberAccessException("Tried to generate setter for readonly field " +
						member.Name + " in " + member.ReflectedType);
				}
			} break;

			case Reflect.MemberTypes.Property:
				ValidatePropertyForGenerateSetter(member);
				break;

			default:
				throw new MemberAccessException("Tried to generate setter for unsupported member type " +
					member.Name + " in " + member.ReflectedType);
			}
		}

		/// <summary>Generate a specific member setter for a specific value type</summary>
		/// <typeparam name="T">The type which contains the member</typeparam>
		/// <typeparam name="TValue">The member's actual type</typeparam>
		/// <param name="memberName">The member's name as defined in <typeparamref name="T"/></param>
		/// <returns>A compiled lambda which can access (set) the member</returns>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// void SetMethod(ref T @this, TValue value)
		/// {
		///     @this.memberName = value;
		/// }
		/// </code>
		/// </remarks>
		public static ValueTypeMemberSetterDelegate<T, TValue> GenerateValueTypeMemberSetter<T, TValue>(string memberName)
			where T : struct
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<ValueTypeMemberSetterDelegate<T, TValue>>() != null);

			// Get a "ref type" of the value-type we're dealing with
			// Eg: Guid => "System.Guid&"
			var this_ref = typeof(T).MakeByRefType();

			var param_this =	Expr.Parameter(this_ref, kThisName);
			var param_value =	Expr.Parameter(typeof(TValue), kValueName);		// the member's new value
			var member =		Expr.PropertyOrField(param_this, memberName);	// i.e., 'this.memberName'

			ValidateMemberForGenerateSetter(member.Member);

			var assign =		Expr.Assign(member, param_value);				// i.e., 'this.memberName = value'
			var lambda =		Expr.Lambda<ValueTypeMemberSetterDelegate<T, TValue>>(
									assign, param_this, param_value);

			return lambda.Compile();
		}

		/// <summary>Generate a specific member setter for a specific reference type</summary>
		/// <typeparam name="T">The type which contains the member</typeparam>
		/// <typeparam name="TValue">The member's actual type</typeparam>
		/// <param name="memberName">The member's name as defined in <typeparamref name="T"/></param>
		/// <returns>A compiled lambda which can access (set) the member</returns>
		/// <exception cref="MemberAccessException"><paramref name="memberName"/> is readonly</exception>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// void SetMethod(T @this, TValue value)
		/// {
		///     @this.memberName = value;
		/// }
		/// </code>
		/// </remarks>
		public static ReferenceTypeMemberSetterDelegate<T, TValue> GenerateReferenceTypeMemberSetter<T, TValue>(string memberName)
			where T : class
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<ReferenceTypeMemberSetterDelegate<T, TValue>>() != null);

			var param_this =	Expr.Parameter(typeof(T), kThisName);
			var param_value =	Expr.Parameter(typeof(TValue), kValueName);		// the member's new value
			var member =		Expr.PropertyOrField(param_this, memberName);	// i.e., 'this.memberName'

			ValidateMemberForGenerateSetter(member.Member);

			var assign =		Expr.Assign(member, param_value);				// i.e., 'this.memberName = value'
			var lambda =		Expr.Lambda<ReferenceTypeMemberSetterDelegate<T, TValue>>(
									assign, param_this, param_value);

			return lambda.Compile();
		}
		/// <summary>Generate a specific member setter for a specific reference type</summary>
		/// <typeparam name="TValue">The member's actual type</typeparam>
		/// <param name="type">The type which contains the member</param>
		/// <param name="memberName">The member's name as defined in <paramref name="type"/></param>
		/// <returns>A compiled lambda which can access (set) the member</returns>
		/// <exception cref="MemberAccessException"><paramref name="memberName"/> is readonly</exception>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// void SetMethod(object @this, TValue value)
		/// {
		///     ((type)@this).memberName = value;
		/// }
		/// </code>
		/// </remarks>
		public static ReferenceTypeMemberSetterDelegate<object, TValue> GenerateReferenceTypeMemberSetter<TValue>(Type type, string memberName)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentException>(!type.IsGenericTypeDefinition);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Requires<ArgumentException>(!type.IsValueType, "Type must be a reference type");
			Contract.Ensures(Contract.Result<ReferenceTypeMemberSetterDelegate<object, TValue>>() != null);

			var param_this =	Expr.Parameter(typeof(object), kThisName);
			var param_value =	Expr.Parameter(typeof(TValue), kValueName);		// the member's new value
			var cast_this =		Expr.Convert(param_this, type);					// i.e., '((type)this)'
			var member =		Expr.PropertyOrField(cast_this, memberName);	// i.e., 'this.memberName'

			ValidateMemberForGenerateSetter(member.Member);

			var assign =		Expr.Assign(member, param_value);				// i.e., 'this.memberName = value'
			var lambda =		Expr.Lambda<ReferenceTypeMemberSetterDelegate<object, TValue>>(
									assign, param_this, param_value);

			return lambda.Compile();
		}

		/// <summary>Generate a specific static property setter for a specific reference type</summary>
		/// <typeparam name="T">The type which contains the member</typeparam>
		/// <typeparam name="TValue">The static member's actual type</typeparam>
		/// <param name="memberName">The member's name as defined in <typeparamref name="T"/></param>
		/// <returns>A compiled lambda which can access (set) the member</returns>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// void SetMethod(TValue value)
		/// {
		///     T.memberName = value;
		/// }
		/// </code>
		/// </remarks>
		public static Action<TValue> GenerateStaticPropertySetter<T, TValue>(string memberName)
			where T : class
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<Action<TValue>>() != null);

			var param_value =	Expr.Parameter(typeof(TValue), kValueName);	// the member's new value
			var member =		Expr.Property(null, typeof(T), memberName);	// i.e., 'T.memberName'

			ValidatePropertyForGenerateSetter(member.Member);

			var assign =		Expr.Assign(member, param_value);			// i.e., 'T.memberName = value'
			var lambda =		Expr.Lambda<Action<TValue>>(assign, param_value);

			return lambda.Compile();
		}
		/// <summary>Generate a specific static field setter for a specific reference type</summary>
		/// <typeparam name="T">The type which contains the member</typeparam>
		/// <typeparam name="TValue">The static member's actual type</typeparam>
		/// <param name="memberName">The member's name as defined in <typeparamref name="T"/></param>
		/// <returns>A compiled lambda which can access (set) the member</returns>
		/// <exception cref="MemberAccessException"><paramref name="memberName"/> is readonly</exception>
		/// <remarks>Generates a method similar to this:
		/// <code>
		/// void SetMethod(TValue value)
		/// {
		///     T.memberName = value;
		/// }
		/// </code>
		/// </remarks>
		public static Action<TValue> GenerateStaticFieldSetter<T, TValue>(string memberName)
			where T : class
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(memberName));
			Contract.Ensures(Contract.Result<Action<TValue>>() != null);

			var param_value =	Expr.Parameter(typeof(TValue), kValueName);	// the member's new value
			var member =		Expr.Field(null, typeof(T), memberName);	// i.e., 'T.memberName'

			ValidateMemberForGenerateSetter(member.Member);

			var assign =		Expr.Assign(member, param_value);			// i.e., 'T.memberName = value'
			var lambda =		Expr.Lambda<Action<TValue>>(assign, param_value);

			return lambda.Compile();
		}
		#endregion

		#region PropertyNameFromExpr
		static string PropertyNameFromMemberExpr(Exprs.MemberExpression expr) => expr.Member.Name;

		static string PropertyNameFromUnaryExpr(Exprs.UnaryExpression expr)
		{
			if (expr.NodeType == Exprs.ExpressionType.ArrayLength)
				return "Length";

			var mem_expr = expr.Operand as Exprs.MemberExpression;

			return PropertyNameFromMemberExpr(mem_expr);
		}

		static string PropertyNameFromLambdaExpr(Exprs.LambdaExpression expr)
		{
			if (expr.Body is Exprs.MemberExpression)
				return PropertyNameFromMemberExpr(expr.Body as Exprs.MemberExpression);
			else if (expr.Body is Exprs.UnaryExpression)
				return PropertyNameFromUnaryExpr(expr.Body as Exprs.UnaryExpression);

			throw new NotSupportedException(expr.ToString());
		}

		public static string PropertyNameFromExpr<TProp>(Exprs.Expression<Func<TProp>> expr)
		{
			Contract.Requires<ArgumentNullException>(expr != null);
			Contract.Requires<ArgumentException>(
				expr.Body is Exprs.MemberExpression || expr.Body is Exprs.UnaryExpression);

			return PropertyNameFromLambdaExpr(expr);
		}

		public static string PropertyNameFromExpr<T, TProp>(Exprs.Expression<Func<T, TProp>> expr)
		{
			Contract.Requires<ArgumentNullException>(expr != null);
			Contract.Requires<ArgumentException>(
				expr.Body is Exprs.MemberExpression || expr.Body is Exprs.UnaryExpression);

			return PropertyNameFromLambdaExpr(expr);
		}
		#endregion

		#region MemberFromExpr
		static Reflect.MemberInfo MemberFromExprMemberExpr(Exprs.MemberExpression expr) => expr.Member;

		static Reflect.MemberInfo MemberFromExprUnaryExpr(Exprs.UnaryExpression expr)
		{
			if (expr.NodeType == Exprs.ExpressionType.ArrayLength)
				throw new NotSupportedException();

			var mem_expr = expr.Operand as Exprs.MemberExpression;

			return MemberFromExprMemberExpr(mem_expr);
		}
		static Reflect.MemberInfo MemberFromLambdaExpr(Exprs.LambdaExpression expr)
		{
			if (expr.Body is Exprs.MemberExpression)
				return MemberFromExprMemberExpr(expr.Body as Exprs.MemberExpression);
			else if (expr.Body is Exprs.UnaryExpression)
				return MemberFromExprUnaryExpr(expr.Body as Exprs.UnaryExpression);

			throw new NotSupportedException(expr.ToString());
		}
		public static Reflect.MemberInfo MemberFromExpr<T, TProp>(Exprs.Expression<Func<T, TProp>> expr)
		{
			Contract.Requires(expr != null);
			Contract.Requires(expr.Body is Exprs.MemberExpression || expr.Body is Exprs.UnaryExpression);

			return MemberFromLambdaExpr(expr);
		}

		public static Reflect.PropertyInfo PropertyFromExpr<TProp>(Exprs.Expression<Func<TProp>> expr)
		{
			Contract.Requires(expr != null);
			Contract.Requires(expr.Body is Exprs.MemberExpression || expr.Body is Exprs.UnaryExpression);

			var member = MemberFromLambdaExpr(expr);
			Contract.Assert(member.MemberType == Reflect.MemberTypes.Property);

			return member as Reflect.PropertyInfo;
		}
		public static Reflect.PropertyInfo PropertyFromExpr<T>(Exprs.Expression<Func<T, object>> expr)
		{
			Contract.Requires(expr != null);
			Contract.Requires(expr.Body is Exprs.MemberExpression || expr.Body is Exprs.UnaryExpression);

			var member = MemberFromLambdaExpr(expr);
			Contract.Assert(member.MemberType == Reflect.MemberTypes.Property);

			return member as Reflect.PropertyInfo;
		}
		public static Reflect.PropertyInfo PropertyFromExpr<T, TProp>(Exprs.Expression<Func<T, TProp>> expr)
		{
			Contract.Requires(expr != null);
			Contract.Requires(expr.Body is Exprs.MemberExpression || expr.Body is Exprs.UnaryExpression);

			var member = MemberFromLambdaExpr(expr);
			Contract.Assert(member.MemberType == Reflect.MemberTypes.Property);

			return member as Reflect.PropertyInfo;
		}
		#endregion
	};
}

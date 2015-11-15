﻿namespace TrackerDog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a set of reflection related operations
    /// </summary>
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Creates an instance using reflection of given type where the type has generic parameters.
        /// </summary>
        /// <param name="some">The type to instantiate</param>
        /// <param name="genericArgs">The whole generic parameters</param>
        /// <param name="args">The constructor arguments of the given type</param>
        /// <returns>The instance of given type</returns>
        public static object CreateInstanceWithGenericArgs(this Type some, IEnumerable<object> args, params Type[] genericArgs)
        {
            Contract.Requires(some != null, "Cannot create an instance of a null type");
            Contract.Requires(genericArgs?.Count() != null, "One generic argument must be provided at least");
            Contract.Ensures(Contract.Result<object>() != null);

            return Activator.CreateInstance(some.MakeGenericType(genericArgs), args?.ToArray());
        }

        /// <summary>
        /// Determines if current member is a property getter.
        /// </summary>
        /// <param name="member">The member to determine if it's a property getter</param>
        /// <returns><codeInline>true</codeInline> if it's a property getter, <codeInline>false</codeInline> if it's not a property getter</returns>
        public static bool IsPropertyGetter(this MemberInfo member)
        {
            Contract.Requires(member != null, "Given member cannot be null");

            return member.Name.StartsWith("get_");
        }

        /// <summary>
        /// Determines if the given property is an indexer.
        /// </summary>
        /// <param name="property">The whole property to check</param>
        /// <returns><literal>true</literal> if it's an indexer, <literal>false</literal> if it's not an indexer</returns>
        public static bool IsIndexer(this PropertyInfo property)
        {
            Contract.Requires(property != null, "Given property cannot be null");

            return property.GetIndexParameters().Length > 0;
        }

        /// <summary>
        /// Determines if current member is a property setter.
        /// </summary>
        /// <param name="member">The member to determine if it's a property setter</param>
        /// <returns><codeInline>true</codeInline> if it's a property setter, <codeInline>false</codeInline> if it's not a property setter</returns>
        public static bool IsPropertySetter(this MemberInfo member)
        {
            Contract.Requires(member != null, "Given member cannot be null");

            return member.Name.StartsWith("set_");
        }

        /// <summary>
        /// Determines if current member is a property getter or setter.
        /// </summary>
        /// <param name="member">The member to determine if it's a property getter or setter</param>
        /// <returns><codeInline>true</codeInline> if it's a property getter or setter, <codeInline>false</codeInline> if it's not a property getter or setter</returns>
        public static bool IsPropertyGetterOrSetter(this MemberInfo member)
        {
            return member.IsPropertyGetter() || member.IsPropertySetter();
        }

        /// <summary>
        /// Removes the <codeInline>get_</codeInline> or <codeInline>set_</codeInline> prefix of some property
        /// getter or setter method name.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string NormalizePropertyGetterSetterName(this MemberInfo member)
        {
            Contract.Requires(member != null, "Given member cannot be null");

            return member.Name.Replace("get_", string.Empty).Replace("set_", string.Empty);
        }

        /// <summary>
        /// Gets base property implementation of a trackable object
        /// </summary>
        /// <param name="property">The derived property</param>
        /// <returns>The base property implementation</returns>
        public static PropertyInfo GetBaseProperty(this PropertyInfo property)
        {
            Contract.Requires(property != null, "Given property cannot be null");
            Contract.Ensures(Contract.Result<PropertyInfo>() != null);

            if (property.DeclaringType.IsTrackable())
                return property.DeclaringType.BaseType.GetProperty(property.Name);
            else
                return property;
        }

        /// <summary>
        /// Determines if a given property is an implementation of <see cref="System.Collections.Generic.IList{T}"/>
        /// </summary>
        /// <param name="property">The property to check</param>
        /// <returns><codeInline>true</codeInline> if its an implementation of <see cref="System.Collections.Generic.IList{T}"/>, <codeInline>false</codeInline> if it's not an implementation of <see cref="System.Collections.Generic.IList{T}"/></returns>
        public static bool IsList(this PropertyInfo property)
        {
            Contract.Requires(property != null, "Given property cannot be null");

            return property.PropertyType.IsGenericType
                &&
                (
                    property.PropertyType.IsList()
                    || property.PropertyType.GetInterfaces()
                                .Any(i => i.IsList())
                );
        }
        /// <summary>
        /// Determines if a given type is an implementation of <see cref="System.Collections.Generic.IList{T}"/>
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns><codeInline>true</codeInline> if its an implementation of <see cref="System.Collections.Generic.IList{T}"/>, <codeInline>false</codeInline> if it's not an implementation of <see cref="System.Collections.Generic.IList{T}"/></returns>
        public static bool IsList(this Type type)
        {
            Contract.Requires(type != null, "Given type cannot be null");

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>);
        }

        /// <summary>
        /// Determines if a given property is an implementation of <see cref="System.Collections.Generic.ISet{T}"/>
        /// </summary>
        /// <param name="property">The property to check</param>
        /// <returns><codeInline>true</codeInline> if its an implementation of <see cref="System.Collections.Generic.ISet{T}"/>, <codeInline>false</codeInline> if it's not an implementation of <see cref="System.Collections.Generic.ISet{T}"/></returns>
        public static bool IsSet(this PropertyInfo property)
        {
            Contract.Requires(property != null, "Given property cannot be null");

            return property.PropertyType.IsGenericType && typeof(ISet<>).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition());
        }
        /// <summary>
        /// Determines if a given type is an implementation of <see cref="System.Collections.Generic.ISet{T}"/>
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns><codeInline>true</codeInline> if its an implementation of <see cref="System.Collections.Generic.ISet{T}"/>, <codeInline>false</codeInline> if it's not an implementation of <see cref="System.Collections.Generic.ISet{T}"/></returns>
        public static bool IsSet(this Type type)
        {
            Contract.Requires(type != null, "Given type cannot be null");

            return type.IsGenericType && typeof(ISet<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        /// <summary>
        /// Given a <see cref="System.Collections.Generic.IEnumerable{T}"/> implementation, gets the item type
        /// (i.e. the generic type argument on collection implementation)
        /// </summary>
        /// <param name="some">The collection object</param>
        /// <returns>The type of collection items</returns>
        public static Type GetCollectionItemType(this object some)
        {
            Contract.Requires(some != null, "Given collection object cannot be null");
            Contract.Ensures(Contract.Result<Type>() != null);

            Type enumerableInterface = some.GetType().GetInterfaces()
                                           .SingleOrDefault
                                           (
                                                i => i.IsGenericType
                                                && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                                && i.GetGenericTypeDefinition().GetGenericArguments().Length == 1
                                            );

            Contract.Assert(enumerableInterface != null, "Given object is not a supported collection");
            Contract.Assert(enumerableInterface.GetGenericArguments().Length == 1, "Given collection has no generic type parameter or has more than a parameter");

            return enumerableInterface.GetGenericArguments()[0];
        }

        /// <summary>
        /// Turns an object held by a property into a change-trackable one.
        /// </summary>
        /// <param name="property">The whole property</param>
        /// <param name="parentObject">The object owning the property</param>
        public static void MakeTrackable(this PropertyInfo property, IChangeTrackableObject parentObject)
        {
            Contract.Requires(property != null, "Cannot turn the object held by the property because the given property is null");
            Contract.Requires(parentObject != null, "A non-null reference to the object owning given property is mandatory");

            property.SetValue(parentObject, TrackableObjectFactory.CreateForCollection(property.GetValue(parentObject), parentObject, property));
        }

        /// <summary>
        /// Determines if given type is a <see cref="System.Dynamic.DynamicObject"/> derived class
        /// </summary>
        /// <param name="some">The whole type of the possible dynamic object</param>
        /// <returns><literal>true</literal> if it's a dynamic object, <literal>false</literal> if it's not a dynamic object</returns>
        public static bool IsDynamicObject(this Type some)
        {
            Contract.Requires(some != null, "Given type cannot be null");

            return typeof(DynamicObject).IsAssignableFrom(some);
        }

        /// <summary>
        /// Checks if given property is declared on <see cref="System.Dynamic.DynamicObject"/>.
        /// </summary>
        /// <param name="property">The whole property</param>
        /// <returns><literal>true</literal> if it's of <see cref="System.Dynamic.DynamicObject"/>, <literal>false</literal> if it's not.</returns>
        public static bool IsPropertyOfDynamicObject(this PropertyInfo property)
        {
            Contract.Requires(property != null, "Property must be provided");

            return property.GetBaseProperty().DeclaringType != typeof(DynamicObject);
        }

        /// <summary>
        /// Checks if given method is declared on <see cref="System.Dynamic.DynamicObject"/>.
        /// </summary>
        /// <param name="method">The whole method</param>
        /// <returns><literal>true</literal> if it's of <see cref="System.Dynamic.DynamicObject"/>, <literal>false</literal> if it's not.</returns>
        public static bool IsMethodOfDynamicObject(this MethodInfo method)
        {
            Contract.Requires(method != null, "Method must be provided");

            return method.GetRuntimeBaseDefinition().DeclaringType == typeof(DynamicObject);
        }

        /// <summary>
        /// Determines if given object type is a <see cref="System.Dynamic.DynamicObject"/> derived class
        /// </summary>
        /// <param name="some">The whole possible dynamic object</param>
        /// <returns><literal>true</literal> if it's a dynamic object, <literal>false</literal> if it's not a dynamic object</returns>
        public static bool IsDynamicObject(this object some)
        {
            Contract.Requires(some != null, "Given object cannot be null");

            return IsDynamicObject(some.GetType());
        }

        public static object CallMethod(this object some, string name, IEnumerable<object> args, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return some.GetType().GetMethod(name, bindingFlags).Invoke(some, args?.ToArray());
        }
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// This attribute can be used on action parameters and types, to indicate model level metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class BindAttribute : Attribute, IModelNameProvider, IPropertyBindingPredicateProvider
    {
        private static readonly Func<ModelBindingContext, string, bool> _defaultFilter =
            (context, propertyName) => true;

        private ObjectFactory _factory;

        private Func<ModelBindingContext, string, bool> _predicateFromInclude;

        /// <summary>
        /// Creates a new instace of <see cref="BindAttribute"/>.
        /// </summary>
        /// <param name="include">Names of parameters to include in binding.</param>
        public BindAttribute(params string[] include)
        {
            var items = new List<string>();
            foreach (var item in include)
            {
                items.AddRange(SplitString(item));
            }

            Include = items.ToArray();
        }

        /// <summary>
        /// Creates a new instance of <see cref="BindAttribute"/>.
        /// </summary>
        /// <param name="predicateProviderType">The type which implements
        /// <see cref="IPropertyBindingPredicateProvider"/>.
        /// </param>
        public BindAttribute([NotNull] Type predicateProviderType)
        {
            if (!typeof(IPropertyBindingPredicateProvider).GetTypeInfo()
                    .IsAssignableFrom(predicateProviderType.GetTypeInfo()))
            {
                var message = Resources.FormatPropertyBindingPredicateProvider_WrongType(
                    predicateProviderType.FullName,
                    typeof(IPropertyBindingPredicateProvider).FullName);
                throw new ArgumentException(message, nameof(predicateProviderType));
            }

            PredicateProviderType = predicateProviderType;
        }

        /// <inheritdoc />
        public Type PredicateProviderType { get; }

        /// <summary>
        /// Gets the names of properties to include in model binding.
        /// </summary>
        public string[] Include { get; }

        /// <summary>
        /// Allows a user to specify a particular prefix to match during model binding.
        /// </summary>
        // This property is exposed for back compat reasons.
        public string Prefix { get; set; }

        /// <summary>
        /// Represents the model name used during model binding.
        /// </summary>
        string IModelNameProvider.Name
        {
            get
            {
                return Prefix;
            }
        }

        /// <inheritdoc />
        public Func<ModelBindingContext, string, bool> PropertyFilter
        {
            get
            {
                if (PredicateProviderType != null)
                {
                    var factory = GetFactory();
                    return CreatePredicateFromProviderType(factory);
                }
                else if (Include != null && Include.Length > 0)
                {
                    if (_predicateFromInclude == null)
                    {
                        _predicateFromInclude =
                            (context, propertyName) => Include.Contains(propertyName, StringComparer.Ordinal);
                    }

                    return _predicateFromInclude;
                }
                else
                {
                    return _defaultFilter;
                }
            }
        }

        private ObjectFactory GetFactory()
        {
            if (_factory == null)
            {
                _factory = ActivatorUtilities.CreateFactory(PredicateProviderType, Type.EmptyTypes);
            }
            return _factory;
        }

        private static Func<ModelBindingContext, string, bool> CreatePredicateFromProviderType(
            ObjectFactory factory)
        {
            // Holding state to avoid execessive creation of the provider.
            var initialized = false;
            Func<ModelBindingContext, string, bool> predicate = null;

            return (ModelBindingContext context, string propertyName) =>
            {
                if (!initialized)
                {
                    var services = context.OperationBindingContext.HttpContext.RequestServices;

                    var provider = (IPropertyBindingPredicateProvider)factory(services, arguments: null);

                    initialized = true;
                    predicate = provider.PropertyFilter ?? _defaultFilter;
                }

                return predicate(context, propertyName);
            };
        }

        private static IEnumerable<string> SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = original.Split(',').Select(piece => piece.Trim()).Where(piece => !string.IsNullOrEmpty(piece));

            return split;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.CodeGenerators;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class MvcTagHelperAttributeValueCodeRenderer : TagHelperAttributeValueCodeRenderer
    {
        private const string ModelLambdaVariableName = "__model";

        private readonly GeneratedTagHelperAttributeContext _context;

        /// <summary>
        /// Instantiates a new instance of <see cref="MvcTagHelperAttributeValueCodeRenderer"/>.
        /// </summary>
        /// <param name="context">Contains code generation information for rendering attribute values.</param>
        public MvcTagHelperAttributeValueCodeRenderer([NotNull] GeneratedTagHelperAttributeContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <remarks>If the attribute being rendered is of the type
        /// <see cref="GeneratedTagHelperAttributeContext.ModelExpressionTypeName"/>, then a model expression will be
        /// created by calling into <see cref="GeneratedTagHelperAttributeContext.CreateModelExpressionMethodName"/>.
        /// </remarks>
        public override void RenderAttributeValue(
            [NotNull] TagHelperAttributeDescriptor attributeDescriptor,
            [NotNull] CSharpCodeWriter writer,
            [NotNull] CodeGeneratorContext codeGeneratorContext,
            [NotNull] Action<CSharpCodeWriter> renderAttributeValue,
            bool complexValue)
        {
            if (attributeDescriptor.TypeName.Equals(_context.ModelExpressionTypeName, StringComparison.Ordinal))
            {
                writer
                    .WriteStartMethodInvocation(_context.CreateModelExpressionMethodName)
                    .Write(ModelLambdaVariableName)
                    .Write(" => ");
                if (!complexValue)
                {
                    writer
                        .Write(ModelLambdaVariableName)
                        .Write(".");

                }

                renderAttributeValue(writer);

                writer.WriteEndMethodInvocation(endLine: false);
            }
            else
            {
                base.RenderAttributeValue(
                    attributeDescriptor,
                    writer,
                    codeGeneratorContext,
                    renderAttributeValue,
                    complexValue);
            }
        }
    }
}